using System;
using System.Collections.Generic;
using ComplexNetGeneratorLib;

namespace ComplexNetGeneratorLib
{
    class RandomNetwork : BasicNetwork
    {
        private int _maxNodes;

        
        Random rd = new Random((int)DateTime.Now.Ticks);
        public RandomNetwork(int maxNodes)
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
                Node node = new Node(Node.ArbitraryFunctionType);
                _nodes.Add(node);

                switch (i)
                {
                    case 0:
                        continue;
                    case 1:
                        _interactions.Add(new Interaction(node, _nodes[i - 1], InteractionType.POSITIVE));
                        continue;
                    case 2:
                        _interactions.Add(new Interaction(node, _nodes[i - 1], InteractionType.POSITIVE));
                        _interactions.Add(new Interaction(node, _nodes[i - 2], InteractionType.POSITIVE));
                        continue;
                }
            }

            for (int i = 3; i < _maxNodes; i++)
            {
                //Node nd = new Node(NodeType.LEAF);
                Node nd = new Node();
                _nodes.Add(nd);
            }

            
            for (int i = 0; i < Nodes.Count - 1; i++)
            {
                for (int j = i + 1; j < Nodes.Count; j++)
                {

                    int d = rd.Next(0, 2);

                    if (d == 0)
                    {
                        _interactions.Add(new Interaction(Nodes[i], Nodes[j], InteractionType.POSITIVE));
                    }
                }
            }

        }

    }
}

