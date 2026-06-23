using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using NetSimulation.Lib;
using BasicNet;
using NetSimulation.Community;
using System.Diagnostics;
namespace BasicNet
{
    public class OptimizerModularityExactly
    {
        /**
  * Returns the negative modularity.
  * @param interAtedges  edge weight between different clusters
  * @param interAtpairs  weighted node pairs between different clusters
  * @param atedges  total edge weight of the graph
  * @param atpairs  total weighted node pairs of the graph
  * @return negative modularity
  */
        private double quality(double interAtedges, double interAtpairs,
                double atedges, double atpairs)
        {
            return interAtedges / atedges - interAtpairs / atpairs;
        }


        /**
         * Improves a graph clustering by greedily moving nodes between clusters.
         * @param nodeToCluster  graph nodes with their current clusters 
         *   (input and output parameter)
         * @param nodeToEdges  graph nodes with their incident edges
         * @param atedges  total edge weight of the graph
         * @param atpairs  total weighted node pairs of the graph
         */
        private double refine(Dictionary<Node, int> nodeToCluster, Dictionary<Node, List<Interaction>> nodeToEdges,
                double atedges, double atpairs)
        {
            
                int maxCluster = 0;
                foreach (int cluster in nodeToCluster.Values)
                {
                    maxCluster = Math.Max(maxCluster, cluster);
                }

                // compute clusterToAtnodes, interAtedges, interAtpairs
                double[] clusterToAtnodes = new double[nodeToCluster.Keys.Count + 1];
                foreach (Node node in nodeToCluster.Keys)
                {
                    clusterToAtnodes[nodeToCluster[node]] += node.weight;
                }
                double interAtedges = 0.0;
                foreach (List<Interaction> edges in nodeToEdges.Values)
                {
                    foreach (Interaction edge in edges)
                    {
                        if (!nodeToCluster[edge.startNode].Equals(nodeToCluster[edge.endNode]))
                        {
                            interAtedges += edge.weight;
                        }
                    }
                }
                double interAtpairs = 0.0;
                foreach (Node node in nodeToCluster.Keys) interAtpairs += node.weight;
                interAtpairs *= interAtpairs;
                foreach (double clusterAtnodes in clusterToAtnodes) interAtpairs -= clusterAtnodes * clusterAtnodes;

                // greedily move nodes between clusters 
                double prevQuality = Double.MaxValue;
                double mquality = quality(interAtedges, interAtpairs, atedges, atpairs);
                
                //User.One.MessageToUser("Refining " + nodeToCluster.Keys.Count
                //                               + " nodes, initial modularity " + -mquality);
                while (mquality < prevQuality)
                {
                    prevQuality = mquality;
                    //foreach (Node node in nodeToCluster.Keys)
                    //{
                    for (int i = 0; i < nodeToCluster.Keys.Count; i++) // replacing for "foreach (Node node in nodeToCluster.Keys)" 
                    {
                        Node node = nodeToCluster.Keys.ElementAt(i);// replacing for "foreach (Node node in nodeToCluster.Keys)"

                        int bestCluster = 0;
                        double bestQuality = mquality, bestInterAtedges = interAtedges, bestInterAtpairs = interAtpairs;
                        double[] clusterToAtedges = new double[nodeToCluster.Keys.Count + 1];
                        foreach (Interaction edge in nodeToEdges[node])
                        {
                            if (!edge.endNode.Equals(node))
                            {
                                // count weight twice to include reverse edge
                                clusterToAtedges[nodeToCluster[edge.endNode]] += 2 * edge.weight;
                            }
                        }
                        int cluster = nodeToCluster[node];
                        for (int newCluster = 0; newCluster <= maxCluster + 1; newCluster++)
                        {
                            if (cluster == newCluster) continue;
                            double newInterPairs = interAtpairs
                                + clusterToAtnodes[cluster] * clusterToAtnodes[cluster]
                                - (clusterToAtnodes[cluster] - node.weight) * (clusterToAtnodes[cluster] - node.weight)
                                + clusterToAtnodes[newCluster] * clusterToAtnodes[newCluster]
                                - (clusterToAtnodes[newCluster] + node.weight) * (clusterToAtnodes[newCluster] + node.weight);
                            double newInterEdges = interAtedges
                                + clusterToAtedges[cluster]
                                - clusterToAtedges[newCluster];
                            double newQuality = quality(newInterEdges, newInterPairs, atedges, atpairs);
                            if (bestQuality - newQuality > 1e-8)
                            {
                                bestCluster = newCluster;
                                bestQuality = newQuality; bestInterAtedges = newInterEdges; bestInterAtpairs = newInterPairs;
                            }
                        }
                        if (bestQuality < mquality)
                        {
                            clusterToAtnodes[cluster] -= node.weight;
                            clusterToAtnodes[bestCluster] += node.weight;
                            nodeToCluster[node] = bestCluster;
                            maxCluster = Math.Max(maxCluster, bestCluster);
                            mquality = bestQuality; interAtedges = bestInterAtedges; interAtpairs = bestInterAtpairs;
                            //User.One.MessageToUser(" Moving " + node + " to " + bestCluster + ", "
                            //        + "new modularity " + -mquality);
                        }
                    }
                }
                return -mquality;
        }
        public class EdgeComparer : Comparer<Interaction>
        {
            public override int Compare(Interaction e1, Interaction e2)
            {
                if (e1.density == e2.density) return 0;
                return e1.density < e2.density ? +1 : -1;
            }
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
            private Dictionary<Node, int> cluster(BasicNetwork Net, ICollection<Node> nodes, List<Interaction> edges,
                    double atedges, double atpairs, ref double modularity)
            {
                //User.One.MessageToUser("Contracting " + nodes.Count + " nodes, " + edges.Count + " edges");

                // contract nodes
                //Collections.sort(edges, new Comparator<Edge>() { 
                //    public int compare(Edge e1, Edge e2) {
                //        if (e1.density == e2.density) return 0;
                //        return e1.density < e2.density ? +1 : -1;
                //    }
                //});
                edges.Sort(new EdgeComparer());
                Dictionary<Node, Node> nodeToContr = new Dictionary<Node, Node>();
                List<Node> contrNodes = new List<Node>();
                foreach (Interaction edge in edges)
                {
                    if (edge.density < atedges / atpairs) break;
                    if (edge.startNode.Equals(edge.endNode)) continue;
                    if (nodeToContr.ContainsKey(edge.startNode) || nodeToContr.ContainsKey(edge.endNode)) continue;
                    // randomize contraction
                    // if (!nodeToContr.isEmpty() && Math.random() < 0.5) continue;

                    //User.One.MessageToUser(" Contracting " + edge);
                    Node contrNode = Net.NewNode(
                            edge.startNode.name + " " + edge.endNode.name,null,
                            edge.startNode.weight + edge.endNode.weight);
                    nodeToContr[edge.startNode] = contrNode;
                    nodeToContr[edge.endNode] = contrNode;
                    contrNodes.Add(contrNode);
                }
                // terminal case: no nodes to contract
                if (nodeToContr.Count == 0)
                {
                    Dictionary<Node, int> mnodeToCluster = new Dictionary<Node, int>();
                    int clusterId = 0;
                    foreach (Node node in nodes) mnodeToCluster[node] = clusterId++;
                    return mnodeToCluster;
                }
                // "contract" singleton clusters
                foreach (Node node in nodes)
                {
                    if (!nodeToContr.ContainsKey(node))
                    {
                        Node contrNode = Net.NewNode(node.name, null, node.weight);//new Node(node.name, node.weight);
                        nodeToContr[node] = contrNode;
                        contrNodes.Add(contrNode);
                    }
                }

                // contract edges
                Dictionary<Node, Dictionary<Node, Double>> startToEndToWeight = new Dictionary<Node, Dictionary<Node, double>>();
                foreach (Node contrNode in contrNodes)
                {
                    startToEndToWeight[contrNode] = new Dictionary<Node, double>();
                }
                foreach (Interaction edge in edges)
                {
                    Node contrStart = nodeToContr[edge.startNode];
                    Node contrEnd = nodeToContr[edge.endNode];
                    double contrWeight = 0.0;
                    Dictionary<Node, Double> endToWeight = startToEndToWeight[contrStart];
                    if (endToWeight.ContainsKey(contrEnd))
                    {
                        contrWeight = endToWeight[contrEnd];
                    }
                    endToWeight[contrEnd] = contrWeight + edge.weight;
                }
                List<Interaction> contrEdges = new List<Interaction>();
                foreach (Node contrStart in startToEndToWeight.Keys)
                {
                    Dictionary<Node, Double> endToWeight = startToEndToWeight[contrStart];
                    foreach (Node contrEnd in endToWeight.Keys)
                    {
                        Interaction contrEdge = new Interaction(contrStart, contrEnd, Interaction.ArbitraryValue, "",endToWeight[contrEnd]);
                        contrEdges.Add(contrEdge);
                    }
                }

                // cluster contracted graph
                Dictionary<Node, int> contrNodeToCluster
                    = cluster(Net, contrNodes, contrEdges, atedges, atpairs, ref modularity);

                // decontract clustering
                Dictionary<Node, int> nodeToCluster = new Dictionary<Node, int>();
                foreach (Node node in nodeToContr.Keys)
                {
                    nodeToCluster[node] = contrNodeToCluster[nodeToContr[node]];
                }

                // refine decontracted clustering
                Dictionary<Node, List<Interaction>> nodeToEdge = new Dictionary<Node, List<Interaction>>();
                foreach (Node node in nodes) nodeToEdge[node] = new List<Interaction>();
                foreach (Interaction edge in edges) nodeToEdge[edge.startNode].Add(edge);
                modularity=refine(nodeToCluster, nodeToEdge, atedges, atpairs);

                return nodeToCluster;
            }


