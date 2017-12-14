using System;
using System.Collections.Generic;
using System.Linq;
using EntropiaApp.Models;
using EntropiaApp.Utils;

namespace EntropiaApp.Services
{
    public class LastRowEntropyService
    {
        private List<Occurence> LastRowOccurences { get; }
        private List<string[]> DecisionRows { get; }

        public LastRowEntropyService(List<string[]> decisionRows)
        {
            LastRowOccurences = new List<Occurence>();
            DecisionRows = decisionRows;
        }

        private string[] GetUniqueValues(int position) => DecisionRows.Select(array => array[position]).Distinct().ToArray();

        private void ShowDecisionsCountMessage() => Console.WriteLine($"There are {DecisionRows.Count} decisions");

        private void ShowLastDigitsAndOccurences()
        {
            foreach (var occurence in LastRowOccurences)
            {
                Console.WriteLine($"Number {occurence.Value} appears {occurence.Amount} times in last column in rows {string.Join(",", occurence.RowNumbers.Select(row => row.ToString()).ToArray())}");
            }
        }

        private List<int> GetRowNumbersForOccurences(string value)
        {
            var rowNumbers = new List<int>();
            for (var i = 0; i < DecisionRows.Count; i++)
            {
                var decision = DecisionRows[i].Last();
                if (decision == value)
                {
                    rowNumbers.Add(i + 1);
                }
            }

            return rowNumbers;
        }
        private void GetOccurencesList(int position)
        {
            var uniques = GetUniqueValues(position);
            foreach (var unique in uniques)
            {
                var rows = GetRowNumbersForOccurences(unique);
                LastRowOccurences.Add(new Occurence(unique, DecisionRows.Count(array => array[position] == unique), rows));
            }
            ShowLastDigitsAndOccurences();
            ShowDecisionsCountMessage();
        }

        private double CalculateTotalEntropy()
        {
            var decisions = LastRowOccurences.Sum(occurence => occurence.Amount);
            var totalEntropy = 0d;
            foreach (var occurence in LastRowOccurences)
            {
                totalEntropy += (double)occurence.Amount / decisions * occurence.Entropy;
            }

            return totalEntropy;
        }

        private void GetInformationGain(double totalEntropy)
        {
            foreach (var occurence in LastRowOccurences)
            {
                occurence.InformationGain = totalEntropy - occurence.Entropy;
            }
        }

        public List<Occurence> CalculateEntropies()
        {
            GetOccurencesList(DecisionRows.First().Length - 1);
            foreach (var occurence in LastRowOccurences)
            {
                occurence.Entropy = -(MathExtension.GetBinaryLogaritm(occurence.Amount, DecisionRows.Count) + MathExtension.GetBinaryLogaritm(DecisionRows.Count - occurence.Amount, DecisionRows.Count));
            }
            var totalEntropy = CalculateTotalEntropy();
            GetInformationGain(totalEntropy);
            Console.WriteLine($"Total entropy is equal to {totalEntropy}");
            var decisionColumnsCalculator = new DecisionColumnsEntropyService(LastRowOccurences, DecisionRows, totalEntropy);
            var decisionColumns = decisionColumnsCalculator.CalculateDecisions();

            return LastRowOccurences;
        }
    }
}
