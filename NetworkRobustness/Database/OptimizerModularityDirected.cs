using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Collections;
using NetSimulation.Community;
using NetSimulation.Lib;
using Mathutil;
namespace BasicNet
{
    public class OptimizerModularityDirected
    {
        /// <summary>
        /// Returns the negative modularity
        /// </summary>
        /// <param name="insideAtedges">edge weight inside clusters</param>
        /// <param name="totalWinWout">Total of in-weight x out-weight</param>
        /// <param name="atedges">total edge weight of the graph</param>
        /// <returns>total weighted node pairs of the graph</returns>
        private double directedquality(double insideAtedges, double totalWinWout,
                double atedges)
        {
            return -insideAtedges / atedges + totalWinWout / (atedges * atedges);
        }
       
        private double directedquality(Dictionary<Node, int> nodeToCluster, Dictionary<Node, List<Interaction>> nodeToEdges,           
                double atedges)
        {
            int nCluster = nodeToCluster.Keys.Count + 1;
            double totalWinWout = 0;
            Dictionary<Node, double> inDegree = new Dictionary<Node, double>();
            foreach (Node n in nodeToCluster.Keys)
                inDegree[n] = 0;

            Dictionary<Node, double> outDegree = new Dictionary<Node, double>();
            foreach (Node n in nodeToCluster.Keys)
                outDegree[n] = 0;
            // sum of edge weights that the edges are in the same clusters
            double inClusteredges = 0.0;// (2) wii Total of weight of links in all clusters

            //Dictionary<int, HashSet<Interaction>> inclusterLinks = new Dictionary<int,HashSet<Interaction>>();
            foreach (List<Interaction> edges in nodeToEdges.Values)
            {
                foreach (Interaction edge in edges)
                {
                    //for a link in the module
                    if (nodeToCluster[edge.startNode].Equals(nodeToCluster[edge.endNode]))
                    {
                        inClusteredges += edge.weight;
                    }
                    //if (edge.Direction == Interaction.DirectionType.undirected)// for a heterogeneous network with 02 types of direction
                    //{
                    //    inDegree[edge.endNode] += edge.weight;
                    //    inDegree[edge.startNode] += edge.weight;
                    //    outDegree[edge.startNode] += edge.weight;
                    //    outDegree[edge.endNode] += edge.weight;
                    //}
                    //else // for a directed network with its direction defaully considered as directed link.
                    //{
                        inDegree[edge.endNode] +=  edge.weight;
                        outDegree[edge.startNode] += edge.weight;
                    //}
                }
            }


            double[] TotalInDegree = new double[nCluster];
            double[] TotalOutDegree = new double[nCluster];
            foreach (Node n in nodeToCluster.Keys)
            {
                TotalInDegree[nodeToCluster[n]] += inDegree[n];
                TotalOutDegree[nodeToCluster[n]] += outDegree[n];
            }

            
            for (int i = 0; i < nCluster; i++)
                totalWinWout += TotalInDegree[i] * TotalOutDegree[i];
            return directedquality(inClusteredges, totalWinWout, atedges);
        }
        private int MoveNodeToNewCluster(Node theNode, int newCluster, Dictionary<Node, int> nodeToCluster, Dictionary<Node, List<Interaction>> nodeToEdges)
        {
            int oldCluster = nodeToCluster[theNode];
            nodeToCluster[theNode] = newCluster;
            return oldCluster;
        }
        
