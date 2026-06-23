using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Collections;
using NetSimulation.Community;
namespace BasicNet
{
    //Copyright (C) 2008 Andreas Noack
//
//This library is free software; you can redistribute it and/or
//modify it under the terms of the GNU Lesser General Public
//License as published by the Free Software Foundation; either
//version 2.1 of the License, or (at your option) any later version.
//
//This library is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//Lesser General Public License for more details.
//
//You should have received a copy of the GNU Lesser General Public
//License along with this library; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA 

/**
 * Optimizer for a generalization of Newman and Girvan's Modularity measure,
 *   for computing graph clusterings.
 * The Modularity measure is generalized to arbitrary node weights;
 *   it is recommended to set the weight of each node to its degree,
 *   i.e. the total weight of its edges, as Newman and Girvan did.
 * For more information on the (used version of the) Modularity measure, see
 *   M. E. J. Newman: "Analysis of weighted networks", 
 *   Physical Review E 70, 056131, 2004.
 * For the relation of Modularity to the LinLog energy model, see
 *   Andreas Noack: <a href="http://arxiv.org/abs/0807.4052">
 *   "Modularity clustering is force-directed layout"</a>,
 *   Preprint arXiv:0807.4052, 2008.
 *   
 * @author Andreas Noack (an@informatik.tu-cottbus.de)
 * @version 13.11.2008
 */
public class OptimizerModularity 
{

