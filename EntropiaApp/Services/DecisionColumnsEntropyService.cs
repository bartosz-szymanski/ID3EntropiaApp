using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using EntropiaApp.Enums;
using EntropiaApp.Models;
using EntropiaApp.Utils;
using MoreLinq;

namespace EntropiaApp.Services
{
    public class DecisionColumnsEntropyService
    {
        private List<Occurence> LastRowOccurences { get; }
        private List<string[]> DecisionRows { get; }
        public List<DecisionColumn> DecisionColumns { get; set; }
        private double TotalEntropy { get; }
        public List<int> AlreadyUsedIndexes { get; set; }
        public Tree TheTree { get; set; }

        public DecisionColumnsEntropyService(List<Occurence> lastRowOccurences, List<string[]> decisionRows, double totalEntropy)
        {
            LastRowOccurences = lastRowOccurences;
            DecisionRows = decisionRows;
            TotalEntropy = totalEntropy;
            AlreadyUsedIndexes = new List<int>();
        }

        public List<DecisionColumn> CalculateDecisions()
        {
            DecisionColumns = new List<DecisionColumn>();
            for (int i = 0; i < DecisionRows.First().Length - 1; i++)
            {
                var decisionColumn = new DecisionColumn();
                var uniqueValues = DecisionRows.Select(value => value[i]).Distinct().ToList();
                foreach (var uniqueValue in uniqueValues)
                {
                    var decisionAttributeOccurence = new DecisionAttributeOccurence(uniqueValue);
                    for (int j = 0; j < DecisionRows.Count; j++)
                    {
                        if (DecisionRows[j][i] == uniqueValue)
                        {
                            var lastColumnDecision = LastRowOccurences.Where(occurence => occurence.RowNumbers.Contains(j + 1)).Select(occurence => occurence.Decision).First();
                            if (lastColumnDecision == DecisionType.Positive)
                            {
                                decisionAttributeOccurence.PositiveRowNumbers.Add(j + 1);
                            }
                            else
                            {
                                decisionAttributeOccurence.NegativeRowNumbers.Add(j + 1);
                            }
                        }
                    }
                    decisionColumn.Attributes.Add(decisionAttributeOccurence);
                }
                DecisionColumns.Add(decisionColumn);
            }
            CalculateColumnsEntropies();
            CalculateColumnInformationGain();
            var rejectedList = DecisionColumns.Where(column => AlreadyUsedIndexes.Contains((int)column.ColumnIndex));
            var bestOne = DecisionColumns.Except(rejectedList).MaxBy(column => column.InformationGain); //Nieużyta z największym przyrostem informacji

            var indexes = new List<int> {(int) bestOne.ColumnIndex};
            DoUnknownThing(indexes, null);

            return DecisionColumns;
        }

        private void CalculateColumnsEntropies()
        {
            foreach (var decisionColumn in DecisionColumns)
            {
                foreach (var attribute in decisionColumn.Attributes)
                {
                    var positiveEntropy = attribute.PositiveRowNumbers.Count != 0
                        ? MathExtension.GetBinaryLogaritm(attribute.PositiveRowNumbers.Count, attribute.Amount)
                        : 0d;
                    var negativeEntropy = attribute.NegativeRowNumbers.Count != 0
                        ? MathExtension.GetBinaryLogaritm(attribute.NegativeRowNumbers.Count, attribute.Amount)
                        : 0d;

                    decisionColumn.Entropy += (double)attribute.Amount / DecisionRows.Count * (-positiveEntropy - negativeEntropy);
                }
            }
        }

        private void CalculateColumnInformationGain()
        {
            foreach (var decisionColumn in DecisionColumns)
            {
                decisionColumn.InformationGain = TotalEntropy - decisionColumn.Entropy;
                decisionColumn.ColumnIndex = DecisionColumns.IndexOf(decisionColumn);
            }
        }

