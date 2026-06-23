using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BasicNet
{
    public class BiNetwork : BasicNetwork
    {
        public override NetBased CreateObject()
        {
            throw new Exception("CreateObject of BiNetwork has not been implemented yet");
        }
        public override void Assign(object Source)
        {
            base.Assign(Source);
            BiNetwork o = Source as BiNetwork;
            this._source = Node.Clone(o._source);
            this._dest = Node.Clone(o._dest);
            this.originalGraph = o.originalGraph.Clone() as BasicNetwork;
        }
        HashSet<Node> _source = null;
        HashSet<Node> _dest = null;
        BasicNetwork originalGraph = null;
        Node SelectSource(string name)
        {
            var nodes = from p in _source where p.name == name select p;
            if (nodes.Count() > 0)
                return nodes.ElementAt(0);
            return null;
        }
        Node SelectDest(string name)
        {
            var nodes = from p in _dest where p.name == name select p;
            if (nodes.Count() > 0)
                return nodes.ElementAt(0);
            return null;
        }
        public HashSet<Node> Source
        {
            get
            {
                return _source;
            }
        }
        public HashSet<Node> Dest
        {
            get
            {
                return _dest;
            }
        }
        public BiNetwork(BasicNetwork Net)
        {
            originalGraph = Net;
            _source=Node.Clone(Net.Nodes);
            _dest =Node.Clone(Net.Nodes);
            _nodes.UnionWith(_dest);
            _nodes.UnionWith(_source);
            foreach (Interaction arc in Net.Arcs)
            {
                this.AddArc(new Interaction(SelectSource(arc.startNode.name), SelectDest(arc.endNode.name), arc.Type, arc.Name, arc.weight, arc.Direction));
            }
        }
        
        /// <summary>
        /// Find driver nodes on a directed network, represented by this bipartite representation, by Hungarian algorithm
        /// Note: there are multiple driver nodes
        /// </summary>
        /// <returns>Driver nodes</returns>
        public HashSet<Node> findDriverNodes()
        {
            Maximalmatching mm = new Maximalmatching(this.Source, this.Dest);
            HashSet<Node> driverNodes =new HashSet<Node>();

            //Find driver nodes
            foreach (Node node in originalGraph.Nodes)
            {
                bool hasInComeMatchingEdge = false;
                foreach (Interaction arc in node.InLink)
                {
                    if (mm.IsMathchingEdge(arc))
                    {
                        hasInComeMatchingEdge = true;
                        break;
                    }
                        
                }
                if (!hasInComeMatchingEdge) 
                    driverNodes.Add(node);
            }
            return driverNodes;
        }
        /// <summary>
        /// Find driver nodes of a directed network, represented under a bipartite.
        /// Note: there are multiple driver nodes
        /// </summary>
        /// <param name="augmentingGraph">The augmenting graph, including augmenting paths stored in the Arcs of graph</param>
        /// <returns>Driver nodes</returns>
        public HashSet<Node> findDriverNodes(out BasicNetwork augmentingGraph)
        {
            Maximalmatching mm = new Maximalmatching(this.Source, this.Dest);
            HashSet<Node> driverNodes = new HashSet<Node>();

            //Find driver nodes
            foreach (Node node in originalGraph.Nodes)
            {
                bool hasInComeMatchingEdge = false;
                foreach (Interaction arc in node.InLink)
                {
                    if (mm.IsMathchingEdge(arc))
                    {
                        hasInComeMatchingEdge = true;
                        break;
                    }

                }
                if (!hasInComeMatchingEdge)
                    driverNodes.Add(node);
            }
            augmentingGraph = originalGraph.CreateObject() as BasicNetwork;


            foreach (var arc in mm.MatchingEdges)
            {
                Node start = augmentingGraph.AddNode(arc.Key.name);
                Node end = augmentingGraph.AddNode(arc.Value.name);
                IEnumerable<Interaction> originalInteractions=originalGraph.SelectInteraction(arc.Key,arc.Value);
                int type=originalInteractions.Count()>0?originalInteractions.ElementAt(0).Type:0;
                string name = originalInteractions.Count() > 0 ? originalInteractions.ElementAt(0).Name : "";
                double weight = originalInteractions.Count() > 0 ? originalInteractions.ElementAt(0).weight : 0;
                Interaction.DirectionType direction=originalInteractions.Count()>0?originalInteractions.ElementAt(0).Direction: Interaction.DirectionType.undirected;
                augmentingGraph.AddArc(new Interaction(start, end, type,name, weight, direction));
            }
            return driverNodes;
        }
        
    }
}
