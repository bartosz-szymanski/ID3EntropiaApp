using System.Collections.Generic;
using EntropiaApp.Enums;

namespace EntropiaApp.Models
{
    public class Occurence
    {
        public Occurence(string value, int amount, List<int> rowNumbers)
        {
            Value = value;
            Amount = amount;
            RowNumbers = rowNumbers;
        }
        public string Value { get; set; }
        public DecisionType Decision
        {
            get
            {
                var decision = DecisionType.Negative;
                if (Value == "1")
                {
                    decision = DecisionType.Positive;
                }
                return decision;
            }
        }

        public int Amount { get; set; }
        public List<int> RowNumbers { get; set; }
        public double Entropy { get; set; }
        public double InformationGain { get; set; }
    }
}