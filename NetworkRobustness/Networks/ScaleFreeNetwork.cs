using System;
using System.Collections.Generic;
using ComplexNetGeneratorLib;

namespace ComplexNetGeneratorLib
{
    class ScaleFreeNetwork : BasicNetwork
    {
        private int _maxNodes;

       

        static Random rd = new Random((int)DateTime.Now.Ticks);
        
        public ScaleFreeNetwork(int maxNodes)
        {
            if (maxNodes <= 3)
            {
                throw new Exception();
            }
            _maxNodes = maxNodes;
            _nodes = new List<Node>();
            _interactions = new List<Interaction>();

            for (int i = 0; i < 3; i++)
            {
                //Node node = new Node(NodeType.HUB);
                Node node = new Node();
                _nodes.Add(node);

                switch (i)
                {
                    case 0:
                        continue;
                    case 1:

                        _interactions.Add(Interaction.RandomInteraction(node, _nodes[i - 1]));
                        continue;
                    case 2:
                        _interactions.Add(Interaction.RandomInteraction(node, _nodes[i - 1]));
                        _interactions.Add(Interaction.RandomInteraction(node, _nodes[i - 2]));
                        continue;
                }
            }

            
            for (int i = 3; i < _maxNodes; i++)
            {
                //Node nd = new Node(NodeType.LEAF);
                Node nd = new Node();
                _nodes.Add(nd);

                int total = 0;

                foreach (Node node in _nodes)
                {
                    total += node.GetSrcNodes().Count;
                }

                foreach (Node node in _nodes)
                {
                    float prob;
                    if (total == 0)
                    {
                        prob = (float) 0.9;
                    }
                    else
                    {
                        prob = (float) node.GetSrcNodes().Count/total;
                    }
                    
                    double d = rd.NextDouble() % 1;

                    if (d <= prob * 2) 
                    {
                        _interactions.Add(Interaction.RandomInteraction(nd, node));
                    }
                }
            }

           // this.WriteGraph("scalefree" + (++ind).ToString()+ ".txt");
        }

    }
}
