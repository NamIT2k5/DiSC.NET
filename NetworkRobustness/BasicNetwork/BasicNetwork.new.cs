using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using BasicNet;
using System.Xml;
using System.IO;
using System.Diagnostics;
using NetSimulation.Community;
using NetSimulation.Lib;
using Mathutil;
using Fuzzy;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Concurrent;
using System.Threading;
namespace BasicNet
{
    /// <summary>
    /// 1. The network topology is considered a set of links, which there is only one link between 2 nodes the link can be namely discribed by: a arc, or a pair of arcs
    /// Given a node pair, all arcs between the node pair are structurally merged as an edge, in which each arc plays a relationship kind between two nodes.
    /// 2. Arcs and _arcs properties:
    ///     - To create, delete links into network
    ///     - To compute dynamic properties suchs as Robustness
    /// 3. Edges and _edge properties:
    ///     - To show real network structures (with only one link between two nodes)
    ///     - To compute structural properties such as centrality, modularity
    /// </summary>
    public class  BasicNetwork:NetBased
    {
        #region Methods have to be overrided in sub-classes
        public override void Assign(object Source)
        {
            BasicNetwork o = Source as BasicNetwork;
            this.name = o.name;
            //this._nodes.UnionWith(Netutil.CloneNode(o._nodes));

            for (int i = 0; i < o._arcs.Count; i++)
            {
                Interaction arc = o._arcs.ElementAt(i);

                Node start = this.AddNode((Node)arc.startNode.Clone());
                Node end = this.AddNode((Node)arc.endNode.Clone());
                this.AddArc(new Interaction(start, end, arc.Type, arc.Name, arc.weight, arc.Direction));
            }
            IEnumerable<Node> isolateNodes = Netutil.SubstractNodeListByName(o._nodes, this._nodes);
            foreach (Node n in isolateNodes)
                this.AddNode((Node)n.Clone());
        }
        public override NetBased CreateObject()
        {
            return new BasicNetwork();
        }
        public virtual Node NewNode(string name, object para, double weight = 1.0)
        {
            return new Node(name, weight);
            
        }
        public virtual Node[,] NewNodeArray(int n, int m)
        {
            return new Node[n, m];
            
        }
        public virtual Node[] NewNodeArray(int n)
        {
            return new Node[n];
        }
        public virtual Node[] NewNodeArray(params Node[] node)
        {
            return node;
        }
        /// <summary>
        /// The node in the network
        /// </summary>
        /// <param name="NodeName">name of the node</param>
        /// <returns></returns>
        public Node this[string NodeName]
        {
            get 
            {
                return nodeNameDictionary[NodeName];
            }
            
        }
        public Node this[int id]
        {
            get
            {
                return nodeIdDictionary[id];
            }

        }
        public IEnumerable<Interaction> this[string start,string end]
        {
            get
            {
                return ArcDictionary[BasicNetwork.ArcKey(nodeNameDictionary[start].id,nodeNameDictionary[end].id)];
            }

        }
        #endregion

        #region properties
        
        
        protected HashSet<Interaction> _arcs = new HashSet<Interaction>();
        
        protected HashSet<Node> _nodes = new HashSet<Node>();
        protected string name = "no name";
        
        
        #region member to speed up access (not using in copying object)
        //protected HashSet<Interaction> _edges = new HashSet<Interaction>();
        public Dictionary<string, Node> nodeNameDictionary = new Dictionary<string, Node>();
        public Dictionary<int, Node> nodeIdDictionary = new Dictionary<int, Node>();
        /// <summary>
        /// Store all arcs in the graph
        /// </summary>
        public Dictionary<long, HashSet<Interaction>> ArcDictionary = new Dictionary<long, HashSet<Interaction>>();
        /// <summary>
        /// Store only one are between a pair of node
        /// </summary>
        //public Dictionary<long, Interaction> EdgeDictionary = new Dictionary<long, Interaction>();

        /// <summary>
        /// Return list of directed arcs that are groupped by value of ArcKey function between two nodes (unique for an arc A -> B)
        /// </summary>
        public Dictionary<long, Interaction> CreateDirectedArcDictionary()
        {
           
            return (from e in Arcs
                    where e.Direction == Interaction.DirectionType.directed
                    select e).ToDictionary(p => BasicNetwork.ArcKey(p)) ;

           
        }
       
        /// <summary>
        /// Return list of undirected arcs that are groupped by value of EdgKey function between two nodes (with the same value for two arcs: A -> B and A <- B)
        /// </summary>
        public Dictionary<long, Interaction> CreateUndirectedArcDictionary()
        {

            return (from e in Arcs
                    where e.Direction == Interaction.DirectionType.undirected
                    select e).ToDictionary(p => BasicNetwork.EdgeKey(p));


        }
       
        /// <summary>
        /// Edges are pairs of nodes having at least 01 interaction between them (direction or undirection is not different from each other)
        /// Return a list of 1-arbitrary interaction between two nodes
        /// </summary>
        public IEnumerable<Interaction> Edges
        {
            get
            {
                //return EdgeDictionary.Values;
                var groups = (from e in Arcs
                              group e by BasicNetwork.EdgeKey(e) into g
                              select g.ElementAt(0));
                foreach (var e in groups)
                    yield return e;
            }
        }
        /// <summary>
        /// Key of an interaction to add into the dictionary of arcs
        /// </summary>
        /// <param name="interaction">The arc to get its key</param>
        /// <returns></returns>
        public static long ArcKey(Interaction interaction)
        {
            return ArcKey(interaction.startNode.id, interaction.endNode.id);
        }
        
        /// <summary>
        /// Key of an interaction, which is represented from start node to end node, to add into the dictionary of arcs
        /// </summary>
        /// <param name="startNode">The start node of the arc</param>
        /// <param name="endNode">The end node of the arc</param>
        /// <returns></returns>
        public static long ArcKey(int startNode, int endNode)
        {
            //return startNode + ">" + endNode;
            return Mathutil.NumericMath.HashTwoNumber(startNode, endNode);
        }
        /// <summary>
        /// Return an unique string from two given strings, regarless order
        /// </summary>
        /// <param name="A">The first</param>
        /// <param name="B">The second</param>
        /// <returns></returns>
        public static long EdgeKey(int A, int B)
        {
            if (A> B)
                return Mathutil.NumericMath.HashTwoNumber(A, B);
            else
                return Mathutil.NumericMath.HashTwoNumber(B, A);
        }
        public static long EdgeKey(Interaction interaction)
        {
            return EdgeKey(interaction.startNode.id, interaction.endNode.id);
        }

        protected void AddArcToDictionary(Interaction arc)
        {
            long KeyArc = ArcKey(arc);
            if (!ArcDictionary.ContainsKey(KeyArc))
                ArcDictionary[KeyArc] = new HashSet<Interaction>();
            ArcDictionary[KeyArc].Add(arc);
            
            //Add the arc to the edge dictionary
            //long KeyEdge = BasicNetwork.EdgeKey(arc);
            //if (!EdgeDictionary.ContainsKey(KeyEdge))
            //    EdgeDictionary[KeyEdge] = arc;

           

        }
        protected void RemoveArcFromDictionary(Interaction arc)
        {
            long KeyArc = ArcKey(arc);
            if (ArcDictionary.ContainsKey(KeyArc))
            {
                ArcDictionary[KeyArc].Remove(arc);
                if (ArcDictionary[KeyArc].Count == 0)
                {
                    ArcDictionary.Remove(KeyArc);

                    //Remove the arc to the edge dictionary if there is no arc any more
                    //EdgeDictionary.Remove(BasicNetwork.EdgeKey(arc));
                    //Debug.WriteLine(string.Format("Removing edge {0} --{1} {2}", arc.startNode.name, arc.Type == InteractionType.NEGATIVE ? "|" : ">", arc.endNode.name));
                }
            }
            // Remove directed/undirected arcs from the direction dictionaries
            //if (arc.Direction == Interaction.DirectionType.directed)
            //{

            //    DirectedArcDictionary[KeyArc].Remove(arc);
            //}
            //else// (arc.Direction == Interaction.DirectionType.undirected)
            //{
            //    UndirectedArcDictionary[BasicNetwork.EdgeKey(arc)].Remove(arc);
            //}
        }
        /// <summary>
        /// Check whether there is any arc between a node pair
        /// </summary>
        /// <param name="A">The first node</param>
        /// <param name="B">The second node</param>
        /// <returns>True => at least 1 arce between the pairs, otherwise false</returns>
        protected bool hasAnyArcBetween(Node A, Node B)
        {
            long KeyAtoB = ArcKey(A.id, B.id);
            long KeyBtoA = ArcKey(B.id, A.id);
            if (ArcDictionary.ContainsKey(KeyAtoB) || ArcDictionary.ContainsKey(KeyBtoA))
                return true;
            return false;
        }
        /// <summary>
        /// Get arcs between 2 nodes
        /// </summary>
        /// <param name="A">The first node</param>
        /// <param name="B">The second node</param>
        /// <param name="linkType">link type for filter</param>
        /// <returns>The list of arcs from _arcs to connect between A and B</returns>
        public IEnumerable<Interaction> GetArcsBetween2Node(Node A, Node B, int linkType)
        {
            //return (from p in _arcs
            //        where p.Type == linkType &&
            //            ((p.startNode.name == A.name && p.endNode.name == B.name) || (p.endNode.name == A.name && p.startNode.name == B.name))
            //        select p);
            return from p in GetArcsBetween2Node(A, B) where p.Type == linkType select p;
        }
        /// <summary>
        /// Get arcs between 2 nodes
        /// </summary>
        /// <param name="A">The first node</param>
        /// <param name="B">The second node</param>
        /// <returns>The list of arcs from _arcs to connect between A and B</returns>
        public IEnumerable<Interaction> GetArcsBetween2Node(Node A, Node B)
        {
            long KeyAtoB = BasicNetwork.ArcKey(A.id, B.id);
            HashSet<Interaction> AtoBarcs = new HashSet<Interaction>();
            if (ArcDictionary.ContainsKey(KeyAtoB))
            {
                AtoBarcs = ArcDictionary[KeyAtoB];
            }
            long KeyBtoA = BasicNetwork.ArcKey(B.id, A.id);
            HashSet<Interaction> BtoAarcs = new HashSet<Interaction>();
            if (ArcDictionary.ContainsKey(KeyBtoA))
            {
                BtoAarcs = ArcDictionary[KeyBtoA];
            }
            
            foreach (var p in AtoBarcs)
                yield return p;
            foreach (var p in BtoAarcs)
                yield return p;

        }

        public IEnumerable<Interaction> GetArcsBetween2Node(Node A, Node B, Interaction.DirectionType linkType)
        {
            long KeyAtoB = BasicNetwork.ArcKey(A.id, B.id);
            HashSet<Interaction> AtoBarcs = new HashSet<Interaction>();
            if (ArcDictionary.ContainsKey(KeyAtoB))
            {
                AtoBarcs = ArcDictionary[KeyAtoB];
            }
            long KeyBtoA = BasicNetwork.ArcKey(B.id, A.id);
            HashSet<Interaction> BtoAarcs = new HashSet<Interaction>();
            if (ArcDictionary.ContainsKey(KeyBtoA))
            {
                BtoAarcs = ArcDictionary[KeyBtoA];
            }

            foreach (var p in AtoBarcs)
                if (p.Direction == linkType)
                yield return p;
            foreach (var p in BtoAarcs)
                if (p.Direction == linkType)
                yield return p;

        }

        /// <summary>
        /// Get arcs that start from a node and end at another node
        /// </summary>
        /// <param name="start">The start node</param>
        /// <param name="end">The end node</param>
        /// <returns>a list of the arcs that own the start node and the end node </returns>
        public IEnumerable<Interaction> GetArcsFromStartToEnd(Node start, Node end)
        {
            long KeyAtoB = BasicNetwork.ArcKey(start.id, end.id);
            HashSet<Interaction> AtoBarcs = new HashSet<Interaction>();
            if (ArcDictionary.ContainsKey(KeyAtoB))
            {
                AtoBarcs = ArcDictionary[KeyAtoB];
            }
           
            foreach (var p in AtoBarcs)
                yield return p;
        }

        public IEnumerable<Interaction> GetArcsFromStartToEnd(Node start, Node end, Interaction.DirectionType linkType)
        {
            long KeyAtoB = BasicNetwork.ArcKey(start.id, end.id);
            HashSet<Interaction> AtoBarcs = new HashSet<Interaction>();
            if (ArcDictionary.ContainsKey(KeyAtoB))
            {
                AtoBarcs = ArcDictionary[KeyAtoB];
            }

            foreach (var p in AtoBarcs)
                if (p.Direction == linkType)
                yield return p;
        }

        public IEnumerable<Interaction> EdgeWithMultipleOppositeArcs
        {
            get
            {
                var multiArcs = from p in this.Arcs where p.endNode.hasLinkTo(p.startNode) select p;
                return multiArcs.Distinct();
               
            }
        }
        #endregion

        
       
        //public static Random random = new Random((int)DateTime.Now.Ticks);
        /// <summary>
        /// Get average in-degree of the nodes in the network
        /// </summary>
        public double AverageInDeg
        {
            get
            {
                return (from p in this.Nodes select p).Average(t => t.InDegree);
            }
        }
        /// <summary>
        /// Get maximum degree of the nodes in the network
        /// </summary>
        public int MaxInDeg
        {
            get
            {
              
                return (from p in this.Nodes select p).Max(t => t.InDegree);
            }
        }
        /// <summary>
        /// Mamimum in-degree and undirected degree
        /// </summary>
        public int MaxInDegMixing
        {
            get
            {

                return (from p in this.Nodes select p).Max(t => t.InDegree+t.UndirectedDegree);
            }
        }
        /// <summary>
        /// Mamimum out-degree and undirected degree
        /// </summary>
        public int MaxOutDegMixing
        {
            get
            {

                return (from p in this.Nodes select p).Max(t => t.OutDegree + t.UndirectedDegree);
            }
        }
        public float EdgeDensity
        {
            get
            {
                return (float)(this.EdgesWithoutSelfLoops.Count() * 2) / (this.Nodes.Count() * this.Nodes.Count() - this.Nodes.Count());
            }
        }
        public float ArcDensity
        {
            get
            {
                return (float)this.ArcsWithoutSelfLoops.Count() / (this.Nodes.Count() * this.Nodes.Count() - this.Nodes.Count());
            }
        }
        public IEnumerable<Interaction> EdgesWithoutSelfLoops
        {
            
            get
            {
                foreach (var e in Edges)
                {
                    if(e.endNode.id!=e.startNode.id)
                        yield return e;
                }
            }
        }
        public IEnumerable<Interaction> ArcsWithoutSelfLoops
        {
            get
            {
                foreach (var e in this.Arcs)
                {
                    if (e.endNode != e.startNode)
                        yield return e;
                }
               
            }
        }
        
        
        /// <summary>
        /// Set of arcs or directed edges of the network: handle on the network, adding or removing links, by this property
        /// </summary>
        public IEnumerable<Interaction> Arcs
        {
            get
            {
                return _arcs;
            }
        }
        /// <summary>
        /// Shuffle the list order of arc randomly
        /// </summary>
        
