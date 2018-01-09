using System.Collections.Generic;

namespace EntropiaApp.Models
{
    public class DecisionAttributeOccurence
    {
        public DecisionAttributeOccurence(string value)
        {
            Value = value;
            PositiveRowNumbers = new List<int>();
            NegativeRowNumbers = new List<int>();
        }

        public DecisionAttributeOccurence(string value, List<int> positiveValues, List<int> negativeValues)
        {
            Value = value;
            PositiveRowNumbers = positiveValues;
            NegativeRowNumbers = negativeValues;

        }
        public string Value { get; set; }
        public int Amount => PositiveRowNumbers.Count + NegativeRowNumbers.Count;
        public List<int> PositiveRowNumbers { get; set; }
        public List<int> NegativeRowNumbers { get; set; }
        public double CaseEntropy { get; set; }
    }
}