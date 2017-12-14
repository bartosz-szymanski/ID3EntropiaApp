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
        public string Value { get; set; }
        public int Amount => PositiveRowNumbers.Count + NegativeRowNumbers.Count;
        public List<int> PositiveRowNumbers { get; set; }
        public List<int> NegativeRowNumbers { get; set; }
    }
}