        public IEnumerable<Interaction> ArcsType(int linkType)
        {
            
            return (from p in _arcs where p.Type==linkType select p);
            
        }
        
        
        /// <summary>
        /// Get self-loop nodes and their loop arcs
        /// </summary>
        /// <returns>A list of self-loop nodes with their self-loop arcs</returns>
        public Dictionary<Node, HashSet<Interaction>> GetSelfLoopNode()
        {
            Dictionary<Node, HashSet<Interaction>> selfLoopNode = new Dictionary<Node, HashSet<Interaction>>();
            foreach (Node n in Nodes)
            {
                if (this.hasEdge(n, n))// Exists a self-loop edge
                {
                    if (!selfLoopNode.ContainsKey(n))
                        selfLoopNode[n] = new HashSet<Interaction>();

                    foreach (Interaction i in ArcDictionary[BasicNetwork.ArcKey(n.id, n.id)])//add node's self-loop arcs to the list
                        selfLoopNode[n].Add(i);
                }

            }
            return selfLoopNode;
        }
        /// <summary>
        /// If the network is a connected or unconnected network?
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return BasicNetwork.BreadthFirstTraversal(this.Nodes.ElementAt(0)).Count() == this.Nodes.Count();   
            }
        }
        public List<IEnumerable<Node>> ConnectedComponents
        {
            get
            {
                Node StartNode=this.Nodes.ElementAt(0);
                List<IEnumerable<Node>> Components=new List<IEnumerable<Node>>();
                IEnumerable<Node> theRemainNodes = this.Nodes;
                while(StartNode!=null)
                {
                    var TraversalNodes = BasicNetwork.BreadthFirstTraversal(StartNode);
                    Components.Add(TraversalNodes);

                    var UnTraversalNodes = from e in theRemainNodes
                                        join f in TraversalNodes on e equals f into g
                                        from sub in g.DefaultIfEmpty()
                                        where sub==null
                                        select e;
                    if (UnTraversalNodes.Count() > 0)
                    {
                        StartNode = UnTraversalNodes.ElementAt(0);
                        theRemainNodes = UnTraversalNodes;
                    }
                    else
                        StartNode = null;
                }
                return Components;
            }
        }
        public string Name
        {
            get { return name;      }
            set { name = value;     }
 
        }
        /// <summary>
        /// Check if a numbemeric sequence is an degree sequence (graphical sequence)
        /// http://en.wikipedia.org/wiki/Degree_(graph_theory)
        /// </summary>
        /// <param name="degseq"></param>
        /// <returns></returns>
        public static bool IsGraphicalDegreeSequence(IEnumerable<int> degseq)
        {
            //sort to make it to be non-increasing sequence
            degseq = from e in degseq orderby e descending select e;
            for (int k = 0; k < degseq.Count(); k++)
            {
                int sdi=0;
                for (int i = 0; i < k; i++)
                    sdi += degseq.ElementAt(i);

                int mindik = 0;
                for (int i = k; i < degseq.Count(); i++)
                    mindik += Math.Min(k, degseq.ElementAt(i));
                if (!(sdi <= k * (k - 1) + mindik))
                    return false;

            }
            return true;
        }
        #region Node

        public IEnumerable<Interaction> CloneInteraction()
        {
            BasicNetwork NetforNewNode = this.CreateObject() as BasicNetwork;
            if (this.Arcs == null) return null;
            HashSet<Interaction> pNews = new HashSet<Interaction>();
            
            foreach (Interaction i in this.Arcs)
            {
            
                Interaction newInter = new Interaction(NetforNewNode.AddNode(i.startNode.name), NetforNewNode.AddNode(i.endNode.name), i.Type, i.Name, i.weight, i.Direction);
                newInter.density = i.density;
                pNews.Add(newInter);

            }
            

            return pNews;
        }
        /// <summary>
        /// Create a copy of interaction with new node and new interaction having the same parameters with the olds
        /// </summary>
        /// <param name="NetSample">The network as the sample to create nodes</param>
        /// <param name="pInteractions">The interactions to clone</param>
        /// <returns>The clone interaction</returns>
        public static IEnumerable<Interaction> CloneInteraction(BasicNetwork NetSample, IEnumerable<Interaction> pInteractions)
        {
            BasicNetwork NetforNewNode = NetSample.CreateObject() as BasicNetwork;
            
            HashSet<Interaction> pNews = new HashSet<Interaction>();

            foreach (Interaction i in pInteractions)
            {

                Interaction newInter = new Interaction(NetforNewNode.AddNode(i.startNode.name), NetforNewNode.AddNode(i.endNode.name), i.Type, i.Name, i.weight, i.Direction);
                newInter.density = i.density;
                pNews.Add(newInter);

            }
            return pNews;
        }
        /// <summary>
        /// Create a clone network from a set of interactions
        /// </summary>
        /// <param name="NetSample">The network as the sample to create nodes</param>
        /// <param name="pInteractions">The interactions to clone</param>
        /// <returns>The new network with clone interactions</returns>
        public static BasicNetwork CloneInteractionToNetwork(BasicNetwork NetSample, IEnumerable<Interaction> pInteractions)
        {
            BasicNetwork NewNet = NetSample.CreateObject() as BasicNetwork;

            HashSet<Interaction> pArcs = new HashSet<Interaction>();
            
            foreach (Interaction i in pInteractions)
            {
               
                Interaction newInter = new Interaction(NewNet.AddNode(i.startNode.name), NewNet.AddNode(i.endNode.name), i.Type, i.Name, i.weight, i.Direction);
                newInter.density = i.density;
                pArcs.Add(newInter);

            }

            NewNet.AddArc(pArcs.ToArray());
            return NewNet;
        }

        /// <summary>
        /// Create the zero-based index of nodes in the graph
        /// </summary>
        /// <returns></returns>
        public void CreateNodeIndex(out Dictionary<string, int> nodeToIndex, out Dictionary<int, string> indexToNode)
        {

            nodeToIndex = new Dictionary<string, int>();
            indexToNode = new Dictionary<int, string>();
            for (int i = 0; i < Nodes.Count(); i++)
            {
                nodeToIndex.Add(Nodes.ElementAt(i).name, i);
                indexToNode.Add(i, Nodes.ElementAt(i).name);
            }
            
            
        }
        /// <summary>
        /// Create the zero-based index of nodes in the graph
        /// </summary>
        /// <param name="nodeGroup"></param>
        /// <returns></returns>
        public void CreateNodeIndex(IEnumerable<Node> nodeGroup, out Dictionary<string, int> nodeToIndex, out Dictionary<int, string> indexToNode)
        {

            nodeToIndex = new Dictionary<string, int>();
            indexToNode = new Dictionary<int, string>();
            for (int i = 0; i < nodeGroup.Count(); i++)
            {
                nodeToIndex.Add(nodeGroup.ElementAt(i).name, i);
                indexToNode.Add(i, nodeGroup.ElementAt(i).name);
            }

        }
         /// <summary>
        /// Get a node by its zero-based index
        /// </summary>
        /// <param name="zeroBasedIndex"></param>
        /// <returns></returns>
        public Node GetNodeFromIndex(int zeroBasedIndex)
        {
            
            return Nodes.ElementAt(zeroBasedIndex);

        }
        /// <summary>
        /// Get an zero-based index of a node (this method is slow to get the index, please use CreateZeroBasedIndex() function instead)
        /// </summary>
        /// <param name="node">The node to get the index</param>
        /// <returns>The index, return -1 if being an invalid node</returns>
        public int IndexOfNode(Node node)
        {
            
            for (int i = 0; i < Nodes.Count(); i++)
                if (Nodes.ElementAt(i).id == node.id)
                    return i;

            return -1;
        }

        public double[,] CreateAdjacentMatrix(out Dictionary<string, int> nodeToIndex, out Dictionary<int, string> indexToNode)
        {
            IEnumerable<Interaction> links = Arcs;

            CreateNodeIndex(out nodeToIndex, out indexToNode);
            double[,] Ma = new double[nodeToIndex.Count(), nodeToIndex.Count()];
            
            foreach (Interaction inter in links)
            {
                //Ma[nodeToIndex[inter.startNode.name], nodeToIndex[inter.endNode.name]] = (WeightAs1 ? 1 : inter.weight);
                FillInteractionToAdjacentList(Ma, inter, nodeToIndex[inter.startNode.name], nodeToIndex[inter.endNode.name],  inter.weight);

                

            }
            return Ma;
        }
        
       
        private void FillInteractionToAdjacentList(double[,] Ma, Interaction inter, int row, int col, double value)
        {
            Ma[row, col] += value;
            if (inter.Direction == Interaction.DirectionType.undirected)
                Ma[col, row] += value;
        }
        private void FillInteractionToAdjacentList(Dictionary<int, Dictionary<int, double>> Ma, Interaction inter, int row, int col, double value)
        {
            if (!Ma.ContainsKey(row))
                Ma[row] = new Dictionary<int, double>();

            Ma[row][col] = !Ma[row].ContainsKey(col) ? value : Ma[row][col]+value;
            if (inter.Direction == Interaction.DirectionType.undirected)
            {
                if (!Ma.ContainsKey(col))
                    Ma[col] = new Dictionary<int, double>();

                Ma[col][row] = !Ma[col].ContainsKey(row) ? value : Ma[col][row]+value;
            }
        }
        /// <summary>
        /// Create a adjacent matrix for big graph
        /// </summary>
        /// <param name="nodeToIndex">Node index mapping between node name and zero-based index</param>
        /// <param name="indexToNode">Node index mapping between node name and zero-based index</param>
        /// <param name="isForwardLink">true if link's direction is considered from startNode to endNode; false if link's direction is considered from endNode to startNode</param>
        /// <param name="WeightAs1">if true (default value), the weight =1; else the real weight of interactions is used</param>
        /// <returns></returns>
        public Dictionary<int, Dictionary<int, double>> CreateAdjacentList(out Dictionary<string, int> nodeToIndex, out Dictionary<int, string> indexToNode, bool isForwardLink = true)
        {
            IEnumerable<Interaction> links = Arcs;

            CreateNodeIndex(out nodeToIndex, out indexToNode);
            Dictionary<int, Dictionary<int, double>> Ma = new Dictionary<int, Dictionary<int, double>>();

          
            if (isForwardLink)
            {
                foreach (Interaction inter in links)
                    FillInteractionToAdjacentList(Ma, inter, nodeToIndex[inter.startNode.name], nodeToIndex[inter.endNode.name], inter.weight);
                    
            }
            else
            {
                foreach (Interaction inter in links)
                        FillInteractionToAdjacentList(Ma, inter, nodeToIndex[inter.endNode.name], nodeToIndex[inter.startNode.name], inter.weight);
            }

            return Ma;
        }
        
        /// <summary>
        /// Extract the interactions that two end-nodes in the a group of nodes
        /// </summary>
        /// <param name="nodeGroup">The group of nodes that contains nodes of links</param>
        /// <returns></returns>
        public HashSet<Interaction> InteractionFromNodeGroup(IEnumerable<Node> nodeGroup)
        {
            HashSet<Interaction> subInteraction = new HashSet<Interaction>();
            IEnumerable<Interaction> links = this.Arcs;
            foreach (Interaction e in links)
            {
                if (nodeGroup.Contains(e.startNode) && nodeGroup.Contains(e.endNode))
                    subInteraction.Add(e);
            }
            return subInteraction;

        }
        public Dictionary<int, Dictionary<int, double>> CreateAdjacentList(IEnumerable<Node> nodeGroup, out Dictionary<string, int> nodeIndex, out Dictionary<int, string> indexNode)
        {
            HashSet<Interaction> links = InteractionFromNodeGroup(nodeGroup);

            CreateNodeIndex(nodeGroup, out nodeIndex, out indexNode);
            Dictionary<int, Dictionary<int, double>> Ma = new Dictionary<int, Dictionary<int, double>>();
            
            foreach (Interaction inter in links)
            {
                FillInteractionToAdjacentList(Ma, inter, nodeIndex[inter.startNode.name], nodeIndex[inter.endNode.name], inter.weight);
               

            }
            return Ma;
        }
       
       
        /// <summary>
        /// Get the shortest path from start to target vertices (this run slowly for loops due to create adjcent list in many times)
        /// </summary>
        /// <param name="from">The start vertex</param>
        /// <param name="to">The target vertex</param>
        /// <returns>The list of vertex name as the shortest path</returns>
        public IEnumerable<string> ShortestPath(string from, string to)
        {
            Dijkstra dijk = new Dijkstra();
            Dictionary<string, int> nodeToIndex = null;
            Dictionary<int, string> indexToNode = null;
            Dictionary<int, Dictionary<int, double>> adjacentList = this.CreateAdjacentList(out nodeToIndex, out indexToNode);
            int[] path = dijk.FindShortestPathToTarget(adjacentList, nodeToIndex[from], nodeToIndex[to]);
            if (path == null)
                return null;
            return (from p in nodeToIndex where path.Contains(p.Value) select p.Key);

        }
        /// <summary>
        /// Get the vertices in a node group closest to a node
        /// </summary>
        /// <param name="from">The node the distance calculated from</param>
        /// <param name="toGroup">The group of nodes</param>
        /// <returns>The list of the nodes in toGroup closest to the from </returns>
        public string[] ClosestVertices(Node from, IEnumerable<Node> toGroup)
        {
            Dijkstra dijk = new Dijkstra();
            Dictionary<string, int> nodeIndex = null;
            Dictionary<int, string> indexNode = null;
            Dictionary<int, Dictionary<int, double>> adjacentList = this.CreateAdjacentList(toGroup.Union(this.NewNodeArray(from)), out nodeIndex, out indexNode);
            dijk.FindShortestPathAndDistance(adjacentList, nodeIndex[from.name]);
            int[] index= dijk.GetClosestVertex();
            if (index == null)
                return null;

            
            string[] vertex = new string[index.Length];
            for (int i = 0; i < vertex.Length; i++)
                vertex[i] = indexNode[index[i]];
            return vertex;
        }
        /// <summary>
        /// Get the shortest path from start to target vertices (this is use for loops due to recuding creation of adjcent list in many times)
        /// </summary>
        /// <param name="nodeIndex">Node index of the network</param>
        /// <param name="adjacentList">The adjacent list  (return from function CreateAdjacentList)</param>
        /// <param name="from">The start vertex</param>
        /// <param name="to">The target vertex</param>
        /// <returns>The list of vertex name as the shortest path</returns>
        public static IEnumerable<string> ShortestPath(Dictionary<string, int> nodeIndex, Dictionary<int, Dictionary<int, double>> adjacentList, string from, string to)
        {
            Dijkstra dijk = new Dijkstra();
            int[] path = dijk.FindShortestPathToTarget(adjacentList, nodeIndex[from], nodeIndex[to]);
            if (path == null)
                return null;
            return (from p in nodeIndex where path.Contains(p.Value) select p.Key);

        }
        
        public IEnumerable<Node> Nodes
        {
            get
            {
                return _nodes;
            }
        }
        ///// <summary>
        ///// Remove given nodes from the node list managed by this network.
        ///// Notice: the links of the given nodes are still kept intact, not destroyed
        ///// </summary>
        ///// <param name="detachedNodes">The nodes to be removed from the node list of the network</param>
        ///// <returns>true if all nodes in the list are detached successfully</returns>
        //public bool DetachNode(IEnumerable<Node> detachedNodes)
        //{
        //    bool allRemoved = true;
        //    foreach (var n in detachedNodes)
        //        allRemoved = allRemoved && _nodes.Remove(n);
        //    return allRemoved;
        //}
        ///// <summary>
        ///// Add given nodes to the node list managed by the network
        ///// Notice: the links of the given nodes are still kept intact
        ///// </summary>
        ///// <param name="attachedNodes">The nodes to be added to the node list of the network</param>
        //public void AttachNode(IEnumerable<Node> attachedNodes)
        //{
            
        //    foreach (var n in attachedNodes)
        //        _nodes.Add(n);
            
        //}
        //int idcounter = 0;
        protected void addNode2Network(Node n)
        {
            _nodes.Add(n);
            nodeNameDictionary.Add(n.name, n);

            //nodeIdDictionary.Add(n.id = idcounter++, n);
            nodeIdDictionary.Add(n.id, n);
        }
        protected void RemoveNode(Node node)
        {
            this._nodes.Remove(node);
            nodeNameDictionary.Remove(node.name);
            nodeIdDictionary.Remove(node.id);
        }
        /// <summary>
        /// Add node to network, not add its arcs to the network
        /// </summary>
        /// <param name="nodes"></param>
        //public void AddNode(Node node)
        virtual public void AddNode(params Node[] nodes)
        {
            
            foreach (Node n in nodes)
            {
                if (!nodeNameDictionary.ContainsKey(n.name))
                {
                    addNode2Network(n);
                }else
                    throw new Exception(string.Format("Node \"{0}\" has existent already!", n.name));
            }
        }
        /// <summary>
        /// Add the new node or select old node to the network
        /// </summary>
        /// <param name="node">The node to add</param>
        /// <returns>The node added into the network</returns>
        virtual public Node AddNode(Node node)
        {
            if (!nodeNameDictionary.ContainsKey(node.name))
            {
                addNode2Network(node);
                return node;
            }
            else
            {
                //throw new Exception(string.Format("Node \"{0}\" has existent already!", node.name));
                return nodeNameDictionary[node.name];
            }
            
        }

        virtual public Node AddNode(string nodeName)
        {
            if (!nodeNameDictionary.ContainsKey(nodeName))
            {
                Node node=this.NewNode(nodeName, null);
                addNode2Network(node);
                return node;
            }
            else
            {
                return nodeNameDictionary[nodeName];
            }
        }
        
        /// <summary>
        /// Return true if the node's name is existent
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public bool hasNode(string nodeName)
        {
            return nodeNameDictionary.ContainsKey(nodeName);
        }
        
       
        #endregion
       
       
        #region function handles on arcs of network
        /// <summary>
        /// Add an arc to the network, if arc's nodes are not existent, add them to the network (edge is added automatically)
        /// Note: if the node in the interaction is not existing in the node list, the interaction will be Dispose and a new Interaction is then create for the newnode
        /// </summary>
        /// <param name="interaction">The arc to add, if one of two nodes in the interaction are not existent in the network this interaction will be disposed and new interaction will create </param>
        /// <returns>New interaction or existent interaction added into network</returns>
        public Interaction AddNodeAndArc(Interaction interaction)
        {
            //var start = from p in Nodes where p.name == interaction.startNode.name select p;
            //var end = from p in Nodes where p.name == interaction.endNode.name select p;
            
            //if (start.Count() == 0)
            Node existingStart=AddNode(interaction.startNode);
            //if (end.Count() == 0)
            Node existingEnd = AddNode(interaction.endNode);

            if ((existingStart != interaction.startNode) ||
                (existingEnd != interaction.endNode))
            {
                Interaction realInteraction = new Interaction(existingStart, existingEnd, interaction.Type, interaction.Name, interaction.weight, interaction.Direction);
                interaction.Dispose();//To remove node connecting to this interaction
                AddArc(realInteraction);
                return realInteraction;
            }
            else
            {
                AddArc(interaction);
                return interaction;
            }

        }
        public Interaction AddNodeAndArc(string StartNode, string EndNode, int Type, double weight, Interaction.DirectionType Direction, string Name = "")
        {
            Node existingStart = AddNode(StartNode);
            //if (end.Count() == 0)
            Node existingEnd = AddNode(EndNode);

           
            Interaction realInteraction = new Interaction(existingStart, existingEnd,Type,Name , weight, Direction);

            AddArc(realInteraction);
            return realInteraction;
           
        }
        /// <summary>
        /// Answer if there is any arc formed by two nodes in the network
        /// </summary>
        /// <param name="start">The start node</param>
        /// <param name="end">The target node</param>
        /// <returns></returns>
        public bool hasArc(Node start, Node end)
        {
            return ArcDictionary.ContainsKey(BasicNetwork.ArcKey(start.id, end.id));
        }
        /// <summary>
        /// Answer if there is any edge formed by two nodes in the network
        /// </summary>
        /// <param name="A">The first node</param>
        /// <param name="B">The second node</param>
        /// <returns></returns>
        public bool hasEdge(Node A, Node B)
        {
            return ArcDictionary.ContainsKey(BasicNetwork.ArcKey(A.id, B.id)) ||
                ArcDictionary.ContainsKey(BasicNetwork.ArcKey(B.id, A.id));
            
        }
        public bool hasEdge(string A, string B)
        {
            if(nodeNameDictionary.ContainsKey(A)||nodeNameDictionary.ContainsKey(B))
                return false;
            Node nA=nodeNameDictionary[A];
            Node nB=nodeNameDictionary[B];
            return hasEdge(nA, nB);
        }
        public double MixingRateOfModule(Dictionary<Node, int> pCluster)
        {
            Dictionary<int, List<Node>> clustering = Clustering.ConvertCluster(pCluster);
            int internalLink = 0;
            int externalLink = 0;
            double mixing = 0;
            //HashSet<Interaction> iLink = new HashSet<Interaction>();
            //HashSet<Interaction> oLink = new HashSet<Interaction>();
            foreach (var e in clustering)
            {
                //iLink.Clear();
                //oLink.Clear();
                //this.SelectInOutGroupInteraction(e.Value, ref iLink, ref oLink);
                //Netutil.DumpInteraction(iLink.ToArray());
                //Netutil.DumpInteraction(oLink.ToArray());
                this.CountInOutGroupInteraction(e.Value, ref internalLink, ref externalLink);
                mixing += (double)externalLink / (internalLink + externalLink);

            }
            return mixing/clustering.Count;
        }
        /// <summary>
        /// Get arcs so that their endpoints do not touch given nodes
        /// </summary>
        /// <param name="pNode">The set of nodes</param>
        /// <returns></returns>
        public IEnumerable<Interaction> GetArcNonAdjNode(IEnumerable<Node> pNode)
        {
            var NoStartNode= from p in this.Arcs
                             join q in pNode on p.startNode.id equals q.id into groupJoin
                   from subNode in groupJoin.DefaultIfEmpty()
                   where subNode == null
                   select p;
            var NoEndNode = from p in NoStartNode
                            join q in pNode on p.endNode.id equals q.id into groupJoin
                              from subNode in groupJoin.DefaultIfEmpty()
                              where subNode == null
                              select p;
            return NoEndNode;
        }
        /// <summary>
        /// Remove nodes and their arcs from the network
        /// </summary>
        /// <param name="nodes">The list of nodes</param>
        public void RemoveNodeAndArc(params Node[] nodes)
        {
            foreach (Node n in nodes)
            {
                RemoveNode(n);
                this.RemoveArc(n.Arcs.ToArray());
            }
        }
        
        public void RemoveNodeAndArc(params string[] nodes)
        {
            foreach (string node in nodes)
            {
                RemoveNodeAndArc(nodeNameDictionary[node]);
            }
        }
        /// <summary>
        /// Get node object from node's name
        /// </summary>
        /// <param name="nodeName">The name of node</param>
        /// <returns>Node object</returns>
        public Node GetNodeFromName(string nodeName)
        {
            if (nodeNameDictionary.ContainsKey(nodeName))
                return nodeNameDictionary[nodeName];
            else
                return null;
        }
        /// <summary>
        /// Add an arc into the network (edge is added automatically) with two nodes added already into the network
        /// This function doese Not Add nodes to the network 
        /// </summary>
        /// <param name="interactions">The arc for adding</param>
        //public void AddArc(Interaction arc)
        public void AddArc(params Interaction[] interactions)
        {
            foreach(Interaction arc in interactions)
            {
                if (_arcs.Contains(arc))
                    return;

                _arcs.Add(arc);
                //Debug.WriteLine(string.Format("Adding arc {0} --{1} {2}", arc.startNode.name, arc.Type == InteractionType.NEGATIVE ? "|" : ">", arc.endNode.name));
                AddArcToDictionary(arc);

                arc.startNode.AddArc(true, arc); //Add the arc into Arcs of the node as well
                arc.endNode.AddArc(false, arc);

                //AddEdge(arc);
            }
        }
        //protected void AddEdge(Interaction arc)
        //{
        //    //Add the interaction as an edge as well
        //    var existentEdge = from p in _edges
        //                       where (p.startNode.name == arc.startNode.name && p.endNode.name == arc.endNode.name) ||
        //                           (p.startNode.name == arc.endNode.name && p.endNode.name == arc.startNode.name)
        //                       select p;
        //    if (existentEdge.Count() == 0)
        //        _edges.Add(arc);
        //}
        /// <summary>
        /// Remove an arc from the network (there are 02 removal function: RemoveArc and RemoveEdge
        /// </summary>
        /// <param name="interaction">The arc for removal</param>
        public void RemoveArc(params Interaction[] interactions)
        {
            foreach (Interaction arc in interactions)
            {
                if (!_arcs.Contains(arc)) continue;

                //if (arc.endNode == arc.startNode)
                //{
                //    int x = 0;
                //    x++;
                //}
                _arcs.Remove(arc);
                //Debug.WriteLine(string.Format("Removing arc {0} --{1} {2}", arc.startNode.name, arc.Type == InteractionType.NEGATIVE ? "|" : ">", arc.endNode.name));
                RemoveArcFromDictionary(arc);
                
                arc.startNode.RemoveArc(arc);//Remove the arc from Arcs of the node as well
                arc.endNode.RemoveArc(arc);


                //check if there is any arc between the pair of node
                //var existentLink = from p in _arcs
                //                   where (p.startNode.name == arc.startNode.name && p.endNode.name == arc.endNode.name) ||
                //                       (p.startNode.name == arc.endNode.name && p.endNode.name == arc.startNode.name)
                //                   select p;

                //if there is no arc between two nodes, the edge is removed as well
                //if (existentLink.Count() == 0)
                //    _removeEdge(arc.startNode, arc.endNode);
                //if (!hasAnyArcBetween(arc.startNode, arc.endNode))
                //    _removeEdge(arc.startNode, arc.endNode);
            }
        }

        public void RemoveArc(bool includedIsolateNodes, int linkType, params Interaction[] interactions)
        {

            foreach (Interaction arc in interactions)
            {
                if (!this.ArcsType(linkType).Contains(arc)) continue;

                _arcs.Remove(arc);
                arc.startNode.RemoveArc(arc);//Remove the arc from Arcs of the node as well
                arc.endNode.RemoveArc(arc);
                if (includedIsolateNodes)
                {
                    if (arc.startNode.Arcs.Count() == 0)
                        this.RemoveNode(arc.startNode);
                    if (arc.endNode.Arcs.Count() == 0)
                        this.RemoveNode(arc.endNode);
                }
                //check if there is any arc between the pair of node
                //var existentLink = from p in _arcs
                //                   where (p.startNode.name == arc.startNode.name && p.endNode.name == arc.endNode.name) ||
                //                       (p.startNode.name == arc.endNode.name && p.endNode.name == arc.startNode.name)
                //                   select p;

                ////if there is no arc between two nodes, the edge is removed as well
                //if (existentLink.Count() == 0)
                //    _removeEdge(arc.startNode, arc.endNode);

                //if (!hasAnyArcBetween(arc.startNode, arc.endNode))
                //    _removeEdge(arc.startNode, arc.endNode);
            }

        }
        private class BipartitleVisitor<T> : OrderedVisitor<T>
        {
            public Dictionary<Node, bool> ColorNode = new Dictionary<Node, bool>();
            public BipartitleVisitor(OrderType orderType)
            :base(orderType)
            {}
            public override bool VisitPreOrder(T parent, T obj)
            {

                Node parentNode = parent as Node;
                Node node = obj as Node;
                if (parentNode == null)
                    ColorNode.Add(node, true);
                else
                    ColorNode.Add(node, !ColorNode[parentNode]);
                return true;
               
            }
        }
        /// <summary>
        /// Select pairs of vertices (L, R) in the graph so that each pair forms a bipartite graph. using depth-first search. See http://en.wikipedia.org/wiki/Bipartite_graph
        /// Note: Maybe exist multiple bipartite graphs after select
        /// </summary>
        /// <returns>Pairs of bipartite graphs </returns>
        public List<Pair<IEnumerable<Node>, IEnumerable<Node>>> SelectUndirectedBipartiteGraph()
        {
            List<Pair<IEnumerable<Node>, IEnumerable<Node>>> BipartiteNodes = new List<Pair<IEnumerable<Node>, IEnumerable<Node>>>();
            
            IEnumerable<Node> remainedNode = this.Nodes;
            Node startVisit = null;
            while (remainedNode.Count() > 0)
            {
                startVisit = remainedNode.ElementAt(0);
                BipartitleVisitor<Node> visitedNode = new BipartitleVisitor<Node>(OrderedVisitor<Node>.OrderType.PreOrder);
                this.DepthFirstTraversal(visitedNode, startVisit);
                var edgeWithColor = from p in
                                        (from edge in this.Edges join startc in visitedNode.ColorNode on edge.startNode equals startc.Key select new { edge, startColor = startc.Value })
                                    join endc in visitedNode.ColorNode on p.edge.endNode equals endc.Key
                                    select new { startColor = p.startColor, p.edge, endColor = endc.Value };

                bool isNotBipartitle = (from p in edgeWithColor where p.endColor == p.startColor select p).Count() > 0;
                if (isNotBipartitle)
                {
                    //foreach (var e in edgeWithColor)
                    //{
                    //    Debug.WriteLine(string.Format("{0}\t({1} - {2})\t{3}", e.startColor, e.edge.startNode.name, e.edge.endNode.name, e.endColor));
                    //}
                    break;
                }
                //foreach (var e in edgeWithColor)
                //{
                //    Debug.WriteLine(string.Format("{0}\t({1} - {2})\t{3}", e.startColor, e.edge.startNode.name, e.edge.endNode.name, e.endColor));
                //}
                IEnumerable<Node> L = from p in visitedNode.ColorNode where p.Value == true select p.Key;
                IEnumerable<Node> R = from p in visitedNode.ColorNode where p.Value == false select p.Key;

                BipartiteNodes.Add(new Pair<IEnumerable<Node>, IEnumerable<Node>>(L, R));
                remainedNode = from p in remainedNode where !(L.Contains(p) || R.Contains(p)) select p;
            }
            return BipartiteNodes;

        }
        
        /// <summary>
        /// Helper function to remove all arcs connecting between two nodes
        /// </summary>
        /// <param name="nodeA">The first node name</param>
        /// <param name="nodeB">The second node name</param>
        private void _removeAllArc(Node nodeA, Node nodeB)
        {
            var existentLink = nodeA.ArcsBetween(nodeB);

            while (existentLink.Count() > 0)
            {
                existentLink.ElementAt(0).endNode.RemoveArc(existentLink.ElementAt(0));
                existentLink.ElementAt(0).startNode.RemoveArc(existentLink.ElementAt(0));
                RemoveArc(existentLink.ElementAt(0));
            }
        }

     
        #endregion
        #region Functions handle on the edges of the network
        
        /// <summary>
        /// Remove the edge and links between 2 nodes, node order is not important to removal
        /// </summary>
        /// <param name="nodeA">Fist node name</param>
        /// <param name="nodeB">Second node name</param>
        public void RemoveEdge(Node nodeA, Node nodeB)
        {
            // Remove all links connecting between two nodes from the network
            _removeAllArc(nodeA, nodeB);

            //Remove the edge between two nodes
            //_removeEdge(nodeA,nodeB);

        }
        #endregion
        #endregion
        
        #region Contructor & Destructor
        ~BasicNetwork()
        {
            ClearData();
        }
        private void ClearData()
        {
            _nodes.Clear();
            _arcs.Clear();
            //_edges.Clear();
            nodeNameDictionary.Clear();
            nodeIdDictionary.Clear();
            ArcDictionary.Clear();
            
            //EdgeDictionary.Clear();
            //DirectedArcDictionary.Clear();
            //UndirectedArcDictionary.Clear();
            
        }
        /// <summary>
        /// Check whether the network is valid or not
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            IEnumerable<int> nodeOfInteraction = (from p in _arcs
                                                     select p.startNode.id).Union(from p in _arcs select p.endNode.id).OrderByDescending(p => p);

            var nodeOnArcs= (from p in _arcs select p.startNode).Union(from p in _arcs select p.endNode);
            
            IEnumerable<int> nodes = (from p in Nodes
                                         select p.id).OrderByDescending(p => p);
            // check weather nodes in the arcs are in the list of nodes or not?
            bool isThesame = nodes.ToArray().SequenceEqual<int>(nodeOfInteraction.ToArray());

            //Check if there is any nodes whose name is the same but is allocated on different object
            var duplicateNodes = from p in nodeOnArcs
                                 group p by p.id into ng
                                 where ng.Count() > 1
                                 select new { node = ng.Key, DuplicateNode = ng.Count() };
          
            if (this.DuplicatedArcs.Count() > 0)
                throw new Exception("Duplicate arcs !");             
            if(!this.IsConnected)
                throw new Exception("Network is unconnected !");             
            return isThesame && (duplicateNodes.Count() == 0);
        }
        #endregion
        /// <summary>
        /// Nodes that own no link
        /// </summary>
        public IEnumerable<Node> IsolateNodes
        {
            get
            {
                IEnumerable<Node> nodeOfInteraction = (from p in _arcs
                                                         select p.startNode).Union(from p in _arcs select p.endNode);
                
                //return (from p in _nodes where !nodeOfInteraction.Any(t => t == p) select p);
                //outer leftjoin
                return from p1 in Nodes join p2 in nodeOfInteraction on p1.id equals p2.id into right
                        from subset in right.DefaultIfEmpty() 
                        where subset == null
                        select p1;
                
            }
        }

        #region Helper functions

        
        #endregion


        public bool IsEmpty
        {
            get
            {
                
                return Nodes.Count() == 0;
            }
        }
        public IEnumerable<Node> InputNodes
        {
            get
            {
                return from n in this.Nodes where n.OutDegree == 0 select n;
            }
        }
        public IEnumerable<Node> OutputNodes
        {
            get
            {
                return from n in this.Nodes where n.InDegree == 0 select n;
            }
        }
        /// <summary>
        /// Retrieve a list of node objects by their name
        /// </summary>
        /// <param name="nodeName">The name for the selection</param>
        /// <returns></returns>
        public IEnumerable<Node> SelectNode(IEnumerable<string> nodeName)
        {
            //return (from node in Nodes join name in nodeName on node.name equals name select node);
            foreach (string name in nodeName)
            {
                yield return nodeNameDictionary[name];
            }
        }
        public IEnumerable<Node> SelectNode(IEnumerable<Node> nodeName)
        {
            //return (from node in Nodes join name in nodeName on node.name equals name.name select node);
            foreach (Node node in nodeName)
            {
                yield return nodeNameDictionary[node.name];
            }
        }

        /// <summary>
        /// Select internal interactions that contain node specified in the parameter
        /// </summary>
        /// <param name="pNode">The node list</param>
        /// <returns></returns>
        public IEnumerable<Interaction> SelectInternalInteraction(IEnumerable<Node> pNode)
        {
            IEnumerable<Interaction> interactions = this.Arcs;
            return from e in
                             (from e1 in interactions join end in pNode on e1.endNode.name equals end.name select e1)
                         join start in pNode on e.startNode.name equals start.name
                         select e;
        }
        /// <summary>
        /// Select external interaction that has only one end-point connects to a set of node
        /// </summary>
        /// <param name="pNode">The node list</param>
        /// <returns></returns>
        public IEnumerable<Interaction> SelectInOutGroupInteraction(IEnumerable<Node> pNode)
        {
            foreach (Node n in pNode)
            {
                foreach (Interaction it in n.Arcs)
                {
                    if (!pNode.Contains(it.GetPartnerVertex(n), new Node.NodeComparer()))
                        yield return  it;
                } 
            }
        }
       

        public void CountInOutGroupInteraction(IEnumerable<Node> pNode, ref int interLink, ref int exterLink)
        {
            interLink = 0;
            exterLink = 0;
            int selfLoop = 0;
            
            foreach (Node n in pNode)
            {
                Node realNode = this[n.name];
                foreach (Interaction it in realNode.Arcs)
                {
                    if (it.GetPartnerVertex(realNode) == realNode)
                        selfLoop++;
                    else if (pNode.Contains(it.GetPartnerVertex(realNode), new Node.NodeComparer()))
                        interLink++;
                    else
                        exterLink++;
                }
            }
            interLink = interLink / 2 + selfLoop;

        }
        public IEnumerable<Interaction> SelectInteraction(Node start, Node end)
        {
            //IEnumerable<Interaction> interactions = this.Arcs;
            //return from e in this.Arcs
            //       where (e.startNode.name == start.name && e.endNode.name == end.name)
            //       select e;
            if (!ArcDictionary.ContainsKey(BasicNetwork.ArcKey(start.id, end.id)))
                return new HashSet<Interaction>();
            return ArcDictionary[BasicNetwork.ArcKey(start.id, end.id)];
        }
       
        /// <summary>
        /// Create a copy of subnetwork from the network by simply find interactions connecting among them
        /// </summary>
        /// <param name="pNode">The list of node in the network to locate the subnetwork</param>
        /// <returns>The subnetwork</returns>
        public BasicNetwork CreateNewExtractedSubnetwork(IEnumerable<Node> pNode)
        {
            BasicNetwork newNet = this.CreateObject() as BasicNetwork;
            if (pNode == null || (pNode != null && pNode.Count() == 0))
                return newNet;

            //pNode = Netutil.CloneNode(pNode); // new nodes for new network
            newNet = BasicNetwork.CloneInteractionToNetwork(this, this.SelectInternalInteraction(pNode));
            
            
            //Find isolate node in the node list
            var isolateNodes = from p in pNode where !newNet.Nodes.Any(t => t.name == p.name) select (Node)p.Clone();
            newNet.AddNode(isolateNodes.ToArray());

            
            
            return newNet;
            
        }
        public IEnumerable<Node> driverNodes
        {
            get
            {
                BiNetwork biNet = new BiNetwork(this);
                return biNet.findDriverNodes();
            }
        }

        #region Modularity calculation
        /// <summary>
        /// Create a explicite network at cluster level
        /// </summary>
        /// <param name="Cluster">The cluster of the network</param>
        /// <param name="byArcs">List of interactions (edges or arcs) in this network: true: Arcs is used (Existing multi-links probably between each groupnode pair) ; false: Edges is used</param>
        /// <returns>The network of modules</returns>
        public BasicNetwork CreateClusterNework(Dictionary<Node, int> Cluster, bool byArcs=true)
        {
            IEnumerable<Interaction> interactions = byArcs ? this.Arcs : this.Edges;
            BasicNetwork clusterNet = this.CreateObject() as BasicNetwork;
            //Netutil.DumpInteraction(interactions.ToArray());

            var edgesIncludingModuleID = from e in
                                             (from e1 in interactions join cend in Cluster on e1.endNode.id equals cend.Key.id select new { interaction = e1, endnodeModuleID = cend.Value })
                                         join cstart in Cluster on e.interaction.startNode.id equals cstart.Key.id
                                         select new { e.interaction, e.endnodeModuleID, startnodeModuleID = cstart.Value };

           

            var inModuleEdg = (from p in edgesIncludingModuleID
                                 where p.endnodeModuleID == p.startnodeModuleID
                                 select p);

            
            var outModuleEdg = (from p in edgesIncludingModuleID
                                 where p.endnodeModuleID != p.startnodeModuleID
                                 select p);//Node have not unique ID

            

            var ClusterIDs = from p in Cluster group p by p.Value into g select g.Key;
            Dictionary<int, Node> groupNodes = new Dictionary<int, Node>();
            foreach (int clusID in ClusterIDs)
            {
                var InternalEdg = from p in inModuleEdg where p.endnodeModuleID == clusID select p.interaction;
                Node groupNode = clusterNet.NewNode(clusID.ToString(), null);
                groupNode.CreateSubnetwork(clusterNet, clusID.ToString(), InternalEdg);
                groupNodes.Add(clusID, groupNode);
            }

            clusterNet._nodes.UnionWith(groupNodes.Values);

            foreach (var externalEdge in outModuleEdg)
            {
                clusterNet.AddArc(new Interaction(groupNodes[externalEdge.startnodeModuleID],
                    groupNodes[externalEdge.endnodeModuleID], externalEdge.interaction.Type, externalEdge.interaction.Name, externalEdge.interaction.weight, externalEdge.interaction.Direction));
            }

            return clusterNet;

        }

        public BasicNetwork CreateNetworkByMergedNode(IEnumerable<Node> nodeGroup, ref Node mergedNode)
        {
            BasicNetwork newNet = this.CreateObject() as BasicNetwork;
            IEnumerable<Interaction> interactions = this.Arcs;

            IEnumerable<Node> exclusiveNode = this.Nodes.Except(nodeGroup);
            IEnumerable<Interaction> inGroupInteraction = this.SelectInternalInteraction(nodeGroup);
            IEnumerable<Interaction> outGroupInteraction = this.SelectInternalInteraction(exclusiveNode);
            IEnumerable<Interaction> interTwoGroupInteraction = this.SelectInOutGroupInteraction(nodeGroup);
            //IEnumerable<Interaction> interTwoGroupInteraction = Netutil.SubstractInteractionList(interactions, inGroupInteraction);
            //interTwoGroupInteraction = Netutil.SubstractInteractionList(interTwoGroupInteraction, outGroupInteraction);


           

            foreach (Interaction inter in outGroupInteraction)
            {
                Node startN = newNet.AddNode(newNet.NewNode(inter.startNode.name, null));
                Node endN = newNet.AddNode(newNet.NewNode(inter.endNode.name, null));
               
                newNet.AddArc(new Interaction(startN, endN, inter.Type, inter.Name, inter.weight, inter.Direction));
                //newNet.AddNodeAndArc(inter.Clone() as Interaction);
            }
            IEnumerable<Node> isoNode = Netutil.SubstractNodeListByName<Node>(exclusiveNode, newNet.Nodes);
            newNet.AddNode(isoNode.ToArray());

           

            string clusID = string.Join("+", (from p in nodeGroup select p.name).ToArray());
            mergedNode = newNet.AddNode(newNet.NewNode(clusID, null));
            mergedNode.CreateSubnetwork(newNet, clusID, inGroupInteraction);
            

            foreach (Interaction inter in interTwoGroupInteraction)
            {
                
                if (nodeGroup.Contains(inter.endNode))//inlink
                {

                    Node pStart = newNet[inter.startNode.name];
                    if (inter.Direction == Interaction.DirectionType.directed)
                    {
                        if (newNet.GetArcsFromStartToEnd(pStart, mergedNode, Interaction.DirectionType.directed).Count() > 0)
                            newNet.GetArcsFromStartToEnd(pStart, mergedNode, Interaction.DirectionType.directed).ElementAt(0).weight += inter.weight;
                        else
                            newNet.AddNodeAndArc(new Interaction(pStart, mergedNode, inter.Type, inter.Name, inter.weight, inter.Direction));
                    }else
                    {
                        if (newNet.GetArcsBetween2Node(pStart, mergedNode, Interaction.DirectionType.undirected).Count() > 0)
                            newNet.GetArcsBetween2Node(pStart, mergedNode, Interaction.DirectionType.undirected).ElementAt(0).weight += inter.weight;
                        else
                            newNet.AddNodeAndArc(new Interaction(pStart, mergedNode, inter.Type, inter.Name, inter.weight, inter.Direction));
                    }

                }
                else
                {
                    Node pEnd = newNet[inter.endNode.name];
                    if (inter.Direction == Interaction.DirectionType.directed)
                    {
                        if (newNet.GetArcsFromStartToEnd(mergedNode, pEnd, Interaction.DirectionType.directed).Count() > 0)
                            newNet.GetArcsFromStartToEnd(mergedNode, pEnd, Interaction.DirectionType.directed).ElementAt(0).weight += inter.weight;
                        else
                            newNet.AddNodeAndArc(new Interaction(mergedNode, pEnd, inter.Type, inter.Name, inter.weight, inter.Direction));
                    }
                    else
                    {
                        if (newNet.GetArcsBetween2Node(mergedNode, pEnd, Interaction.DirectionType.undirected).Count() > 0)
                            newNet.GetArcsBetween2Node(mergedNode, pEnd, Interaction.DirectionType.undirected).ElementAt(0).weight += inter.weight;
                        else
                            newNet.AddNodeAndArc(new Interaction(mergedNode, pEnd, inter.Type, inter.Name, inter.weight, inter.Direction));
                    }
                }

            }


            return newNet;

        }
        
        public void CreateClusterNeworkWithWeight(BasicNetwork fromNet, Dictionary<Node, int> Cluster, bool byArcs = true)
        {
            IEnumerable<Interaction> interactions = byArcs ? fromNet.Arcs : fromNet.Edges;

            //Netutil.DumpInteraction(interactions.ToArray());
            //Netutil.DumpCluster(Cluster);

            var edgesIncludingModuleID = from e in
                                             (from e1 in interactions join cend in Cluster on e1.endNode.id equals cend.Key.id select new { interaction = e1, endnodeModuleID = cend.Value })
                                         join cstart in Cluster on e.interaction.startNode.id equals cstart.Key.id
                                         select new { e.interaction, e.endnodeModuleID, startnodeModuleID = cstart.Value };


            //string buffer = "Dump interaction with module...\n";
            //int i = 0;
            //if (edgesIncludingModuleID.Count() == 0) buffer += "Empty";
            //foreach (var n in edgesIncludingModuleID)
            //{
            //    buffer += string.Format("{0}-\t[{1}]\tstart:{2,5}\tmoduleID:{3,3}\ttype:{4,3}\tend:{5,1}\tmoduleID:{6,1}\n", ++i, n.interaction.ObjectID, n.interaction.startNode.name, n.startnodeModuleID, n.interaction.Type, n.interaction.endNode.name, n.endnodeModuleID);
            //}
            //Debug.WriteLine(buffer);
            

            var inModuleEdg = (from p in edgesIncludingModuleID
                               where p.endnodeModuleID == p.startnodeModuleID
                               select p);

            //Dump internal
            //i = 0;
            //buffer = "Dump internal with module...\n";
            //foreach (var n in inModuleEdg)
            //{
            //    buffer += string.Format("{0}-\t[{1}]\tstart:{2,5}\tmoduleID:{3,3}\ttype:{4,3}\tend:{5,1}\tmoduleID:{6,1}\n", ++i, n.interaction.ObjectID, n.interaction.startNode.name, n.startnodeModuleID, n.interaction.Type, n.interaction.endNode.name, n.endnodeModuleID);
            //}
            //Debug.WriteLine(buffer);

            //
            //var outModuleEdg1 = from t in edgesIncludingModuleID where t.endnodeModuleID != t.startnodeModuleID select t;

            ////Dump external
            //i = 0;
            //buffer = "Dump external Original interaction with module...\n";
            //foreach (var n in outModuleEdg1)
            //{
            //    buffer += string.Format("{0}-\t[{1}]\tstart:{2,5}\tmoduleID:{3,3}\ttype:{4,3}\tend:{5,1}\tmoduleID:{6,1}\n", ++i, n.interaction.ObjectID, n.interaction.startNode.name, n.startnodeModuleID,
            //        n.interaction.Type, n.interaction.endNode.name, n.endnodeModuleID);
            //}
            //Debug.WriteLine(buffer);

            //

            //Allow only one edge between two modules
            var outModuleEdg = from p in (from t in edgesIncludingModuleID where t.endnodeModuleID != t.startnodeModuleID select t) 
                               group p by new { p.endnodeModuleID, p.startnodeModuleID } into g
                           select new {Link=g.ElementAt(0),LinkCount = g.Count() };

            
            
            //Dump external
            //i = 0;
            //buffer = "Dump external Reduced interaction with module...\n";
            //foreach (var n in outModuleEdg)
            //{
            //    buffer += string.Format("{0}-\t[{1}]\tstart:{2,5}\tmoduleID:{3,3}\ttype:{4,3}\tend:{5,1}\tmoduleID:{6,1}\tlink count={7,1}\n", ++i, n.Link.interaction.ObjectID, n.Link.interaction.startNode.name, n.Link.startnodeModuleID,
            //        n.Link.interaction.Type, n.Link.interaction.endNode.name, n.Link.endnodeModuleID,n.LinkCount);
            //}
            //Debug.WriteLine(buffer);

            //

            var ClusterIDs = from p in Cluster group p by p.Value into g select g.Key;
            Dictionary<int, Node> groupNodes = new Dictionary<int, Node>();
            foreach (int clusID in ClusterIDs)
            {
                var InternalEdg = from p in inModuleEdg where p.endnodeModuleID == clusID select p.interaction;
                Node GroupNode = this.NewNode(clusID.ToString(), null);
                GroupNode.CreateSubnetwork(this,clusID.ToString(), InternalEdg);
                groupNodes.Add(clusID, GroupNode);
            }

            //BooleanNetwork net = new BooleanNetwork();//create new network
            this._nodes.UnionWith(groupNodes.Values);

            float weight = 0;
            foreach (var externalEdge in outModuleEdg)
            {
                weight = (float)externalEdge.LinkCount / (groupNodes[externalEdge.Link.startnodeModuleID].SubNetwork.Arcs.Count() * groupNodes[externalEdge.Link.endnodeModuleID].SubNetwork.Arcs.Count());

                this.AddArc(new Interaction(groupNodes[externalEdge.Link.startnodeModuleID],
                    groupNodes[externalEdge.Link.endnodeModuleID], externalEdge.Link.interaction.Type,externalEdge.Link.interaction.Name, weight));
            }

            

        }
        /// <summary>
        /// Calculate modularity from list of edges of the network that is considered a weighted undirected network 
        /// NOTE: returned Node is a COPY of object in the network (use its name to select the object in the network)
        /// </summary>
        /// <param name="Cluster"></param>
        /// <returns></returns>
        public double modularity(ref Dictionary<Node, int> Cluster)
        {
            //GraphData graph = GraphData.Convert(Arcs);
            double modularity = 0.0;
            //Cluster=GraphData.ClusterGraph(this,graph, ref modularity);
            Cluster = OptimizerModularity.ClusterGraph(this,true, ref modularity);
            
            return modularity;
        }
        /// <summary>
        /// Calculate modularity of the weighted directed network
        /// </summary>
        /// <param name="Cluster">The cluster of the directed network as output</param>
        /// <returns>Modularity value</returns>
        public double modularityWeightedDirected(ref Dictionary<Node, int> Cluster)
        {
            double modularity = 0.0;
            Cluster = OptimizerModularityDirected.ClusterGraph(this, ref modularity);// GraphData.ClusterGraph2(this, ref modularity);

            return modularity;
        }
        /// <summary>
        /// Calculate modularity of the network with an option for arcs or for edges corresponding to weighted or unweighted undirected network respectively
        /// </summary>
        /// <param name="Cluster">The output cluster</param>
        /// <param name="usingArc">The option: True for Arcs (undirected weighted modularity function); otherwise for Edges (undirected unweighted modularity function)</param>
        /// <returns></returns>
        public double modularity(ref Dictionary<Node, int> Cluster, bool usingArc)
        {
            //GraphData graph = usingArc?GraphData.Convert(Arcs):GraphData.Convert(Edges);
            double modularity = 0.0;
            Cluster = OptimizerModularity.ClusterGraph(this, usingArc, ref modularity);//GraphData.ClusterGraph(this, graph, ref modularity);
            
            return modularity;
        }
        /// <summary>
        /// Find a subnetwork whose modularity >0 from an initial given node set
        /// </summary>
        /// <param name="pStartNode">The initial node set to expand for finding subnetwork</param>
        /// <param name="maxExclusiveDensity">Maximum valid modularity</param>
        /// <returns>The subnetwork or non</returns>
        public BasicNetwork DetectSubnetworkByModularity(IEnumerable<Node> pStartNode, float maxExclusiveDensity = 1)
        {
            this.ClearData();
            HashSet<Node> pNode = new HashSet<Node>(pStartNode);
            BasicNetwork newNet=this.CreateNewExtractedSubnetwork(pNode);
            Dictionary<Node, int> pCluster=null;
            double Mo = 0;
            while (Mo <= 0)
            {
                while (!(newNet.EdgeDensity < maxExclusiveDensity && newNet.Nodes.Count() < this.Nodes.Count()))
                {
                    pNode.UnionWith(Node.NeighbourOfGroup(pNode));

                    newNet=this.CreateNewExtractedSubnetwork(pNode);
                }
                if (newNet.Nodes.Count() == this.Nodes.Count() && newNet.EdgeDensity >= maxExclusiveDensity)
                    return newNet;

                Mo = newNet.modularity(ref pCluster);
            }
            return newNet;
            
        }
        /// <summary>
        /// Ranking modules based on Tran centrality of module's centers
        /// </summary>
        /// <param name="tranCentralityIndex"></param>
        /// <returns></returns>
        public Dictionary<Node, float> RankModulesByTranCentrality(Dictionary<string, float> tranCentralityIndex)
        {
            Dictionary<Node, float> result = new Dictionary<Node, float>();
            Dictionary<Node, int> pCluster = null;
            this.modularity(ref pCluster);
            BasicNetwork clusterNet =  this.CreateClusterNework(pCluster) as BasicNetwork;
            //clusterNet.CreateClusterNework(this,pCluster);
            IEnumerable<string> centralNodes = null;
            foreach (Node node in clusterNet.Nodes)
            {
                centralNodes=node.SubNetwork.CenterNodeByTranCentrality();
                if(centralNodes.Count()>0)
                    result.Add(node, tranCentralityIndex[centralNodes.ElementAt(0)]);
                else
                    result.Add(node, float.PositiveInfinity);

            }
            return result;
        }

        #endregion
        /// <summary>
        /// Calculate modules' modularity
        /// </summary>
        /// <param name="Clusters">The modules to calculate modularity. 
        /// Have to sure the node in the modules on the network</param>
        /// <returns>Modularity list of modules identified by module ID</returns>
        public Dictionary<int, double> ModuleModularity(Dictionary<Node, int> Clusters)
        {
            Dictionary<int, double> dModuleModularity = new Dictionary<int, double>();
            //Clusters = SelectCluster(Clusters);
            IEnumerable<int> clusterIndexes = Clusters.Values.Distinct();

            double mo = 0;
            float gEdge = Edges.Count();
            float gDegree2 = Nodes.Sum(t => t.EdgeDegree); gDegree2 *= gDegree2;

            foreach (int cIndex in clusterIndexes)
            {
                IEnumerable<Node> aCluster = (from p in Clusters where p.Value == cIndex select p.Key);
                IEnumerable<Interaction> InClusterEdges = (from q in Edges where aCluster.Any(t => t.id == q.endNode.id) && aCluster.Any(t => t.id == q.startNode.id) select q);
                float mDegree = aCluster.Sum(t => t.EdgeDegree);
                mo = InClusterEdges.Count() / gEdge - mDegree * mDegree / gDegree2;
                dModuleModularity.Add(cIndex, mo);
            }


            return dModuleModularity;
        }
        #region Centrality
        /// <summary>
        /// Degree centrality of whole network, not arcs, http://en.wikipedia.org/wiki/Centrality
        /// </summary>
        public float DegreeCentrality
        {
            get
            {
                Dictionary<Node, float> NodeMap = new Dictionary<Node, float>();

                int nodeCount = Nodes.Count();
                foreach (Node node in Nodes)//Calculate degree centrality for each node
                    NodeMap[node] = (float)(node.EdgeDegree) / (nodeCount - 1);

                float maxCd = NodeMap.Max(p => p.Value); // With the node being the biggest hub, we select

                return NodeMap.Sum(node => (maxCd - node.Value) / ((nodeCount - 1) * (nodeCount - 2)));
            }
        }
        
       
        

        /// <summary>
        /// Find out central nodes basing on Tran's centrality of any network
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> CenterNodeByTranCentrality()
        {
            Dictionary<string, double> tranIndex = this.HierarchicalClosenessCentrality();

            return (from p in tranIndex where !double.IsInfinity(p.Value) && p.Value == tranIndex.Values.Min() select p.Key);

        }
        /// <summary>
        /// This novel status is defined by Tran Tien Dzung
        /// </summary>
        /// <param name="x">The node index that needs to be calculated</param>
        /// <param name="adjacentList">The adjacent list of the network</param>
        /// <param name="nNode">The total of network nodes</param>
        /// <returns>The status of a vertex, generally true with directed and undirected network</returns>
        private double DirectedStatus(int x, Dictionary<int, Dictionary<int, double>> adjacentList)
        {
            Dijkstra dijk = new Dijkstra();
            dijk.FindShortestPathAndDistance(adjacentList, x);
            // The unconnected nodes and the shortest distance between connected nodes have to minimize
            
            //<------- minumum reaching is 0
            //return (dijk.Distance.Count - 1) + (from p in dijk.Distance where p.Value > 0 select p).Sum(t => 1 / t.Value)/(this.Nodes.Count() - 1);

            //<------- minumum reaching is 1
            return dijk.Distance.Count + (from p in dijk.Distance where p.Value > 0 select p).Sum(t => 1 / t.Value) / (this.Nodes.Count() - 1);
            
        }
        private static double DirectedStatusAnalysis(int x, Dictionary<int, Dictionary<int, double>> adjacentList, int nNode, ref double connectivity, ref double closeness)
        {
            Dijkstra dijk = new Dijkstra();
            dijk.FindShortestPathAndDistance(adjacentList, x);
            // The unconnected nodes and the shortest distance between connected nodes have to minimize
            //connectivity = dijk.Distance.Count-1;
            connectivity = dijk.Distance.Count;

            
            closeness = (from p in dijk.Distance where p.Value>0 select p).Sum(t => 1/t.Value)/(nNode-1);
            return connectivity + closeness;
        }
        private float ClusteringCoefficient(Node u, Node v)
        {
            IEnumerable<Node> Nu = u.OutNeighbours;
            IEnumerable<Node> Nv = v.OutNeighbours;
            IEnumerable<Node> sharedNeighbour = from p in Nu join q in Nv on p.id equals q.id select q;
            return (sharedNeighbour.Count() + 1) / Math.Min(u.InDegree + u.OutDegree, v.InDegree + v.OutDegree);


        }
        private static float ExtraStatus(string xName, BasicNetwork Net)
        {
            //Node x = Net.GetNodeFromName(xName);
            //float Sum = 0;
            //foreach (Node e in Net.Nodes)
            //{
            //    Sum += Net.ClusteringCoefficient(x, e);
            //}
            //return Sum;
            Node n= Net.GetNodeFromName(xName);
            return (float)1/(n.OutDegree + n.InDegree);
            
        }
        
        
        /// <summary>
        /// Median values of vertices on the network (The median value order is  similar to one of centroid value)
        /// </summary>
        /// <returns>pairs of (node name (key), its median value)</returns>
        public Dictionary<string, double> HierarchicalClosenessCentrality()
        {
            Dictionary<string, int> nodeToIndex = null;
            Dictionary<int, string> indexToNode = null;
            Dictionary<int, Dictionary<int, double>> adjacentList = CreateAdjacentList(out nodeToIndex, out indexToNode);

            Dictionary<string, double> dxs = new Dictionary<string, double>(), infinityList = new Dictionary<string, double>();
            double temp = 0;

            foreach (KeyValuePair<string, int> node in nodeToIndex)
            {
                temp = DirectedStatus(node.Value, adjacentList);

                if (double.IsInfinity(temp))// if the node is unreachable, add separately to a different list to avoid being stadarlized
                    //infinityList.Add(node.Key, float.PositiveInfinity);
                    infinityList.Add(node.Key, 0);
                else
                    dxs.Add(node.Key, temp);

            }
           
            //Netutil.standardizeMedianValues(dxs);
            foreach (KeyValuePair<string, double> e in infinityList)
                dxs.Add(e.Key, e.Value);
            return dxs;

        }
        public Dictionary<string, double> NormalizeCloseness(Dictionary<string, double> theCloseness)
        {
            
            double a = theCloseness.Count - 1;// the number of nodes - 1

            for (int i = 0; i < theCloseness.Count; i++)
                theCloseness[theCloseness.Keys.ElementAt(i)] = theCloseness[theCloseness.Keys.ElementAt(i)] / a;
            return theCloseness;
            
        }
        /// <summary>
        /// Closeness centrality in new mode
        /// The closeness measure used is in the links
        /// http://www.sciencedirect.com/science/article/pii/S0378873310000183#
        /// http://toreopsahl.com/2010/03/20/closeness-centrality-in-networks-with-disconnected-components/#comments
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, double> ClosenessCentrality()
        {
            
            Dictionary<string, int> nodeToIndex = null;
            Dictionary<int, string> indexToNode = null;
            Dictionary<int, Dictionary<int, double>> adjacentList = CreateAdjacentList(out nodeToIndex, out indexToNode);

            Dictionary<string, double> dxs = new Dictionary<string, double>(), infinityList = new Dictionary<string, double>();
            double temp = 0;

            foreach (KeyValuePair<string, int> node in nodeToIndex)
            {
                
                Dijkstra dijk = new Dijkstra();
                dijk.FindShortestPathAndDistance(adjacentList, node.Value);
                
                
                temp = (from p in dijk.Distance where p.Value>0 select p).Sum(t => 1/t.Value)/(this.Nodes.Count()-1);
                

                if (double.IsInfinity(temp))// if the node is unreachable, add separately to a different list to avoid being stadarlized
                    //infinityList.Add(node.Key, float.NegativeInfinity);
                    infinityList.Add(node.Key, 0);
                else
                    dxs.Add(node.Key, temp);

            }

            foreach (KeyValuePair<string, double> e in infinityList)
                dxs.Add(e.Key, e.Value);
            return dxs;

        }
        
        //Check neighbours
        public bool checkNeighbours(Node node,Node node1)
        {  
            var listnode=new List<Node>();
            listnode=node.Neighbours.ToList();
            if(listnode.Contains(node1))
            {
                return true;
            }    
            return false;
        }

        //Determine the number of foci
        public List<Node[]> Foci()
        {
            var listFoci = new List<Node[]>();
            var list = new List<Node>();
            var listEdge = allEdge();

            for (int i = 0; i < listEdge.Count(); i++)
			{
                list.Clear();
                list.Add(listEdge[i][0]);
                list.Add(listEdge[i][1]);
                bool check = true;
                
                foreach (var item1 in this.Nodes)
	            {
                    check = true;
                    for (int j = 0; j < list.Count(); j++)
			        {        
                        if (!list.Contains(item1))
                        {
                            if(!checkNeighbours(item1, list[j]))
                            {
                                check = false;
                            }
                        }
                        else
                        {
                            check = false;
                        }
	                }
                    if (check == true)
                    {
                         list.Add(item1);
                    }
			    }
                if(!checkListDuplicates(listFoci, list))
                {
                    listFoci.Add(list.ToArray());
                }   
			}
            return listFoci;           
        }

        //Get all edges of the graph
        public List<Node[]> allEdge()
        {
            var listEdge = new List<Node[]>();
            foreach (var item in this.Nodes)
	        {
                foreach (var item1 in this.Nodes)
	            {
                    if(item != item1)
                    {
                        if(checkNeighbours(item,item1))
                        {
                            var edge = new List<Node>();
                            edge.Add(item);
                            edge.Add(item1);
                            if(!checkListDuplicates(listEdge, edge))
                            {
                                listEdge.Add(edge.ToArray());
                            }         
                        }    
	                }
	            }
	        }
            return listEdge;
        }

        //Determine the number of triadic closure
        public List<Node[]> triadicClosure()
        {       
            var list=new List<Node[]>();
            foreach (var item in this.Nodes)
	        {
                foreach (var item1 in this.Nodes)
	            {
                    foreach (var item2 in this.Nodes)
	                {
                        if(item != item1 && item != item2 && item1 != item2)
                        {
                            if(checkNeighbours(item,item1) && checkNeighbours(item,item2) && checkNeighbours(item1,item2) )
                            {
                                var triangle = new List<Node>();
                                triangle.Add(item);
                                triangle.Add(item1);
                                triangle.Add(item2);
                                if(!checkListDuplicates(list, triangle))
                                {
                                    list.Add(triangle.ToArray());
                                }
                            }    
                                                         
                        }
	                }
	            }
	        }
            return list;
        }

        //Check the list for duplicates
        public static Boolean checkListDuplicates(List<Node[]> list1, List<Node> list2)
        {
            bool check = false;
            for(int i = 0; i< list1.Count(); i++)
            {
                IEnumerable<Node> result = list1[i].Except(list2);     
                int count = new List<Node>(result).Count();
                if(count == 0)
                {
                    check = true;
                    break;
                }         
            }
            return check;
        }

        List<Node> listNodes = new List<Node>();

        //Determine the number of nodes of the graph
        public int nodesSocialnetwork(){
            var listNodeSocialNetwork = listNodes.Distinct().ToList();
            return listNodeSocialNetwork.Count();
        }

        //Determine the edges of the graph
        public List<Node[]> socialNetwork()
        {
            var listnode=neighboursEndNodes();
            var list=new List<Node[]>();
            foreach (var item in listnode)
	        {   
                foreach (var item1 in item)
	            {
                    foreach (var item2 in item)
	                {
                        if(item1 != item2)
                        {
                            var edge = new List<Node>();
                            edge.Add(item1);
                            edge.Add(item2);

                            listNodes.Add(item1);
                            listNodes.Add(item2);

                            if(!checkListDuplicates(list, edge))
                            {
                               list.Add(edge.ToArray());
                            }                 
                        }
	                }  
	            }
	        }
            return list;
        }

        //Get neighbors of end nodes
        public List<Node[]> neighboursEndNodes()
        {
            var listnode=new List<Node>();
            var listnode1=new List<Node[]>();
            var listEndNodes = endNodes();
            foreach (var item in listEndNodes)
	        {   
                listnode = item.Neighbours.ToList();
                listnode1.Add(listnode.ToArray());
	        }
            return listnode1;
        }      

        //Get the end column nodes
        public List<Node> endNodes()
        {  
            var listEndNodes=new List<Node>();
            foreach (Interaction e in this.Arcs)
            {
                    Node end = e.endNode;
                    listEndNodes.Add(end);
            }
            List<Node> distinct = listEndNodes.Distinct().ToList();
            return distinct;
        }

        public List<Node[]> BalanceCentrality()
        {
            var list=new List<Node[]>();
            foreach (var item in this.Nodes)
	        {
                foreach (var item1 in this.Nodes)
	            {
                    foreach (var item2 in this.Nodes)
	                {
                        if(item != item1 && item != item2 && item1 != item2)
                        {
                            if(checkNeighbours(item,item1) && checkNeighbours(item,item2)
                                && checkNeighbours(item1,item2) )
                            {
                                var triangle=new List<Node>();                            
                                triangle.Add(item);
                                triangle.Add(item1);
                                triangle.Add(item2);
                                if(!checkTriangle(list, triangle))
                                {
                                    list.Add(triangle.ToArray());
                                }
                                  
                            }    
                                                         
                        }
	                }
	            }
	        }
            return list;
        }
        public List<Node[]> triangleNotBalance()
        {
            var list = new List<Node[]>();
            var listNot = new List<Node[]>();

            list = BalanceCentrality();
            var check = false;
            var count = 0;
            foreach (var item in list)
            {
                if (!checkBalance(item[0], item[1], item[2]))
                {
                    var triangle = new List<Node>();
                    triangle.Add(item[0]);
                    triangle.Add(item[1]);
                    triangle.Add(item[2]);
                    listNot.Add(triangle.ToArray());
                }
            }

            return listNot;
        }

        public static Boolean checkTriangle(List<Node[]> triangleList, List<Node> triangle)
        {
            bool check = false;
            for(int i = 0; i< triangleList.Count; i++)
            {
               IEnumerable<Node> result  = triangleList[i].Except(triangle);
                int count = new List<Node>(result).Count;
                if(count == 0)
                {
                    check = true;
                    break;
                }
            }
            return check;
        }

        public bool Balance()
        {
            var list=new List<Node[]>();
            list=BalanceCentrality();
            var check = false;
            var count = 0;
            foreach (var item in list)
	        {
                if(checkBalance(item[0],item[1],item[2]))
                {
                    count++;
                }   
            }
            if(count == list.Count())
            {
                check = true;
            }
            return check;
        }

        public bool checkBalance(Node a,Node b,Node c)
        {
            if(checkWeight(a,b) && checkWeight(a,c) && checkWeight(b,c))
            {
                return true;
            }  
            if(checkWeight(a,b))
            {
                if(!checkWeight(a,c) && !checkWeight(b,c))
                {
                    return true;
                }    
            }   
            if(checkWeight(a,c))
{
                if(!checkWeight(a,b) && !checkWeight(b,c))
                {
                    return true;
                } 
            }    
            if(checkWeight(b,c))
            {
                if(!checkWeight(a,c) && !checkWeight(a,b))
                {
                    return true;
                }  
            }    
            return false;
        }

        public bool checkWeight(Node a,Node b)
        {
            
            foreach (var item in a.Arcs)
	        {
                foreach (var item1 in b.Arcs)
	            {
                    if(item.Equals(item1))
                    {
                        if(item.weight==1)
                        {
                            return true;
                        }    
                    }    
	            }
	        }
            return false;
        }

        public List<Node[]> Positive()
        {
            var list=new List<Node[]>();
            foreach (var item in this.Nodes)
	        {
                foreach (var item1 in this.Nodes)
	            {
                    if(item != item1)
                    {
                        if(checkNeighbours(item,item1) )
                        {
                            var arc=new List<Node>();                            
                            arc.Add(item);
                            arc.Add(item1);
                            if(!checkArcAlive(list, arc))
                            {
                                list.Add(arc.ToArray());
                            }
                        }    
                    }	                
	            }
	        }
            return list;
        }
        
        public List<Node[]> Negative()
        {
            var list=new List<Node[]>();
            foreach (var item in this.Nodes)
	        {
                foreach (var item1 in this.Nodes)
	            {
                    if(item != item1)
                    {
                        if(!checkNeighbours(item,item1) )
                        {
                            var arc=new List<Node>();                            
                            arc.Add(item);
                            arc.Add(item1);
                            if(!checkArcAlive(list, arc))
                            {
                                list.Add(arc.ToArray());
                            }
                        }    
                    }	                
	            }
	        }
            return list;
        }

        public static Boolean checkArcAlive(List<Node[]> arcList, List<Node> arc)
        {
            bool check = false;
            for(int i = 0; i< arcList.Count; i++)
            {
               IEnumerable<Node> result  = arcList[i].Except(arc);
                int count = new List<Node>(result).Count;
                if(count == 0)
                {
                    check = true;
                    break;
                }
            }
            return check;
        }

        ////Largest Strong Connected Component
        public IList<Node> FindGiantScc()
        {
            var indices = new Dictionary<string, int>();
            var lowlinks = new Dictionary<string, int>();
            var connected = new List<Node[]>();
            var stack = new Stack<Node>();
            bool excludeSingleItems = false;


            foreach (var vertex in this.Nodes)
            {
                if (!indices.ContainsKey(vertex.name))
                {
                    TarjansStronglyConnectedComponentsAlgorithm(excludeSingleItems, vertex, indices, lowlinks, connected, stack, 0);
                }
            }
            return connected.OrderBy(a => a.Length).Last();

        }
        ////All OutPut
        public void Out(Node a, HashSet<Node> list)
        {

            foreach (var item in a.OutNeighbours)
            {
                while (!list.Contains(item))
                {
                    list.Add(item);
                    Out(item, list);
                }

            }

        }
        public HashSet<Node> FindAllOutput(IList<Node> scc)
        {

            var AllOutPut = new HashSet<Node>();
            var NodeOut = new List<Node>();
            foreach (Node i in scc)
            {
                foreach (var edge in i.OutUnLink)
                {
                    var next = edge.endNode;

                    if (!scc.Contains(next))
                    {
                        NodeOut.Add(next);

                    }
                }
            }

            foreach (var item in NodeOut)
            {
                AllOutPut.Add(item);
            }
            foreach (var item in NodeOut)
            {
                Out(item, AllOutPut);
            }
            return AllOutPut;

        }

        ////All InPut
        public void In(Node a, HashSet<Node> list)
        {
            foreach (var item in a.InNeighbours)
            {
                while (!list.Contains(item))
                {
                    list.Add(item);
                    In(item, list);
                }

            }

        }
        public HashSet<Node> FindAllInput(IList<Node> scc)
        {

            var AllInPut = new HashSet<Node>();
            var NodeIn = new List<Node>();
            foreach (Node i in scc)
            {
                foreach (var edge in i.InLink)
                {
                    var previous = edge.startNode;
                    if (!scc.Contains(previous))
                    {
                        NodeIn.Add(previous);
                    }
                }
            }

            foreach (var item in NodeIn)
            {
                AllInPut.Add(item);
            }
            foreach (var item in NodeIn)
            {
                In(item, AllInPut);
            }
            return AllInPut;

        }
        ////Disconnected Components
        public IList<Node> FindDisconnected()
        {
            var Disconnected = new List<Node>();
            var maingraph = new List<Node>();

            var componentsconnect = new List<Node[]>();

            Dictionary<Node, bool> visited = new Dictionary<Node, bool>();

            foreach (var item in this.Nodes)
            {
                visited.Add(item, false);
            }
            foreach (var item in this.Nodes)
            {
                if (!visited[item])
                {

                    var list = new List<Node>();
                    dfs(visited, item, list);
                    componentsconnect.Add(list.ToArray());
                }
            }
            maingraph = componentsconnect.OrderBy(a => a.Length).Last().ToList();
            foreach (var item in this.Nodes)
            {
                if (!maingraph.Contains(item))
                {
                    Disconnected.Add(item);
                }
            }
            return Disconnected;
        }
        public void dfs(IDictionary<Node, bool> visited, Node node, IList<Node> list)
        {
            visited[node] = true;
            list.Add(node);
            foreach (var item in node.Neighbours)
            {
                if (!visited[item])
                {
                    dfs(visited, item, list);
                }
            }
        }

        ////Tubes
        public bool CheckHasPath(Node startVertex, string endVertex)
        {
            if (IsMatch(startVertex, endVertex)) return true;

            Uti.ArgumentNotNull(startVertex, "startVertex");

            Dictionary<string, Node> visitedVertices = new Dictionary<string, Node>();

            var visitableQueue = new Queue<Node>();

            visitableQueue.Enqueue(startVertex);
            visitedVertices.Add(startVertex.name, startVertex);

            while (!(visitableQueue.Count == 0))
            {
                var vertex = visitableQueue.Dequeue();

                //Start visit here
                if (IsMatch(vertex, endVertex)) return true;
                //End visit

                var edges = vertex.OutLink;

                for (var i = 0; i < edges.Count(); i++)
                {
                    var vertexToVisit = edges.ElementAt(i).GetPartnerVertex(vertex);

                    if (!visitedVertices.ContainsKey(vertexToVisit.name))
                    {
                        visitableQueue.Enqueue(vertexToVisit);
                        visitedVertices.Add(vertexToVisit.name, vertexToVisit);
                    }
                }
            }
            return false;
        }

        public void OutTobe(Node start, Node end, HashSet<Node> list, IList<Node> scc, HashSet<Node> t)
        {


            list.Add(start);
            foreach (var item in start.OutNeighbours)
            {
                if (!t.Contains(item))
                {
                    break;
                }

                if (!CheckHasPath(item, end.name))
                {
                    break;
                }
                if (item == end)
                {
                    break;
                }
                if (scc.Contains(item))
                {
                    break;
                }
                while (!list.Contains(item))
                {
                    list.Add(item);
                    OutTobe(item, end, list, scc, t);
                }
            }

        }

        public HashSet<Node> FindTubes(IList<Node> scc, HashSet<Node> t, HashSet<Node> input, HashSet<Node> output)
        {
            //var AllInPut= FindAllInput();
            // var AllOutPut= FindAllOutput();
            var list = new HashSet<Node>();


            foreach (var outnode in output)
            {
                foreach (var innode in input)
                {
                    OutTobe(innode, outnode, list, scc, t);
                }
            }
            list.ExceptWith(input);
            return list;
        }
        ////Tendrils
        public HashSet<Node> FindTendrils(IList<Node> scc, HashSet<Node> AllInPut, HashSet<Node> AllOutPut)
        {

            var disconnected = FindDisconnected();
            var Tendrils = new HashSet<Node>();

            foreach (var item in this.Nodes)
            {
                if (!scc.Contains(item) && !AllOutPut.Contains(item) && !AllInPut.Contains(item) && !disconnected.Contains(item))
                {
                    Tendrils.Add(item);
                }
            }

            return Tendrils;

        }
        public HashSet<Node> TendrilsNew(HashSet<Node> t, HashSet<Node> tobe)
        {
            t.ExceptWith(tobe);
            return t;

        }
        public bool checkGateKepperzz(Node Start , Node End, List<Node> temp, List<Node> result)
          {
            //add diem dau
            if(temp.Count == 0)
            {
                temp.Add(Start);
            }
            
            if (Start.Equals(End))
            {
                //neu co duong di nhieu hon 2 diem thi duyet
                if (temp.Count > 2)
                {
                     //tao moi luu cai cu, clear cai cu 
                    List<Node> tempResult = new List<Node>(result);
                    result.Clear();
                    //chay bo dau bo cuoi , 
                    for(int i = 1; i < temp.Count-1; i++)
                    {
                        //neu tap hop dinhn cu chua phan tu cua duong di dang xet thi add vao tap hop dinh di qua
                        if (tempResult.Contains(temp[i]))
                        {
                            result.Add(temp[i]);
                        }
                    }
                    if(result.Count == 0)
                    {
                        return false;
                    }
                }
                else // 2 dinh false
                {
                    result.Clear();
                    return false;
                }
            }
            else
            {
                var ds = Start.Arcs.ToList();
                
                foreach(var item in ds)
                {
                    
                    if(item.Direction == Interaction.DirectionType.directed)
                    {
                        if (!temp.Contains(item.endNode))
                        {
                       
                            temp.Add(item.endNode);
                            if(!checkGateKepperzz(item.endNode, End, temp, result))
                            {
                                break;
                            }
                            temp.RemoveAt(temp.Count-1);
                        }
                    }
                    else
                    {
                        if(item.startNode == Start)
                        {
                            if (!temp.Contains(item.endNode))
                            {
                       
                                temp.Add(item.endNode);
                                if(!checkGateKepperzz(item.endNode, End, temp, result))
                                {
                                    break;
                                }
                                temp.RemoveAt(temp.Count-1);
                            }
                        }
                        else
                        {
                            if (!temp.Contains(item.startNode))
                            {
                       
                                temp.Add(item.startNode);
                                if(!checkGateKepperzz(item.startNode, End, temp, result))
                                {
                                    break;
                                }
                                temp.RemoveAt(temp.Count-1);
                            }
                        }
                    }
                }
            }
            return true;
        }

        public static bool IsEqualRankingStates(Dictionary<Node, float> Ranking1, Dictionary<Node, float> Ranking2)
        {
            if (Ranking1.Count() != Ranking2.Count())
            {
                return false;
            }

            foreach (Node i in Ranking1.Keys)
            {
                if (Ranking1[i] != Ranking2[i])
                {
                    return false;
                }
            }
            return true;
        }

        
        /// <summary>
        /// Xep hang so luong nguoi di taxi sau mot qua trinh ngau nhien di chuyen trong mang co trong so la so luot di chuyen.
        /// NOTICE: Thu tu cua cac canh KHONG anh huong den ket qua
        /// <param name="damping">Ti le %nguoi di taxi phat sinh them tai moi noi domain=(-1,0] + [0,1): 0: khong tang khong giam; >0: Tang them; nho hon 0: giam di </param>
        /// </summary>
        /// <returns> So nguoi hoi tu o cac node
        /// Node co thu hang nho nhat -> Node co so luong nguoi di nhieu nhat.
        /// Node co thu hang lon nhat -> Node co so luong nguoi den nhieu nhat.
        /// </returns>
        public Dictionary<Node, float> TaxiPassengerRank(float damping = 0.0f)
        {
            /// x(t): So nguoi di taxi ban dau tai thoi diem t; 
            /// x(t+1) = x(t) + damping. x(t) + Sum(so nguoi di taxi tu node khac den theo xac suat weight/sumWeight)

            const int maxIterations = 500;
            const double tolerance = 2 * double.Epsilon;
            //const float damping = 0.85f;
            float sumWeight = (float)(from p in this.Arcs select p.weight).Sum();

            Dictionary<Node, float> Ranking = new Dictionary<Node, float>();// So luong nguoi di taxi tai thoi diem t
            Dictionary<Node, float> nextRanking = new Dictionary<Node, float>();// So luong nguoi di taxi tai thoi diem t+1
            Dictionary<Node, float> r = new Dictionary<Node, float>();// So luong nguoi di chuyen giua cac nut
            float nPeople = (float)this.Nodes.Count();// So luong nguoi ban dau
            foreach (Node n in this.Nodes)
                Ranking.Add(n, nPeople);

            double error = 0;
            int iter = 0;

            do
            {
                foreach (Node n in this.Nodes)
                    r[n] = 0;
                error = 0;
                foreach (Interaction e in this.Arcs)
                {
                    Node end = e.endNode;
                    Node start = e.startNode;
                    
                    r[end] += Ranking[start] * (float)e.weight / sumWeight;// tong luong chuyen tu nguon sang dich
                    r[start] -= Ranking[start] * (float)e.weight / sumWeight;// luong nguon con lai
                    
                    if (e.Direction == Interaction.DirectionType.undirected)// chieu nguoc lai neu la 2 chieu
                    {
                        r[start] += Ranking[end] * (float)e.weight / sumWeight;// tong luong chuyen tu nguon sang dich
                        r[end] -= Ranking[end] * (float)e.weight / sumWeight;// luong nguon con lai
                    }
                   
                }

                foreach (Node n in this.Nodes)
                {
                    //damping = Mathutil.NumericMath.RandomCraft.Next(-10, 10) / 100.0f;
                    nextRanking[n] = Ranking[n] + damping * Ranking[n] + r[n];
                    error += Math.Abs(Ranking[n] - nextRanking[n]);
                    Ranking[n] = nextRanking[n];
                }

            } while (error > tolerance && ++iter < maxIterations);
            return Ranking;
        }
        /// <summary>
        /// Probability of states with an edge representing the probability to change two states
        /// NOTICE: domain of edge weight is [0, 1]
        /// </summary>
        /// <returns>Steady-state probability for each state happening</returns>
        public Dictionary<Node, float> MarkovProbability()
        {
            
            /// x(t): Xac suat ban dau tai thoi diem t; 
            /// x(t+1) = x(t) + Xac suat co dieu kien

            const int maxIterations = 500;
            const double tolerance = 2 * double.Epsilon;
            //const float damping = 0.85f;
            

            Dictionary<Node, float> Ranking = new Dictionary<Node, float>();// So luong nguoi di taxi tai thoi diem t
            Dictionary<Node, float> nextRanking = new Dictionary<Node, float>();// So luong nguoi di taxi tai thoi diem t+1
            Dictionary<Node, float> r = new Dictionary<Node, float>();// So luong nguoi di chuyen giua cac nut
            float nPeople = 1.0f/(float)this.Nodes.Count();// Xac suat moi node bang nhau luc dau
            foreach (Node n in this.Nodes)
                Ranking.Add(n, nPeople);

            double error = 0;
            int iter = 0;

            do
            {
                foreach (Node n in this.Nodes)
                    r[n] = 0;
                error = 0;
                foreach (Interaction e in this.Arcs)
                {
                    Node end = e.endNode;
                    Node start = e.startNode;

                    r[end] += Ranking[start] * (float)e.weight;// tong luong chuyen tu nguon sang dich
                    r[start] -= Ranking[start] * (float)e.weight;// luong nguon con lai

                    if (e.Direction == Interaction.DirectionType.undirected)// chieu nguoc lai neu la 2 chieu
                    {
                        r[start] += Ranking[end] * (float)e.weight;// tong luong chuyen tu nguon sang dich
                        r[end] -= Ranking[end] * (float)e.weight;// luong nguon con lai
                    }

                }

                foreach (Node n in this.Nodes)
                {
                    //damping = Mathutil.NumericMath.RandomCraft.Next(-10, 10) / 100.0f;
                    nextRanking[n] = Ranking[n] +  r[n];
                    error += Math.Abs(Ranking[n] - nextRanking[n]);
                    Ranking[n] = nextRanking[n];
                }

            } while (error > tolerance);
            return Ranking;
        }
        /*
        /// <summary>
        /// Xep hang so luong nguoi di taxi sau mot qua trinh ngau nhien di chuyen trong mang co trong so la so luot di chuyen.
        /// NOTICE: Thu tu cua cac canh CO anh huong den ket qua vi la xac suat co dieu kien
        /// <param name="damping">Ti le %nguoi di taxi phat sinh them tai moi noi domain=(-1,0] + [0,1): 0: khong tang khong giam; >0: Tang them; nho hon 0: giam di </param>
        /// </summary>
        /// <returns> So nguoi hoi tu o cac node
        /// Node co thu hang nho nhat -> Node co so luong nguoi di nhieu nhat.
        /// Node co thu hang lon nhat -> Node co so luong nguoi den nhieu nhat.
        /// </returns>
        public Dictionary<Node, float> TaxiPassengerRank(float damping=0.0f)
        {
            /// x(t): So nguoi di taxi ban dau tai thoi diem t; 
            /// x(t+1) = x(t) + damping. x(t) + Sum(so nguoi di taxi tu node khac den theo xac suat weight/sumWeight)
            
            const int maxIterations = 1000;
            const double tolerance = 2 * double.Epsilon;
            //const float damping = 0.85f;
            float sumWeight = (float)(from p in this.Arcs select p.weight).Sum();

            Dictionary<Node, float> Ranking = new Dictionary<Node, float>();// So luong nguoi di taxi tai thoi diem t
            Dictionary<Node, float> nextRanking = new Dictionary<Node, float>();// So luong nguoi di taxi tai thoi diem t+1
            float nPeople = (float)this.Nodes.Count();// So luong nguoi ban dau
            foreach (Node n in this.Nodes)
                Ranking.Add(n, nPeople);

            double error = 0;
            int iter = 0;
            
            foreach (Node n in this.Nodes)
                nextRanking[n] = Ranking[n];
            
            do
            {
                error = 0;
                foreach (KeyValuePair<Node, float> de in Ranking)
                {
                    Node end = de.Key;
                    float rank = de.Value;
                    float r = 0;
                    IEnumerable<Interaction> vInteraction = end.InUnLink;
                    foreach (Interaction e in vInteraction)
                    {
                        Node start = e.GetPartnerVertex(end);// start node

                        r += nextRanking[start] * (float)e.weight / sumWeight;// tong luong chuyen tu nguon sang dich
                        nextRanking[start] -= nextRanking[start] * (float)e.weight / sumWeight;// luong nguon con lai
                    }
                    //damping = Mathutil.NumericMath.RandomCraft.Next(-10, 10) / 100.0f;
                    float newRank = nextRanking[end] + damping * nextRanking[end] + r;// Cap nhat dich
                    nextRanking[end] = newRank;
                }
   
                foreach (Node n in this.Nodes)
                {
                    error += Math.Abs(Ranking[n] - nextRanking[n]);
                    Ranking[n] = nextRanking[n];
                }

                iter++;
            } while (error > tolerance && iter < maxIterations);
            return Ranking;
        }
        */
        /// <summary>
        /// A document is important if it is highly cited by other documents. Moreover, citations from important documents have more weight than citations from unimportant documents. 
        /// PageRankInLink ranks a document according to the number of highly ranked documents that point TO it
        /// PageRankOutLink is not reverted order of PageRankInLink and vice versa
        /// The idea behind PageRank is simple and intuitive: pages that are important are referenced
        /// by other important pages. There is an important literature on the web that explains 
        /// PageRank: http://www-db.stanford.edu/~backrub/google.html
        /// The PageRank is computed by using the following iterative formula
        /// PR(A) = (1-d) + d (PR(T1)/C(T1) + ... + PR(Tn)/C(Tn)) 
        /// where PR is the PageRank, d is a damping factor usually set to 0.85,
        /// C(A) is defined as the number of links going out of page A.  
        /// NOTE: PAGE RANK for Undirected graph approximate to degree centrality
        /// </summary>
        /// <returns></returns>
        /*public Dictionary<Node, float> PageRankCentralityInLink()
        {
            const int maxIterations = 100;
            const double tolerance = 2 * double.Epsilon;
            const float damping = 0.85f;

            Dictionary<Node, float> Ranking = new Dictionary<Node, float>();
            Dictionary<Node, float> tempRanking = new Dictionary<Node, float>();
            float iniProbability = 1 / (float)this.Nodes.Count();
            foreach (Node n in this.Nodes)
                Ranking.Add(n, iniProbability);
            
            double error = 0;
            int iter = 0;
            do
            {
                error = 0;
                foreach(KeyValuePair<Node,float> de in Ranking)
                {
                    Node v = de.Key;
                    float rank = de.Value;
                    float r = 0;
                    IEnumerable<Interaction> vInteraction = v.InUnLink;
                    foreach (Interaction e in vInteraction)
                    {
                        Node neibourNode = e.GetPartnerVertex(v);
                        //r += Ranking[neibourNode] / neibourNode.OutDegree;
                        r += Ranking[neibourNode] / neibourNode.OutUnDegree;
                    }
                    
                    float newRank = (1 - damping) + damping * r;
                    tempRanking[v] = newRank;
                    error += Math.Abs(rank - newRank);
                }

                // swap ranks
                Dictionary<Node, float> temp = Ranking;
                Ranking = tempRanking;
                tempRanking = temp;

                iter++;
            } while (error > tolerance && iter < maxIterations);
            return Ranking;
        }
        */
        /// <summary>
        /// PageRank for taxi probe data (response for the request from Mr. Tang)
        /// </summary>
        /// <returns>a list of nodes and their ranking </returns>
        public Dictionary<Node, float> PageRankCentralityInLink()
        {
            // khoi tao, bo damping
            const int maxIterations = 100;
            const double tolerance = 2 * double.Epsilon;
            const float damping = 0.3f;// Changed from 0.85f to 0.3f
            // tu dien luu tru trang thai cua not
            Dictionary<Node, float> Ranking = new Dictionary<Node, float>();
            Dictionary<Node, float> tempRanking = new Dictionary<Node, float>();
            float iniProbability = 1 / (float)this.Nodes.Count();
            foreach (Node n in this.Nodes)
                Ranking.Add(n, iniProbability);
            // khoi tao
            double error = 0;
            int iter = 0;
            do
            {
                error = 0;
                foreach (KeyValuePair<Node, float> de in Ranking)
                {
                    Node v = de.Key;
                    float rank = de.Value;
                    float r = 0;
                    IEnumerable<Interaction> vInteraction = v.InLink;
                    foreach (Interaction e in vInteraction)
                    {
                        Node neibourNode = e.GetPartnerVertex(v);
                        //r += Ranking[neibourNode] / neibourNode.OutDegree;
                        r += Ranking[neibourNode] * (float)(e.weight / neibourNode.OutTotalWeight); // Using various probability between node pairs
                    }

                    float newRank = (1 - damping) + damping * r;
                    tempRanking[v] = newRank;
                    error += Math.Abs(rank - newRank);
                }

                // swap ranks
                Dictionary<Node, float> temp = Ranking;
                Ranking = tempRanking;
                tempRanking = temp;

                iter++;
            } while (error > tolerance && iter < maxIterations);
            return Ranking;
        }

        public BasicNetwork CreateInvertedLinkGraph()
        {
            BasicNetwork Net=this.CreateObject() as BasicNetwork;
            foreach (Interaction interact in this.Arcs)
            {
                Node s = Net.AddNode(interact.endNode.name);
                Node e = Net.AddNode(interact.startNode.name);
                Net.AddNodeAndArc(new Interaction(s, e, interact.Type, interact.Name, interact.weight, interact.Direction));
            }
            return Net;
        }

        /// <summary>
        /// A document is important if it is highly refer to other documents. Moreover, citations from important documents have more weight than citations from unimportant documents. 
        /// PageRankOutLink ranks a document according to the number of highly ranked documents that point FROM it
        /// PageRankOutLink is not reverted order of PageRankInLink and vice versa
        /// The idea behind PageRank is simple and intuitive: pages that are important are referenced
        /// by other important pages. There is an important literature on the web that explains 
        /// PageRankInLink here: http://www-db.stanford.edu/~backrub/google.html
        /// The PageRank is computed by using the following iterative formula
        /// PR(A) = (1-d) + d (PR(T1)/C(T1) + ... + PR(Tn)/C(Tn)) 
        /// where PR is the PageRank, d is a damping factor usually set to 0.85,
        /// C(A) is defined as the number of links going in of page A.  
        /// NOTE: PAGE RANK for Undirected graph approximate to degree centrality
        /// </summary>
        /// <returns></returns>
        public Dictionary<Node, float> PageRankCentralityOutLink()
        {
            const int maxIterations = 60;
            const double tolerance = 2 * double.Epsilon;
            const float damping = 0.85f;

            Dictionary<Node, float> Ranking = new Dictionary<Node, float>();
            Dictionary<Node, float> tempRanking = new Dictionary<Node, float>();
            float iniProbability = 1 / (float)this.Nodes.Count();
            foreach (Node n in this.Nodes)
                Ranking.Add(n, iniProbability);

            double error = 0;
            int iter = 0;
            do
            {
                error = 0;
                foreach (KeyValuePair<Node, float> de in Ranking)
                {
                    Node v = de.Key;
                    float rank = de.Value;
                    float r = 0;
                    IEnumerable<Interaction> vInteraction = v.OutUnLink;
                    foreach (Interaction e in vInteraction)
                    {
                        Node neibourNode = e.GetPartnerVertex(v);
                        //r += Ranking[neibourNode] / neibourNode.InDegree;
                        r += Ranking[neibourNode] / neibourNode.InUnDegree;
                    }

                    float newRank = (1 - damping) + damping * r;
                    tempRanking[v] = newRank;
                    error += Math.Abs(rank - newRank);
                }

                // swap ranks
                Dictionary<Node, float> temp = Ranking;
                Ranking = tempRanking;
                tempRanking = temp;

                iter++;
            } while (error > tolerance && iter < maxIterations);
            return Ranking;
        }

        /// <summary>
        /// Katz centrality computes the relative influence of a node within a network
        /// by measuring the number of the immediate neighbors (first degree nodes)
        /// and also all other nodes in the network that connect to the node under
        /// consideration through these immediate neighbors.
        /// Leo Katz: A New Status Index Derived from Sociometric Index. Psychometrika 18(1):39–43, 1953 
        /// http://people.cs.vt.edu/badityap/classes/cs6604-Fall13/readings/katz-1953.pdf
        /// </summary>
        /// <returns></returns>
        public Dictionary<Node, float> KatzCentrality()
        {
            const int maxIterations = 1000;
            const double tolerance = 2 * double.Epsilon;
            const float alpha = 0.1f;
            const float beta = 1.0f;

            Dictionary<Node, float> Ranking = new Dictionary<Node, float>();

            foreach (Node n in this.Nodes)
                Ranking.Add(n, beta);

            Dictionary<Node, float> tempRanking = new Dictionary<Node, float>(Ranking);

            int iter = 0;
            double error = 0;
            double sum2;
            double norm;

            do
            {
                sum2 = 0;
                foreach (Node n in this.Nodes)
                {
                    Ranking[n] = 0.0f;

                    foreach (Interaction e in n.InLink)
                    {
                        Ranking[n] += tempRanking[e.startNode];
                    }

                    Ranking[n] = alpha * Ranking[n] + beta;

                    sum2 += Ranking[n] * Ranking[n];
                }

                // normalization
                norm = Math.Sqrt(sum2);
                error = 0;

                foreach (Node n in this.Nodes)
                {
                    Ranking[n] /= (float) norm;

                    if (Math.Abs(Ranking[n] - tempRanking[n]) > error)
                    {
                        error = Math.Abs(Ranking[n] - tempRanking[n]);
                    }
                }

                // swap ranks
                Dictionary<Node, float> temp = Ranking;
                Ranking = tempRanking;
                tempRanking = temp;

                iter++;
            } while (error > tolerance && iter < maxIterations);

            return Ranking;
        }

        /// <summary>
        /// Measure betweeness centrality on unweight graph, see algorithm in papr "A Faster Algorithm for Betweenness Centrality, Ulrik Brandes, 2001"
        /// betweeness centrality  original version is defined at link http://en.wikipedia.org/wiki/Betweenness_centrality
        /// </summary>
        /// <returns></returns>
        public Dictionary<Node, float> BetweenessCentrality()
        {
            Dictionary<Node, float> Cb = new Dictionary<Node, float>();
            Dictionary<Node, float> sigma = new Dictionary<Node, float>();
            Dictionary<Node, float> d = new Dictionary<Node, float>();
            Stack<Node> S = new Stack<Node>();
            Dictionary<Node, HashSet<Node>> P = new Dictionary<Node, HashSet<Node>>();
            Queue<Node> Q = new Queue<Node>();
            Dictionary<Node, float> delta = new Dictionary<Node, float>();

            //Initialize Cb
            foreach (Node v in this.Nodes)
                Cb[v] = 0;

            foreach (Node s in this.Nodes)
            {
                S.Clear();
                P.Clear();
                foreach(Node t in this.Nodes)
                {
                    sigma[t] = 0;
                    d[t] = -1;
                    delta[t] = 0; // early intitialize delta
                }
                sigma[s] = 1;
                d[s] = 0;
                Q.Clear();
                Q.Enqueue(s);
                while (Q.Count > 0)
                {
                    Node v = Q.Dequeue();
                    S.Push(v);
                    //w found for the first time?
                    IEnumerable<Node> neighbourOfv = v.InUnNeighbours;//determine if the work is on directed or undirected graph?
                    foreach (Node w in neighbourOfv)
                    {
                        if(d[w]<0)
                        {
                            Q.Enqueue(w);
                            d[w]=d[v]+1;
                        }
                        //shortest path to w via v?
                        if (d[w] == d[v] + 1)
                        {
                            sigma[w] = sigma[w] + sigma[v];
                            if (!P.ContainsKey(w))
                                P[w] = new HashSet<Node>();
                            P[w].Add(v);
                        }
                    }
                }
                // delta has been intitialized already
                // S returns vertices in order of non-increasing distance from s
                while (S.Count > 0)
                {
                    Node w=S.Pop();
                    if(P.ContainsKey(w))//
                    foreach (Node v in P[w])
                        delta[v] = delta[v] + sigma[v] / sigma[w] * (1 + delta[w]);

                    if (w != s)Cb[w] = Cb[w] + delta[w];
                }
            }
            return Cb;
        }
       
        /// <summary>
        /// Measure eigen centrality on graph with fastest dominant eigenvector alogrithm
        /// </summary>
        /// <returns></returns>
        public Dictionary<Node, double> EigenCentrality()
        {
            Dictionary<string, int> nameToIndex = null;
            Dictionary<int, string> indexToName = null;
            Matrix A = Matrix.Create(this.CreateAdjacentMatrix(out nameToIndex, out indexToName));
            double eigenValue = 0;
            Vector eigenVector = EigenvalueDecomposition.DominantEigenvector(A, out eigenValue);
            Dictionary<Node, double> result = new Dictionary<Node, double>();
            for (int i = 0; i < eigenVector.Length; i++)
            {
                result.Add(this.GetNodeFromIndex(i), eigenVector[i]);
            }
            return result;
            
        }
        /// <summary>
        /// Return a node triple of (HC centrality, connectivity, closeness centrality) respectively
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Triple<double>> HierarchicalClosenessCentralityAnalysis()
        {
            Dictionary<string, int> nodeToIndex = null;
            Dictionary<int, string> indexToNode = null;
            Dictionary<int, Dictionary<int, double>> adjacentList = CreateAdjacentList(out nodeToIndex, out indexToNode);

            Dictionary<string, Triple<double>> dxs = new Dictionary<string, Triple<double>>(), infinityList = new Dictionary<string, Triple<double>>();
            double temp = 0, connectivity = 0; double closeness = 0;

            foreach (KeyValuePair<string, int> node in nodeToIndex)
            {
                temp = DirectedStatusAnalysis(node.Value, adjacentList, this.Nodes.Count(), ref connectivity, ref closeness);

                if (double.IsInfinity(temp))// if the node is unreachable, add separately to a different list to avoid being stadarlized
                    //infinityList.Add(node.Key, new Triple<double>(float.MinValue, connectivity, float.MinValue));
                    infinityList.Add(node.Key, new Triple<double>(0, connectivity, 0));
                else
                    dxs.Add(node.Key, new Triple<double>(temp, connectivity, closeness));

            }

            foreach (KeyValuePair<string, Triple<double>> e in infinityList)
                dxs.Add(e.Key, e.Value);
            return dxs;

        }
        /// <summary>
        /// Compute X entropy of nodes, where X is maybe centrality values of the nodes
        /// </summary>
        /// <param name="pNodeList">The node list with centrality value</param>
        /// <returns>X entropy of nodes</returns>
        public static double EntropyOfNodes(Dictionary<Node, double> pNodeList)
        {
            int n = pNodeList.Keys.Count;
            double entropy = -(from p in pNodeList group p by p.Value into g select new { groupID = g.Key, p = (double)g.Count() / n }).Sum(t => t.p * Math.Log(t.p, 2));
            //return entropy / Math.Log(n, 2); 
            return entropy;
        }
        public static double EntropyOfNodes(Dictionary<Node, float> pNodeList)
        {
            int n = pNodeList.Keys.Count;
            double entropy = -(from p in pNodeList group p by p.Value into g select new { groupID = g.Key, p = (double)g.Count() / n }).Sum(t => t.p * Math.Log(t.p, 2));
            //return entropy / Math.Log(n, 2); 
            return entropy;
        }
        public static double EntropyOfNodes(Dictionary<Node, int> pNodeList)
        {
            int n = pNodeList.Keys.Count;
            double entropy = -(from p in pNodeList group p by p.Value into g select new { groupID = g.Key, p = (double)g.Count() / n }).Sum(t => t.p * Math.Log(t.p, 2));
            //return entropy / Math.Log(n, 2); 
            return entropy;
        }
        /// <summary>
        /// Compute X entropy of nodes, where X is maybe centrality values of the nodes
        /// </summary>
        /// <param name="pNodeList">The node list with centrality value</param>
        /// <returns>X entropy of nodes</returns>
        public static double EntropyOfNodes(Dictionary<string, double> pNodeList)
        {
            int n = pNodeList.Keys.Count;
            double entropy = -(from p in pNodeList group p by p.Value into g select new { groupID = g.Key, p = (double)g.Count() / n }).Sum(t => t.p * Math.Log(t.p, 2));
            //return entropy / Math.Log(n, 2);
            return entropy;
        }
        

        #region Analysis Tool
        public Dictionary<int, List<KeyValuePair<Node, float>>> SelectTopNode(int nNode,Dictionary<string, float> nodeIndices)
        {
            Dictionary<int, List<KeyValuePair<Node, float>>> nodeLayers = new Dictionary<int, List<KeyValuePair<Node, float>>>();
            if (nodeIndices.Count < nNode)
                return null;
            var rankingMedian = from e in nodeIndices orderby e.Value descending select e;
            for (int i = 0; i < nNode; i++)
            {
                if (!nodeLayers.ContainsKey(i))
                    nodeLayers[i] = new List<KeyValuePair<Node, float>>();
                nodeLayers[i].Add(
                        new KeyValuePair<Node, float>(
                            this.GetNodeFromName(rankingMedian.ElementAt(i).Key),
                            rankingMedian.ElementAt(i).Value
                            )
                            );
            }
            return nodeLayers;
        }
        /// <summary>
        /// Select nodes by their total degree in the range [min.. max]
        /// </summary>
        /// <param name="minDegree">includesive minimum degree</param>
        /// <param name="maxDegree">includesive maximum degree</param>
        /// <returns></returns>
        public IEnumerable<Node> SelectNodeByDegree(int minDegree, int maxDegree)
        {
            return from e in Nodes where minDegree <= e.TotalDegree && e.TotalDegree <= maxDegree select e;
        }
        public double AverageTotalDegree
        {
            get
            {
                return (from e in Nodes select e.TotalDegree).Average();
            }
        }
        public IEnumerable<Node> SelectHighestDegreeHub()
        {
            
            return from e in Nodes where e.TotalDegree ==Nodes.Max(t => t.TotalDegree) select e;
            
        }
        public int MaxTotalDeg
        {
            get
            {

                return (from p in this.Nodes select p).Max(t => t.TotalDegree);
            }

        }
        public int MaxReaching
        {
            get
            {

                return (from p in this.Nodes select p).Max(t => t.Reaching);
            }

        }
        public IEnumerable<Node> SelectHubs(int minConnectivity=-1)
        {
            minConnectivity=(minConnectivity==-1?(int)Math.Ceiling(this.AverageTotalDegree):minConnectivity);
            return from e in Nodes where e.TotalDegree >=minConnectivity select e;

        }
        /// <summary>
        /// Select nodes that satisfy criteria
        /// 1) whose total degree in the range [minDegree, maxDegree] and 
        /// 2) Be highest ranking
        /// </summary>
        /// <param name="minDegree">The min total degree</param>
        /// <param name="maxDegree">The max total degree</param>
        /// <param name="rankingIndex">The ranking index of nodes, identified by node's name</param>
        /// <param name="isAscRankingFromTop">true: if highest ranking is the smallest value in the rankingIndex; else highest ranking is the biggest value in the rankingInde </param>
        /// <returns></returns>
        public IEnumerable<Node> SelectHighestRankingNode(int minDegree, int maxDegree, Dictionary<string,float> rankingIndex, bool isAscRankingFromTop=false)
        {
            var degreeMatchingNodes = from e in Nodes join p in rankingIndex on e.name equals p.Key where minDegree <= e.TotalDegree && e.TotalDegree <= maxDegree select new { e, p.Value };
            return from d in degreeMatchingNodes where d.Value == (isAscRankingFromTop ? degreeMatchingNodes.Min(t => t.Value) : degreeMatchingNodes.Max(t => t.Value)) select d.e;
        }
        /// <summary>
        /// Create a subnetwork that probably contains additional vertices to form a network whose pairs of no direct-connection vertices are connected by a shortest path.
        /// This network has minimum interaction to connect vertices
        /// </summary>
        /// <param name="fromNet">Source network</param>
        /// <param name="Vertices">Initial vertices</param>
        /// <returns></returns>
        public BasicNetwork CreateNewSortestConnectionSubnetwork(IEnumerable<Node> Vertices)
        {

            HashSet<string> nodeSet = new HashSet<string>();
            nodeSet.UnionWith(from e in Vertices select e.name);

            for(int i=0;i<Vertices.Count()-1;i++)
                for (int j = i + 1; j < Vertices.Count(); j++)
                {
                    IEnumerable<string> forwardPath = this.ShortestPath(Vertices.ElementAt(i).name, Vertices.ElementAt(j).name);
                    IEnumerable<string> backwardPath = this.ShortestPath(Vertices.ElementAt(j).name, Vertices.ElementAt(i).name);

                    //Use the shortest path connecting between two nodes
                    if (forwardPath != null && forwardPath.Count() > 0)
                        nodeSet.UnionWith(forwardPath);
                    if (backwardPath != null && backwardPath.Count() > 0)
                        nodeSet.UnionWith(backwardPath);
                }
            IEnumerable<Node> ns = this.SelectNode(nodeSet);
           
            return this.CreateNewExtractedSubnetwork(ns);
        }
        /// <summary>
        /// Renew random subnetwork that is exactly a copy from source network (without median additional nodes)
        /// </summary>
        /// <param name="fromNet">The source network that is extracted to create a copy</param>
        /// <param name="nSize">The size (node) of subnetwork</param>
        public BasicNetwork CreateNewRandomSubnetwork(int nSize)
        {
            Debug.Assert(nSize > 0);
            Debug.Assert(nSize <= this.Nodes.Count());
            HashSet<Node> nodes = new HashSet<Node>();
            
            while (nSize > 0)
            {
                int idx = Mathutil.NumericMath.RandomCraft.Next(this.Nodes.Count());
                if(!nodes.Contains(this.Nodes.ElementAt(idx)))
                {
                    nodes.Add(this.Nodes.ElementAt(idx));
                    nSize--;
                }
               
            }
            return this.CreateNewSortestConnectionSubnetwork(nodes);
        }

        /// <summary>
        /// Calculcate accumulative modularity of node layers whose amount is specified in the parameter
        /// </summary>
        /// <param name="nLayer">The number of layer divided from center based on Tran centrality</param>
        /// <param name="TranCentrality">Tran's centrality indies</param>
        /// <returns></returns>
        public Dictionary<int, double> ModularityOfLayers(int nLayer, Dictionary<string, float> TranCentrality)
        {
            Dictionary<int, List<KeyValuePair<Node, float>>> Layers = this.CreateNodeLayers(nLayer, TranCentrality);
            Debug.Assert(Layers != null);
            
            Dictionary<Node, int> pCluster = null;
            Dictionary<int, double> layerModularity = new Dictionary<int, double>();
            HashSet<Node> calcutingNode = new HashSet<Node>();
            for(int i=0;i<nLayer;i++)
            {
                var nodeOfLayer = from e in Layers[i] select e.Key;
                calcutingNode.UnionWith(nodeOfLayer);
                BasicNetwork Net = this.CreateNewExtractedSubnetwork(calcutingNode);

                layerModularity.Add(i, Net.modularity(ref pCluster));
            }
            return layerModularity;
        }
        /// <summary>
        /// Create layers that indexing following Tran centrality indices (lower index value => more centrality
        /// </summary>
        /// <param name="nLayer">The number of layers should be created</param>
        /// <param name="nodeIndices">Tran centrality indices</param>
        /// <returns>The layer list (maybe including the padding layer containing redudent nodes)</returns>
        public Dictionary<int, List<KeyValuePair<Node, float>>> CreateNodeLayers(int nLayer, Dictionary<string, float> nodeIndices)
        {

            Dictionary<int, double> dlayerNodeRo = new Dictionary<int, double>();

            //begin building up layer's range
            var rankingMedian = from e in nodeIndices orderby e.Value descending select e;


            int nElement = rankingMedian.Count() / nLayer;
            if (nElement == 0) return null;

            Dictionary<int, List<KeyValuePair<Node, float>>> nodeLayers = new Dictionary<int, List<KeyValuePair<Node, float>>>();
            int visitingElement = 0;
            for (int i = 0; i < nLayer; i++)
            {
                if (!nodeLayers.ContainsKey(i))
                    nodeLayers[i] = new List<KeyValuePair<Node, float>>();
                for (int j = 0; j < nElement; j++)
                {
                    nodeLayers[i].Add(
                        new KeyValuePair<Node, float>(
                            this.GetNodeFromName(rankingMedian.ElementAt(i * nElement + j).Key),
                            rankingMedian.ElementAt(i * nElement + j).Value
                            )
                            );
                    visitingElement++;
                }
            }
            int nExtraElement = rankingMedian.Count() % nLayer;
            if (nExtraElement > 0)
            {
                nExtraElement--;
                nodeLayers[nLayer] = new List<KeyValuePair<Node, float>>();
                for (; nExtraElement >= 0; nExtraElement--)
                    nodeLayers[nLayer].Add(
                        new KeyValuePair<Node, float>(
                        this.GetNodeFromName(rankingMedian.ElementAt(nLayer * nElement + nExtraElement).Key),
                        rankingMedian.ElementAt(nLayer * nElement + nExtraElement).Value
                        )
                        );
            }

            return nodeLayers;
        }
        #endregion



      
        
        #endregion
        #region Find key nodes

        public IEnumerable<Set<string>> GetKeyNodes()
        {

            IEnumerable<Pair<Set<string>, Set<string>>> Laws = (from p in Arcs
                                                                select new Pair<Set<string>, Set<string>>(
                                                                    new Set<string>(p.startNode.name),
                                                                    new Set<string>(p.endNode.name))
                                                                   );
            IEnumerable<Set<string>> Keys = Set<string>.FindSmallestKeySet(Laws);
            return Keys;
        }
        #endregion
        #region Finding strongly-connected components in a directed graph
        /// <summary>
        /// Finds cycles in a graph using Tarjan's strongly connected components algorithm.
        /// See http://en.wikipedia.org/wiki/Tarjan's_strongly_connected_components_algorithm
        /// A directed graph is called strongly connected if there is a path from each vertex in the graph to every other vertex. 
        /// In particular, this means paths in each direction; a path from a to b and also a path from b to a.
        /// </summary>
        /// <param name="excludeSingleItems">if set to <c>true</c>, nodes with no edges are excluded</param>
        /// <returns>A list of of vertice arrays (paths) that form cycles in the graph.</returns>
        public IList<Node[]> FindCycles(bool excludeSingleItems)
        {
            var indices = new Dictionary<string, int>();
            var lowlinks = new Dictionary<string, int>();
            var connected = new List<Node[]>();
            var stack = new Stack<Node>();


            foreach (var vertex in this.Nodes)
            {
                if (!indices.ContainsKey(vertex.name))
                {
                    TarjansStronglyConnectedComponentsAlgorithm(excludeSingleItems, vertex, indices, lowlinks, connected, stack, 0);
                }
            }

            return connected;
        }

        public IList<Node[]> FindCycles(bool excludeSingleItems, int edgeType)
        {
            var indices = new Dictionary<string, int>();
            var lowlinks = new Dictionary<string, int>();
            var connected = new List<Node[]>();
            var stack = new Stack<Node>();

            var edgeTypeNode = this.NodeTypeLink(edgeType);
            foreach (var vertex in this.Nodes)
            {
                if (!indices.ContainsKey(vertex.name))
                {
                    TarjansStronglyConnectedComponentsAlgorithm(excludeSingleItems, vertex, indices, lowlinks, connected, stack, 0, edgeType);
                }
            }

            return connected;
        }
        /// <summary>
        /// Executes Tarjan's algorithm on the graph.
        /// </summary>
        /// <param name="excludeSinlgeItems">if set to <c>true</c> [exclude sinlge items].</param>
        /// <param name="vertex">The vertex to start with.</param>
        /// <param name="indices">The current indices.</param>
        /// <param name="lowlinks">The current lowlinks.</param>
        /// <param name="connected">The connected components.</param>
        /// <param name="stack">The stack.</param>
        /// <param name="index">The current index.</param>
        private static void TarjansStronglyConnectedComponentsAlgorithm(
            bool excludeSinlgeItems,
            Node vertex,
            IDictionary<string, int> indices,
            IDictionary<string, int> lowlinks,
            ICollection<Node[]> connected,
            Stack<Node> stack,
            int index)
        {
            indices[vertex.name] = index;
            lowlinks[vertex.name] = index;
            index++;

            stack.Push(vertex);

            // foreach (var edge in vertex.EmanatingEdges)
            foreach (var edge in vertex.OutLink)
            {
                var next = edge.endNode;//edge.ToVertex;

                if (!indices.ContainsKey(next.name))
                {
                    TarjansStronglyConnectedComponentsAlgorithm(excludeSinlgeItems, next, indices, lowlinks, connected, stack, index);
                    lowlinks[vertex.name] = Math.Min(lowlinks[vertex.name], lowlinks[next.name]);
                }
                else if (stack.Where(p => p.name == next.name).Count() > 0)//stack.Contain(next)
                {
                    lowlinks[vertex.name] = Math.Min(lowlinks[vertex.name], lowlinks[next.name]);
                }
            }

            if (lowlinks[vertex.name] == indices[vertex.name])
            {
                Node next;
                var component = new List<Node>();

                do
                {
                    next = stack.Pop();
                    component.Add(next);

                } while (next != vertex);

                if (!excludeSinlgeItems || (component.Count > 1))
                {
                    connected.Add(component.ToArray());
                }
            }
        }


        private static void TarjansStronglyConnectedComponentsAlgorithm(
            bool excludeSinlgeItems,
            Node vertex,
            IDictionary<string, int> indices,
            IDictionary<string, int> lowlinks,
            ICollection<Node[]> connected,
            Stack<Node> stack,
            int index,
            int edgeType
            )
        {
            indices[vertex.name] = index;
            lowlinks[vertex.name] = index;
            index++;

            stack.Push(vertex);

            // foreach (var edge in vertex.EmanatingEdges)
            var vertextOutLink = vertex.OutTypeLink(edgeType);
            foreach (var edge in vertextOutLink)
            {
                var next = edge.endNode;//edge.ToVertex;

                if (!indices.ContainsKey(next.name))
                {
                    TarjansStronglyConnectedComponentsAlgorithm(excludeSinlgeItems, next, indices, lowlinks, connected, stack, index);
                    lowlinks[vertex.name] = Math.Min(lowlinks[vertex.name], lowlinks[next.name]);
                }
                else if (stack.Where(p => p.name == next.name).Count() > 0)//stack.Contain(next)
                {
                    lowlinks[vertex.name] = Math.Min(lowlinks[vertex.name], lowlinks[next.name]);
                }
            }

            if (lowlinks[vertex.name] == indices[vertex.name])
            {
                Node next;
                var component = new List<Node>();

                do
                {
                    next = stack.Pop();
                    component.Add(next);

                } while (next != vertex);

                if (!excludeSinlgeItems || (component.Count > 1))
                {
                    connected.Add(component.ToArray());
                }
            }
        }
        #endregion
        #region Traversal of network


        /// <summary>
        /// Determines whether this graph is cyclic (contains cycles).
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance contains cycles; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>The topological sort algorithm is only valid for a directed, acyclic (cycle free) graph.</remarks>
        /// <remarks>In order to detect cycles, a topological sort of the graph is computed.</remarks>
        /// <exception cref="InvalidOperationException">The graph contains cycles.</exception>
        /// <example>
        /// <code source="..\..\Source\Examples\ExampleLibraryCSharp\DataStructures\General\GraphExamples.cs" region="IsCyclic" lang="cs" title="The following example shows how to use the IsCyclic method."/>
        /// <code source="..\..\Source\Examples\ExampleLibraryVB\DataStructures\General\GraphExamples.vb" region="IsCyclic" lang="vbnet" title="The following example shows how to use the IsCyclic method."/>
        /// </example>
        public bool IsCyclic()
        {
            var visitor = new DummyVisitor<Node>();

            var count = TopologicalSortTraversalInternal(visitor);

            // If the visitor has not visited each and every vertex in the 
            // graph, it has cycles in it.
            return count < this.Nodes.Count();
        }

       

        /// <summary>
        /// Computes the topological sort of the graph.
        /// </summary>
        /// <remarks>This operation is only defined on a directed graph.</remarks>
        /// <remarks>The topological sort algorithm is only valid for a directed, acyclic (cycle free) graph.</remarks>
        /// <returns>A list of vertices in topological order.</returns>
        /// <exception cref="InvalidOperationException">The graph contains cycles.</exception>
        /// <exception cref="ArgumentException">The graph is not directed.</exception>
        /// <example>
        /// <code source="..\..\Source\Examples\ExampleLibraryCSharp\DataStructures\General\GraphExamples.cs" region="TopologicalSort" lang="cs" title="The following example shows how to use the TopologicalSort method."/>
        /// <code source="..\..\Source\Examples\ExampleLibraryVB\DataStructures\General\GraphExamples.vb" region="TopologicalSort" lang="vbnet" title="The following example shows how to use the TopologicalSort method."/>
        /// </example>
        public IList<Node> TopologicalSort()
        {
            var visitor = new TrackingVisitor<Node>();
            TopologicalSortTraversal(visitor);

            return visitor.TrackingList;
        }

        public IList<Node> TopologicalSort(int edgeType)
        {
            var visitor = new TrackingVisitor<Node>();
            TopologicalSortTraversal(visitor, edgeType);

            return visitor.TrackingList;
        }


        /// <summary>
        /// Visits very vertex in the graph (provided it doesn't have cycles) in topological order.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <exception cref="ArgumentException">The graph is not directed.</exception>
        /// <remarks>The topological sort algorithm is only valid for a directed, acyclic (cycle free) graph.</remarks>
        /// <exception cref="InvalidOperationException">The graph contains cycles.</exception>
        /// <example>
        /// <code source="..\..\Source\Examples\ExampleLibraryCSharp\DataStructures\General\GraphExamples.cs" region="TopologicalSortTraversal" lang="cs" title="The following example shows how to use the TopologicalSortTraversal method."/>
        /// <code source="..\..\Source\Examples\ExampleLibraryVB\DataStructures\General\GraphExamples.vb" region="TopologicalSortTraversal" lang="vbnet" title="The following example shows how to use the TopologicalSortTraversal method."/>
        /// </example>
        public void TopologicalSortTraversal(IVisitor<Node> visitor)
        {
            var count = TopologicalSortTraversalInternal(visitor);

            if (count < this.Nodes.Count())
            {
                throw new InvalidOperationException("A cycle was found in the graph.");
            }
        }
        public IEnumerable<Node> NodeTypeLink(int edgeType)
        {
            return from p in this.Nodes where p.Arcs.Where(e => e.Type == edgeType).Count() > 0 select p;
        }
        public void TopologicalSortTraversal(IVisitor<Node> visitor, int edgeType)
        {
            var count = TopologicalSortTraversalInternal(visitor, edgeType);

            if (count < this.NodeTypeLink(edgeType).Count())
            {
                throw new InvalidOperationException("A cycle was found in the graph.");
            }
        }
       

        private int TopologicalSortTraversalInternal(IVisitor<Node> visitor, int edgeType)
        {
            #region Validation

            Uti.ArgumentNotNull(visitor, "visitor");

           

            #endregion

            var visitCount = 0;
            var edgeTypeNode = from p in this.Nodes where p.Arcs.Where(e => e.Type == edgeType).Count() > 0 select p;

            if (edgeTypeNode.Count()>0)
            {
                var depth = new Dictionary<string, int>(edgeTypeNode.Count());

                // Create a new queue to store the vertices to visit.
                var queue = new Queue<Node>();

                foreach (var vertex in edgeTypeNode)
                {
                    var incomingTypeCount = vertex.IncomingTypeEdgeCount(edgeType);//.IncomingEdgeCount;
                    depth.Add(vertex.name, incomingTypeCount);

                    // Enqueue those with depth 0
                    if (incomingTypeCount == 0)
                    {
                        queue.Enqueue(vertex);
                    }
                }

                // If no vertices are found with incoming edge count 0, the graph is cyclic,
                // and we don't visit any vertices
                if (queue.Count > 0)
                {
                    while ((queue.Count > 0) && (!visitor.HasCompleted))
                    {
                        var vertex = queue.Dequeue();
                        depth.Remove(vertex.name);

                        // Visit the vertex in the topological sort order
                        visitor.Visit(vertex);

                        // Keep track of the amount of vertices we visit,
                        // so we can know if the graph has cycles in it or not.
                        visitCount++;

                        // Enumerate through all the edges emanating from this node,
                        // decreasing the depth of the vertex (thereby "removing" it
                        // from the graph, and enqueue all those with depth 0.  The
                        // effect is an ordering by incoming edge counts.
                        foreach (var edge in vertex.OutTypeLink(edgeType) )
                        {
                            var partnerVertex = edge.endNode;

                            if (depth.ContainsKey(partnerVertex.name))
                            {
                                depth[partnerVertex.name]--;

                                if (depth[partnerVertex.name] == 0)
                                {
                                    queue.Enqueue(partnerVertex);
                                }
                            }
                        }
                    }
                }
            }

            return visitCount;
        }

        /// <summary>
        /// Allows a visitor to visit each vertex in topological order.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <returns>The number of items visited.</returns>
        private int TopologicalSortTraversalInternal(IVisitor<Node> visitor)
        {
            #region Validation

            Uti.ArgumentNotNull(visitor, "visitor");

            

            #endregion

            var visitCount = 0;

            if (!IsEmpty)
            {
                var depth = new Dictionary<string, int>(Nodes.Count());

                // Create a new queue to store the vertices to visit.
                var queue = new Queue<Node>();

                foreach (var vertex in Nodes)
                {
                    var incomingCount = vertex.IncomingEdgeCount;//.IncomingEdgeCount;
                    depth.Add(vertex.name, incomingCount);

                    // Enqueue those with depth 0
                    if (incomingCount == 0)
                    {
                        queue.Enqueue(vertex);
                    }
                }

                // If no vertices are found with incoming edge count 0, the graph is cyclic,
                // and we don't visit any vertices
                if (queue.Count > 0)
                {
                    while ((queue.Count > 0) && (!visitor.HasCompleted))
                    {
                        var vertex = queue.Dequeue();
                        depth.Remove(vertex.name);

                        // Visit the vertex in the topological sort order
                        visitor.Visit(vertex);

                        // Keep track of the amount of vertices we visit,
                        // so we can know if the graph has cycles in it or not.
                        visitCount++;

                        // Enumerate through all the edges emanating from this node,
                        // decreasing the depth of the vertex (thereby "removing" it
                        // from the graph, and enqueue all those with depth 0.  The
                        // effect is an ordering by incoming edge counts.
                        foreach (var edge in vertex.OutLink)
                        {
                            var partnerVertex = edge.endNode;

                            if (depth.ContainsKey(partnerVertex.name))
                            {
                                depth[partnerVertex.name]--;

                                if (depth[partnerVertex.name] == 0)
                                {
                                    queue.Enqueue(partnerVertex);
                                }
                            }
                        }
                    }
                }
            }

            return visitCount;
        }
        /// <summary>
        /// Performs a depth-first traversal, starting at the specified vertex.
        /// </summary>
        /// <param name="visitor">The visitor to use.  In-order traversal is not applicable in a graph.</param>
        /// <param name="startVertex">The vertex to start from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is a null reference (<c>Nothing</c> in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="startVertex"/> is a null reference (<c>Nothing</c> in Visual Basic).</exception>
        /// <example>
        /// <code source="..\..\Source\Examples\ExampleLibraryCSharp\DataStructures\General\GraphExamples.cs" region="DepthFirstTraversal" lang="cs" title="The following example shows how to use the DepthFirstTraversal method."/>
        /// <code source="..\..\Source\Examples\ExampleLibraryVB\DataStructures\General\GraphExamples.vb" region="DepthFirstTraversal" lang="vbnet" title="The following example shows how to use the DepthFirstTraversal method."/>
        /// </example>
        public IEnumerable<Node> DepthFirstTraversal(OrderedVisitor<Node> visitor, Node startVertex)
        {
            Uti.ArgumentNotNull(visitor, "visitor");
            Uti.ArgumentNotNull(startVertex, "startVertex");

            Dictionary<string, Node> visitedVertices = new Dictionary<string, Node>();

            DepthFirstTraversal(visitor, null, startVertex, ref visitedVertices);
            return visitedVertices.Values;
        }

        /// <summary>
        /// Performs a depth-first traversal.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="startVertex">The start vertex.</param>
        /// <param name="visitedVertices">The visited vertices.</param>
        private static void DepthFirstTraversal(OrderedVisitor<Node> visitor, Node parent, Node startVertex, ref Dictionary<string, Node> visitedVertices)
        {
            if (visitor.HasCompleted)
            {
                return;
            }

            // Add the vertex to the "visited" list
            visitedVertices.Add(startVertex.name, startVertex);

            // Visit the vertex in pre-order
            if (!visitor.VisitPreOrder(parent, startVertex))
                return;

            // Get the list of emanating edges from the vertex
            var edges = startVertex.OutUnLink;

            for (var i = 0; i < edges.Count(); i++)
            {
                // Get the partner vertex of the start vertex
                var vertexToVisit = edges.ElementAt(i).GetPartnerVertex(startVertex);

                // If the vertex hasn't been visited before, do a depth-first
                // traversal starting at that vertex
                if (!visitedVertices.ContainsKey(vertexToVisit.name))
                {
                    DepthFirstTraversal(visitor, startVertex, vertexToVisit, ref visitedVertices);
                }
            }

            // Visit the vertex in post order
            if (!visitor.VisitPostOrder(parent, startVertex))
                return;
        }
        public IEnumerable<Node> DepthFirstTraversalStack(OrderedVisitor<Node> visitor, OrderedVisitor<Node>.OrderType orderType, Node startVertex)
        {
            Dictionary<string, Node> visitedVertices = new Dictionary<string, Node>();

            Stack<Node> S = new Stack<Node>();
            Stack<Node> Parent = new Stack<Node>();
            S.Push(startVertex);
            Parent.Push(null);

            while(S.Count>0)
            {
                if (orderType == OrderedVisitor<Node>.OrderType.PreOrder)
                {
                    startVertex = S.Pop();
                    Node parent = Parent.Pop();
                    visitedVertices.Add(startVertex.name, startVertex);
                    // Visit the vertex in pre-order
                    if (!visitor.VisitPreOrder(parent, startVertex))
                        return visitedVertices.Values;
                }

                // Get the list of emanating edges from the vertex
                var edges = startVertex.OutUnLink;

                for (var i = 0; i < edges.Count(); i++)
                {
                    // Get the partner vertex of the start vertex
                    var vertexToVisit = edges.ElementAt(i).GetPartnerVertex(startVertex);
                    if (!visitedVertices.ContainsKey(vertexToVisit.name))
                    {
                        S.Push(vertexToVisit);
                        Parent.Push(startVertex);
                    }
                }
                if (orderType == OrderedVisitor<Node>.OrderType.PostOrder)
                {
                    startVertex = S.Pop();
                    Node parent = Parent.Pop();
                    visitedVertices.Add(startVertex.name, startVertex);
                    // Visit the vertex in post order
                    if (!visitor.VisitPostOrder(parent, startVertex))
                        return visitedVertices.Values;
                }
            }
            return visitedVertices.Values;
        }
        /// <summary>
        /// Performs a breadth-first traversal from the specified vertex.
        /// </summary>
        /// <param name="visitor">The visitor to use.</param>
        /// <param name="startVertex">The vertex to start from.</param>
        /// <returns>The list of visited vertices</returns>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is a null reference (<c>Nothing</c> in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="startVertex"/> is a null reference (<c>Nothing</c> in Visual Basic).</exception>
        /// <example>
        /// <code source="http://code.google.com/p/ngenerics/source/browse/trunk/Source/NGenerics" lang="cs" title="The following example shows how to use the BreadthFirstTraversal method."/>
        /// <code source="http://code.google.com/p/ngenerics/source/browse/trunk/Source/NGenerics" region="BreadthFirstTraversal" lang="vbnet" title="The following example shows how to use the BreadthFirstTraversal method."/>
        /// </example>
        public static IEnumerable<Node> BreadthFirstTraversal(IVisitor<Node> visitor, Node startVertex)
        {

            Uti.ArgumentNotNull(visitor, "visitor");
            Uti.ArgumentNotNull(startVertex, "startVertex");

            Dictionary<string, Node> visitedVertices = new Dictionary<string, Node>();

            var visitableQueue = new Queue<Node>();

            visitableQueue.Enqueue(startVertex);
            visitedVertices.Add(startVertex.name, startVertex);

            while (!((visitableQueue.Count == 0) || (visitor.HasCompleted)))
            {
                var vertex = visitableQueue.Dequeue();

                visitor.Visit(vertex);

                var edges =vertex.OutUnLink;

                for (var i = 0; i < edges.Count(); i++)
                {
                    var vertexToVisit = edges.ElementAt(i).GetPartnerVertex(vertex);

                    if (!visitedVertices.ContainsKey(vertexToVisit.name))
                    {
                        visitableQueue.Enqueue(vertexToVisit);
                        visitedVertices.Add(vertexToVisit.name, vertexToVisit);
                    }
                }
            }
            return visitedVertices.Values;
        }
        public static IEnumerable<Node> BreadthFirstTraversal(Node startVertex)
        {

            Uti.ArgumentNotNull(startVertex, "startVertex");

            Dictionary<string, Node> visitedVertices = new Dictionary<string, Node>();

            var visitableQueue = new Queue<Node>();

            visitableQueue.Enqueue(startVertex);
            visitedVertices.Add(startVertex.name, startVertex);

            while (!((visitableQueue.Count == 0)))
            {
                var vertex = visitableQueue.Dequeue();

                var edges = vertex.OutUnLink;

                for (var i = 0; i < edges.Count(); i++)
                {
                    var vertexToVisit = edges.ElementAt(i).GetPartnerVertex(vertex);

                    if (!visitedVertices.ContainsKey(vertexToVisit.name))
                    {
                        visitableQueue.Enqueue(vertexToVisit);
                        visitedVertices.Add(vertexToVisit.name, vertexToVisit);
                    }
                }
            }
            return visitedVertices.Values;
        }

        
        /// <summary>
        /// Performs a breadth-first traversal from the specified vertex.
        /// </summary>
        /// <param name="startVertex">The vertex to start from.</param>
        /// <param name="edgeType">The type of edge to move</param>
        /// <returns>The list of visited vertices</returns>
        public static IEnumerable<Node> BreadthFirstTraversal(Node startVertex, int edgeType)
        {
            
            Uti.ArgumentNotNull(startVertex, "startVertex");

            Dictionary<string, Node> visitedVertices = new Dictionary<string, Node>();

            var visitableQueue = new Queue<Node>();

            visitableQueue.Enqueue(startVertex);
            visitedVertices.Add(startVertex.name, startVertex);

            while (!(visitableQueue.Count == 0))
            {
                var vertex = visitableQueue.Dequeue();

                var edges =vertex.OutUnTypeLink(edgeType);
                //Netutil.DumpInteraction(edges.ToArray());

                for (var i = 0; i < edges.Count(); i++)
                {
                    var vertexToVisit = edges.ElementAt(i).GetPartnerVertex(vertex);

                    if (!visitedVertices.ContainsKey(vertexToVisit.name))
                    {
                        visitableQueue.Enqueue(vertexToVisit);
                        visitedVertices.Add(vertexToVisit.name,vertexToVisit);
                    }
                }
            }
            return visitedVertices.Values;
        }
        public Node GetRndNode()
        {
            return this.Nodes.ElementAt(NumericMath.RandomCraft.Next(this.Nodes.Count()));
        }
        public static bool CheckDegreePreserving(BasicNetwork Org, BasicNetwork New)
        {
            if (Org.Nodes.Count() == New.Nodes.Count() && Org.Arcs.Count() == New.Arcs.Count())
            {
                foreach (Node n in Org.Nodes)
                {
                    if (!(n.TotalDegree == New[n.name].TotalDegree &&
                        n.InDegree == New[n.name].InDegree &&
                        n.OutDegree == New[n.name].OutDegree))
                        return false;

                }
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Create a new random network from this network by shuffling the edges of the network while preserving the degree
        /// NOTE: If wrong shuffling network. i.e., duplicating network (more than two links between A-> B, A -> B), then the relationship between Modularity & Robustness is wrong (>0)
        /// Ref source code: http://chianti.ucsd.edu/svn/csplugins/trunk/soc/pjmcswee/src/cytoscape/randomnetwork/DegreePreservingNetworkRandomizer.java
        /// Algorithm: http://www.cmth.bnl.gov/~maslov/matlab.htm
        /// </summary>
        /// <param name="pShuffles">The number of shuffling arcs</param>
        /// <param name="rewiringArc">true for arcs; otherwise for edges</param>
        /// <returns>The random network whose arcs/edges are shuffled from this network</returns>
        public BasicNetwork ShufflePreservingDegree(int pShuffles, bool rewiringArc=true)
        {

            BasicNetwork newGraph = this.Clone() as BasicNetwork;
            
            if (newGraph.Arcs.Count() != this.Arcs.Count() && newGraph.Nodes.Count() != this.Nodes.Count())
            {
                User.One.MessageToUser("Clone network is error!");
                throw new Exception("Clone network is error!");
            }

            //Get an iterator for the nodes
            int N = newGraph.Nodes.Count();


            if (N <= 1)
                throw new Exception("Node size has to be at least 2!");

            // select nodes that have at least 1 neighbor
            var connectedNodes = from p in newGraph.Nodes where p.TotalDegree > 0 select p;
            if (connectedNodes.Count() < 4)
            {
                throw new Exception("Network must have at least 4 connected nodes to shuffle!");
            }

            for (int e = 0; e < pShuffles; e++)
            {
                //Variables to hold onto two edges: A, B
                Node A = null, B = null;//, sourceA = null, sourceB = null;
                Node targetA = null, targetB = null;
                Interaction edge1 = null, edge2 = null;
                //Iterate until we find two suitable edges
                bool done = false;
                while (!done)
                {

                    //Choose two random nodes
                    do
                    {

                        A = connectedNodes.ElementAt(Mathutil.NumericMath.RandomCraft.Next(connectedNodes.Count()));
                        B = connectedNodes.ElementAt(Mathutil.NumericMath.RandomCraft.Next(connectedNodes.Count()));
                    } while (A == B);

                    //Get their connection information
                    IEnumerable<Interaction> linkA = rewiringArc?A.Arcs:A.Edges;
                    IEnumerable<Interaction> linkB = rewiringArc?B.Arcs:B.Edges;

                    ///See what their degrees are
                    //int aDegree = linkA.Count();
                    //int bDegree = linkB.Count();

                    //Choosen two random neighbors from these nodes

                    //Find Target As so that Target A is different source B
                    var pedge1 = from p in linkA 
                                 where p.GetPartnerVertex(A) != B 
                                 select p;
                    if (pedge1.Count() == 0) continue;
                    edge1 = pedge1.ElementAt(Mathutil.NumericMath.RandomCraft.Next(pedge1.Count()));
                    

                    //Find Target Bs so that Target A is different source A
                    var pedge2 = from p in linkB
                                 where p.GetPartnerVertex(B) != A 
                                 select p;
                    if (pedge2.Count() == 0) continue;
                    edge2 = pedge2.ElementAt(Mathutil.NumericMath.RandomCraft.Next(pedge2.Count()));

                    A = edge1.startNode; targetA=edge1.endNode;
                    B = edge2.startNode; targetB = edge2.endNode;
                    
                    //Make sure the targets do not match with each other, or their alternate sources
                    if (A==B||targetA == targetB||A==targetB||B==targetA)
                    {
                        continue;
                    }

                    //check if there is any existing arcs between (sourceA -> targetB) or (sourceB -> targetA) to avoid the same DUPLICATE direction arcs between node pairs?
                    if (rewiringArc)
                    {
                        if (A.hasLinkTo(targetB))
                            continue;
                        if (B.hasLinkTo(targetA))
                            continue;
                    }
                    else
                    {
                        if (A.hasNeighbor(targetB))
                            continue;

                        if (B.hasNeighbor(targetA))
                            continue;
                    }

                    //If we got this far then we are done
                    done = true;
                }

                if (done)
                {

                    if (rewiringArc)
                    {
                        //Remove these two edges
                        newGraph.RemoveArc(edge1);
                        newGraph.RemoveArc(edge2);

                        //Create the two new edges
                        newGraph.AddArc(new Interaction(A, targetB, edge1.Type,edge1.Name, edge1.weight, edge1.Direction));
                        newGraph.AddArc(new Interaction(B, targetA, edge2.Type,edge2.Name, edge2.weight, edge2.Direction));
                       
                    }
                    else
                    {
                        newGraph.RewireEdge(A, targetA, targetB);
                        newGraph.RewireEdge(B, targetB, targetA);
                    }

                 
                  
                }

            }
            return newGraph;
        }

        
        /// <summary>
        /// Rewire an edge (all arcs) connected between this and end to the Target node
        /// </summary>
        /// <param name="startA">Start node that rewiring arcs points from </param>
        /// <param name="endA">End node that rewiring arcs pointed to</param>
        /// <param name="targeB">Target node that rewiring arcs will point to</param>
        /// <returns></returns>
        public void RewireEdge(Node startA, Node endA, Node targeB)
        {
            IEnumerable<Interaction> edgeA = from p in startA.Arcs where p.GetPartnerVertex(startA) == endA select p;

            HashSet<Interaction> startAtargetBs = new HashSet<Interaction>();
            for(int i=0;i<edgeA.Count();i++)
            {
                Interaction e = edgeA.ElementAt(i);
                if (e.startNode == startA)
                    startAtargetBs.Add(new Interaction(startA, targeB, e.Type, e.Name, e.weight, e.Direction));
                else
                    startAtargetBs.Add(new Interaction(targeB, startA, e.Type, e.Name,e.weight, e.Direction));
                
            }
            this.RemoveEdge(startA, endA);

            this.AddArc(startAtargetBs.ToArray());
        }
        /// <summary>
        /// Write the network into a file
        /// </summary>
        /// <param name="filename">The file to save the network's data</param>
        public void WriteToFile(string filename)
        {
            TextDB.WriteTextFile(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", "Start", "Direction", "End", "Interaction", "Weight", "Name"), filename);
            foreach (Interaction inter in this.Arcs)
            {

                TextDB.WriteTextFile(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", inter.startNode.name, (inter.Direction == Interaction.DirectionType.undirected ? 0 : 1), inter.endNode.name,
                     inter.Type, inter.weight, inter.Name), filename);
            }
            IEnumerable<Node> isolateNodes = this.IsolateNodes;
            if (isolateNodes.Count() > 0)
            {
                
                foreach (Node n in isolateNodes)
                {
                    TextDB.WriteTextFile(string.Format("{0}", n.name), filename);
                }
            }
        }
       
        public void ReadFromXML(string filename)
        {
            XmlDataDocument xmldoc = new XmlDataDocument();
            XmlNodeList xmlnode;
            int i = 0;
            
            string fileName = Netutil.InPutDirector+"\\"+filename;
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            xmldoc.Load(fs);
            xmlnode = xmldoc.GetElementsByTagName("entry");
            for (i = 0; i <= xmlnode.Count - 1; i++)
            {
                string id =xmlnode[i].Attributes["id"].Value;
                this.AddNode(id);
                Debug.Write(string.Format("\"{0}\"\t", id));
               
            }

            xmlnode = xmldoc.GetElementsByTagName("relation");
            for (i = 0; i <= xmlnode.Count - 1; i++)
            {
                string start = xmlnode[i].Attributes["entry2"].Value;
                string end = xmlnode[i].Attributes["entry1"].Value;
                Node nstart = this[start];
                Node nend=this[end];
                string stype=xmlnode[i].Attributes["type"].Value;
                string sname = xmlnode[i].Attributes["name"].Value;
                int type=0;
                switch(stype)
                {
                    case "activation":
                        type=1;
                        break;
                    default:
                        type =0;
                        break;

                }

                this.AddArc(new Interaction(nstart, nend, type, sname, 1, type == 0 ? Interaction.DirectionType.undirected : Interaction.DirectionType.directed));

                Debug.WriteLine(string.Format("\"{0}\" --> \"{1}\" ", start,end));
            }
            fs.Close();
            
        }
        public void WriteToExcelfile(string filename)
        {
            TextDB.WriteTextFile(string.Format("{0}\t{1}\t{2}", "Start", "Interaction", "End"), filename);
            ExcelDB exFile = new ExcelDB();

            try
            {
                for(int i=0;i<this.Arcs.Count();i++)
                {
                    Interaction inter = this.Arcs.ElementAt(i);
                    exFile.WriteRow(new object[] { inter.startNode.name, inter.Type, inter.endNode.name });
                }
                IEnumerable<Node> isolateNodes = this.IsolateNodes;
                if (isolateNodes.Count() > 0)
                {

                    foreach (Node n in isolateNodes)
                    {
                        exFile.WriteRow(new object[]{string.Format("{0}", n.name)});
                    }
                }
                exFile.SaveToFile(filename);
            }
            finally
            {
                exFile.Dispose();
            }
        }
        public IEnumerable<Interaction> DuplicatedArcs
        {
            get
            {
                var acs= from p in this.ArcDictionary where p.Value.Count() > 1 select p.Value;
                foreach (var ac in acs)
                    foreach(var a in ac)
                    yield return a;
            }
        }
        
        /// <summary>
        /// Remove duplicate arcs in the network so that, 
        /// among duplicate arcs, directed type arcs are removed first and an undirected type arc is kept if existent
        /// </summary>
        /// <param name="plusWeight">= true: the weight of the remained arc is modified so that it is total of all weights of all links; otherwise the weight of the remained arc is not changed</param>
        public void RemoveDuplicatedArcs(bool plusWeight = true)
        {
            // Directed arcs
            var DirectedArcDictionary = from e in Arcs
                                        where e.Direction == Interaction.DirectionType.directed
                                        group e by BasicNetwork.ArcKey(e) into g
                                        where g.Count() > 1
                                        select g;
             

            // Undirected arcs
            var UndirectedArcDictionary = from e in Arcs
                                          where e.Direction == Interaction.DirectionType.undirected
                                          group e by BasicNetwork.EdgeKey(e) into g
                                          where g.Count() > 1
                                          select g;
            double dweight = 0;
            double uweight = 0;
            foreach (var e in DirectedArcDictionary)
            {
                //Remove directed links first
                dweight = 0;
                for (int i = 1; i < e.Count();i++ )
                {
                    dweight += e.ElementAt(i).weight;
                    this.RemoveArc(e.ElementAt(i));

                }
                if (plusWeight)
                    e.ElementAt(0).weight += dweight;
                
            }
            foreach (var e in UndirectedArcDictionary)
            {
                //The remove undirected links
                uweight = 0;
                for (int i = 1; i < e.Count(); i++)
                {
                    uweight += e.ElementAt(i).weight;
                    this.RemoveArc(e.ElementAt(i));
                }
                if (plusWeight)
                    e.ElementAt(0).weight += uweight;
            }
        }
        //public void RemoveDuplicatedArcs(bool plusWeight=true)
        //{
        //    var keys = from p in this.ArcDictionary where p.Value.Count() > 1 select p.Key;
        //    foreach (var k in keys)
        //    {
        //        double weight = 0;
        //        //Removed directed links first, keep an undirected link
        //        var sortedList = from p in ArcDictionary[k] orderby p.Direction descending select p;
        //        while (sortedList.Count() > 1)
        //        {
        //            weight += sortedList.ElementAt(0).weight;
        //            this.RemoveArc(sortedList.ElementAt(0));
        //        }
        //        if (plusWeight)
        //            sortedList.ElementAt(0).weight += weight;
        //    }
        //}
        private bool IsMatch(Node startVertex, string endVertex)
        {
            return startVertex.name == endVertex;
        }
        /// <summary>
        /// Check whether to exist a Directed path from start to end
        /// </summary>
        /// <param name="startVertex">The start starting the finding</param>
        /// <param name="endVertex">The goal or end node for find</param>
        /// <param name="interaction">The type of path to find</param>
        /// <returns>true if existing a path otherwise return false</returns>
        public bool hasDirectedPath(Node startVertex, string endVertex, InteractionType interaction)
        {
            if(IsMatch(startVertex,endVertex)) return true;

            Uti.ArgumentNotNull(startVertex, "startVertex");

            Dictionary<string, Node> visitedVertices = new Dictionary<string, Node>();

            var visitableQueue = new Queue<Node>();
            
            visitableQueue.Enqueue(startVertex);
            visitedVertices.Add(startVertex.name,startVertex);

            while (!(visitableQueue.Count == 0) )
            {
                var vertex = visitableQueue.Dequeue();

                //Start visit here
                if(IsMatch(vertex,endVertex)) return true;
                //End visit

                var edges = vertex.OutTypeLink(interaction);

                for (var i = 0; i < edges.Count(); i++)
                {
                    var vertexToVisit = edges.ElementAt(i).GetPartnerVertex(vertex);

                    if (!visitedVertices.ContainsKey(vertexToVisit.name))
                    {
                        visitableQueue.Enqueue(vertexToVisit);
                        visitedVertices.Add(vertexToVisit.name, vertexToVisit);
                    }
                }
            }
            return false;
        }
        
        /// <summary>
        /// Check whether to exist a Undirected path from start to end
        /// <param name="startVertex">The start starting the finding</param>
        /// <param name="endVertex">The goal or end node for find</param>
        /// <param name="interaction">The type of path to find</param>
        /// <returns>true if existing a path otherwise return false</returns>
        public bool hasUndirectedConnection(Node startVertex, string endVertex, InteractionType interaction)
        {
            if (IsMatch(startVertex, endVertex)) return true;

            Uti.ArgumentNotNull(startVertex, "startVertex");

            Dictionary<string, Node> visitedVertices = new Dictionary<string, Node>();

            var visitableQueue = new Queue<Node>();

            visitableQueue.Enqueue(startVertex);
            visitedVertices.Add(startVertex.name, startVertex);

            while (!(visitableQueue.Count == 0))
            {
                var vertex = visitableQueue.Dequeue();

                //Start visit here
                if (IsMatch(vertex, endVertex)) return true;
                //End visit

                var edges = vertex.EdgeTypeLink(interaction);

                for (var i = 0; i < edges.Count(); i++)
                {
                    var vertexToVisit = edges.ElementAt(i).GetPartnerVertex(vertex);

                    if (!visitedVertices.ContainsKey(vertexToVisit.name))
                    {
                        visitableQueue.Enqueue(vertexToVisit);
                        visitedVertices.Add(vertexToVisit.name, vertexToVisit);
                    }
                }
            }
            return false;
        }
        #endregion
        #region Network decomposition
        /// <summary>
        /// Find k-shell decomposition of a network
        /// In undirected network, the nodes with the highest k-shell index are inﬂuential spreaders in complex undirected networks (see "Identiﬁcation of inﬂuential spreaders in complex networks" in http://www.sciencedirect.com/science/article/pii/S0378437113010406)
        /// </summary>
        /// <returns>k-shell indice and their lists of nodes from the network corresponding the k-shell index</returns>
        public Dictionary<Node,int> K_ShellCentrality()
        {
            BasicNetwork Net = this.Clone() as BasicNetwork;
            Dictionary<Node, int> kshellNet = new Dictionary<Node, int>();

            int maxDeg = Net.MaxTotalDeg;
            for (int k = 1; k <= maxDeg; k++)
            {
                
                var iDegreeNode = (from p in Net.Nodes where p.TotalDegree <= k select p);
                while (iDegreeNode.Count() > 0)
                {
                    Node[] idegNode=iDegreeNode.ToArray();
                    foreach (Node n in idegNode)
                        kshellNet[this[n.id]] = k;

                    Net.RemoveNodeAndArc(idegNode); // removing all nodes and their arcs will maybe make that iDegreeNode has other nodes
                }

                if (Net.Nodes.Count() == 0)// stop if having no node anymore
                    break;
            }
            return kshellNet;
        }
        /// <summary>
        /// Find R-shell (Reaching shell) decomposition of a directed network.
        /// </summary>
        /// <returns>R-shell indice and their lists of nodes from the network corresponding the R-shell index</returns>
        public Dictionary<Node, int> R_ShellCentrality()
        {
            BasicNetwork Net = this.Clone() as BasicNetwork;
            Dictionary<Node, int> kreachingNet = new Dictionary<Node, int>();

            int maxReaching = Net.MaxReaching;
            //for (int k = 0; k <= maxReaching; k++)

            for (int k = 1; k <= maxReaching; k++)
            {

                var iReachingNode = (from p in Net.Nodes where p.Reaching <= k select p);
                while (iReachingNode.Count() > 0)
                {
                    Node[] idegNode = iReachingNode.ToArray();
                    foreach (Node n in idegNode)
                        kreachingNet[this[n.id]] = k;

                    Net.RemoveNodeAndArc(idegNode); // removing all nodes and their arcs will maybe make that iDegreeNode has other nodes
                }

                if (Net.Nodes.Count() == 0)// stop if having no node anymore
                    break;
            }
            return kreachingNet;
        }
        /// <summary>
        /// The coreness centrality for undirected unweighted network for "Identifying and ranking influential spreaders in complex networks by neighborhood coreness", published by Joonhyun Bae, 2013
        /// This is an extension of K-shell centrality
        /// </summary>
        /// <returns>Nodes and their coreness indies</returns>
        public Dictionary<Node, int> K_CorenessCentrality()
        {
            Dictionary<Node, int> kshellIdx = this.K_ShellCentrality();
            Dictionary<Node, int> corenessIdx = new Dictionary<Node, int>();
            int ks = 0;
            foreach (Node n in this.Nodes)
            {
                ks = 0;
                foreach (Node e in n.Neighbours)
                {
                    ks += KshellOfneighbor(kshellIdx, e);
                }
                corenessIdx[n] = ks;
            }
            return corenessIdx;
        }
        private int KshellOfneighbor(Dictionary<Node, int> kshell, Node node)
        {
            int ks = 0;
            foreach (Node n in node.Neighbours)
                ks += kshell[n];
            return ks;

        }
        #endregion
        #region file reading
        /// <summary>
        /// Decomposing a pathway map into a network
        /// For each interaction in the form S -> E where S and E are two group of genes respectively: {s1, s2, ... sn}, {e1, e2, ... en} 
        //  1) The interaction S -> E is decomposed into sub-interactions   (s1 -> e1, s1 -> e2, ... s1 -> en), 
        //  (s2 -> e1, s2 -> e2, ... s2 -> en), (sn -> e1, sn -> e2, ... sn -> en) 
        //  where type of S and E is gene or group
        //  2) S and E are decomposed into PPIs interactions, respectively: (s1 --s2, s1 -- sn, s2 -- sn), (e1 -- e2, e1 -- en, e2 -- en) 
        //  where type of S and E is not "group" or "map"
        /// </summary>
        /// <param name="Net">A fresh network for importing interactions and nodes</param>
        /// <param name="filename">The name of Kegg Xml file</param>
        /// <param name="parsingOpt">Parsing option: 0) decomposing into group level, keeping multiple gene/group nodes 1) decomposing into gene level </param>
        public static void ReadNetworkFromKeggXML(BasicNetwork Net, string filename, int parsingOpt=1)
        {
            
            XmlDocument doc = new XmlDocument();
            filename=Netutil.GetFullInputFileName(filename);
            doc.Load(filename);
            
         
            Dictionary<int, string> geneDict = new Dictionary<int, string>();
            Dictionary<int, string> mapDict = new Dictionary<int, string>();
            Dictionary<int, List<int>> groupDict = new Dictionary<int, List<int>>();

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                string text = node.InnerText; //or loop through its children as well
                //or read an attribute
                switch (node.Name)
                {
                    case "entry": // Node id
                        switch(node.Attributes["type"].Value)// type of node
                        {
                            case "gene":// the node as a single gene or multiple genes (homologous genes) in the name
                                geneDict.Add(Convert.ToInt32(node.Attributes["id"].Value), node.Attributes["name"].Value);
                                break;
                            case "map":// the node as a pathway
                                mapDict.Add(Convert.ToInt32(node.Attributes["id"].Value), node.Attributes["name"].Value);
                                break;
                            case "group":// the node as a group of nodes indicated in this children with Name = component
                                int id = Convert.ToInt32(node.Attributes["id"].Value);
                                groupDict.Add(id, new List<int>());
                                foreach (XmlNode child in node.ChildNodes)
                                    if (child.Name == "component")
                                        groupDict[id].Add(Convert.ToInt32(child.Attributes["id"].Value));
                                break;
                        }
                       
                        break;
                    case "relation":// the relation between node's ids
                       
                        int istart = Convert.ToInt32(node.Attributes["entry1"].Value);
                        int iend = Convert.ToInt32(node.Attributes["entry2"].Value);
                        if (node.HasChildNodes)
                        {
                            foreach (XmlNode subtype in node.ChildNodes)
                            {
                                int type = 0;
                                Interaction.DirectionType direction = Interaction.DirectionType.undirected;
                                switch (subtype.Attributes["value"].Value)
                                {
                                    case "-->": //activation, expression
                                        type = 1;
                                        direction = Interaction.DirectionType.directed;
                                        break;
                                    case "--|": //inhibition
                                        type = -1;
                                        direction = Interaction.DirectionType.directed;
                                        break;
                                    case "..>": //indirect_effect
                                        type = 2;
                                        direction = Interaction.DirectionType.directed;
                                        break;
                                    case "-o->": //via_compound
                                        type = 3;
                                        direction = Interaction.DirectionType.directed;
                                        break;
                                    case "-/-"://missing_interaction
                                        type = 4;
                                        direction = Interaction.DirectionType.directed;
                                        break;
                                    case "+p": //phosphorylation
                                        type = 5;
                                        direction = Interaction.DirectionType.directed;
                                        break;
                                    case "---": //PPIs_in_complex, binding/association
                                    case "-+-"://dissociation
                                        type = 0;
                                        direction = Interaction.DirectionType.undirected;
                                        break;
                                    default:
                                        //User.One.SendErrorToUser(new Exception("Network work type " + subtype.Attributes["value"].Value + ";" + subtype.Attributes["name"].Value + " is new"));
                                        break;
                                }
                                if (parsingOpt != 0)//decomposing into gene level
                                {
                                    Node[] nStarts = null, nEnds = null;
                                    if (geneDict.ContainsKey(istart))
                                    {
                                        string[] geneids = geneDict[istart].Split(' ');
                                        nStarts = Net.NewNodeArray(geneids.Count());
                                        for (int i = 0; i < geneids.Count(); i++)
                                            nStarts[i] = Net.NewNode(geneids[i], null);

                                    }
                                    else if (groupDict.ContainsKey(istart))
                                    {

                                        nStarts = Net.NewNodeArray(groupDict[istart].Count());
                                        for (int i = 0; i < groupDict[istart].Count(); i++)
                                            nStarts[i] = Net.NewNode(geneDict[groupDict[istart][i]], null);
                                        //Each group is a multiple geneid

                                    }
                                    else
                                        continue;

                                    if (geneDict.ContainsKey(iend))
                                    {
                                        string[] geneids = geneDict[iend].Split(' ');
                                        nEnds = Net.NewNodeArray(geneids.Count());
                                        for (int i = 0; i < geneids.Count(); i++)
                                            nEnds[i] = Net.NewNode(geneids[i], null);
                                    }
                                    else if (groupDict.ContainsKey(iend))
                                    {

                                        nEnds = Net.NewNodeArray(groupDict[iend].Count());
                                        for (int i = 0; i < groupDict[iend].Count(); i++)
                                            nEnds[i] = Net.NewNode(geneDict[groupDict[iend][i]], null);

                                    }
                                    else
                                        continue;
                                    
                                    //PPi conneections between a multiple gene node (homologous genes)
                                    for (int i = 0; i < nStarts.Count() - 1; i++)
                                        for (int j = i + 1; j < nStarts.Count(); j++)
                                            if (!Net.hasEdge(nStarts[i], nStarts[j]))
                                            {
                                                Net.AddNodeAndArc(new Interaction(
                                                    nStarts[i],
                                                    nStarts[j],
                                                    0,
                                                    "---;PPIs in a multi-gene node",
                                                    1,
                                                    Interaction.DirectionType.undirected));
                                            }
                                    //PPi conneections between a multiple gene node (homologous genes)
                                    for (int i = 0; i < nEnds.Count() - 1; i++)
                                        for (int j = i + 1; j < nEnds.Count(); j++)
                                            if (!Net.hasEdge(nEnds[i], nEnds[j]))
                                            {
                                                Net.AddNodeAndArc(new Interaction(
                                                   nEnds[i],
                                                   nEnds[j],
                                                   0,
                                                   "---;PPIs in a multi-gene node",
                                                   1,
                                                   Interaction.DirectionType.undirected));
                                            }
                                     
                                    //Conneections between {s1, s2, ... sn} -> {e1, e2, ... en} 
                                    // decomposed into s1 -> e1, s1 -> e2, ... s1 -> en,
                                    //                  s2 -> e1, s2 -> e2, ... s2 -> en,
                                    //                  sn -> e1, sn -> e2, ... sn -> en,
                                    for (int i = 0; i < nStarts.Count(); i++)
                                        for (int j = 0; j < nEnds.Count(); j++)
                                        {
                                            Net.AddNodeAndArc(new Interaction(
                                            nStarts[i],
                                            nEnds[j],
                                            type,
                                            subtype.Attributes["value"].Value + ";" + subtype.Attributes["name"].Value,
                                            1,
                                            direction));
                                        }
                                }
                                else //if parsingOpt == 0
                                {
                                    Node[] nStarts = null, nEnds = null;
                                    if (geneDict.ContainsKey(istart))
                                    {
                                        
                                        nStarts = Net.NewNodeArray(1);

                                        nStarts[0] = Net.NewNode(geneDict[istart], null);

                                    }
                                    else if (groupDict.ContainsKey(istart))
                                    {

                                        nStarts = Net.NewNodeArray(groupDict[istart].Count());
                                        for (int i = 0; i < groupDict[istart].Count(); i++)
                                            nStarts[i] = Net.NewNode(geneDict[groupDict[istart][i]], null);
                                        //Each group is a multiple geneid

                                    }
                                    else
                                        continue;

                                    if (geneDict.ContainsKey(iend))
                                    {
                                        nEnds = Net.NewNodeArray(1);

                                        nEnds[0] = Net.NewNode(geneDict[iend], null);
                                    }
                                    else if (groupDict.ContainsKey(iend))
                                    {

                                        nEnds = Net.NewNodeArray(groupDict[iend].Count());
                                        for (int i = 0; i < groupDict[iend].Count(); i++)
                                            nEnds[i] = Net.NewNode(geneDict[groupDict[iend][i]], null);

                                    }
                                    else
                                        continue;

                                    //PPi conneections between a multiple gene node (homologous genes)
                                    for (int i = 0; i < nStarts.Count() - 1; i++)
                                        for (int j = i + 1; j < nStarts.Count(); j++)
                                            if (!Net.hasEdge(nStarts[i], nStarts[j]))
                                            {
                                                Net.AddNodeAndArc(new Interaction(
                                                    nStarts[i],
                                                    nStarts[j],
                                                    0,
                                                    "---;PPIs in a multi-gene node",
                                                    1,
                                                    Interaction.DirectionType.undirected));
                                            }
                                    //PPi conneections between a multiple gene node (homologous genes)
                                    for (int i = 0; i < nEnds.Count() - 1; i++)
                                        for (int j = i + 1; j < nEnds.Count(); j++)
                                            if (!Net.hasEdge(nEnds[i], nEnds[j]))
                                            {
                                                Net.AddNodeAndArc(new Interaction(
                                                   nEnds[i],
                                                   nEnds[j],
                                                   0,
                                                   "---;PPIs in a multi-gene node",
                                                   1,
                                                   Interaction.DirectionType.undirected));
                                            }
                                    
                                    //Conneections between {s1, s2, ... sn} -> {e1, e2, ... en} 
                                    // decomposed into s1 -> e1, s1 -> e2, ... s1 -> en,
                                    //                  s2 -> e1, s2 -> e2, ... s2 -> en,
                                    //                  sn -> e1, sn -> e2, ... sn -> en,
                                    for (int i = 0; i < nStarts.Count(); i++)
                                        for (int j = 0; j < nEnds.Count(); j++)
                                        {
                                            Net.AddNodeAndArc(new Interaction(
                                            nStarts[i],
                                            nEnds[j],
                                            type,
                                            subtype.Attributes["value"].Value + ";" + subtype.Attributes["name"].Value,
                                            1,
                                            direction));
                                        }
                                }// else if parsingOption ==0
                            }// end foreach
                        }//end if
                       
                        break;//case switch
                }

            }

            Net.RemoveDuplicatedArcs();
            
            

        }// end function
        #endregion
    }
}