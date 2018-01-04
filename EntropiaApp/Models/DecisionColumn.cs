using System.Collections.Generic;
using System.Security.Authentication.ExtendedProtection;

namespace EntropiaApp.Models
{
    public class DecisionColumn
    {
        public DecisionColumn()
        {
            Attributes = new List<DecisionAttributeOccurence>();
        }
        public List<DecisionAttributeOccurence> Attributes { get; set; }
        public double Entropy { get; set; }
        public double InformationGain { get; set; }
        public int? ColumnIndex { get; set; }
    }
}
