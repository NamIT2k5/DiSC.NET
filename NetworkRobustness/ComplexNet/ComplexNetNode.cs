using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BasicNet
{
    public class ComplexNetNode
    {
        public String id;
        public HashSet<String> attributes;
        public List<ComplexNetEdge> edges;

        public ComplexNetNode()
        {
            attributes = new HashSet<String>();
            edges = new List<ComplexNetEdge>();
        }
    }
}