            /**
             * Computes a clustering of a given graph by maximizing the Modularity.
             * @param nodes  weighted nodes of the graph.
             *   It is recommended to set the weight of each node to the sum 
             *   of the weights of its edges.  Weights must not be negative.   
             * @param edges  weighted edges of the graph.
             *   Omit edges with weight 0.0 (i.e. non-edges).  
             *   For unweighted graphs use weight 1.0 for all edges.
             *   Weights must not be negative.   
             *   Weights must be symmetric, i.e. the weight  
             *   from node <code>n1</code> to node <code>n2</code> must be equal to
             *   the weight from node <code>n2</code> to node <code>n1</code>. 
             * @param ignoreLoops  set to <code>true</code> to use an adapted version
             *   of Modularity for graphs without loops (edges whose start node
             *   equals the end node)
             * @return clustering with large Modularity,
             *   as map from graph nodes to cluster IDs. 
             */
            public Dictionary<Node, int> execute(BasicNetwork Net,
                    List<Node> nodes, List<Interaction> edges,
                    Boolean ignoreLoops, ref double modularity)
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
                return cluster(Net,nodes, edges, atedgeCnt, atpairCnt, ref modularity);
            }

            public static Dictionary<Node, int> ClusterGraph(BasicNetwork Net, bool isarc, ref double modularity)
            {
                GraphData graph = isarc?GraphData.Convert(Net.Arcs):GraphData.Convert(Net.Edges);
                
                //To avoid weight = -1
                
                foreach (string start in graph.Data.Keys)
                    for (int i = 0; i < graph.Data[start].Keys.Count; i++)
                        if (graph.Data[start][graph.Data[start].Keys.ElementAt(i)] < 0)
                            graph.Data[start][graph.Data[start].Keys.ElementAt(i)] = 1;
                //To avoid weight = -1

                graph = GraphData.makeSymmetricGraph(graph);
                Dictionary<string, Node> nameToNode = GraphData.makeNodes(Net, graph);
                List<Node> nodes = new List<Node>(nameToNode.Values);
                List<Interaction> edges = GraphData.makeEdges(graph, nameToNode);

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
                    OptimizerModularityExactly optimizer = new OptimizerModularityExactly();
                    Dictionary<Node, int> nodeToClusterTemp =
                            optimizer.execute(Net, nodes, edges, false, ref tempModularity);
                    if (modularity < tempModularity)
                    {
                        modularity = tempModularity;
                        nodeToCluster = nodeToClusterTemp;
                        User.One.MessageToUser("Modularity = " + modularity);
                    }
                    
                }
                   
                   
                /*
                //writePositions(nodeToPosition, nodeToCluster, args[2]);

        
                (new GraphFrame(nodeToPosition, nodeToCluster)).setVisible(true);
                 */
                return nodeToCluster;
            }
         public static void DumpNode(List<Node> nodes)
        {
            int i=0;
            foreach(Node n in nodes)
            {

                Debug.WriteLine(++i+"\t"+n.name+"\t"+n.weight);
            }
        }
        public static void DumpEdge(List<Interaction> edges)
        {
            int i=0;
            foreach(Interaction e in edges)
            {

                Debug.WriteLine(++i + "\t" + e + "\t" + e.weight + "\t" + e.density);
            }
        }
        
    }
}
