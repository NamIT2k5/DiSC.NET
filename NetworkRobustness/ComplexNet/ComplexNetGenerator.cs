using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetSimulation;
using Mathutil;
using System.Diagnostics;
using NetSimulation.Lib;
//using Algorithms.ShortestPath;
using MathNet.Numerics.LinearAlgebra;
namespace BasicNet
{
    /// <summary>
    /// Generate random networks (graphs) for network samples. This uses network model published recently
    /// reference source code: https://github.com/snap-stanford/snap/blob/master/snap-core/ggen.cpp
    /// </summary>
    public class ComplexNetGenerator
    {
        #region Small world
        public BooleanNetwork generateSmallWorld_Old(int N, int r, double p)
        {
            //Tworzenie N wezlow
            BooleanNetwork net = new BooleanNetwork();
            for (int i = 0; i < N; i++)
            {
                Node node = net.NewNode(i.ToString(),null);
                net.AddNode(node);
            }

            //Tworzenie sieci regularnej
            for (int i = 0; i < N; i++)
            {
                for (int j = 1; j <= r; j++)
                {
                    Interaction edge = Interaction.RandomInteraction(net.Nodes.ElementAt(i), net.Nodes.ElementAt((i + j) % N), Interaction.ArbitraryValue,"");
                    net.AddArc(edge);
                }
            }

            Random random = new Random();
            for (int i = 0; i < N; i++) //i - nr wezla referencyjnego
            {
                for (int j = 0; j < net.Nodes.ElementAt(i).Arcs.Count(); j++) //przegladamy wszystkie jego krawedzie z sasiadami
                {
                    if (random.NextDouble() <= p)//przepinamy krawedz?
                    {
                        Node newTarget = net.Nodes.ElementAt(random.Next(N)); //losowy wezel docelowy
                        if (!net.Nodes.ElementAt(i).name.Equals(newTarget.name)) //jesli nie jest tym samym wezlem
                        {
                            if (!nodesConnected(net.Arcs, net.Nodes.ElementAt(i), newTarget)) //jesli wezly nie sa polaczone
                            {
                                net.Nodes.ElementAt(i).Arcs.ElementAt(j).endNode = newTarget; //polacz
                            }
                        }
                    }
                }
            }

            return net;
        }
       /// <summary>
       /// Generate a smallwork network with N nodes and r interactions
       /// </summary>
       /// <param name="N">The number of nodes</param>
       /// <param name="r">The numbef of links</param>
       /// <param name="p">The probability has links not connecting to node groups with r links</param>
       /// <returns>The network</returns>
        public BooleanNetwork generateSmallWorld(int N, int r, double p=0.5)
        {
            //Tworzenie N wezlow
            BooleanNetwork net = new BooleanNetwork();
            for (int i = 0; i < N; i++)
            {
                Node node = net.NewNode(i.ToString(),null);
                net.AddNode(node);
            }

            //Tworzenie sieci regularnej
            for (int i = 0; i < N; i++)
            {
                for (int j = 1; j <= r; j++)
                {
                    Interaction edge = Interaction.RandomInteraction(net.Nodes.ElementAt(i), net.Nodes.ElementAt((i + j) % N), Interaction.ArbitraryValue);
                    //Interaction edge = new Interaction();
                    //edge.id = "edge" + i + j;
                    //edge.directed = false;
                    //edge.source = net.Nodes[i];
                    //edge.target = net.Nodes[(i + j) % N];
                    //net.Nodes[i].Arcs.Add(edge);
                    //net.Nodes[(i + j) % N].Arcs.Add(edge);
                    net.AddArc(edge);
                }
            }

            Random random = new Random();

            for (int i = 0; i < N; i++) //i - nr wezla referencyjnego
            {
                for (int j = 0; j < net.Nodes.ElementAt(i).Arcs.Count(); j++) //przegladamy wszystkie jego krawedzie z sasiadami
                {
                    if (random.NextDouble() < p)//przepinamy krawedz?
                    {
                        Node newTarget = net.Nodes.ElementAt(random.Next(N)); //losowy wezel docelowy
                        if (!net.Nodes.ElementAt(i).name.Equals(newTarget.name)) //to avoid self-connections
                        {
                            if (!nodesConnected(net.Arcs, net.Nodes.ElementAt(i), newTarget)) //whether two nodes are connected together by an edge or not
                            {
                                net.RemoveArc(net.Nodes.ElementAt(i).Arcs.ElementAt(j));
                                net.AddArc(Interaction.RandomInteraction(net.Nodes.ElementAt(i), newTarget, Interaction.ArbitraryValue));
                                /*
                                Interaction edge = net.Nodes[i].Arcs[j];

                                //Remove link at end of node[i].edges[j]
                                edge.endNode.Arcs.Remove(edge);

                                //dodac do nowego wezla ta krawedz
                                newTarget.Arcs.Add(edge);

                                //polaczyc wezly
                                //net.Nodes[i].Arcs[j].endNode = newTarget; 
                                edge.endNode = newTarget;
                                 */
                            }
                        }
                    }
                }
            }

            return net;
        }
        /// <summary>
        /// Check whether two nodes are connected together by an edge or not
        /// </summary>
        /// <param name="edges">A set of edges, which maybe contains nodes in an edge</param>
        /// <param name="node1">the first node</param>
        /// <param name="node2">the second node</param>
        /// <returns>true if connected, else false</returns>
        //private bool nodesConnected(List<Interaction> edges, Node node1, Node node2)
        private bool nodesConnected(IEnumerable<Interaction> edges, Node node1, Node node2)
        {
            foreach (Interaction edge in edges)
            {
                if(edge.startNode!=null && edge.endNode!=null)
                    if ((edge.startNode.name.Equals(node1.name) && edge.endNode.name.Equals(node2.name)) || (edge.startNode.name.Equals(node2.name) && edge.endNode.name.Equals(node1.name)))
                {
                    return true;
                }
            }
            return false;
        }
       

#endregion
        #region Protein network
        /// <summary>
        /// Create a full-connected network
        /// </summary>
        /// <param name="N">The number of nodes</param>
        /// <returns>The full connected network, a small-world network</returns>
        public BooleanNetwork generateFullConnectedNetwork(int N)
        {
            BooleanNetwork net = new BooleanNetwork();
            //Create N nodes
            for (int i = 0; i < N; i++)
            {
                Node node = net.NewNode(i.ToString(), null);
                net.AddNode(node);
            }
            //Add full-connected links to all nodes
            for(int i=0;i<N;i++)
                for (int j = i+1; j < N; j++)
                {
                    Interaction edge = Interaction.RandomInteraction(net.Nodes.ElementAt(i), net.Nodes.ElementAt(j), Interaction.ArbitraryValue);
                    net.AddArc(edge);
                }
            return net;
        }
        private double Pj(Node node, double alpha)
        {
            return node.EdgeDegree > 1 ? Math.Pow(node.EdgeDegree, -alpha) : 0;
        }
        private double Pij(BooleanNetwork net, Node ni, Node nj)
        {
            IEnumerable<Node> neighbourNi = ni.Neighbours;
            IEnumerable<Node> neighbourNj = nj.Neighbours;
            IEnumerable<Node> sameNeighbour= from p in neighbourNi where neighbourNj.Any(e => e.name == p.name) select p;
            return (sameNeighbour.Count() * sameNeighbour.Count()) / (ni.EdgeDegree * nj.EdgeDegree);
        }
        private IEnumerable<Node> GetSharedCommonNeighbourNodes(BooleanNetwork net)
        {
            //Netutil.DumpNode(net.GetNeighbourNodes(net.Nodes.ElementAt(0)).ToArray());
            //Netutil.DumpNode(net.GetNeighbourNodes(net.Nodes.ElementAt(1)).ToArray());
            return from P in net.Nodes
                   where P.Neighbours.Any(neighbourP => 
                       net.Nodes.Any(R => R.name != P.name && R.Neighbours.Any(neighbourR => neighbourR.name == neighbourP.name)))
                             select P;

                            
        }
        /// <summary>
        /// Generate a protein network based on the model published at http://www.ncbi.nlm.nih.gov/pubmed/21867262
        /// </summary>
        /// <param name="N">The number of nodes</param>
        /// <param name="L">The number of links, which satisfies >= N </param>
        /// <param name="alpha">The parameter to tune the network topology</param>
        /// <returns>A random network by the network model</returns>
        public BooleanNetwork generateProtein(int N, int L, double alpha)
        {
            
            //Step 1. Create a full-connected network
            BooleanNetwork net = generateFullConnectedNetwork(N);

            //Step 2. Reduce the full-connected network by preferential depletion: the lower the node degree, the lower the probability to maintain interactions
            net = RemoveLinkByPreferredDepletion(net, N, alpha);

            
            //Step 3. Add links to the reduced network by similarity: the more common neighbors two nodes share, the higher is the probability to have have an interaction
            net = AddLinkBySimilarity(net, L);

            return net;
        }
        public BooleanNetwork RemoveLinkByPreferredDepletion(BooleanNetwork net, int N, double alpha)
        {
            //Netutil.DumpNet(net);

            Node node = null;
            IEnumerable<Node> nodeList = null;
            while (net.Arcs.Count() > N)// The number of links are reduced to N
            {
                //Select a random node
                nodeList = (from e in net.Nodes where e.Neighbours.Count() > 0 select e);
                node = nodeList.ElementAt(NumericMath.RandomCraft.Next(nodeList.Count()));
                
                //Netutil.DumpNode(nodeList.ToArray());
                //Netutil.DumpNode(node);

                //Select a random interaction by a probability
                double p = 1, sum = 0;
                IEnumerable<KeyValuePair<Node, Interaction>> neighbours = node.NeighbourPairs;
                foreach (KeyValuePair<Node, Interaction> n in neighbours)
                    sum += Pj(n.Key, alpha) + 1;

                double r = NumericMath.RandomCraft.NextDouble();// pick up a random value between 0..1
                int k = 0;
                //Which range the value r belong to?
                for (; k < neighbours.Count() && r <= p; k++)
                    p -= (Pj(neighbours.ElementAt(k).Key, alpha) + 1) / sum;
                k = Math.Max(k - 1, 0);

                net.RemoveArc(neighbours.ElementAt(k).Value);
            }
            return net;
        }
        public BooleanNetwork AddLinkBySimilarity(BooleanNetwork net, int L)
        {
            Node node = null;
            IEnumerable<Node> nodeList = null;
            while (net.Arcs.Count() < L) // The number of links are added upto L
            {
                //Netutil.DumpNet(net);

                nodeList = GetSharedCommonNeighbourNodes(net);
                //Netutil.DumpNode(nodeList.ToArray());
                node = nodeList.ElementAt(NumericMath.RandomCraft.Next(nodeList.Count()));
                //Netutil.DumpNode(node);
                //Select all neighbours of neighbours of the node

                nodeList = (from a in nodeList
                            where a.name != node.name &&
                                a.Neighbours.Any(t => t.Neighbours.Any(e => e.name == node.name))
                            select a);

                //nodeList = (from a in nodeList where a.name!=node.name &&
                //                net.GetNeighbourNodes(a) ) 

                //Netutil.DumpNode(nodeList.ToArray());
                double p = 1, sum = 0;
                foreach (Node n in nodeList)
                    sum += Pij(net,node, n) + 1;

                double r = NumericMath.RandomCraft.NextDouble();// pick up a random value between 0..1
                int k = 0;
                for (; k < nodeList.Count() && r <= p; k++)
                    p -= (Pij(net, node, nodeList.ElementAt(k)) + 1) / sum;
                k = Math.Max(k - 1, 0);

                net.AddArc(new Interaction(node, nodeList.ElementAt(k), Interaction.ArbitraryValue));
            }
            return net;
        }
        #endregion
        #region Random graph
        /// <summary>
        /// Gerate a network with between-nodes connection probability of p
        /// </summary>
        /// <param name="n">The number of nodes</param>
        /// <param name="p">The probability of connection between two nodes</param>
        /// <returns>The network</returns>
        public BooleanNetwork generateGraphP(int n, double p)
        {
            BooleanNetwork net = new BooleanNetwork();

            //tworzenie n wezlow
            for (int i = 0; i < n; i++)
            {
                Node node = net.NewNode(i.ToString(),null);
                net.AddNode(node);
            }

            Random random = new Random();
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    if (random.NextDouble() <= p)
                    {
                        //tworzenie krawedzi
                        Interaction edge = Interaction.RandomInteraction(net.Nodes.ElementAt(i), net.Nodes.ElementAt(j), Interaction.ArbitraryValue);
                        
                        //net.Nodes[i].AddEdge(edge);
                        //net.Nodes[j].AddEdge(edge);
                        net.AddArc(edge);
                    }
                }
            }
            return net;
        }
        /// <summary>
        /// Generate a network with nodes of n and random links of k
        /// </summary>
        /// <param name="n">The number of nodes</param>
        /// <param name="k">The number of links</param>
        /// <returns>The network</returns>
        public BooleanNetwork generateGraphK(int n, int k)
        {
            BooleanNetwork net = new BooleanNetwork();

            //tworzenie n wezlow
            for (int i = 0; i < n; i++)
            {
                Node node = net.NewNode(i.ToString(), null);
                net.AddNode(node);
            }

            List<Interaction> edgeList = new List<Interaction>();

            //tworzenie krawedzi
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    Interaction edge = Interaction.RandomInteraction(net.Nodes.ElementAt(i), net.Nodes.ElementAt(j), Interaction.ArbitraryValue);
                    //edge.id = "edge" + i + j;
                    //edge.directed = false;
                    //edge.source = net.Nodes[i];
                    //edge.target = net.Nodes[j];
                    edgeList.Add(edge);
                }
            }
            //losowanie k krawedzi
            Random random = new Random();
            for (int i = 0; i < k; i++)
            {
                int w = random.Next(edgeList.Count - 1);
                //edgeList[w].startNode.AddEdge(edgeList[w]);
                //edgeList[w].endNode.AddEdge(edgeList[w]);
                net.AddArc(edgeList[w]);
                edgeList.Remove(edgeList[w]);
            }
            return net;
        }
