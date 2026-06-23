using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using BasicNet;
using System.Diagnostics;
using System.Linq;
using Fuzzy;
using NetSimulation.Lib;
using Mathutil;
namespace BasicNet
{
    public class Node : NetBased
    {
        //protected bool _isLock = false;//always false at the first time
        
        public double weight = 1.0;
        private string _name;
        public string name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value.Trim();
                _id = _name.GetHashCode();
            }
        }
        private int _id = -1;
        /// <summary>
        /// Please use "id" rather than "name" for calculating faster with nodes WITHIN a network
        /// id is assigned by the owner network automatically so that id is corresponding to an unique name, don't change id manually
        /// id is unique in the owner network only. That means two different networks can assign two different ids
        /// </summary>
        public int id
        {
            get
            {
                return _id;
            }
            //set
            //{
            //    _id = value;
            //}
        }
        #region subnetwork of the node
        private BasicNetwork subNet = null;
        public int SubnetID
        {
            get
            {
                return Convert.ToInt32(subNet.Name);
            }
        }
        public BasicNetwork SubNetwork
        {
            get
            {
                return subNet;
            }
            set
            {
                subNet = value;
            }
        }
        public virtual object Perturb(Perturbation.Kind perturbType)
        {
            throw new Exception("Have not defined perturb yet!");
        }
        public virtual void Unperturb(Perturbation.Kind perturbType, object state)
        {
            throw new Exception("Have not defined Unperturb yet!");
        }
        /// <summary>
        /// Create a subnetwork stored inside the node
        /// </summary>
        /// <param name="subNetworkTemplate">The object a presentative for the class of the creating subnetwork</param>
        /// <param name="internalInteractions">Interaction forms the subnetwork</param>
        /// <param name="nodes">Extra nodes is added into the subnet</param>
        /// <returns>a subnetwork created with the class template from parameter subNetworkTemplate</returns>
        public BasicNetwork CreateSubnetwork(BasicNetwork subNetworkTemplate, string subNetname, IEnumerable<Interaction> internalInteractions, IEnumerable<Node> nodes = null)
        {
            subNet = subNetworkTemplate.CreateObject() as BasicNetwork;
            subNet.Name = subNetname;

            if (internalInteractions != null)
            {
                var newint = BasicNetwork.CloneInteraction(subNetworkTemplate, internalInteractions);

                if (nodes == null)
                    subNet.AddNode(Netutil.Union2nodeListByName(
                        (from p in newint select p.startNode),
                        (from p in newint select p.endNode)
                        ).ToArray());
                else//in the case the number of nodes > the number of nodes in the internalInteractions
                {
                    subNet.AddNode(Netutil.Union2nodeListByName(
                        (from p in newint select p.startNode),
                        (from p in newint select p.endNode)
                        ).ToArray());

                    var newNodes = from p in nodes where !subNet.Nodes.Any(t => t.name == p.name) select (Node)p.Clone();
                    subNet.AddNode(newNodes.ToArray());
                }

                subNet.AddArc(newint.ToArray());
            }
            else if (nodes != null)
            {
                subNet.AddNode(nodes.ToArray());
            }
            return subNet;
        }
        #endregion
        public object Tag = null;
        // contain all directed and undirected links
        protected HashSet<Interaction> _arcs = new HashSet<Interaction>();
        //contain ONLY directed links in the _arcs
        protected HashSet<Interaction> _directedArc = new HashSet<Interaction>();
        //contain ONLY undirected links in the _arcs
        protected HashSet<Interaction> _undirectedArc = new HashSet<Interaction>();
        //Edges merged from _arcs
        //protected HashSet<Interaction> _edges = new HashSet<Interaction>();
        protected Dictionary<long, HashSet<Interaction>> ArcDictionary = new Dictionary<long, HashSet<Interaction>>();
        //protected Dictionary<string, Interaction> EdgeDictionary = new Dictionary<string, Interaction>();
        protected Dictionary<long, Interaction> EdgeDictionary = new Dictionary<long, Interaction>();
        public override NetBased CreateObject()
        {
            return new Node("null");
        }
        public override void Assign(Object Source)
        {
            Node Src = Source as Node;
            this.name = Src.name;
            //this._id = Src._id;
            this.weight = Src.weight;
            this.Tag = Src.Tag;
            if (Src.subNet != null)
                this.subNet = Src.subNet.Clone() as BasicNetwork;
        }

        protected void AddArcToDictionary(Interaction arc)
        {
            long KeyArc = BasicNetwork.ArcKey(arc);
            if (!ArcDictionary.ContainsKey(KeyArc))
                ArcDictionary[KeyArc] = new HashSet<Interaction>();
            ArcDictionary[KeyArc].Add(arc);

            long KeyEdge = BasicNetwork.EdgeKey(arc);
            if (!EdgeDictionary.ContainsKey(KeyEdge))
                EdgeDictionary[KeyEdge] = arc;
        }
        protected void RemoveArcFromDictionary(Interaction arc)
        {
            long KeyArc = BasicNetwork.ArcKey(arc);
            if (ArcDictionary.ContainsKey(KeyArc))
            {
                ArcDictionary[KeyArc].Remove(arc);
                if (ArcDictionary[KeyArc].Count == 0)
                {
                    ArcDictionary.Remove(KeyArc);
                    EdgeDictionary.Remove(BasicNetwork.EdgeKey(arc));
                }
            }
        }
        /// <summary>
        /// Attach an edge to the node
        /// </summary>
        /// <param name="isTheStartNode">Type of attachment
        /// true: if this node is a start node
        /// false: if this node is an end node </param>
        /// <param name="arc">The interaction needs attachment</param>
        public void AddArc(bool isTheStartNode, Interaction arc)
        {
            
            if (_arcs.Contains(arc)) return;

            _arcs.Add(arc);
            if (arc.Direction == Interaction.DirectionType.directed)
                _directedArc.Add(arc);
            else
                _undirectedArc.Add(arc);
            AddArcToDictionary(arc);

            if (isTheStartNode)
                arc.startNode = this;
            else
                arc.endNode = this;
        }
        /// <summary>
        /// remove an arc from the network
        /// </summary>
        /// <param name="arc">The arc needs to remove</param>
        public void RemoveArc(Interaction arc)
        {
            //Debug.WriteLine(string.Format("Before removing {0}: Arc={1}, Directed={2}, Undirected={3}",this.name,_arcs.Count,_directedArc.Count,_undirectedArc.Count));
            if (!_arcs.Contains(arc)) return;

            _arcs.Remove(arc);
            _directedArc.Remove(arc);
            _undirectedArc.Remove(arc);
            RemoveArcFromDictionary(arc);
            //Debug.WriteLine(string.Format("After removing {0}: Arc={1}, Directed={2}, Undirected={3}", this.name, _arcs.Count, _directedArc.Count, _undirectedArc.Count));

        }
      
        public IEnumerable<Interaction> Edges
        {
            get
            {
                return EdgeDictionary.Values;
            }
        }
        
      
        public class LinkingNode
        {

            private Node node = null;
            private Interaction interaction = null;
            public LinkingNode(Node node, Interaction interaction)
            {
                this.node = node; this.interaction = interaction;
            }
            public Node Node
            {
                get
                {
                    return node;
                }
            }
            public InteractionType InteractionType
            {
                get
                {
                    return interaction.Type;
                }
            }
            public double InteractionWeight
            {
                get
                {
                    return interaction.weight;
                }
            }
        }
        public List<LinkingNode> GetDesLinkingNodes()
        {
            List<LinkingNode> nodes = new List<LinkingNode>();

            foreach (Interaction itr in OutLink)
            {
                nodes.Add(new LinkingNode(itr.GetPartnerVertex(this), itr));
            }

            return nodes;
        }

        public List<LinkingNode> GetSrcLinkingNodes()
        {
            List<LinkingNode> nodes = new List<LinkingNode>();

            foreach (Interaction itr in InLink)
            {

                nodes.Add(new LinkingNode(itr.GetPartnerVertex(this), itr));

            }
            return nodes;
        }
        /// <summary>
        /// Get both incoming directed links and undirected links
        /// </summary>
        /// <returns></returns>
        public List<LinkingNode> GetSrcMixingLinkingNodes()
        {
            List<LinkingNode> nodes = new List<LinkingNode>();

            foreach (Interaction itr in this.UnLink)
            {
                nodes.Add(new LinkingNode(itr.GetPartnerVertex(this), itr));
            }
            foreach (Interaction itr in InLink)
            {

                nodes.Add(new LinkingNode(itr.GetPartnerVertex(this), itr));

            }
            return nodes;
        }
        public static HashSet<Node> Clone(IEnumerable<Node> pNode)
        {
            HashSet<Node> Copy=new HashSet<Node>();
            foreach (Node node in pNode)
                Copy.Add(node.Clone()as Node);
            return Copy;
        }
        public static HashSet<Node> NeighbourOfGroup(IEnumerable<Node> pNode)
        {
            HashSet<Node> allNeighbour = new HashSet<Node>();
            IEnumerable<Node> aNeighbour = null;

            foreach (Node n in pNode)
            {
                aNeighbour = n.Neighbours;
                if (aNeighbour.Count() == 0)
                    continue;
                var newElements = aNeighbour.Except(pNode);
                if (newElements.Count() > 0)
                    allNeighbour.UnionWith(newElements);
            }
            return allNeighbour;
        }
        /// <summary>
        /// Get the neighbour nodes arcoding to interaction type
        /// </summary>
        /// <param name="interaction">The interaction type</param>
        /// <returns></returns>
        public IEnumerable<Node> GetNeighbour(InteractionType interaction)
        {
            return (from p in this.Arcs where p.Type == interaction select p.GetPartnerVertex(this));
        }
        public IEnumerable<Node> Neighbours
        {
            get
            {
                var nodes= (from p in this.Arcs select p.GetPartnerVertex(this));
                foreach (Node n in nodes)
                    yield return n;
            }
        }
        public IEnumerable<KeyValuePair<Node, Interaction>> NeighbourPairs
        {
            get
            {
                return (from p in this.Arcs select new KeyValuePair<Node,Interaction>(p.GetPartnerVertex(this),p));
            }
        }
        /// <summary>
        /// The nodes are partner nodes from IN-links
        /// </summary>
        public IEnumerable<Node> InNeighbours
        {
             get
            {
                 var nodes= (from p in this.InLink select p.GetPartnerVertex(this));
                foreach (Node n in nodes)
                    yield return n;
            }
        }
        /// <summary>
        /// The nodes are partner nodes from IN- and UNDIRECTED- links
        /// </summary>
        public IEnumerable<Node> InUnNeighbours
        {
            get
            {
                var nodes = (from p in this.InLink select p.GetPartnerVertex(this));
                foreach (Node n in nodes)
                    yield return n;

                //And undriected links
                nodes = (from p in this.UnLink select p.GetPartnerVertex(this));
                foreach (Node n in nodes)
                    yield return n;
            }
        }
        /// <summary>
        /// The nodes are partner nodes from OUT-links  
        /// </summary>
        public IEnumerable<Node> OutNeighbours
        {
            get
            {
                var nodes = (from p in this.OutLink select p.GetPartnerVertex(this));
                foreach (Node n in nodes)
                    yield return n;
                
            }
        }
        /// <summary>
        /// The nodes are partner nodes from OUT- and UNDIRECTED- links  
        /// </summary>
        public IEnumerable<Node> OutUnNeighbours
        {
            get
            {
                var nodes = (from p in this.OutLink select p.GetPartnerVertex(this));
                foreach (Node n in nodes)
                    yield return n;
                //And undriected links
                nodes = (from p in this.UnLink select p.GetPartnerVertex(this));
                foreach (Node n in nodes)
                    yield return n;
            }
        }

        public IEnumerable<Interaction> ArcName(string arcName)
        {
            arcName=arcName.ToLower();
            return from p in this.Arcs where p.Name.ToLower() == arcName select p;

        }

        public IEnumerable<Interaction> ArcTypeLink(int edgeType)
        {
            
            return from p in this.Arcs where p.Type == edgeType select p;

        }
        public IEnumerable<int> typeOfLink
        {
            get
            {
                return from p in this.Arcs group p by p.Type into gj select gj.Key; 
            }

        }
        public IEnumerable<Interaction> EdgeTypeLink(int edgeType)
        {
            return from p in this.Edges where p.Type == edgeType select p;

        }
        /// <summary>
        /// In-links are filtered by link type (undirected links are considered bi-directed links)
        /// </summary>
        /// <param name="edgeType"></param>
        /// <returns></returns>
        public IEnumerable<Interaction> InTypeLink(int edgeType)
        {
            //return from p in Arcs where p.Type == edgeType && p.endNode.name == this.name select p;
            return from p in InLink where p.Type == edgeType select p;

        }
        public IEnumerable<Interaction> InUnTypeLink(int edgeType)
        {
            //return from p in Arcs where p.Type == edgeType && p.endNode.name == this.name select p;
            return from p in InUnLink where p.Type == edgeType select p;

        }
        /// <summary>
        /// Out-links are filtered by link type (undirected links are considered bi-directed links)
        /// </summary>
        /// <param name="edgeType"></param>
        /// <returns></returns>
        public IEnumerable<Interaction> OutTypeLink(int edgeType)
        {
            //return from p in Arcs where p.Type == edgeType && p.startNode.name == this.name select p;
            return from p in OutLink where p.Type == edgeType select p;

        }
        public IEnumerable<Interaction> OutUnTypeLink(int edgeType)
        {
            //return from p in Arcs where p.Type == edgeType && p.startNode.name == this.name select p;
            return from p in OutUnLink where p.Type == edgeType select p;

        }
        /// <summary>
        /// All directed (in- and out- links) and undirected links
        /// </summary>
        public IEnumerable<Interaction> Arcs
        {
            get
            {
                return _arcs;
               
            }
        }
       
        /// <summary>
        /// Directed links in-going to the node
        /// </summary>
        public IEnumerable<Interaction> InLink
        {
            get
            {
                
                //InDirectedLink
                //foreach (Interaction p in Arcs)
                foreach (Interaction p in DirectedLink)
                    if (p.endNode.name == this.name)
                        yield return p;
                
            }
        }
        public IEnumerable<Interaction> InUnLink
        {
            get
            {

               
                foreach (Interaction p in DirectedLink)
                    if (p.endNode.name == this.name)
                        yield return p;
                foreach (Interaction p in this.UnLink)
                        yield return p;

            }
        }
        /// <summary>
        /// Undirected links to the node
        /// </summary>
        public IEnumerable<Interaction> UnLink
        {
            get
            {
                return _undirectedArc;
                //foreach (Interaction e in _undirectedArc)
                //    if (!(e.startNode.isLock || e.endNode.isLock))
                //        yield return e;
            }
        }
        /// <summary>
        /// Out- and in- directed links
        /// </summary>
        public IEnumerable<Interaction> DirectedLink
        {
            get
            {
                return _directedArc;
                //foreach (Interaction e in _directedArc)
                //    if (!(e.startNode.isLock || e.endNode.isLock))
                //        yield return e;
            }
        }
       
        /// <summary>
        /// Directed links out-going from the node
        /// </summary>
        public IEnumerable<Interaction> OutLink
        {
            get
            {
                
                foreach (Interaction p in DirectedLink)
                    if (p.startNode.name == this.name)
                        yield return p;
                 
            }
        }

        public IEnumerable<Interaction> OutUnLink
        {
            get
            {

                foreach (Interaction p in DirectedLink)
                    if (p.startNode.name == this.name)
                        yield return p;
                foreach (Interaction p in UnLink)
                        yield return p;

            }
        }
        public int IncomingEdgeCount
        {
            get
            {
                return InLink.Count() - OutLink.Count();
            }
        }
        public int IncomingTypeEdgeCount(int edgeType)
        {

            return InTypeLink(edgeType).Count() - OutTypeLink(edgeType).Count();
            
        }
        /// <summary>
        /// Check if a given node is a neighbor (connecting To or From) or not
        /// </summary>
        /// <param name="node">The node to be neighbor maybe</param>
        /// <returns>True, is ok</returns>
        public bool hasNeighbor(Node node)
        {
            //var existingArcs = from p in this.Arcs where p.GetPartnerVertex(this).name == node.name select p;
            //if (existingArcs.Count() > 0)
            //    return true;
            //return false;
            return hasLinkTo(node) || hasLinkFrom(node);
        }
       
        public bool hasLinkTo(Node node)
        {
            long KeyThisToNode = BasicNetwork.ArcKey(this.id, node.id);
            return ArcDictionary.ContainsKey(KeyThisToNode);
            //var existingArcs = from p in this.Arcs where (p.startNode.name==this.name) && (p.endNode.name == node.name) select p;
            //if (existingArcs.Count() > 0)
            //    return true;
            //return false;
        }
        public bool hasLinkFrom(Node node)
        {
            long KeyThisFromNode = BasicNetwork.ArcKey(node.id, this.id);
            return ArcDictionary.ContainsKey(KeyThisFromNode);

            //var existingArcs = from p in this.Arcs where (p.endNode.name == this.name) && (p.startNode.name == node.name) select p;
            //if (existingArcs.Count() > 0)
            //    return true;
            //return false;
        }
       
        
        /// <summary>
        /// Return arcs between the node and another node
        /// </summary>
        /// <param name="A">Another node that links from this node connect from or to</param>
        /// <returns>The set of arcs between two nodes</returns>
        public IEnumerable<Interaction> ArcsBetween(Node A)
        {
            long KeyThisToNode = BasicNetwork.ArcKey(this.id, A.id);
            HashSet<Interaction> ThisToNode = new HashSet<Interaction>();

            if (ArcDictionary.ContainsKey(KeyThisToNode))
                ThisToNode = ArcDictionary[KeyThisToNode];

            long KeyThisFromNode = BasicNetwork.ArcKey(A.id, this.id);
            HashSet<Interaction> ThisFromNode = new HashSet<Interaction>();

            if (ArcDictionary.ContainsKey(KeyThisFromNode))
                ThisFromNode = ArcDictionary[KeyThisFromNode];

            foreach (var p in ThisToNode)
                yield return p;
            foreach (var p in ThisFromNode)
                yield return p;
        }
        
        
        public Node(String name, double weight=1.0)
        {
            this.name = name;
            this.weight = weight;
        }
  
        /// <summary>
        /// The number of in-going links, excluding out-going and undirected links
        /// </summary>
        public int InDegree
        {
            get
            {
                return InLink.Count();
            }
        }
        public int InUnDegree
        {
            get
            {
                return InLink.Count()+UnLink.Count();
            }
        }
        /// <summary>
        /// The number of out-going links, excluding in-going and undirected links
        /// </summary>
        public int OutDegree
        {
            get
            {
                return OutLink.Count();
            }
        }
        public double OutWeight
        {
            get
            {
                return (from p in OutLink select p.weight).Sum();
            }
        }
        public double InWeight
        {
            get
            {
                return (from p in InLink select p.weight).Sum();
            }
        }
        public double TotalWeight
        {
            get
            {
                return (from p in Arcs select p.weight).Sum();
            }
        }
        public double OutTotalWeight
        {
            get
            {
                return (from e in OutLink select e.weight).Sum();
            }
        }
        public int OutUnDegree
        {
            get
            {
                return OutLink.Count()+UnLink.Count();
            }
        }
        /// <summary>
        /// Arc degree = in-degree + out-degree + undirected-degree
        /// </summary>
        public int TotalDegree
        {
            get
            {
                
                //return Arcs.Count();
                return InDegree + OutDegree + UnLink.Count();// do not use arcs.Count() for Total degree due to avoid self-loop nodes
            }
        }
        /// <summary>
        /// The number of nodes this node can reach to.
        /// Note: Include this node. Therefore minimum reaching is 1.
        /// </summary>
        public int Reaching
        {
            get
            {


                return BasicNetwork.BreadthFirstTraversal(this).Count();
            }
        }
        public int UndirectedDegree
        {
            get
            {
                //return Arcs.Count();
                return UnLink.Count();// do not use arcs.Count() for Total degree due to avoid self-loop nodes
            }
        }
        public int DirectedDegree
        {
            get
            {
                //return Arcs.Count();
                return this.DirectedLink.Count();// do not use arcs.Count() for Total degree due to avoid self-loop nodes
            }
        }
        public int EdgeDegree
        {
            get
            {
                return Edges.Count();
            }
        }
      
        public IEnumerable<Node> DesNodes
        {
            get
            {
                return (from p in OutLink select p.GetPartnerVertex(this));
            }
        }

        public IEnumerable<Node> SrcNodes
        {
            get
            {
                return (from p in InLink select p.GetPartnerVertex(this));
            }
        }
        public override string ToString()
        {
            //return this.name + "(" + id + ")";
            return string.Format("[{0}]\tname:{1,5}\tDegree:{2}\tIn-Degree:{3}\tOut-Degree:{4}\n", ObjectID, name, EdgeDegree, InDegree, OutDegree);
            //return this.name;
        }
        public class NodeComparer : IEqualityComparer<Node>
        {
            public bool Equals(Node x, Node y)
            {
                //return x.id.Equals(y.id);
                return x.name.Equals(y.name);
            }

            public int GetHashCode(Node obj)
            {
                return obj.name.GetHashCode();
            }
        }
    }
}