        /// <summary>
        /// Improves a graph clustering by greedily moving nodes between clusters.
        /// </summary>
        /// <param name="nodeToCluster">graph nodes with their current clusters (input and output parameter)</param>
        /// <param name="nodeToEdges">graph nodes with their incident edges</param>
        /// <param name="atedges">total edge weight of the graph</param>
        /// <param name="atpairs">total weighted node pairs of the graph</param>
        /// <returns></returns>
        private double refine(Dictionary<Node, int> nodeToCluster, Dictionary<Node, List<Interaction>> nodeToEdges,
                 double atedges, double atpairs)
        {

            //Maximum cluster index
            int maxCluster = 0;


            //find the maximum cluster Index
            foreach (int cluster in nodeToCluster.Values)
            {
                maxCluster = Math.Max(maxCluster, cluster);
            }

            // greedily move nodes between clusters 
            double prevQuality = Double.MaxValue;
            double mquality = directedquality(nodeToCluster, nodeToEdges, atedges);
            Debug.WriteLine("Refining " + nodeToCluster.Keys.Count
                                           + " nodes, initial modularity " + -mquality);

            while (mquality < prevQuality)
            {
                prevQuality = mquality;
                //foreach (Node node in nodeToCluster.Keys) 
                for (int i = 0; i < nodeToCluster.Keys.Count; i++) // replacing for "foreach (Node node in nodeToCluster.Keys)" 
                {
                    Node node = nodeToCluster.Keys.ElementAt(i);// replacing for "foreach (Node node in nodeToCluster.Keys)"

                    int bestCluster = 0;
                    double bestQuality = mquality;
                    
                    int cluster = nodeToCluster[node];// select the cluster of the node
                    for (int newCluster = 0; newCluster <= maxCluster + 1; newCluster++)
                    {
                        if (cluster == newCluster) continue;

                        int oldCluster = MoveNodeToNewCluster(node, newCluster, nodeToCluster, nodeToEdges);
                        double newQuality = directedquality(nodeToCluster, nodeToEdges, atedges);
                        MoveNodeToNewCluster(node, oldCluster, nodeToCluster, nodeToEdges);

                        //If it's better movement (lower quality)
                        if (bestQuality - newQuality > 1e-8)
                        {
                            bestCluster = newCluster;
                            bestQuality = newQuality; 
                        }
                    }
                    if (bestQuality < mquality)
                    {
                        nodeToCluster[node] = bestCluster;

                        maxCluster = Math.Max(maxCluster, bestCluster);
                        mquality = bestQuality; 
                        Debug.WriteLine(" Moving " + node + " to " + bestCluster + ", "
                                + "new modularity " + -mquality);
                    }
                }
            }
            return -mquality;
        }


        /**
         * Computes a graph clustering with a multi-scale algorithm.
         * @param nodes  graph nodes
         * @param edges  graph edges
         * @param atedges  total edge weight of the graph
         * @param atpairs  total weighted node pairs of the graph
         * @return clustering with large Modularity,
         *   as map from graph nodes to cluster IDs. 
         */
        public class EdgeComparer : Comparer<Interaction>
        {
            public override int Compare(Interaction e1, Interaction e2)
            {
                if (e1.density == e2.density) return 0;
                return e1.density < e2.density ? +1 : -1;
            }

        }
        /// <summary>
        /// Cluster the graph
        /// </summary>
        /// <param name="nodes">The node list of the graph</param>
        /// <param name="edges">The edge list of the graph</param>
        /// <param name="atedges">Sum of edge weights</param>
        /// <param name="atpairs">Sum of node weights</param>
        /// <param name="Modularity">the returned modularity of the graph</param>
        /// <returns></returns>
        private Dictionary<Node, int> cluster(BasicNetwork Net, ICollection<Node> nodes, List<Interaction> edges,
                 double atedges, double atpairs, ref double Modularity)
        {
            //System.out.println("Contracting " + nodes.size() + " nodes, " + edges.size() + " edges");
            Debug.WriteLine("Contracting " + nodes.Count() + " nodes, " + edges.Count + " edges");

            edges.Sort(new EdgeComparer());
            // contract nodes
            //Collections.sort(edges, new Comparator<Edge>() { 
            //    public int compare(Edge e1, Edge e2) {
            //        if (e1.density == e2.density) return 0;
            //        return e1.density < e2.density ? +1 : -1;
            //    }
            //});

            Dictionary<Node, Node> nodeToContr = new Dictionary<Node, Node>();
            List<Node> contrNodes = new List<Node>();
            foreach (Interaction edge in edges)
            {
                if (edge.density < atedges / atpairs) break;
                if (edge.startNode.Equals(edge.endNode)) continue;
                if (nodeToContr.ContainsKey(edge.startNode) || nodeToContr.ContainsKey(edge.endNode)) continue;
                // randomize contraction
                // if (!nodeToContr.isEmpty() && Math.random() < 0.5) continue;

                //System.out.println(" Contracting " + edge);
                Debug.WriteLine(" Contracting " + edge);
                Node contrNode = Net.NewNode(edge.startNode.name + " " + edge.endNode.name, null,
                        edge.startNode.weight + edge.endNode.weight);
                nodeToContr[edge.startNode] = contrNode;
                nodeToContr[edge.endNode] = contrNode;
                contrNodes.Add(contrNode);
            }
            // terminal case: no nodes to contract
            if (nodeToContr.Count == 0)
            {
                Dictionary<Node, int> nodeToCluster = new Dictionary<Node, int>();
                int clusterId = 0;
                foreach (Node node in nodes) nodeToCluster[node] = clusterId++;
                return nodeToCluster;
            }
            // "contract" singleton clusters
            foreach (Node node in nodes)
            {
                if (!nodeToContr.ContainsKey(node))
                {
                    Node contrNode = Net.NewNode(node.name, null, node.weight);
                    nodeToContr[node] = contrNode;
                    contrNodes.Add(contrNode);
                }
            }

            // contract edges
            Dictionary<Node, Dictionary<Node, double>> startToEndToWeight = new Dictionary<Node, Dictionary<Node, double>>();
            foreach (Node contrNode in contrNodes)
            {
                startToEndToWeight[contrNode] = new Dictionary<Node, double>();
            }
            foreach (Interaction edge in edges)
            {
                Node contrStart = nodeToContr[edge.startNode];
                Node contrEnd = nodeToContr[edge.endNode];
                double contrWeight = 0.0f;
                Dictionary<Node, double> endToWeight = startToEndToWeight[contrStart];
                if (endToWeight.ContainsKey(contrEnd))
                {
                    contrWeight = endToWeight[contrEnd];
                }
                endToWeight[contrEnd] = contrWeight + edge.weight;
            }
            List<Interaction> contrEdges = new List<Interaction>();
            foreach (Node contrStart in startToEndToWeight.Keys)
            {
                Dictionary<Node, double> endToWeight = startToEndToWeight[contrStart];
                foreach (Node contrEnd in endToWeight.Keys)
                {
                    Interaction contrEdge = new Interaction(contrStart, contrEnd, Interaction.DefaultValue, "",endToWeight[contrEnd]);
                    contrEdges.Add(contrEdge);
                }
            }

            // cluster contracted graph
            Dictionary<Node, int> contrNodeToCluster
                = cluster(Net, contrNodes, contrEdges, atedges, atpairs, ref Modularity);

            // decontract clustering
            Dictionary<Node, int> mnodeToCluster = new Dictionary<Node, int>();
            foreach (Node node in nodeToContr.Keys)
            {
                mnodeToCluster[node] = contrNodeToCluster[nodeToContr[node]];
            }

            // refine decontracted clustering
            Dictionary<Node, List<Interaction>> nodeToEdge = new Dictionary<Node, List<Interaction>>();
            foreach (Node node in nodes) nodeToEdge[node] = new List<Interaction>();
            foreach (Interaction edge in edges) nodeToEdge[edge.startNode].Add(edge);
            Modularity = refine(mnodeToCluster, nodeToEdge, atedges, atpairs);
            Debug.WriteLine(string.Format("-######### modularity = {0}", Modularity));

            return mnodeToCluster;
        }

