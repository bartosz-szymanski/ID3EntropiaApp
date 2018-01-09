using System;
using System.Collections.Generic;
using System.Linq;
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
            DoUnknownThing();

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

        private void DoUnknownThing()
        {
            var rejectedList = DecisionColumns.Where(column => AlreadyUsedIndexes.Contains((int)column.ColumnIndex));
            var bestOne = DecisionColumns.Except(rejectedList).MaxBy(column => column.InformationGain); //Nieużyta z największym przyrostem informacji
            AlreadyUsedIndexes.Add((int)bestOne.ColumnIndex); //Dodajemy ja do listy na przyszlosc
            Console.WriteLine($"Columnd index {bestOne.ColumnIndex}");
            foreach (var attribute in bestOne.Attributes) // i dla kazdego atrybutu najlepszej kolumny czyli np. dla pogody iterujemy sie po: sloecznie, pochmurno, itd.
            {
                var attributeRows = attribute.NegativeRowNumbers.Concat(attribute.PositiveRowNumbers).ToList(); // bierzemy wszystkie indeksy wierszy w jakich wystepuja
                rejectedList = DecisionColumns.Where(column => AlreadyUsedIndexes.Contains((int)column.ColumnIndex)); // i jaka kolumna teraz? rozwazamy znow wszystkie nieuzyte
                var tempList = DecisionColumns.Except(rejectedList);
                foreach (var decisionColumn in tempList) // i iterujemy sie po wszystkich niewykorzystanych kolumnach 
                {
                    var innerAttributes = decisionColumn.Attributes
                        .Where(innerAttribute => innerAttribute.PositiveRowNumbers.Intersect(attributeRows).Any() || innerAttribute.NegativeRowNumbers.Intersect(attributeRows).Any()).ToList();
                    //to sa te atrybuty, ktory wystepuja w wierszach 'attribute'
                    decisionColumn.CaseEntropy = 0;
                    var allNegatives = innerAttributes.SelectMany(innerAttr => innerAttr.NegativeRowNumbers).ToList();
                    if (allNegatives.Count(neg => attributeRows.Contains(neg)) > 0)
                    {
                        foreach (var innerAttribute in innerAttributes)
                        {
                            var innerRows = innerAttribute.NegativeRowNumbers.Concat(innerAttribute.PositiveRowNumbers)
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

                        Console.WriteLine("Wynik: " + decisionColumn.CaseEntropy);
                    }
                    else
                    {
                        Console.WriteLine("TAAAAAAAK");
                    }
                }


            }
            Console.WriteLine(bestOne);
        }
    }
}