#endregion
        #region Scale-free network
        /// <summary>
        /// Generate Scalefree network with node degree distribution different in the ranges [0, m0] and [m0, N]
        /// In the zone [0, m0] node degree distribution steadily increasing from 1 to m0
        /// In the zon [m0, N] node degree distribution 
        /// </summary>
        /// <param name="N">The number of nodes</param>
        /// <param name="m0"></param>
        /// <param name="M">The maximum number of links on a node connected together by prefferred attachment</param>
        /// <returns>The network</returns>
        public BooleanNetwork generateScaleFree(int N, int m0, int M)
        {
            BooleanNetwork net = new BooleanNetwork();

            //generate m0 nodes
            for (int i = 0; i < m0; i++)
            {
                Node node = net.NewNode(i.ToString(), null);
                net.AddNode(node);
            }

            //create connections between nodes with steady decrease of node degree
            for (int i = 0; i < m0; i++)
            {
                for (int j = i + 1; j < m0; j++)
                {
                    Interaction edge = Interaction.RandomInteraction(net.Nodes.ElementAt(i), net.Nodes.ElementAt(j), Interaction.ArbitraryValue);
                    //edge.id = "edge" + i + j;
                    //edge.directed = false;
                    //edge.source = net.Nodes[i];
                    //edge.target = net.Nodes[j];
                    //net.Nodes[i].Arcs.Add(edge);
                    //net.Nodes[j].Arcs.Add(edge);
                    net.AddArc(edge);
                }
            }

            Random random = new Random();
            for (int i = m0; i < N; i++)
            {
                Node node = net.NewNode(i.ToString(), null);

                int sum = 0; //suma stopni wszystkich wezlow sieci (liczba krawedzi + 1)
                int m = 0;
                double r, p = 0;
                r = random.NextDouble();
                for (int j = 0; j < net.Nodes.Count(); j++)
                {
                    sum += net.Nodes.ElementAt(j).Arcs.Count() + 1;
                }

                for (int j = 0; j < net.Nodes.Count(); j++)
                {
                    if (m == M) break;
                    p += (double)(net.Nodes.ElementAt(j).Arcs.Count() + 1) / sum;
                    if ((r <= p || j == net.Nodes.Count() - 1) && !nodesConnected(net.Arcs, node, net.Nodes.ElementAt(j)))
                    {
                        Interaction edge = Interaction.RandomInteraction(node, net.Nodes.ElementAt(j), Interaction.ArbitraryValue);
                        //edge.id = "edge" + i + j;
                        //edge.directed = false;
                        //edge.source = node;
                        //edge.target = net.Nodes[j];
                        //node.Arcs.Add(edge);
                        //net.Nodes[j].Arcs.Add(edge);
                        net.AddArc(edge);
                        m++;
                    }
                }
                net.AddNode(node);
            }
            return net;
        }

        /// <summary>
        /// Generate a scale-free network
        /// </summary>
        /// <param name="N">the number of generated Nodes</param>
        /// <returns></returns>
        public BooleanNetwork generateScaleFreeSimplified(int N)
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            BooleanNetwork net = new BooleanNetwork();

            //tworzenie wezla (m0 = 1) poczatkowego
            Node node = net.NewNode("0", null);
            

            net.AddNode(node);

            //Random random = new Random();
            for (int i = 1; i < N; i++)
            {
                node = net.NewNode(i.ToString(), null); //dodajemy nowy węzeł  
                //node.id = "node" + i;

                int sum = 0; //suma stopni wszystkich wezlow sieci (liczba krawedzi + 1)
                //int m = 0;
                double r, p = 0;
                r = random.NextDouble();
                for (int j = 0; j < net.Nodes.Count(); j++)
                {
                    sum += net.Nodes.ElementAt(j).Arcs.Count() + 1;
                }

                // M = 1
                int k = random.Next(net.Nodes.Count());
                p += (double)(net.Nodes.ElementAt(k).Arcs.Count() + 1) / sum;
                if (r <= p || k == net.Nodes.Count() - 1)
                {
                    Interaction edge = Interaction.RandomInteraction(node, net.Nodes.ElementAt(k), Interaction.ArbitraryValue);
                    //edge.id = "edge" + i + k;
                    //edge.directed = false;
                    //edge.source = node;
                    //edge.target = net.Nodes[k];
                    //node.AddEdge(edge);
                    //net.Nodes[k].AddEdge(edge);
                    net.AddArc(edge);
                }
                net.AddNode(node);
            }
            return net;
        }


        public Node AddNodeToNet(BooleanNetwork net, string nodeName)
        {
            Node val = null;
            if (net.Nodes.Count() > 0)
            {
                var set = from nodeid in net.Nodes
                          where nodeid.name == nodeName
                          select nodeid;

                if (set != null && set.Count() > 0)
                    val = set.ElementAt(0);
                else
                {
                    val = net.NewNode(nodeName, null);
                    net.AddNode(val);
                }
            }
            else
            {
                val = net.NewNode(nodeName, null);
                net.AddNode(val);
            }
            return val;
        }
        Random random = new Random(DateTime.Now.Millisecond);
        /// <summary>
        /// Generate a simple scale-free network with two nodes connected together by only one link
        /// </summary>
        /// <param name="N">The number of nodes</param>
        /// <returns>The generated network</returns>
        public BooleanNetwork generateSimpleScaleFreeDzung(int N)
        {
            random = new Random(DateTime.Now.Millisecond);
            BooleanNetwork net = new BooleanNetwork();

            //Intitialize the network with a node inside
            Node node = net.NewNode("0",null);
            net.AddNode(node);

            int sum = 1;//Total of node degrees on the network
            

            //Random random = new Random();
            for (int i = 1; i < N; i++)
            {
                node = net.NewNode(i.ToString(), null); //dodajemy nowy węzeł  

                //Randomly choose a node that's preferred by hub
                //Begin
                double r = 0, p = 1;
                r = random.NextDouble(); //NumericMath.RandomCraft.NextDouble();
                int k = 0;
                for (; k < net.Nodes.Count() && r <= p; k++)
                    p -= (double)(net.Nodes.ElementAt(k).Arcs.Count() + 1) / sum;
                k = Math.Max(k - 1, 0);
                //End

                Interaction edge = Interaction.RandomInteraction(random, node, net.Nodes.ElementAt(k), Interaction.ArbitraryValue,"");

                net.AddArc(edge);

                net.AddNode(node);
                sum += 4;
            }
            return net;
        }
        
        /// <summary>
        /// Add links to an existent network by preferred attachment mechanism
        /// </summary>
        /// <param name="net">The network needing to add links</param>
        /// <param name="nExtraLink">The number of links to add</param>
        /// <returns>The network</returns>
        public BooleanNetwork AddScaleFreeLink(BooleanNetwork net, int nExtraLink = -1)
        {
            //Randomly add more links between nodes
            for (; nExtraLink-- > 0; )
            {
                //avaNodes: available node, which is not fully connecting to all nodes
                var avaNodes = from node in net.Nodes
                               where node.Arcs.Count() < net.Nodes.Count() - 1
                               select node;

                if (avaNodes.Count() == 0)
                    break;

                //randomly select a node in the available nodes
                Node selectedNode = avaNodes.ElementAt(NumericMath.RandomCraft.Next(0, avaNodes.Count()));

                //toNodes: Contains nodes have no link to the selected node
                var toNodes = from node in avaNodes
                              //where !node.Arcs.Any(st => st.startNode == selectedNode || st.endNode == selectedNode)
                              where !node.hasNeighbor(selectedNode)
                              select node;

                if (toNodes.Count() == 0)
                    break;

                int sum = 0;//Total of node degrees on the network
                for (int j = 0; j < toNodes.Count(); j++)
                    sum += toNodes.ElementAt(j).Arcs.Count() + 1;

                //Randomly choose a node that's preferred by hub
                //Begin
                double r = 0, p = 1;
                r = NumericMath.RandomCraft.NextDouble();
                int k = 0;
                for (; k < toNodes.Count() && r <= p; k++)
                    p -= (double)(toNodes.ElementAt(k).Arcs.Count() + 1) / sum;
                k = Math.Max(k - 1, 0);
                //End

                Interaction edge = (NumericMath.RandomCraft.NextDouble() > 0.5f ? Interaction.RandomInteraction(selectedNode, toNodes.ElementAt(k), Interaction.ArbitraryValue) : Interaction.RandomInteraction(toNodes.ElementAt(k), selectedNode, Interaction.ArbitraryValue));
                net.AddArc(edge);
            }
            return net;
        }
        public BooleanNetwork AddDirectedScaleFreeLink(BooleanNetwork net, int nExtraLink = -1)
        {
            //Randomly add more links between nodes
            for (; nExtraLink-- > 0; )
            {
                //avaNodes: available node, which is not fully connecting to all nodes
                var avaNodes = from node in net.Nodes
                               where node.Arcs.Count() < net.Nodes.Count() - 1
                               select node;

                if (avaNodes.Count() == 0)
                    break;

                //randomly select a node in the available nodes
                Node selectedNode = avaNodes.ElementAt(NumericMath.RandomCraft.Next(0, avaNodes.Count()));

                //toNodes: Contains nodes have no link to the selected node
                var toNodes = from node in avaNodes
                              //where !node.Arcs.Any(st => st.startNode == selectedNode || st.endNode == selectedNode)
                              where !node.hasNeighbor(selectedNode)
                              select node;

                if (toNodes.Count() == 0)
                    break;

                int sum = 0;//Total of node degrees on the network
                for (int j = 0; j < toNodes.Count(); j++)
                    sum += toNodes.ElementAt(j).Arcs.Count() + 1;

                //Randomly choose a node that's preferred by hub
                //Begin
                double r = 0, p = 1;
                r = NumericMath.RandomCraft.NextDouble();
                int k = 0;
                for (; k < toNodes.Count() && r <= p; k++)
                    p -= (double)(toNodes.ElementAt(k).Arcs.Count() + 1) / sum;
                k = Math.Max(k - 1, 0);
                //End
                if (nExtraLink < 2)
                {
                    Interaction edge = (NumericMath.RandomCraft.NextDouble() > 0.5f ? Interaction.RandomInteraction(selectedNode, toNodes.ElementAt(k), Interaction.ArbitraryValue) : Interaction.RandomInteraction(toNodes.ElementAt(k), selectedNode, Interaction.ArbitraryValue));
                    net.AddArc(edge);
                }
                else //nExtraLink>1
                {
                    k = NumericMath.RandomCraft.Next(3);
                    if (k == 0)
                    {
                        Interaction edge = Interaction.RandomInteraction(selectedNode, toNodes.ElementAt(k), Interaction.ArbitraryValue);
                        net.AddArc(edge);
                    }
                    else if (k == 1)
                    {
                        Interaction edge = Interaction.RandomInteraction(toNodes.ElementAt(k), selectedNode, Interaction.ArbitraryValue);
                        net.AddArc(edge);
                    }
                    else
                    {
                        nExtraLink--;
                        Interaction edge1 = Interaction.RandomInteraction(selectedNode, toNodes.ElementAt(k), Interaction.ArbitraryValue);
                        net.AddArc(edge1);
                        Interaction edge2 = Interaction.RandomInteraction(toNodes.ElementAt(k), selectedNode, Interaction.ArbitraryValue);
                        net.AddArc(edge2);
                    }
                }
            }
            return net;
        }
        /// <summary>
        /// Add a random link from (to) a node to (from) another
        /// </summary>
        /// <param name="net">The network to add the node</param>
        /// <param name="theNode">The node added to the network</param>
        /// <param name="isStartedNode">True: thenode is a start node; else an end node</param>
        /// <param name="nLink">The number of links to add</param>
        /// <returns>succsessful</returns>
        public bool AddAScaleFreeLink(BooleanNetwork net, Node theNode, bool isStartedNode, int nLink=1)
        {
            //Randomly add more links between nodes
            while (nLink-- > 0)
            {
                //avaNodes: available node, which is not fully connecting to all nodes
                var avaNodes = from node in net.Nodes
                               where node.Arcs.Count() < net.Nodes.Count() - 1
                               select node;

                if (avaNodes.Count() == 0)
                    return false;


                //toNodes: Contains nodes have no link to the selected node
                IEnumerable<Node> toNodes = null;
                if (isStartedNode)
                    toNodes = from node in avaNodes
                              where !node.Arcs.Any(st => st.startNode == theNode)
                              select node;
                else
                    toNodes = from node in avaNodes
                              where !node.Arcs.Any(st => st.endNode == theNode)
                              select node;


                if (toNodes.Count() == 0)
                    return false;

                int sum = 0;//Total of node degrees on the network
                for (int j = 0; j < toNodes.Count(); j++)
                    sum += toNodes.ElementAt(j).Arcs.Count() + 1;

                //Randomly choose a node that's preferred by hub
                //Begin
                double r = 0, p = 1;
                r = NumericMath.RandomCraft.NextDouble();
                int k = 0;
                for (; k < toNodes.Count() && r <= p; k++)
                    p -= (double)(toNodes.ElementAt(k).Arcs.Count() + 1) / sum;
                k = Math.Max(k - 1, 0);
                //End

                Interaction edge = (isStartedNode ? new Interaction(theNode, toNodes.ElementAt(k), Interaction.ArbitraryValue) : 
                    new Interaction(toNodes.ElementAt(k), theNode, Interaction.ArbitraryValue));
                net.AddArc(edge);
            }
            return true;
        }
        /*
        /// <summary>
        /// Generate a scale-free network with the number of links being random
        /// </summary>
        /// <param name="nNode">The number of nodes</param>
        /// <returns>The network</returns>
        public BooleanNetwork generateScaleFreeDirectedNetwork(int nNode)
        {
            //return generateComplexScaleFreeDzung(nNode, 0, nExtraSFLink(nNode));
            return generateScaleFreeDirectedNetwork(nNode, NumericMath.RandomCraft.Next(0, nMaxSFLink(nNode) + 1));
        }
         * */
        /// <summary>
        /// Generate a scale-free undirected network with the number of link greater than the number of nodes
        /// </summary>
        /// <param name="nNode">The number of nodes</param>
        /// /// <param name="nLink">The number of links</param>
        /// <returns>The network</returns>
        public BooleanNetwork generateScaleFreeUndirectedNetwork(int nNode, int nLink)
        {
            // nLink is the number of extra links after adding the primary links connecting all nodes
            nLink -= ComplexNetGenerator.nMinSFLink(nNode);

            if (nLink < 0 || nLink > ComplexNetGenerator.nExtraSFULink(nNode))
                throw new Exception(string.Format("The number of links should be in the range [{0} .. {1}]",
                    ComplexNetGenerator.nMinSFLink(nNode), ComplexNetGenerator.nMaxSFULink(nNode)));

            //Generate a network with the minimum links connecting to all nodes by preffered attachment mechanism
            BooleanNetwork net = generateSimpleScaleFreeDzung(nNode);

            //Add extra links by preferred attachment mechanism
            return AddScaleFreeLink(net, nLink);
        }
        
        /// <summary>
        /// Generate a scale-free directed network with the number of link greater than the number of nodes
        /// One edge between 2 nodes can have maximal 2 opposited arcs
        /// </summary>
        /// <param name="nNode">The number of nodes</param>
        /// <param name="nLink">The number of links</param>
        /// <returns></returns>
        public BooleanNetwork generateScaleFreeDirectedNetwork(int nNode, int nLink)
        {
            // nLink is the number of extra links after adding the primary links connecting all nodes
            nLink -= ComplexNetGenerator.nMinSFLink(nNode);

            if (nLink < 0 || nLink > ComplexNetGenerator.nExtraSFDLink(nNode))
                throw new Exception(string.Format("The number of links should be in the range [{0} .. {1}]", 
                    ComplexNetGenerator.nMinSFLink(nNode), ComplexNetGenerator.nMaxSFDLink(nNode)));
            
            //Generate a network with the minimum links connecting to all nodes by preffered attachment mechanism
            BooleanNetwork net = generateSimpleScaleFreeDzung(nNode);
            
            //Add extra links by preferred attachment mechanism
            return AddDirectedScaleFreeLink(net, nLink);

            
        }
         
        /// <summary>
        /// Generate scale free directed network with both in- and out- degree distribution following preferrential attachment
        /// See: Complex Graphs and Networks by Fan Chung and Linyuan Lu http://www.math.ucsd.edu/~fan/complex/ch3.pdf
        /// NOTE: preferential attachment will always generate acyclic networks
        /// </summary>
        /// <param name="template">The network template to create</param>
        /// <param name="nNode">The number of nodes</param>
        /// <param name="nEdge">The number of directed links in the network</param>
        /// <returns></returns>
        public BasicNetwork generateDirectedNetworkByPreferentialAttachment(BasicNetwork template, int nNode, int nLink)
        {
            // nLink is the number of extra links after adding the primary links connecting all nodes

            if (nLink < ComplexNetGenerator.nMinSFLink(nNode) || nLink > ComplexNetGenerator.nMaxSFDLink(nNode))
                throw new Exception(string.Format("The number of links should be in the range [{0} .. {1}]",
                    ComplexNetGenerator.nMinSFLink(nNode), ComplexNetGenerator.nMaxSFDLink(nNode)));

            BasicNetwork net = template.CreateObject() as BasicNetwork;
            
            //Intitialize the network with a node inside
            Node newNode = net.NewNode("0", null);
            net.AddNode(newNode);
            double inRate = Mathutil.NumericMath.RandomCraft.dRandBetween(0.1, 0.9);
            
            Interaction edge =null;
            Node start = null, end=null;

            //Create simple scale-free network, which the most sparse network
            for(int i=1;i<nNode;i++)
            {

                double r = random.NextDouble();
                if (r <= inRate)
                {
                    newNode = net.NewNode(i.ToString(), null);
                    end = SelectPreferentialNodeByInDegree(net.Nodes);
                    edge = new Interaction(newNode, end, Interaction.ArbitraryValue);
                }
                else
                {
                    newNode = net.NewNode(i.ToString(), null);
                    start = SelectPreferentialNodeByOutDegree(net.Nodes);
                    edge = new Interaction(start,newNode, Interaction.ArbitraryValue);
                }
                net.AddNode(newNode);
                net.AddArc(edge);
            }
            //Add scale-free links
            return AddDirectedScaleFreeLink(net, nLink, inRate);
        }
        public BasicNetwork AddDirectedScaleFreeLink(BasicNetwork net, int nLink, double inRate)
        {
            //Randomly add more links between nodes
            Node end=null,start = null;
            while (net.Arcs.Count() < nLink)
            {

                //avaNodes: available node, which is not fully connecting to all nodes
                var avaStartNodes = from node in net.Nodes
                                    where node.OutDegree < net.Nodes.Count() - 1
                                    select node;
                if (avaStartNodes.Count() == 0)
                    break;


                double r = random.NextDouble();
                if (r <= inRate)
                {
                    
                    start = avaStartNodes.ElementAt(random.Next(0, avaStartNodes.Count()));// pick up a node randomly
                    var avaEndNodes = from node in net.Nodes
                                      where !node.hasLinkFrom(start) && node != start //node!=startNode to avoid sefl-loop nodes
                                      select node;
                    end = SelectPreferentialNodeByInDegree(avaEndNodes);//pick up end by preferential attachment
                    
                }
                else
                {
                    start = SelectPreferentialNodeByOutDegree(avaStartNodes);//pick up start by preferential attachment

                    var avaEndNodes = from node in net.Nodes
                                      where !node.hasLinkFrom(start) && node != start //node!=startNode to avoid sefl-loop nodes
                                      select node;
                    end = avaEndNodes.ElementAt(random.Next(0, avaEndNodes.Count()));// pick up end randomly
                }

                Interaction edge = new Interaction(start, end, Interaction.ArbitraryValue);
                net.AddArc(edge);
            }
            return net;
        }
        public Node SelectPreferentialNodeByInDegree(IEnumerable<Node> nodes)
        {
            int sum = (from e in nodes select e.InDegree + 1).Sum();
            double r = 0, p = 1.0;
            r = random.NextDouble(); //NumericMath.RandomCraft.NextDouble();
            int k = 0;
            for (; k < nodes.Count() && r <= p; k++)
                p -= (double)(nodes.ElementAt(k).InDegree + 1) / sum;
            k = Math.Max(k - 1, 0);
            return nodes.ElementAt(k);
        }

        public Node SelectPreferentialNodeByOutDegree(IEnumerable<Node> nodes)
        {
            int sum = (from e in nodes select e.OutDegree + 1).Sum();
            double r = 0, p = 1.0;
            r = random.NextDouble(); //NumericMath.RandomCraft.NextDouble();
            int k = 0;
            for (; k < nodes.Count() && r <= p; k++)
                p -= (double)(nodes.ElementAt(k).OutDegree + 1) / sum;
            k = Math.Max(k - 1, 0);
            return nodes.ElementAt(k);
        }
        
       
        public BooleanNetwork AdjustLink(BooleanNetwork Net, int minInDegree, int minOutDegree)
        {
            var unsatisfiedNodes = from e in Net.Nodes where !(minInDegree <= e.InDegree && minOutDegree <= e.OutDegree) select e;
            Netutil.DumpNode(Net.Nodes.ToArray());
            Netutil.DumpInteraction(Net.Arcs.ToArray());
            Netutil.DumpNode(unsatisfiedNodes.ToArray());
            foreach (Node n in unsatisfiedNodes)
            {
                if(minInDegree - n.InDegree>0)
                    this.AddAScaleFreeLink(Net, n, false, minInDegree - n.InDegree);
                if(minOutDegree - n.OutDegree>0)
                    this.AddAScaleFreeLink(Net, n, true, minOutDegree - n.OutDegree);
            }
            Netutil.DumpNode(Net.Nodes.ToArray());
            Netutil.DumpInteraction(Net.Arcs.ToArray());
            return Net;
        }
        /// <summary>
        /// Randomly change the type of a link by one of 3 relationship types: B infer A, A infer B, and A equivalent with B
        /// All arcs between a node pair are considered an edge, in which each arc plays a relationship kind between two nodes.
        /// </summary>
        /// <param name="net">The network to change</param>
        /// <returns>The network</returns>
        private BooleanNetwork CreateNodePairRelationship(BooleanNetwork net)
        {
            int nOriginalCount=net.Arcs.Count();
            for(int i=0;i< nOriginalCount; i++)
            {
                Interaction arc = net.Arcs.ElementAt(i);
                //There are 03 kind of directed links or relationship between a node pair (A, B): A -> B; A <-B; and A <->B
                // B infer A, A infer B are default relationships that are assigned to arc while an arc is created.

                //if (NumericMath.RandomCraft.Next(0, 3) == 0)// randome select edges having equivalent relationships
                if (random.Next(0, 3) == 0)// randome select edges having equivalent relationships
                {
                    // Create a new arc with the direction reversed with the existent arc
                    
                    net.AddArc(new Interaction(arc.endNode, arc.startNode, random.NextDouble() <= 0.5 ? InteractionType.NEGATIVE : InteractionType.POSITIVE));
                }
            }
            return net;

        }
        
        /// <summary>
        /// Return the number of minimum undirected links of a scale free network
        /// </summary>
        /// <param name="nNode">The number of nodes</param>
        /// <returns>The number of minimum links</returns>
        public static int nMinSFLink(int nNode)
        {
            return (nNode - 1);
        }
        /// <summary>
        /// Return the number of maximum undirected links of a scale free network
        /// </summary>
        /// <param name="nNode">The number of nodes</param>
        /// <returns>The number of maximum links</returns>
        public static int nMaxSFULink(int nNode)
        {
            return (nNode*nNode - nNode)/2;
        }
        /// <summary>
        /// Return the number of maximum directed links of a scale free network
        /// </summary>
        /// <param name="nNode"></param>
        /// <returns></returns>
        public static int nMaxSFDLink(int nNode)
        {
            return nNode * nNode - nNode;
        }
        /// <summary>
        /// Return the number of extra undirected link creating a complex scale-free network from a simple scale-free network
        /// </summary>
        /// <param name="nNode">The number of nodes</param>
        /// <returns>The number of extra links</returns>
        public static int nExtraSFULink(int nNode)
        {
            return nMaxSFULink(nNode) - nMinSFLink(nNode);
        }
        public static int nExtraSFDLink(int nNode)
        {
            return nMaxSFDLink(nNode) - nMinSFLink(nNode);
        }
        /// <summary>
        /// Return the number of nodes on the network if we know its amount of links and network density
        /// </summary>
        /// <param name="nLink">The number of links</param>
        /// <param name="netDensity">Network density</param>
        /// <returns>null if have no valid node else return nodes</returns>
        public static double[] NodeFromDensity(int nLink, float netDensity)
        {
            //netDensity=(nLink*2)/(nNode*nNode-nNode)
            //nNode*nNode - nNode - nLink*2/netDensity=0
            double delta = 1 + 4 * nLink * 2 / netDensity;
            if (delta < 0) return null;
            
            double x1 = (1 - Math.Sqrt(delta)) / 2, x2 = (1 + Math.Sqrt(delta)) / 2;
            if (x1 > 0)
            {
                if (x2 > 0)
                    return new double[] { x1, x2 };
                else
                    return new double[] { x1 };
            }
            else
            {
                if (x2 > 0)
                    return new double[] { x2 };
                else
                    return null;
            }
        }
        
        
        /// <summary>
        /// Create scale-free network in node zones [0, m0] and [m0, N]
        /// Zone [0, m0] has node degrees that decreases steadily from 0 to m0
        /// Zone [m0, N] has node degrees fixed by M
        /// </summary>
        /// <param name="N">Total nodes of the generated network</param>
        /// <param name="m0">Requires: m0 less than N
        /// The border which the left-side and right-side have difference of node degree distribution law</param>
        /// <param name="M">Requires: 0 less than M and M less than m0 
        /// Fixed connection of nodes in the zone [m0, N]</param>
        /// <param name="p">Probability of new connection arisen by prefferred attachment </param>
        /// <param name="q">Probability of connection change (prefferred attachment)</param>
        /// <returns>The network</returns>
        public BooleanNetwork generateBAModel(int N, int m0, int M, double p, double q)
        {
            BooleanNetwork net = new BooleanNetwork();

            //Create m0 nodes
            for (int i = 0; i < m0; i++)
            {
                Node node = net.NewNode(i.ToString(), null);
                net.AddNode(node);
            }

            //For each node, make edges connecting from the node to all other nodes 
            for (int i = 0; i < m0; i++)
            {
                for (int j = i + 1; j < m0; j++)
                {
                    Interaction edge = Interaction.RandomInteraction(net.Nodes.ElementAt(i), net.Nodes.ElementAt(j), Interaction.ArbitraryValue);
                    //edge.id = "edge" + i + j;
                    //edge.directed = false;
                    //edge.source = net.Nodes[i];
                    //edge.target = net.Nodes[j];
                    //net.Nodes[i].Arcs.Add(edge);
                    //net.Nodes[j].Arcs.Add(edge);
                    net.AddArc(edge);
                }
            }

           

            for (int i = m0; i < N; i++)
            {
                double r = NumericMath.RandomCraft.NextDouble();
                if (r <= p)// p: the probability of preffered attachment connection having inside nodes
                {
                    //add new link
                    int sIndex = NumericMath.RandomCraft.Next(net.Nodes.Count());
                    double rd = NumericMath.RandomCraft.NextDouble();
                    double pki = 0;
                    for (int j = 0; j < net.Nodes.Count(); j++)
                    {
                        double sumKl = 0;
                        for (int l = 0; l < net.Nodes.Count(); l++)
                        {
                            sumKl += net.Nodes.ElementAt(l).Arcs.Count() + 1;
                        }
                        pki += (net.Nodes.ElementAt(j).Arcs.Count() + 1) / sumKl;
                        if (rd <= pki && !nodesConnected(net.Arcs, net.Nodes.ElementAt(sIndex), net.Nodes.ElementAt(j)) && sIndex != j)
                        {
                            Interaction edge = Interaction.RandomInteraction(net.Nodes.ElementAt(sIndex), net.Nodes.ElementAt(j), Interaction.ArbitraryValue);
                            //edge.id = "edge" + i + "-" + j;
                            //edge.directed = false;
                            //edge.source = net.Nodes[sIndex];
                            //edge.target = net.Nodes[j];
                            //net.Nodes[sIndex].Arcs.Add(edge);
                            //net.Nodes[j].Arcs.Add(edge);
                            net.AddArc(edge);
                            break;
                        }
                    }

                }
                else if (r <= p + q)// q: the probability of connection change by preffered attachment
                {
                    //rewire link
                    int sIndex = NumericMath.RandomCraft.Next(net.Nodes.Count());
                    int linkIndex = NumericMath.RandomCraft.Next(net.Nodes.ElementAt(sIndex).Arcs.Count());
                    if (net.Nodes.ElementAt(sIndex).Arcs.Count() == 0) continue;
                    Interaction edge = net.Nodes.ElementAt(sIndex).Arcs.ElementAt(linkIndex);// a random edge of the node addressed by the variable sIndex

                    //Remove the edge from a node it connects to the selected node
                    net.Nodes.ElementAt(sIndex).Arcs.ElementAt(linkIndex).endNode.RemoveArc(net.Nodes.ElementAt(sIndex).Arcs.ElementAt(linkIndex));

                    //net.RemoveEdgeFromNode(net.Nodes[sIndex].Arcs[linkIndex].endNode, net.Nodes[sIndex].Arcs[linkIndex]);

                    double rd = NumericMath.RandomCraft.NextDouble();
                    double pki = 0;
                    for (int j = 0; j < net.Nodes.Count(); j++)
                    {
                        double sumKl = 0;
                        for (int l = 0; l < net.Nodes.Count(); l++)
                        {
                            sumKl += net.Nodes.ElementAt(l).Arcs.Count() + 1;
                        }
                        pki += (net.Nodes.ElementAt(j).Arcs.Count() + 1) / sumKl;
                        if (rd <= pki && !nodesConnected(net.Arcs, net.Nodes.ElementAt(sIndex), net.Nodes.ElementAt(j)) && sIndex != j)
                        {
                            //edge.id = "edge" + i + "-" + j;
                            //edge.directed = false;
                            //edge.endNode = net.Nodes[j];
                            //net.Nodes[j].Arcs.Add(edge);

                            net.Nodes.ElementAt(j).AddArc(false, edge);
                            //net.ConnectToNode(edge, net.Nodes.ElementAt(j));
                            break;
                        }
                    }
                }
                else // a new node added to the zone [0, m0] by appriximately M connections by preferred attachment
                {
                    //new node
                    Node node = net.NewNode(i.ToString(), null);
                    net.AddNode(node);

                    for (int j = 0; j < M; j++)
                    {
                        double rd = NumericMath.RandomCraft.NextDouble();
                        double pki = 0;
                        for (int k = 0; k < net.Nodes.Count(); k++)
                        {
                            double sumKl = 0;
                            for (int l = 0; l < net.Nodes.Count(); l++)
                            {
                                sumKl += net.Nodes.ElementAt(l).Arcs.Count() + 1;
                            }

                            pki += (net.Nodes.ElementAt(j).Arcs.Count() + 1) / sumKl;

                            if (rd <= pki && !nodesConnected(net.Arcs, node, net.Nodes.ElementAt(j)) && !node.name.Equals(net.Nodes.ElementAt(k).name))
                            {
                                Interaction edge = Interaction.RandomInteraction(node, net.Nodes.ElementAt(k), Interaction.ArbitraryValue);
                                //edge.id = "edge" + i + "-" + j;
                                //edge.directed = false;
                                //edge.source = node;
                                //edge.target = net.Nodes[k];
                                //node.Arcs.Add(edge);
                                //net.Nodes[k].Arcs.Add(edge);
                                net.AddArc(edge);
                                break;
                            }
                        }
                    }
                }
            }

            return net;
        }
        /// <summary>
        /// Create scale-free network in node zones [0, m0] and [m0, N]
        /// Zone [0, m0] has node degrees that decreases steadily from 0 to m0
        /// Zone [m0, N] has node degrees fixed by M
        /// </summary>
        /// <param name="N">Total nodes in the network</param>
        /// <param name="m0">Requires: m0 less than N
        /// The border which the left-side and right-side have difference of node degree distribution law</param>
        /// <param name="M"> Requires: 0 less than M and M less than m0 
        /// Fixed connection of nodes in the zone [m0, N]</param>
        /// <returns>the network</returns>
        public BooleanNetwork generateBAModelSimplifiedA(int N, int m0, int M)
        {
            BooleanNetwork net = new BooleanNetwork();
            if (0 < M && M < m0 && m0 < N)
            {
                //Create m0 new nodes
                for (int i = 0; i < m0; i++)
                {
                    Node node = net.NewNode(i.ToString(), null);
                    //node.id = "node" + i;
                    net.AddNode(node);
                }

                //Add links to m0 nodes
                for (int i = 0; i < m0; i++)
                {
                    for (int j = i + 1; j < m0; j++)
                    {
                        Interaction edge = Interaction.RandomInteraction(net.Nodes.ElementAt(i), net.Nodes.ElementAt(j), Interaction.ArbitraryValue);
                        //edge.id = "edge" + i + j;
                        //edge.directed = false;
                        //edge.source = net.Nodes[i];
                        //edge.target = net.Nodes[j];
                        //net.Nodes[i].Arcs.Add(edge);
                        //net.Nodes[j].Arcs.Add(edge);
                        net.AddArc(edge);
                    }
                }

                //Add new N-m0 nodes to the nework, in which the new node is connected to M random nodes (M < m0)
                for (int i = m0; i < N; i++)
                {
                    Node node = net.NewNode(i.ToString(), null);
                    
                    net.AddNode(node);

                    for (int j = 0; j < M; j++)
                    {
                        int r = NumericMath.RandomCraft.Next(i);
                        while(nodesConnected(net.Arcs, node, net.Nodes.ElementAt(r))) {
                            r = NumericMath.RandomCraft.Next(i);
                        }

                        Interaction edge = Interaction.RandomInteraction(node, net.Nodes.ElementAt(r), Interaction.ArbitraryValue);
                        //edge.id = "edge" + i + j;
                        //edge.directed = false;
                        //edge.source = node;
                        //edge.target = net.Nodes[r];
                        //net.Nodes[i].Arcs.Add(edge);
                        //net.Nodes[r].Arcs.Add(edge);
                        net.AddArc(edge);
                    }
                }
            }

            return net;
        }
        /// <summary>
        /// Generate scale-free network base on Barabasi model
        /// </summary>
        /// <param name="N">The number of nodes</param> 
        /// <param name="M">The number of running times we randomly select a node by prefferred hubs</param>
        /// <returns>The network</returns>
        public BooleanNetwork generateBAModelSimplifiedB(int N, int M)
        {
            BooleanNetwork net = new BooleanNetwork();
            
            //Create N nodes
            for (int i = 0; i < N; i++)
            {
                Node node = net.NewNode(i.ToString(), null);
                net.AddNode(node);
            }

            //Run M times with randomly selecting a node at each interation
            for (int i = 0; i < M; i++)
            {
                //Pick a random node in the created ones
                int r = NumericMath.RandomCraft.Next(N);
                Node source = net.Nodes.ElementAt(r);
                
                double sumKj = 0;//sumKj: ~ Node degree total
                for (int j = 0; j < N; j++)
                {
                    sumKj += net.Nodes.ElementAt(j).Arcs.Count() + 1;
                }

                double rd = NumericMath.RandomCraft.NextDouble();
                double p = 0;
                for (int j = 0; j < N; j++)
                {
                    p += (net.Nodes.ElementAt(j).Arcs.Count() + 1) / sumKj;//Probability based on node degree
                    //Make a connection between the selected node and a
                    if (rd <= p && !nodesConnected(net.Arcs, source, net.Nodes.ElementAt(j)) && r!=j)
                    {
                        Interaction edge = Interaction.RandomInteraction(source, net.Nodes.ElementAt(j), Interaction.ArbitraryValue);
                        //edge.id = "edge" + i + "-" + j;
                        //edge.directed = false;
                        //edge.source = source;
                        //edge.target = net.Nodes[j];
                        //net.Nodes[r].Arcs.Add(edge);
                        //net.Nodes[j].Arcs.Add(edge);
                        net.AddArc(edge);
                        break;
                    }
                }
            }

            return net;
        }
        
        /// Generates a random scale-free graph with power-law degree distribution with
        /// exponent PowerExp. The method uses either the Configuration model (fast but
        /// the result is approximate) or the Edge Rewiring method (slow but exact).
        public BasicNetwork GenRndPowerLaw(BasicNetwork Template, int Nodes, double PowerExp, bool ConfModel, NumericMath.RandomCraft Rnd) 
        {
          List<int> DegSeqV=new   List<int>();
          int DegSum=0;
          for (int n = 0; n < Nodes; n++) {
            int Val = (int) Math.Round(Rnd.GetPowerDev(PowerExp));
            if (! (Val >= 1 && Val < Nodes/2)) { n--; continue; } // skip nodes with too large degree
            DegSeqV.Add(Val);
            DegSum += Val;
          }
          //printf("%d nodes, %u edges\n", Nodes, DegSum);
          if (DegSum % 2 == 1) { DegSeqV[0] += 1; }
          if (ConfModel) {
            // use configuration model -- fast but does not exactly obey the degree sequence
              return GenConfModel4Undirected(Template, DegSeqV, Rnd);
          } else {
            BasicNetwork G = GenDegSeq(Template,DegSeqV, Rnd);
            return G.ShufflePreservingDegree(10,true);
          }
        }
        
        /// <summary>
        /// Generates a random graph with exact degree sequence DegSeqV. The generated graph has no self loops. The graph generation process
        /// simulates the Configuration Model but if a duplicate edge occurs, we find a
        /// random edge, break it and reconnect it with the duplicate.
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public BasicNetwork GenDegSeq(BasicNetwork Template, List<int> DegSeqV, NumericMath.RandomCraft Rnd) 
        {
            int Nodes = DegSeqV.Count;
            BasicNetwork GraphPt = Template.CreateObject() as BasicNetwork;// TUNGraph::New();
            BasicNetwork Graph = GraphPt;
            //Graph.Reserve(Nodes, -1);

            Dictionary<Node,int> DegH=new Dictionary<Node,int>();//DegH(DegSeqV.Len(), true);
  
            DegSeqV.Sort();
            //Debug.AssertR(DegSeqV.IsSorted(false), "DegSeqV must be sorted in descending order.");
            int DegSum=0, edge=0;
            for (int node = 0; node < Nodes; node++) {
            //Graph.AddNode(node) == node);
            Node nObj=Graph.AddNode(node.ToString());
            DegH.Add(nObj,DegSeqV[node]);
            //DegH.AddDat(node, DegSeqV[node]);
            DegSum += DegSeqV[node];
            }
            Debug.Assert(DegSum % 2 == 0);
            //while (! DegH.Empty()) 
            while (DegH.Count>0) 
            {
            // pick random nodes and connect
            //const int NId1 = DegH.GetKey(DegH.GetRndKeyId(TInt::Rnd, 0.5));
            //const int NId2 = DegH.GetKey(DegH.GetRndKeyId(TInt::Rnd, 0.5));

            Node NId1 = DegH.Keys.ElementAt(random.Next(DegH.Keys.Count));
            Node NId2 = DegH.Keys.ElementAt(random.Next(DegH.Keys.Count));

            if (NId1 == NId2) {
                if (DegH[NId1] == 1) { continue; }
                // find rnd edge, break it, and connect the endpoints to the nodes
                //const TIntPr Edge = TSnapDetail::GetRndEdgeNonAdjNode(GraphPt, NId1, -1);
                IEnumerable<Interaction> temp = Graph.GetArcNonAdjNode(new Node[] { NId1 });
                if (temp.Count() == 0) continue;

                Interaction Edge = temp.ElementAt(random.Next(temp.Count()));
                //Graph.DelEdge(Edge.Val1, Edge.Val2);
                Graph.RemoveArc(Edge);
                
                //Graph.AddEdge(Edge.Val1, NId1);
                Graph.AddNodeAndArc(new Interaction(Edge.startNode,NId1,Interaction.ArbitraryValue));
                
                //Graph.AddEdge(NId1, Edge.Val2);
                Graph.AddNodeAndArc(new Interaction(NId1,Edge.endNode,Interaction.ArbitraryValue));

                if (DegH[NId1] == 2) { DegH.Remove(NId1); }
                else { DegH[NId1] -= 2; }
            } else {
                if (!Graph.hasEdge(NId1, NId2)) //Graph.IsEdge(NId1, NId2)) 
                {
                    Graph.AddNodeAndArc(new Interaction(NId1, NId2,Interaction.ArbitraryValue)); 
                }  // good edge
                else {
                // find rnd edge, break and cross-connect
                IEnumerable<Interaction> temp = Graph.GetArcNonAdjNode(new Node[] { NId1, NId2 });
                if (temp.Count() == 0) continue;
                //const TIntPr Edge = GetRndEdgeNonAdjNode(GraphPt, NId1, NId2);
                //if (Edge.Val1==-1) {continue; }
                 Interaction Edge = temp.ElementAt(random.Next(temp.Count()));
                 Graph.RemoveArc(Edge);
                //Graph.DelEdge(Edge.Val1, Edge.Val2);
                 Graph.AddNodeAndArc(new Interaction(NId1,Edge.startNode, Interaction.ArbitraryValue));

                 //Graph.AddEdge(NId1, Edge.Val2);
                 Graph.AddNodeAndArc(new Interaction(NId2, Edge.endNode, Interaction.ArbitraryValue));

                //Graph.AddEdge(NId1, Edge.Val1);
                //Graph.AddEdge(NId2, Edge.Val2);
                }
                if (DegH[NId1]==1) { DegH.Remove(NId1); }
                else { DegH[NId1] -= 1; }
                if (DegH[NId2]==1) { DegH.Remove(NId2); }
                else { DegH[NId2] -= 1; }
            }
            if (++edge % 1000 == 0) {
                User.One.MessageToUser(string.Format("\r {0}k / {1}k", edge/1000, DegSum/2000)); }
                
            }
            return GraphPt;
        }
        /// Returns a random edge in a graph Graph where the edge does not touch nodes NId1 and NId2.
            
            //IEnumerable<Interaction> GetRndEdgeNonAdjNode(BasicNetwork Graph, Node NId1, Node NId2,Mathutil.NumericMath.RandomCraft rand) {
            //  Node NI1, NI2;
            //  int OutDeg = -1;
            //  do {
            //      NI1 = Graph.GetRndNode();//Graph->GetRndNI();
            //      OutDeg = NI1.OutDegree;// NI1.GetOutDeg();
            //  } while (OutDeg == 0);

            //  NI2 = NI1.DesNodes.ElementAt(rand.GetUniDevInt(NI1.OutDegree)); 
            //  int runs = 0;
            //  while (NI1.IsNeighbor(NId1) || NI1.IsNeighbor(NId2) || NI2.IsNeighbor(NId1) || NI2.IsNeighbor(NId2) || NI1.ID == NI2.ID)
            //  {
            //    do {
            //      NI1 = Graph.GetRndNode();
            //      OutDeg = NI1.OutDegree;
            //    } while (OutDeg == 0);
            //    NI2 = NI1.DesNodes.ElementAt(rand.GetUniDevInt(OutDeg));
            //    if (runs++ == 1000) { return null; }
            //  }
            //  return Graph.SelectInteraction(NI1, NI2);
            //}
        /// Generates a random undirect graph with a given degree sequence DegSeqV.