        /// <summary>
        /// Computes a clustering of a given graph by maximizing the Modularity.
        /// </summary>
        /// <param name="nodes">weighted nodes of the graph
        /// It is recommended to set the weight of each node to the sum
        /// of the weights of its edges.  Weights must not be negative.  </param>
        /// <param name="edges">weighted edges of the graph.
        /// Omit edges with weight 0.0 (i.e. non-edges). 
        /// For unweighted graphs use weight 1.0 for all edges.
        /// Weights must not be negative.  
        /// Weights must be symmetric, i.e. the weight 
        /// from node <code>n1</code> to node <code>n2</code> must be equal to
        /// the weight from node <code>n2</code> to node <code>n1</code>. </param>
        /// <param name="ignoreLoops">set to <code>true</code> to use an adapted version
        /// of Modularity for graphs without loops (edges whose start node
        /// equals the end node)</param>
        /// <param name="modularity">modularity of the division returned</param>
        /// <returns>clustering with large Modularity, as map from graph nodes to cluster IDs.</returns>
        public Dictionary<Node, int> execute(BasicNetwork Net, List<Node> nodes, List<Interaction> edges, bool ignoreLoops, ref double modularity)
        {
            // compute atedgeCnt and atpairCnt
            double atedgeCnt = 0.0;
            foreach (Interaction edge in edges)
            {
                if (!ignoreLoops || !edge.startNode.Equals(edge.endNode))
                {
                    atedgeCnt += edge.weight;
                }
            }
            double atpairCnt = 0.0;
            foreach (Node node in nodes) atpairCnt += node.weight;
            atpairCnt *= atpairCnt;
            if (ignoreLoops)
            {
                foreach (Node node in nodes) atpairCnt -= node.weight * node.weight;
            }

            // compute clustering
            return cluster(Net, nodes, edges, atedgeCnt, atpairCnt, ref modularity);
        }
        public static Dictionary<Node, int> ClusterGraph(BasicNetwork Net, ref double modularity)
        {
            //GraphData graph = GraphData.Convert(Net.Arcs);

            ////To avoid weight = -1

            //foreach (string start in graph.Data.Keys)
            //    for (int i = 0; i < graph.Data[start].Keys.Count; i++)
            //        if (graph.Data[start][graph.Data[start].Keys.ElementAt(i)] < 0)
            //            graph.Data[start][graph.Data[start].Keys.ElementAt(i)] = 1;
            ////To avoid weight = -1

            //graph = GraphData.makeSymmetricGraph(graph);
            //Dictionary<string, Node> nameToNode = GraphData.makeNodes(Net, graph);
            //List<Node> nodes = new List<Node>(nameToNode.Values);
            //List<Interaction> edges = GraphData.makeEdges(graph, nameToNode);


            Dictionary<string, Node> nameToNode = GraphData.makeNodes2(Net);
            List<Node> nodes = new List<Node>(nameToNode.Values);
            List<Interaction> edges = GraphData.makeEdges2(Net, nameToNode);
            



            // see class MinimizerBarnesHut for a description of the parameters;
            // for classical "nice" layout (uniformly distributed nodes), use

            //new MinimizerBarnesHut3(nodes, edges, -1.0, 2.0, 0.05).minimizeEnergy(nodeToPosition, 100);
            //new MinimizerBarnesHut3(nodes, edges, 0.0, 1.0, 0.05).minimizeEnergy(nodeToPosition, 100);
            // see class OptimizerModularity for a description of the parameters
            OptimizerModularityDirected optimizer = new OptimizerModularityDirected();
            Dictionary<Node, int> nodeToCluster =
                    optimizer.execute(Net, nodes, edges, false, ref modularity);

            /*
            //writePositions(nodeToPosition, nodeToCluster, args[2]);

        
            (new GraphFrame(nodeToPosition, nodeToCluster)).setVisible(true);
             */
            return nodeToCluster;
        }
        public static Dictionary<Node, int> ClusterGraphExactly(BasicNetwork Net, ref double modularity)
        {
            //GraphData graph = GraphData.Convert(Net.Arcs);

            ////To avoid weight = -1

            //foreach (string start in graph.Data.Keys)
            //    for (int i = 0; i < graph.Data[start].Keys.Count; i++)
            //        if (graph.Data[start][graph.Data[start].Keys.ElementAt(i)] < 0)
            //            graph.Data[start][graph.Data[start].Keys.ElementAt(i)] = 1;
            ////To avoid weight = -1

            //graph = GraphData.makeSymmetricGraph(graph);
            //Dictionary<string, Node> nameToNode = GraphData.makeNodes(Net, graph);
            //List<Node> nodes = new List<Node>(nameToNode.Values);
            //List<Interaction> edges = GraphData.makeEdges(graph, nameToNode);


            Dictionary<string, Node> nameToNode = GraphData.makeNodes2(Net);
            List<Node> nodes = new List<Node>(nameToNode.Values);
            List<Interaction> edges = GraphData.makeEdges2(Net, nameToNode);


            double tempModularity =modularity= double.MinValue;
                
            Dictionary<Node, int> nodeToCluster = null;
            for (int i = 0; i < 10; i++)
            {
                User.One.ShowWaitIndicator(i, 10);
                //Shuffle the edges to find different modularity solutions
                Netutil.Shuffle<Interaction>(edges);

                // see class MinimizerBarnesHut for a description of the parameters;
                // for classical "nice" layout (uniformly distributed nodes), use

                //new MinimizerBarnesHut3(nodes, edges, -1.0, 2.0, 0.05).minimizeEnergy(nodeToPosition, 100);
                //new MinimizerBarnesHut3(nodes, edges, 0.0, 1.0, 0.05).minimizeEnergy(nodeToPosition, 100);
                // see class OptimizerModularity for a description of the parameters
                OptimizerModularityDirected optimizer = new OptimizerModularityDirected();
                Dictionary<Node, int> nodeToClusterTemp =
                        optimizer.execute(Net, nodes, edges, false, ref tempModularity);
                if (modularity < tempModularity)
                {
                    
                    modularity = tempModularity;
                    nodeToCluster = nodeToClusterTemp;
                    User.One.MessageToUser("Modularity = " + modularity);
                }
                
            }
                   
             
            return nodeToCluster;
        }
   
    }
}