        private DecisionAttributeOccurence GetRowsFromPath(List<int> path)
        {
            DecisionColumn currentDecisionColumn = null;
            DecisionAttributeOccurence currentAttribute = null;
            var positiveRows = new List<int>();
            var negativeRows = new List<int>();
            for (var i = 0; i < path.Count; i++)
            {
                if (i == 0 || i % 2 == 0)
                {
                    currentDecisionColumn = DecisionColumns[path[i]];
                }
                else
                {
                    currentAttribute = currentDecisionColumn?.Attributes[path[i]];
                }
                if (i <= 0) 
                    continue;
                
                if (positiveRows.Count < 1 && negativeRows.Count < 1)
                {
                    positiveRows = currentAttribute.PositiveRowNumbers;
                    negativeRows = currentAttribute.NegativeRowNumbers;
                }
                else
                {
                    positiveRows = positiveRows.Intersect(currentAttribute.PositiveRowNumbers).ToList();
                    negativeRows = negativeRows.Intersect(currentAttribute.NegativeRowNumbers).ToList();
                }
            }
            return new DecisionAttributeOccurence(currentAttribute.Value, positiveRows, negativeRows);
        }
        
        private void DoUnknownThing(List<int> bestOneIndexes, List<List<int>> fullHierarchy)
        {
            var index = -1;
            var selectedIndexesForNextRound = new List<int>();
            var newHierarchy = new List<List<int>>();
            var locallyUsedColumnIndexes = new List<int>();
            var localAttributeValue = string.Empty;
            foreach (var bestOneIndex in bestOneIndexes)
            {
                index++;
                var bestOne = DecisionColumns.First(col => col.ColumnIndex == bestOneIndex);
                bestOne.ColumnIndex = bestOneIndex;
                var rejectedList =
                    DecisionColumns.Where(column => AlreadyUsedIndexes.Contains((int) column.ColumnIndex));
                AlreadyUsedIndexes.Add((int) bestOne.ColumnIndex); //Dodajemy ja do listy na przyszlosc
                Console.WriteLine($"Columnd index {bestOne.ColumnIndex}");

                foreach (var attribute in bestOne.Attributes) // i dla kazdego atrybutu najlepszej kolumny czyli np. dla pogody iterujemy sie po: sloecznie, pochmurno, itd.
                {
                    var attributeRows =
                        attribute.NegativeRowNumbers.Concat(attribute.PositiveRowNumbers)
                            .ToList(); // bierzemy wszystkie indeksy wierszy w jakich wystepuja
                    rejectedList = DecisionColumns.Where(column => AlreadyUsedIndexes.Contains((int) column.ColumnIndex));
                    var tempList = DecisionColumns.Except(rejectedList);
                    foreach (var decisionColumn in tempList
                    ) // i iterujemy sie po wszystkich niewykorzystanych kolumnach 
                    {
                        decisionColumn.ColumnIndex = DecisionColumns.IndexOf(decisionColumn);
                        var innerAttributes = decisionColumn.Attributes
                            .Where(innerAttribute =>
                                innerAttribute.PositiveRowNumbers.Intersect(attributeRows).Any() ||
                                innerAttribute.NegativeRowNumbers.Intersect(attributeRows).Any()).ToList();
                        //to sa te atrybuty, ktory wystepuja w wierszach 'attribute'
                        decisionColumn.CaseEntropy = 0;
                        var allNegatives = innerAttributes.SelectMany(innerAttr => innerAttr.NegativeRowNumbers)
                            .ToList();
                        if (allNegatives.Count(neg => attributeRows.Contains(neg)) > 0)
                        {
                            foreach (var innerAttribute in innerAttributes)
                            {
                                var innerRows = innerAttribute.NegativeRowNumbers
                                    .Concat(innerAttribute.PositiveRowNumbers)
                                    .ToList();
                                var innerPositiveRows = innerAttribute.PositiveRowNumbers
                                    .Where(inner => attributeRows.Contains(inner)).ToList();
                                var innerNegativeRows = innerAttribute.NegativeRowNumbers
                                    .Where(inner => attributeRows.Contains(inner)).ToList();

                                var positiveEntropy = innerPositiveRows.Count != 0
                                    ? MathExtension.GetBinaryLogaritm(innerPositiveRows.Count,
                                        innerNegativeRows.Count + innerPositiveRows.Count)
                                    : 0d;
                                var negativeEntropy = innerNegativeRows.Count != 0
                                    ? MathExtension.GetBinaryLogaritm(innerNegativeRows.Count,
                                        innerNegativeRows.Count + innerPositiveRows.Count)
                                    : 0d;

                                decisionColumn.CaseEntropy +=
                                    (double) (innerNegativeRows.Count + innerPositiveRows.Count) / attributeRows.Count *
                                    (-positiveEntropy - negativeEntropy);
                            }
                        }
                        else
                        {
                            decisionColumn.CaseEntropy = -1;
                        }
                        locallyUsedColumnIndexes.Add(Convert.ToInt32(decisionColumn.ColumnIndex));
                    }
                    Console.WriteLine("Nazwa atrybutu" + attribute.Value);
                    localAttributeValue = attribute.Value;
                    if (tempList.Any())
                    {
                        var x = tempList.MinBy(column => column.CaseEntropy);
                        
                        if (attribute.PositiveRowNumbers.Count == 0 && attribute.NegativeRowNumbers.Count > 0) 
                        {
                            Console.WriteLine("NOPE NOPE NOPE");
                        }
                        else if (x.CaseEntropy > -1)
                        {
                            Console.WriteLine("Index kolumny >>>> " + x.ColumnIndex);
                            selectedIndexesForNextRound.Add(Convert.ToInt32(x.ColumnIndex));
                        }
                        else
                        {
                            if(attribute.PositiveRowNumbers.Count == 0 && attribute.NegativeRowNumbers.Count > 0)
                                Console.WriteLine("Nie");
                            if(attribute.PositiveRowNumbers.Count > 0 && attribute.NegativeRowNumbers.Count == 0)
                                Console.WriteLine("Tak");
                        }
                        var currentBranchHierarchy = new List<int>();
            
                        if (fullHierarchy?[index] != null)
                            currentBranchHierarchy.AddRange(fullHierarchy[index]);
                        else
                            currentBranchHierarchy.Add(bestOneIndexes.First());

                        if (localAttributeValue != "-1" && x.CaseEntropy > -1)
                        {
                            var upperColumn = DecisionColumns[bestOneIndexes[index]];
                            currentBranchHierarchy.Add(upperColumn.Attributes.FindIndex(atr => atr.Value == localAttributeValue));
                            currentBranchHierarchy.Add(selectedIndexesForNextRound[index]);
                            newHierarchy.Add(currentBranchHierarchy);
                        }
                    }
                    else
                    {
                        var newThing = GetRowsFromPath(fullHierarchy[index]);
                        var positiveRows = attribute.PositiveRowNumbers.Intersect(newThing.PositiveRowNumbers).ToList();
                        var negativeRows = attribute.NegativeRowNumbers.Intersect(newThing.NegativeRowNumbers).ToList();
                        var x = attribute.NegativeRowNumbers.Count;
                        var y = attribute.PositiveRowNumbers.Count;
                        if (positiveRows.Count == 0 && negativeRows.Count > 0)
                        {
                            Console.WriteLine("Nie!");
                        }
                        else if( positiveRows.Count > 0 && negativeRows.Count == 0)
                        {
                            Console.WriteLine("Tak!");
                        }
                    }
                }
                //Sprzatanie tutaj?
                Console.WriteLine("----------------------");
            }
            AlreadyUsedIndexes.AddRange(locallyUsedColumnIndexes);
           
            if(selectedIndexesForNextRound.Count > 1)
                DoUnknownThing(selectedIndexesForNextRound, newHierarchy);
        }
    }
}
