using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mathutil;

namespace BasicNet
{
    public class DoubleNode:Node
    {
        protected double state = 0;
        public override NetBased CreateObject()
        {
            return new DoubleNode("null",0);
        }
        public override void Assign(object Source)
        {
            base.Assign(Source);
            DoubleNode o = Source as DoubleNode;
            this.state = o.state;
        }
        public DoubleNode(string name, double state)
            : base(name)
        {
            this.state = state;
        }
       
    }
}