    /// <summary>
    /// Returns the negative modularity
    /// </summary>
    /// <param name="interAtedges">edge weight between different clusters</param>
    /// <param name="interAtpairs">weighted node pairs between different clusters</param>
    /// <param name="atedges">total edge weight of the graph</param>
    /// <param name="atpairs">total weighted node pairs of the graph</param>
    /// <returns>total weighted node pairs of the graph</returns>
    private double quality( double interAtedges, double interAtpairs, 
            double atedges,  double atpairs) {
        return interAtedges/atedges - interAtpairs/atpairs;
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
             double atedges,  double atpairs) {
        
        //Maximum cluster index
        int maxCluster = 0;
        

        //find the maximum cluster Index
        foreach (int cluster in nodeToCluster.Values) {
            maxCluster = Math.Max(maxCluster, cluster);
        }

        // compute clusterToAtnodes, interAtedges, interAtpairs

        //each element is a cluster and saves sum of node weights - node degree
        double[] clusterToAtnodes = new double[nodeToCluster.Keys.Count+1];
       
        foreach (Node node in nodeToCluster.Keys) {
            clusterToAtnodes[nodeToCluster[node]] += node.weight;
        }

        // sum of edge weights that the edges are in the different clusters
        double interAtedges = 0.0;
        foreach (List<Interaction> edges in nodeToEdges.Values) {
            foreach (Interaction edge in edges) {
                if ( !nodeToCluster[edge.startNode].Equals(nodeToCluster[edge.endNode]) ) {
                    interAtedges += edge.weight;
                }
            }
        }

        // square(sum of node weights - node degrees)- sum(square(sum of node weithts that in the clusters); 
        double interAtpairs = 0.0;
        foreach (Node node in nodeToCluster.Keys) interAtpairs += node.weight;
        interAtpairs *= interAtpairs; 
        foreach (double clusterAtnodes in clusterToAtnodes) interAtpairs -= clusterAtnodes * clusterAtnodes;

        // greedily move nodes between clusters 
        double prevQuality = Double.MaxValue;
        double mquality = quality(interAtedges, interAtpairs, atedges, atpairs);
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
                double bestQuality = mquality, bestInterAtedges = interAtedges, bestInterAtpairs = interAtpairs;
                double[] clusterToAtedges = new double[nodeToCluster.Keys.Count+1];
                foreach (Interaction edge in nodeToEdges[node]) {
                    if (!edge.endNode.Equals(node)) {
                        // count weight twice to include reverse edge
                        clusterToAtedges[nodeToCluster[edge.endNode]] += 2*edge.weight;
                    }
                }
                int cluster = nodeToCluster[node];
                for (int newCluster = 0; newCluster <= maxCluster+1; newCluster++) {
                    if (cluster == newCluster) continue;
                    double newInterPairs = interAtpairs
                        + clusterToAtnodes[cluster] * clusterToAtnodes[cluster]
                        - (clusterToAtnodes[cluster]-node.weight) * (clusterToAtnodes[cluster]-node.weight)
                        + clusterToAtnodes[newCluster] * clusterToAtnodes[newCluster]
                        - (clusterToAtnodes[newCluster]+node.weight) * (clusterToAtnodes[newCluster]+node.weight);
                    double newInterEdges = interAtedges 
                        + clusterToAtedges[cluster]
                        - clusterToAtedges[newCluster];
                    double newQuality = quality(newInterEdges, newInterPairs, atedges, atpairs); 
                    if (bestQuality - newQuality > 1e-8) {
                        bestCluster = newCluster;
                        bestQuality = newQuality; bestInterAtedges = newInterEdges; bestInterAtpairs = newInterPairs;
                    }
                }
                if (bestQuality < mquality) {
                    clusterToAtnodes[cluster] -= node.weight;
                    clusterToAtnodes[bestCluster] += node.weight;

                    nodeToCluster[node]=bestCluster;

                    maxCluster = Math.Max(maxCluster, bestCluster);
                    mquality = bestQuality; interAtedges = bestInterAtedges; interAtpairs = bestInterAtpairs;
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
             double atedges,  double atpairs, ref double Modularity) 
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
        
        Dictionary<Node,Node> nodeToContr = new Dictionary<Node,Node>();
        List<Node> contrNodes = new List<Node>();
        foreach (Interaction edge in edges) 
        {
            if (edge.density < atedges/atpairs) break;
            if (edge.startNode.Equals(edge.endNode)) continue;
            if (nodeToContr.ContainsKey(edge.startNode) || nodeToContr.ContainsKey(edge.endNode)) continue;
            // randomize contraction
            // if (!nodeToContr.isEmpty() && Math.random() < 0.5) continue;
            
            //System.out.println(" Contracting " + edge);
            Debug.WriteLine(" Contracting " + edge);
            Node contrNode = Net.NewNode(edge.startNode.name + " " + edge.endNode.name,null,
                    edge.startNode.weight + edge.endNode.weight);
            nodeToContr[edge.startNode]= contrNode;
            nodeToContr[edge.endNode]= contrNode;
            contrNodes.Add(contrNode);
        }
        // terminal case: no nodes to contract
        if (nodeToContr.Count==0) 
        {
            Dictionary<Node,int> nodeToCluster = new Dictionary<Node,int>();
            int clusterId = 0;
            foreach (Node node in nodes) nodeToCluster[node]= clusterId++;
            return nodeToCluster;
        }
        // "contract" singleton clusters
        foreach (Node node in nodes) 
        {
            if (!nodeToContr.ContainsKey(node)) 
            {
                Node contrNode = Net.NewNode(node.name, null, node.weight);
                nodeToContr[node]= contrNode;
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
            Node contrEnd   = nodeToContr[edge.endNode];
            double contrWeight = 0.0f;
            Dictionary<Node, double> endToWeight = startToEndToWeight[contrStart]; 
            if (endToWeight.ContainsKey(contrEnd)) 
            {
                contrWeight = endToWeight[contrEnd];
            }
            endToWeight[contrEnd]= contrWeight + edge.weight;
        }   
        List<Interaction> contrEdges = new List<Interaction>();
        foreach (Node contrStart in startToEndToWeight.Keys) 
        {
            Dictionary<Node, double> endToWeight = startToEndToWeight[contrStart];
            foreach (Node contrEnd in endToWeight.Keys) 
            {
                Interaction contrEdge = new Interaction(contrStart, contrEnd, Interaction.DefaultValue,"", endToWeight[contrEnd]);
                contrEdges.Add(contrEdge);
            }
        }

        // cluster contracted graph
        Dictionary<Node,int> contrNodeToCluster
            = cluster(Net,contrNodes, contrEdges, atedges, atpairs, ref Modularity);
    
        // decontract clustering
        Dictionary<Node,int> mnodeToCluster = new Dictionary<Node,int>();
        foreach (Node node in nodeToContr.Keys) 
        {
            mnodeToCluster[node]= contrNodeToCluster[nodeToContr[node]];
        }
        
        // refine decontracted clustering
        Dictionary<Node,List<Interaction>> nodeToEdge = new Dictionary<Node,List<Interaction>>();
        foreach (Node node in nodes) nodeToEdge[node]= new List<Interaction>();
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
            if (!ignoreLoops || !edge.startNode.Equals(edge.endNode)) { 
                atedgeCnt += edge.weight;
            }
        }
        double atpairCnt = 0.0; 
        foreach (Node node in nodes) atpairCnt += node.weight;
        atpairCnt *= atpairCnt;
        if (ignoreLoops) { 
            foreach (Node node in nodes) atpairCnt -= node.weight*node.weight;
        }
        
        // compute clustering
        return cluster(Net,nodes, edges, atedgeCnt, atpairCnt, ref modularity);
    }
    /// <summary>
    /// Calculate modularity of a network
    /// </summary>
    /// <param name="Net">The network to calculate modularity</param>
    /// <param name="IsArc">True: Use Arcs in the network for computing; otherwise use Edges property</param>
    /// <param name="modularity">The output modularity</param>
    /// <returns></returns>
    public static Dictionary<Node, int> ClusterGraph(BasicNetwork Net, Boolean IsArc, ref double modularity)
    {
        GraphData graph = IsArc?GraphData.Convert(Net.Arcs):GraphData.Convert(Net.Edges);

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

        

        

        // see class MinimizerBarnesHut for a description of the parameters;
        // for classical "nice" layout (uniformly distributed nodes), use

        //new MinimizerBarnesHut3(nodes, edges, -1.0, 2.0, 0.05).minimizeEnergy(nodeToPosition, 100);
        //new MinimizerBarnesHut3(nodes, edges, 0.0, 1.0, 0.05).minimizeEnergy(nodeToPosition, 100);
        // see class OptimizerModularity for a description of the parameters
        OptimizerModularity optimizer = new OptimizerModularity();
        Dictionary<Node, int> nodeToCluster =
                optimizer.execute(Net, nodes, edges, false, ref modularity);

        /*
        //writePositions(nodeToPosition, nodeToCluster, args[2]);

        
        (new GraphFrame(nodeToPosition, nodeToCluster)).setVisible(true);
         */
        return nodeToCluster;
    }
}

}
