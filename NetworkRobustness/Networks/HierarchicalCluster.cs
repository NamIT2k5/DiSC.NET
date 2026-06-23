using System.Collections.Generic;
using ComplexNetGeneratorLib;

namespace ComplexNetGeneratorLib
{
    class HierarchicalCluster
    {
        private Node _root;
        private List<Node> _branchs;
        private List<Node> _leafs;
        private List<Interaction> _interactions;

        public List<Node> Nodes
        {
            get
            {
                List<Node> nodesReturn = new List<Node> {_root};
                nodesReturn.AddRange(_branchs);
                nodesReturn.AddRange(_leafs);
                return nodesReturn;
            }
        }

        public List<Interaction> Interactions
        {
            get { return _interactions; }
        }

        public HierarchicalCluster(Node root, List<Node> branchs, List<Node> leafs, List<Interaction> interactions)
        {
            _root = root;
            //meaning of ??
            // if (branchs != null) { _branchs = branchs; } else { _branchs = new List<Node>();}
            _branchs = branchs ?? new List<Node>();
            _leafs = leafs ?? new List<Node>();
            _interactions = interactions ?? new List<Interaction>();
        }

        public HierarchicalCluster(HierarchicalCluster rootCluster, IEnumerable<HierarchicalCluster> leafClusters)
        {
            rootCluster.ChangeToRootCluster();

            _root = rootCluster._root; //add root

            _branchs = new List<Node>();
            _leafs = new List<Node>();
            _interactions = new List<Interaction>();

            _branchs.AddRange(rootCluster._branchs);
            _branchs.AddRange(rootCluster._leafs);

            _interactions.AddRange(rootCluster._interactions);

            foreach (HierarchicalCluster cl in leafClusters)
            {
                cl.ChangeToLeafCluster();

                _branchs.Add(cl._root);
                _branchs.AddRange(cl._branchs);

                _leafs.AddRange(cl._leafs);

                _interactions.AddRange(cl._interactions);

                foreach (Node nd in cl._leafs)
                {
                    _interactions.Add(new Interaction(nd, _root, InteractionType.POSITIVE));
                }
            }
        }

        private void ChangeToRootCluster()
        {
            /*
            _root.Type = NodeType.HUB;
            foreach (Node nd in _leafs)
            {
                nd.Type = NodeType.BRANCH;
            }
             */
        }

        private void ChangeToLeafCluster()
        {
            //_root.Type = NodeType.BRANCH;
        }
    }
}
