using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BasicNet
{
    public class ComplexNetEdge
    {
        public String id;
        public ComplexNetNode source;
        public ComplexNetNode target;
        public bool directed;
        public HashSet<String> attributes;

        public ComplexNetEdge()
        {
            directed = false;
            attributes = new HashSet<String>();
        }
    }
}