/// Configuration model operates as follows. For each node N, of degree
/// DeqSeqV[N] we create DeqSeqV[N] spokes (half-edges). We then pick two
/// spokes at random, and connect the spokes endpoints. We continue this
/// process until no spokes are left. Generally this generates a multigraph
/// (i.e., spokes out of same nodes can be chosen multiple times).We ignore
/// (discard) self-loops and multiple edges. Thus, the generated graph will
/// only approximate follow the given degree sequence. The method is very fast!
        BasicNetwork GenConfModel4Undirected(BasicNetwork template, List<int> DegSeqV, NumericMath.RandomCraft Rnd) 
{
  int Nodes = DegSeqV.Count;
  BasicNetwork GraphPt =template.CreateObject() as BasicNetwork;
  BasicNetwork Graph = GraphPt;
  //Graph.Reserve(Nodes, -1);
  List<int> NIdDegV = new List<int>(); //TIntV NIdDegV(DegSeqV.Len(), 0);
  
  int DegSum=0, edges=0;
  for (int node = 0; node < Nodes; node++) 
  {
    Graph.AddNode(node.ToString());
    for (int d = 0; d < DegSeqV[node]; d++) 
    { 
        NIdDegV.Add(node); 
    }
    DegSum += DegSeqV[node];
  }
  Netutil.Shuffle<int>(NIdDegV);
  HashSet<Pair<int, int>> EdgeH = new HashSet<Pair<int, int>>();
    //TIntPrSet EdgeH(DegSum/2); // set of all edges, is faster than graph edge lookup

  //if (DegSum % 2 != 0) {
    //printf("Seg seq is odd [%d]: ", DegSeqV.Count);
    //for (int d = 0; d < Math.Min(100, DegSeqV.Count); d++) { printf("  %d", (int)DegSeqV[d]); }
    //printf("\n");
  //}
  int u=0, v=0;
  for (int c = 0; NIdDegV.Count > 1; c++) 
  {
    u = Rnd.GetUniDevInt(NIdDegV.Count);
    while ((v = Rnd.GetUniDevInt(NIdDegV.Count)) == u) { }
    if (u > v) 
        NumericMath.Swap(ref u, ref v); 
    int E1 = NIdDegV[u];
    int E2 = NIdDegV[v];
    if (v == NIdDegV.Count - 1) 
     NIdDegV.RemoveAt(NIdDegV.Count-1); 
    else 
    { 
        NIdDegV[v] = NIdDegV.Last();  //NIdDegV.DelLast(); 
        NIdDegV.RemoveAt(NIdDegV.Count - 1);
    }
    if (u == NIdDegV.Count - 1) 
     NIdDegV.RemoveAt(NIdDegV.Count - 1); 
    else 
    { 
        NIdDegV[u] = NIdDegV.Last(); 
        NIdDegV.RemoveAt(NIdDegV.Count - 1); 
    }
    if (E1 == E2 || EdgeH.Contains(new Pair<int,int>(E1, E2),new ComparePair())) { continue; }
    EdgeH.Add(new Pair<int,int>(E1, E2));
    Graph.AddNodeAndArc(new Interaction(Graph.NewNode(E1.ToString(),null), Graph.NewNode(E2.ToString(),null), Interaction.ArbitraryValue));
    edges++;
    //if (c % (DegSum/100+1) == 0) { printf("\r configuration model: iter %d: edges: %d, left: %d", c, edges, NIdDegV.Count/2); }
  }
  //printf("\n");
  return GraphPt;
}

            class ComparePair : IEqualityComparer<Pair<int,int>>
            {
                public bool Equals(Pair<int, int> x, Pair<int, int> y)
                {
                    if (x.First == y.First && x.Second==y.Second)
                    {
                        return true;
                    }
                    else { return false; }
                }
                public int GetHashCode(Pair<int,int> code)
                {
                    return (int)Mathutil.NumericMath.HashTwoNumber(code.First,code.Second);
                }

            }

        /// <summary>
            /// Create directed network by configure model http://www.quantware.ups-tlse.fr/complexnetworks2012/slides/olvera.pdf
        /// </summary>
        /// <param name="template">The network template for the returning network</param>
        /// <param name="InDegSeqV">in-degree nodes whose order is node ID </param>
        /// <param name="OutDegSeqV">out-degree nodes whose order is node ID</param>
        /// <param name="Rnd">random object</param>
        /// <returns>Network</returns>
            public BasicNetwork GenConfModel4Directed(BasicNetwork template, int Nodes, double inPowerExp, double outPowerExp, NumericMath.RandomCraft Rnd) 
            {
                BasicNetwork kq = null;
                
                //int inDegSum = 0, outDegSum = 0;
                
                    

                //List<int> inDegSeqV = CreatePowerLawDist(Nodes, inPowerExp, Rnd, ref inDegSum);
                //List<int> outDegSeqV = CreatePowerLawDist(Nodes, outPowerExp, Rnd, ref outDegSum);
                //AdjustDegs(inDegSeqV, inDegSum, outDegSeqV, outDegSum);

                List<Pair<int, int>> seq = new List<Pair<int, int>>();
                                          
                seq = DiNetConfigureModel.plbdsgen(Nodes, inPowerExp, outPowerExp, Rnd);
                kq = (new DiNetConfigureModel(seq)).CreateNetwork(template, Rnd, 1);
               
                return kq;
                
            }
            private int AdjustDegs(List<int> inDegSeqV, int inDegSum, List<int> outDegSeqV, int outDegSum)
            {
                if (inDegSum == outDegSum) return inDegSum;
                if (NumericMath.RandomCraft.Next(0, 2) == 1)// increase degree
                {
                    int Gap= Math.Abs(inDegSum -outDegSum);
                    if (inDegSum < outDegSum)
                    {
                        for (int i = 0; i < Gap; i++)
                        {
                            inDegSeqV[NumericMath.RandomCraft.Next(0, inDegSeqV.Count)]++;
                        }
                    }else
                        for (int i = 0; i < Gap; i++)
                            outDegSeqV[NumericMath.RandomCraft.Next(0, outDegSeqV.Count)]++;
                    return Math.Max(inDegSum, outDegSum);

                }
                else // decrease degree
                {
                    int Gap = Math.Abs(inDegSum - outDegSum);
                    if (inDegSum > outDegSum)
                    {
                        for (int i = 0; i < Gap; i++)
                        {
                            int idx = NumericMath.RandomCraft.Next(0, inDegSeqV.Count);
                            if (inDegSeqV[idx] > 0)
                                inDegSeqV[idx]--;
                            else
                                i--;
                        }
                    }
                    else
                        for (int i = 0; i < Gap; i++)
                        {
                            int idx = NumericMath.RandomCraft.Next(0, outDegSeqV.Count);
                            if (outDegSeqV[idx] > 0)
                                outDegSeqV[idx]--;
                            else
                                i--;
                        }

                    return Math.Min(inDegSum, outDegSum);
                }
            }
            private List<int> CreatePowerLawDist(int Nodes, double PowerExp, NumericMath.RandomCraft Rnd, ref int DegSum)
            {
                List<int> DegSeqV = new List<int>();
                DegSum = 0;
                for (int n = 0; n < Nodes; n++)
                {
                    int Val = (int)Math.Round(Rnd.GetPowerDev(PowerExp));
                    
                    if (!(Val >= 1 && Val < Nodes*(float)2/ 3)) { n--; continue; } // skip nodes with too large degree
                    DegSeqV.Add(Val);
                    DegSum += Val;
                }
                if (DegSum % 2 == 1) { DegSeqV[0] += 1; DegSum++; }// Total degree is always even to meet graphic sequence condition http://mathworld.wolfram.com/GraphicSequence.html
                return DegSeqV;
            }
        #endregion
        /// <summary>
        /// Create a SmallWorld network by Kleinberg model (a greedy algorithm using only lo cal information can construct short paths)
        /// </summary>
        /// <param name="n">The size of the lattice or two-dimensional matrix[n x n] </param>
        /// <param name="p">p is lesser than n: fixed connections between n nodes </param>
        /// <param name="q"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public BooleanNetwork generateKleinbergModel(int n, int p, int q, int r)
        {
            BooleanNetwork net = new BooleanNetwork();

            //tworzenie siatki
            Node[,] lattice = net.NewNodeArray(n, n);

            //Create two-dimensional matrix nxn of nodes as elements
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    lattice[i, j] = net.NewNode(i.ToString() + j.ToString(), null);
                    net.AddNode(lattice[i, j]);
                }
            }

            //tworzenie polaczen z sasiadami wg parametru p
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)// for each node in the matrix
                {
                    for (int m = 0; m <= p; m++) // m <= p < n
                    {
                        for (int k = i - m; k <= i + m; k++) //k = [i-m, i+m]
                        {
                            for (int l = j - m; l <= j + m; l++) // l=[j-m, j+m]
                            {
                                if ((l <= k + j - i + m) && (l >= -k + j + i - m) && (l <= -k + j + i + m) && (l >= k + j - i - m))
                                {
                                    if (k >= 0 && k < n && l >= 0 && l < n && !lattice[i, j].Equals(lattice[k, l]))
                                    {
                                        Interaction edge = Interaction.RandomInteraction(lattice[i, j], lattice[k, l], Interaction.ArbitraryValue);
                                        //edge.id = "edge" + i + j + "_" + k + l;
                                        //edge.directed = true;
                                        //edge.source = lattice[i, j];
                                        //edge.target = lattice[k, l];
                                        //lattice[i, j].Arcs.Add(edge);
                                        //lattice[k, l].Arcs.Add(edge);
                                        net.AddArc(edge);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //tworzenie losowych polaczen
            int qe = 0;
            Random random = new Random();
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < n; k++)
                    {
                        for (int l = 0; l < n; l++)
                        {
                            sum += Math.Pow((Math.Abs(k - i) + Math.Abs(l - j)), -r);
                        }
                    }
                    double rand = random.NextDouble();
                    for (int k = 0; k < n; k++)
                    {
                        for (int l = 0; l < n; l++)
                        {
                            if (!lattice[i, j].Equals(lattice[k, l]))
                            {
                                double prob = Math.Pow((Math.Abs(k - i) + Math.Abs(l - j)), -r);// / sum;
                                if (prob >= rand)
                                {
                                    Interaction edge = Interaction.RandomInteraction(lattice[i, j], lattice[k, l], Interaction.ArbitraryValue);
                                    //edge.id = "edge" + i + j + "_" + k + l;
                                    //edge.directed = true;
                                    //edge.source = lattice[i, j];
                                    //edge.target = lattice[k, l];
                                    //lattice[i, j].Arcs.Add(edge);
                                    //lattice[k, l].Arcs.Add(edge);
                                    net.AddArc(edge);
                                    qe++;
                                    if (qe == q) return net;
                                }
                            }
                        }
                    }
                }
            }
            return net;
        }

        public BooleanNetwork generateAlfaModel(int n, double k, double alfa)
        {
            BooleanNetwork net = new BooleanNetwork();

            for (int i = 0; i < n; i++)
            {
                Node node = net.NewNode(i.ToString(), null);
                net.AddNode(node);
            }

            Random random = new Random();
            while (true)
            {
                double p = random.NextDouble() * Math.Pow(calculateNewtonSymbol(n, 2), -2);
                for (int i = 0; i < n; i++)
                {
                    double r = random.NextDouble();
                    double sumRij = 0;
                    double[,] R = new double[n,n];
                    for (int j = 0; j < n; j++)
                    {
                        R[i,j] = calculateRij(net.Nodes.ElementAt(i), net.Nodes.ElementAt(j), numOfVerticesAdjacentTo(net.Nodes.ElementAt(i), net.Nodes.ElementAt(j)), k, p, alfa);
                        sumRij += R[i,j];
                    }

                    double pij = 0;
                    for (int j = 0; j < n; j++)
                    {
                        if (i != j)
                        {
                            pij += R[i, j] / sumRij;
                            if (r <= pij)
                            {
                                Interaction edge = Interaction.RandomInteraction(net.Nodes.ElementAt(i), net.Nodes.ElementAt(j), Interaction.ArbitraryValue);
                                //edge.id = "edge" + i + "_" + j;
                                //edge.directed = false;
                                //edge.source = net.Nodes[i];
                                //edge.target = net.Nodes[j];
                                //net.Nodes[i].Arcs.Add(edge);
                                //net.Nodes[j].Arcs.Add(edge);
                                net.AddArc(edge);
                                if (calculateAverageGraphDegree(net) >= k) return net;
                                break;
                            }
                        }
                    }
                }

            }
            
            //return net;
        }

        private int calculateAverageGraphDegree(BooleanNetwork net)
        {
            return 2 * net.Arcs.Count() / net.Nodes.Count();
        }

        private int numOfVerticesAdjacentTo(Node node1, Node node2)
        {
            int n = 0;
            foreach (Interaction edge in node1.Arcs)
            {
                if ((node1.name.Equals(edge.startNode.name) && nodesConnected(node2.Arcs, node2, edge.endNode)) || (node1.name.Equals(edge.endNode.name) && nodesConnected(node2.Arcs, node2, edge.startNode)))
                {
                    n++;
                }
            }
            return n;
        }

        private double calculateRij(Node nodei, Node nodej, int mij, double k, double p, double alfa)
        {
            if (nodei.name.Equals(nodej.name) || nodesConnected(nodei.Arcs, nodei, nodej)) return 0;
            if (mij >= k) return 1;
            if (mij == 0) return p;

            return Math.Pow(mij / k, alfa) * (1 - p) + p;
        }

        private int calculateNewtonSymbol(int n, int k)
        {
            int result = 1;
            for (int i = 1; i < k; i++)
            {
                result *= (n - i + 1) / i;
            }
            return result;
        }
    }
}
