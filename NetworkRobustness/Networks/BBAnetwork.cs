using System;
using System.Collections.Generic;
using ComplexNetGeneratorLib;

namespace ComplexNetGeneratorLib
{
    class BBAnetwork : BasicNetwork
    {
        private int _maxNodes;
      

       
/*
        public BBAnetwork(int maxNodes)
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
                Node node = new Node(NodeType.HUB);
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
            
            Random rd = new Random((int)DateTime.Now.Ticks);
            for (int i = 3; i < _maxNodes; i++)
            {

                Node selectedNode = _nodes[rd.Next(0, _nodes.Count)]; //get random a node from _nodes

                Node addNode = new Node(NodeType.LEAF);
                _nodes.Add(addNode);

                foreach (Node srcNode in selectedNode.GetSrcNodes())
                {
                    _interactions.Add(new Interaction(srcNode, addNode, InteractionType.POSITIVE));
                }

                foreach (Node dstNode in selectedNode.GetDesNodes())
                {
                    _interactions.Add(new Interaction(addNode, dstNode, InteractionType.POSITIVE));
                }
            }

        }
 */
        public BBAnetwork(int maxNodes)
        {
            if (maxNodes <= 3)
            {
                throw new Exception("The number of nodes have to be greater than 3");
            }
            _maxNodes = maxNodes;
            _nodes = new List<Node>();
            _interactions = new List<Interaction>();

            //for (int i = 0; i < 3; i++)
            //{
                //Node node = new Node(NodeType.HUB);
                Node node = new Node();
                _nodes.Add(node);

            //    switch (i)
            //    {
            //        case 0:
            //            continue;
            //        case 1:
            //            _interactions.Add(new Interaction(node, _nodes[i - 1], InteractionType.POSITIVE));
            //            continue;
            //        case 2:
            //            _interactions.Add(new Interaction(node, _nodes[i - 1], InteractionType.POSITIVE));
            //            _interactions.Add(new Interaction(node, _nodes[i - 2], InteractionType.POSITIVE));
            //            continue;
            //    }
            //}

            Random rd = new Random((int)DateTime.Now.Ticks);
            for (int i = 3; i < _maxNodes; i++)
            {

                Node selectedNode = _nodes[rd.Next(0, _nodes.Count)]; //get random a node from _nodes

                //Node addNode = new Node(NodeType.LEAF);
                Node addNode = new Node();
                _nodes.Add(addNode);

                foreach (Node srcNode in selectedNode.GetSrcNodes())
                {
                    _interactions.Add(new Interaction(srcNode, addNode, InteractionType.POSITIVE));
                }

                foreach (Node dstNode in selectedNode.GetDesNodes())
                {
                    _interactions.Add(new Interaction(addNode, dstNode, InteractionType.POSITIVE));
                }
            }

        }
    }
}
