using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BasicNet
{
    public class DoubleNetwork : BasicNetwork
    {
        public override NetBased CreateObject()
        {
            return new DoubleNetwork();
        }
        public override void Assign(object Source)
        {
            base.Assign(Source);
        }
        public override Node NewNode(string name, object para, double weight = 1.0)
        {
            return new DoubleNode(name,para==null?0:Convert.ToDouble(para));
        }
        public void Test()
        {
            DoubleNetwork net = this.CreateObject() as DoubleNetwork;
            net.AddNodeAndArc(new Interaction(net.AddNode("A"), net.AddNode("B"), 1,"", -1));
            net.AddNodeAndArc(new Interaction(net.AddNode("B"), net.AddNode("A"), 1,"", 1));
            Dictionary<Node,double> states=new Dictionary<Node,double>();
            states.Add(net.SelectNode(new string[]{"A"}).ElementAt(0),3.0);
            states.Add(net.SelectNode(new string[]{"B"}).ElementAt(0),4.0);

            Dictionary<Node,double> attractor=net.FindAttractor(states); 
            
        }
        public Dictionary<Node, double> FindAttractor(Dictionary<Node,double> fromStates)
        {
            const int maxIterations = 6000;
            const double tolerance = 2 * double.Epsilon;
            //const float damping = 0.85f;

            Dictionary<Node, double> nodeStates = new Dictionary<Node, double>();
            Dictionary<Node, double> tempStates = new Dictionary<Node, double>();
            //double iniProbability = 1 / (double)this.Nodes.Count();
            foreach (Node n in this.Nodes)
                nodeStates.Add(n, fromStates[n]);

            double error = 0;
            int iter = 0;
            do
            {
                error = 0;
                foreach (KeyValuePair<Node, double> de in nodeStates)
                {
                    Node v = de.Key;
                    double rank = de.Value;
                    double r = 0;
                    IEnumerable<Interaction> vInteraction = v.InLink;
                    foreach (Interaction e in vInteraction)
                    {
                        Node ingoingNode = e.GetPartnerVertex(v);
                        r += nodeStates[ingoingNode] * e.weight;
                    }

                    double newRank = r;//(1 - damping) + damping * r;
                    tempStates[v] = newRank;
                    error += Math.Abs(rank - newRank);
                }

                // swap ranks
                Dictionary<Node, double> temp = nodeStates;
                nodeStates = tempStates;
                tempStates = temp;

                iter++;
            } while (error > tolerance && iter < maxIterations);
            return nodeStates;
        }
    }
}
