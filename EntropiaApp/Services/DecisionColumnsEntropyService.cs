﻿using System;
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
            var bestOne = DecisionColumns.Except(rejectedList).MaxBy(column => column.InformationGain); //TODO: Except already used
            AlreadyUsedIndexes.Add((int)bestOne.ColumnIndex);
            Console.WriteLine($"Columnd index {bestOne.ColumnIndex}");
            foreach (var attribute in bestOne.Attributes)
            {
                var attributeRows = attribute.NegativeRowNumbers.Concat(attribute.PositiveRowNumbers);
                rejectedList = DecisionColumns.Where(column => AlreadyUsedIndexes.Contains((int)column.ColumnIndex));
                foreach (var decisionColumn in DecisionColumns.Except(rejectedList))
                {
                    var restrictedAttributes = new List<DecisionAttributeOccurence>();
                    foreach (var innerAttribute in decisionColumn.Attributes)
                    {
                        if (innerAttribute.PositiveRowNumbers.Intersect(attributeRows).Any() || innerAttribute.NegativeRowNumbers.Intersect(attributeRows).Any())
                        {
                            restrictedAttributes.Add(innerAttribute);
                            //Okej, robimy to na przyklad dla slonecznie, ktore wystepuje w wierszach, zalozmy 1,4 (positive in last col) oraz 7, 9 (negative in lst col)
                            //I teraz sprawdzam ile ta kolumna decyzyjna (np. wilgotnosc) ma roznych attrybutow (np. bardzo, malo) w wierszach 1,4,7,9
                            //Potrzebuje tego zeby moc policzyc Entropie slonecznie z kazda INNA(!!)* kolumna decyzyjna
                            //* - czyli dlatego uzywam tych co nie ma w AlreadyUsed zeby sie nie okazalo ze slonecznie porownuje z pogoda, lub innym uzytym

                            //Jak to bede mial to moge liczyc entropie, w Entropii uzyje ilosc unikalnych atrybutow w kolumnie dla tych wierszy i  tyle jest tych czesci - nawiasow w wyrazeniu matematycznym
                            //Potem Information Gain jest prosty i mamy go gdzies juz liczonego tutaj wyzej
                        }
                    }
                    decisionColumn.CaseEntropy =
                }


            }
            Console.WriteLine(bestOne);
        }
    }
}
