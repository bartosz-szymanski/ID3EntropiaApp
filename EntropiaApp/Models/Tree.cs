using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Xsl.Runtime;

namespace EntropiaApp.Models
{
    public class Tree
    {
        public Node Node { get; set; }
    }

    public class Node
    {
        public Node()
        {
            
        }

        public DecisionAttributeOccurence PreviousAttribue { get; set; }
        public DecisionColumn  CurrentDecisionColumn { get; set; }
        public List<Node> Nodes { get; set; }
        public string Result { get; set; }
    }
}