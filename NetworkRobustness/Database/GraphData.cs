using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using BasicNet;
using System.IO;
using MatrixLibrary;
using NetSimulation.Lib;
using Mathutil;
namespace NetSimulation.Community
{
    public class GraphData:NetBased {
        public override void Assign(object Source)
        {
            GraphData Src = Source as GraphData;
            this.graph = Uti.CloneDictionary<string, Dictionary<string, double>>(Src.graph);
            this.IsDirected = Src.IsDirected;

            

        }
        public override NetBased CreateObject()
        {
            return new GraphData();
        }
        public string Name
        {
            get
            {
                return _name+"("+ (graph.Count>0?graph.Count.ToString()+" edges":"no data")+")";
            }
            set
            {
                _name = value;
            }
        }
        private string _name = "No name";

        private Dictionary<string, Dictionary<string, double>> graph = null;
        public Dictionary<string, Dictionary<string, double>> Data
        {
            get
            {
                return graph;
            }
        }
        private bool IsDirected = false;
        public GraphData(Dictionary<string, Dictionary<string, double>> graph)
        {
            this.graph = graph;
        }
        public GraphData()
        {
            this.graph = new Dictionary<string, Dictionary<string, double>>();
        }
        public int nEdge
        {
            get
            {
                return this.graph.Count;
            }
        }
        public double this[string Row, string Col]
        {
            
            get { return this.graph[Row][Col]; }
            set { this.graph[Row][Col] = value; }
        }
        public Dictionary<string, double> this[string Row]
        {
            get { return this.graph[Row];}
            set { this.graph[Row] = value; }
        }
        // user-defined conversion from GraphData to Dictionary<string, Dictionary<string, double>>
        public static implicit operator Dictionary<string, Dictionary<string, double>>(GraphData f)
        {
            return f.graph;
        }

        // user-defined conversion from GraphData to Dictionary<string, Dictionary<string, double>>
        public static implicit operator GraphData(Dictionary<string, Dictionary<string, double>> gr)
        {
            GraphData t = new GraphData();
            t.graph = gr;
            return t;
        }
        //public override NetBased Clone()
        //{
        //    GraphData newDict = new GraphData();
        //    newDict.graph = Uti.CloneDictionary<string, Dictionary<string, float>>(this.graph);
        //    newDict.IsDirected = this.IsDirected;
        //    return newDict;
        //}
	
    public static GraphData readGraph(string filename)
    {
        GraphData gr = null;
        if (filename.Contains(".txt"))
        {
            gr = readTextFile(filename);
        }
        else
        {
            gr = readExcelFile(filename);
        }
        try
        {
            gr.Name = filename.Replace(Directory.GetParent(filename).FullName + "\\", "");
        }
        catch { }
        return gr;
    }

    public static List<BooleanNetwork> LoadInputFiles(string[] extensions)
    {
        List<BooleanNetwork> graphs = new List<BooleanNetwork>();
        if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\InPut"))
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\InPut");

        string folder = Directory.GetCurrentDirectory() + "\\InPut"; 
        foreach (string ext in extensions)
        {
            string[] files = Directory.GetFiles(folder, ext, SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(file);
                if (net == null) continue;
                if (net.Arcs.Count() <= 0)
                {
                    User.One.MessageToUser("Having no graph data in \"" + file + "\"");
                    continue;
                }
                graphs.Add(net);
            }
        }
        return graphs;
    }
    public static int srcIndex = 0, tarIndex = 1, egIndex = 2;
    
    /// <summary>
    /// Read graph data in a text file
    /// </summary>
    /// <param name="filename">Name of the text file</param>
    /// <returns>Graph present the text file</returns>
    private static GraphData readTextFile(string filename)
    {
        Dictionary<string, Dictionary<string, double>> result = new Dictionary<string, Dictionary<string, double>>();
        StreamReader file = new StreamReader(filename);
        string line;
        string[] token = null;
        try
        {

           
            
            while ((line = file.ReadLine()) != null)
            {
               
                //token = line.Split(new char[] { ' ', ';', '\t' });
                token = line.Split(new char[] {'\t' });
                if (token == null) continue;
                String source = token[srcIndex];
                String target = token[tarIndex];
                float weight = 1.0f;
                try
                {
                    weight = token.Length > 2 ? float.Parse(token[egIndex]) : 1.0f;
                }
                catch (Exception)
                {
                      int temp=tarIndex;//swap between weight index and target index
                      tarIndex = egIndex;
                      egIndex = temp;

                      source = token[srcIndex];
                      target = token[tarIndex];
                      weight = token.Length > 2 ? float.Parse(token[egIndex]) : 1.0f;
                }
                if (!result.ContainsKey(source)) result[source] = new Dictionary<string, double>();
                result[source][target] = weight;
            }
            file.Close();
        }
        catch (Exception e)
        {
            Debug.WriteLine("Exception while reading the graph:");
            Debug.WriteLine(e.Message);
            return null;
        }
        return result;
    }
    /// <summary>
    /// Read graph data in a excel format file
    /// </summary>
    /// <param name="filename">Name of the text file</param>
    /// <returns>Graph present the text file</returns>
    private static GraphData readExcelFile(string filename)
    {
        Dictionary<string, Dictionary<string, double>> result = new Dictionary<string, Dictionary<string, double>>();
        try
        {

            string[] headername = new string[3];
            headername[srcIndex] = "start";
            headername[tarIndex] = "end";
            headername[egIndex] = "weight";

            ExcelDB file = new ExcelDB(headername);
            file.ReadFile(filename);
            object[] row=null;
            int nrow=ExcelDB.DataRowStart;
            while ((row = file.ReadRow(nrow++)) != null)
            {
                if (row[0] == null) break;

                String source = row[file.srcIdx].ToString();
                String target = row[file.tgIdx].ToString();

                float weight = 1.0f;
                try
                {
                    weight = float.Parse(row[file.edgIdx].ToString());
                }catch{}
                if (!result.ContainsKey(source)) result[source] = new Dictionary<string, double>();
                result[source][target] = weight;
            }
            file.Dispose();
        }
        catch (Exception e)
        {
            
            User.One.SendErrorToUser(e);
            return null;
        }
        return result;
    }


	/**
	 * Returns a symmetric version of the given graph.
	 * A graph is symmetric if and only if for each pair of nodes,
	 * the weight of the edge from the first to the second node
	 * equals the weight of the edge from the second to the first node.
	 * Here the symmetric version is obtained by adding to each edge weight
	 * the weight of the inverse edge.
	 * 
	 * @param graph  possibly unsymmetric graph.
	 * @return symmetric version of the given graph.
	 */
    public static Dictionary<string, Dictionary<string, double>> makeSymmetricGraph
            (Dictionary<string, Dictionary<string, double>> graph) 
    {
        Dictionary<string, Dictionary<string, double>> result = new Dictionary<string, Dictionary<string, double>>();
		foreach (string source in graph.Keys) {
			foreach (string target in graph[source].Keys) {
                double weight = graph[source][target];
                double revWeight = 0.0f;
				if (graph.ContainsKey(target) && graph[target].ContainsKey(source)) 
                //if (graph[target] != null)
                {
					revWeight = graph[target][source];
				}
                if (!result.ContainsKey(source)) result[source] = new Dictionary<string, double>();
				result[source][target]= weight+revWeight;
                if (!result.ContainsKey(target)) result[target] = new Dictionary<string, double>();
				result[target][source]= weight+revWeight;
			}
		}
		return result;
	}

	/// <summary>
    /// Construct a map from node names to nodes for a given graph, 
    /// where the weight of each node is set to its degree,
    /// i.e. the total weight of its edges.
	/// </summary>
    /// <param name="graph">the graph</param>
    /// <returns>map from each node names to nodes</returns>
    public static Dictionary<string, Node> makeNodes(BasicNetwork Net, Dictionary<string, Dictionary<string, double>> graph)
    {
		Dictionary<string,Node> result = new  Dictionary<string,Node>();
		foreach (string nodeName in graph.Keys) {
            double nodeWeight = 0.0;
            foreach (double edgeWeight in graph[nodeName].Values) {
                nodeWeight += edgeWeight;
            }
            result[nodeName] = Net.NewNode(nodeName, null, nodeWeight);
		}
		return result;
	}
    public static Dictionary<string, Node> makeNodes2(BasicNetwork Net)
    {
        Dictionary<string, Node> result = new Dictionary<string, Node>();
        foreach (Node node in Net.Nodes)
        {
            double nodeWeight = 0.0;
            foreach (Interaction inter in node.Arcs)
            {
                nodeWeight += inter.weight;
            }
            result[node.name] = Net.NewNode(node.name, null, nodeWeight);
        }
        return result;
    }
   
    /// <summary>
    /// Converts a given graph into a list of edges.
    /// </summary>
    /// <param name="graph">the graph</param>
    /// <param name="nameToNode">map from node names to nodes</param>
    /// <returns>the given graph as list of edges</returns>
    public static List<Interaction> makeEdges(Dictionary<string, Dictionary<string, double>> graph, 
            Dictionary<string,Node> nameToNode) {
        List<Interaction> result = new List<Interaction>();
        foreach (string sourceName in graph.Keys) {
            foreach (string targetName in graph[sourceName].Keys) {
                Node sourceNode = nameToNode[sourceName];
                Node targetNode = nameToNode[targetName];
                double weight = graph[sourceName][targetName];
                result.Add(new Interaction(sourceNode, targetNode, Interaction.DefaultValue,"", weight));
            }
        }
        return result;
    }

    public static List<Interaction> makeEdges2(BasicNetwork Net, Dictionary<string, Node> nameToNode)
    {
        List<Interaction> result = new List<Interaction>();
        foreach (Interaction inter in Net.Arcs)
        {
            result.Add(new Interaction(nameToNode[inter.startNode.name], nameToNode[inter.endNode.name], inter.Type, inter.Name, inter.weight, inter.Direction));
        }
        return result;
    }
    /// <summary>
    /// Converts a given graph into a list of edges.
    /// </summary>
    /// <param name="graph">the graph</param>
    /// <param name="nameToNode">map from node names to nodes</param>
    /// <returns>the given graph as list of edges</returns>
    public static List<Interaction> makeEdges(BasicNetwork Net, Dictionary<string, Dictionary<string, double>> graph)
    {
        List<Interaction> result = new List<Interaction>();
        foreach (string sourceName in graph.Keys)
        {
            Node start = Net.NewNode(sourceName, null, 1);
            foreach (KeyValuePair<string, double> desName in graph[sourceName])
            {
                Node end = Net.NewNode(desName.Key, null, 1);
                result.Add(new Interaction(start, end, Interaction.DefaultValue, "",desName.Value));
            }

        }
        return result;
    }
    //static Random rand=new Random((int)DateTime.Now.Ticks);
	/**
	 * Returns, for each node in a given list,
	 * a random initial position in two- or three-dimensional space. 
	 * 
	 * @param nodes node list.
     * @param is3d initialize 3 (instead of 2) dimension with random numbers.
	 * @return map from each node to a random initial positions.
	 */
	public static Dictionary<Node,double[]> makeInitialPositions(List<Node> nodes, bool is3d) {
        Dictionary<Node,double[]> result = new Dictionary<Node,double[]>();
		foreach (Node node in nodes) {
            double[] position = {NumericMath.RandomCraft.NextDouble() - 0.5,
                                  NumericMath.RandomCraft.NextDouble()  - 0.5,
                                  is3d ? NumericMath.RandomCraft.NextDouble() - 0.5 : 0.0 };
            result[node]= position;
		}
		return result;
	}
	
	/**
	 * Writes a given layout and clustering
	 * 
	 * into the specified file.
	 * 
	 * @param nodeToPosition map from each node to its layout position.
     * @param nodeToPosition map from each node to its cluster.
	 * @param filename name of the file to write into.
	 */
	private static void writePositions(Dictionary<Node,double[]> nodeToPosition, 
            Dictionary<Node,int>nodeToCluster, string filename) {
		try {
			//BufferedWriter file = new BufferedWriter(new FileWriter(filename));
            StreamWriter file = new StreamWriter(filename);
			foreach (Node node in nodeToPosition.Keys) {
				double[] position = nodeToPosition[node];
                int cluster = nodeToCluster[node];
                file.WriteLine(node.name + " " + position[0] + " " + position[1]
                                     + " " + position[2] + " " + cluster);
			}
			file.Close();
		} catch (Exception e) {
		      Debug.WriteLine("Exception while writing the graph:"); 
			  Debug.WriteLine(e.Message);
			  System.Environment.Exit(1);
		}
	}
        /*
    public static int Index(Dictionary<int, string> NodeList, string Name)
    {
        var s = from par in NodeList
                 where par.Value == Name
                 select par.Key;

        IEnumerator<int> p = s.GetEnumerator();
        if(!p.MoveNext()) return -1;

        return p.Current;

    }
         */
    public Dictionary<string, int> NodeIndexFromName()
    {
        var StartNode = GetNodeNames();

        Dictionary<string, int> MappingNameIndex = new Dictionary<string, int>();
        int i = 0;
        foreach (string s in StartNode)
        {
            MappingNameIndex[s] = i++;
        }
        return MappingNameIndex;
        //return new Dictionary<int,string>(){{0,"A"},{1,"B"},{2,"C"}, {3,"D"}, {4,"E"}, {5,"F"}, {6,"G"}, {7,"H"}, {8,"I"}, {9,"J"}};
    }
    public List<string> GetNodeNames()
    {
        var StartNode =
                from p in graph
                select p.Key;

        foreach (Dictionary<string, double> p in graph.Values)
        {
            StartNode = StartNode.Union(p.Keys);
        }
        return StartNode.ToList();
    }
    public Dictionary<int, string> NodeNameFromIndex()
    {
        var StartNode = GetNodeNames();

        Dictionary<int, string> MappingNameIndex = new Dictionary<int, string>();
        int i=0;
        foreach (string s in StartNode)
        {
            MappingNameIndex[i++] = s;
        }
        return MappingNameIndex;
        //return new Dictionary<int,string>(){{0,"A"},{1,"B"},{2,"C"}, {3,"D"}, {4,"E"}, {5,"F"}, {6,"G"}, {7,"H"}, {8,"I"}, {9,"J"}};
    }
        /// <summary>
        /// Create adjacency matrix from a graph presented in Dictionary class
        /// </summary>
        /// <param name="graph">The graph</param>
        /// <returns>The matrix</returns>
    public Matrix CreateAdjacencyMatrix()
    {

        Dictionary<int, string> NameList = NodeNameFromIndex();

        Matrix ma = new Matrix(NameList.Count, NameList.Count);

        for (int i = 0; i < ma.NoRows; i++)
            for (int j = 0; j < ma.NoCols; j++)
                ma[i, j] = graph.ContainsKey(NameList[i]) && graph[NameList[i]].ContainsKey(NameList[j]) ? graph[NameList[i]][NameList[j]] : 0;


        Debug.WriteLine(Matrix.PrintMat(ma));
        return ma;

    }
    public Matrix CreateDegreeVector()
    {
        Dictionary<int, string> NameList = NodeNameFromIndex();
        Matrix degreevector = new Matrix(NameList.Count, 1);

        for (int i = 0; i < NameList.Count; i++)
        {
            degreevector[i, 0] = GetDegree(NameList[i]);
        }
        return degreevector;
    }

    public int GetDegree(string node)
    {
        if (!IsDirected)
            return GetInDegree(node) + GetOutDegree(node);
        else
            return GetOutDegree(node);
    }
    protected int GetOutDegree(string startNode)
    {
        if (graph.ContainsKey(startNode))
            return graph[startNode].Count;
        else
            return 0;
    }
    protected int GetInDegree(string endNode)
    {
        int count=0;
        foreach (Dictionary<string, double> dnode in graph.Values)
            if(dnode.ContainsKey(endNode)) count++;
        return count;

    }
    public int NoLink
    {
        get
        {
            return graph.Count;
        }
    }
    
	/**
	 * Reads a graph from a specified input file, 
     * computes a layout and a clustering, 
     * writes the layout and the clustering into a specified output file, 
     * and displays them in a dialog.
	 * 
	 * @param args number of dimensions, name of the input file and of the output file.
	 * 	 If <code>args.length != 3</code>, the method outputs a help message.
	 */
	public static void ClusterDataInFile(string[] args) {
		if (args.Length != 3 || (!args[0].Equals("2") && !args[0].Equals("3")) ) {
			Debug.WriteLine(
				  "Usage: java LinLogLayout <dim> <inputfile> <outputfile>\n"
				+ "Computes a <dim>-dimensional layout and a clustering for the graph\n"
                + "in <inputfile>, writes the layout and the clustering into <outputfile>,\n" 
                + "and displays (the first 2 dimensions of) the layout and the clustering.\n"
                + "<dim> must be 2 or 3.\n\n"
				+ "Input file format:\n"
				+ "Each line represents an edge and has the format:\n"
				+ "<source> <target> <nonnegative real weight>\n"
				+ "The weight is optional, the default value is 1.0.\n\n"
				+ "Output file format:\n"
				+ "<node> <x-coordinate> <y-coordinate> <z-coordinate (0.0 for 2D)> <cluster>"
			);
			System.Environment.Exit(0);
		}
        BasicNetwork Net = new BasicNetwork();
        Dictionary<string, Dictionary<string, double>> graph = readGraph(args[1]);
		graph = makeSymmetricGraph(graph);
        Dictionary<string, Node> nameToNode = makeNodes(Net,graph);
        List<Node> nodes = new List<Node>(nameToNode.Values);
        List<Interaction> edges = makeEdges(graph,nameToNode);
		Dictionary<Node,double[]> nodeToPosition = makeInitialPositions(nodes, args[0].Equals("3"));
		// see class MinimizerBarnesHut for a description of the parameters;
		// for classical "nice" layout (uniformly distributed nodes), use
		//new MinimizerBarnesHut(nodes, edges, -1.0, 2.0, 0.05).minimizeEnergy(nodeToPosition, 100);
		new MinimizerBarnesHut(nodes, edges, 0.0, 1.0, 0.05).minimizeEnergy(nodeToPosition, 100);
        // see class OptimizerModularity for a description of the parameters

        OptimizerModularity optimizer = new OptimizerModularity();
        double modularity = 0.0;
        Dictionary<Node,int> nodeToCluster =
            optimizer.execute(Net, nodes, edges, false, ref modularity);
		writePositions(nodeToPosition, nodeToCluster, args[2]);

        /*
		(new GraphFrame(nodeToPosition, nodeToCluster)).setVisible(true);
         */
	}
    /// <summary>
    /// Create KroneckerDelta (http://en.wikipedia.org/wiki/Kronecker_delta) 
    /// where delta[i, j] = 1 whene vertex(i) and vertex(j) are in the same one cluster
    /// </summary>
    /// <param name="NameList">List of name of vertices calculated by the function NodeIndexFromName </param>
    /// <param name="Clusters">The cluster with Values as cluster identifiers and Keys as nodes or vertices</param>
    /// <returns>The Kronecker delta matrix</returns>
    public static Matrix CreateKroneckerDelta(Dictionary<string,int> NameList, Dictionary<Node, int> Clusters)
    {
        Matrix Result = new Matrix(NameList.Count, NameList.Count);
        var Clus=Clusters.Values.Distinct();
        foreach (int ClusterID in Clus)
        {
            IEnumerable<Node> nodes = (from par in Clusters
                         where par.Value == ClusterID
                         select par.Key);

            Node[] anode=nodes.ToArray<Node>();
            for(int i=0;i<anode.Length;i++)
                for (int j = i; j < anode.Length; j++)
                {
                    Result[NameList[anode[i].name], NameList[anode[j].name]] = 1;
                    Result[NameList[anode[j].name], NameList[anode[i].name]] = 1;
                }
        }
        return Result;
    }
        /// <summary>
        /// Convert a graph from list of edges format to GraphData format
        /// </summary>
        /// <param name="edges">The graph in list of edges format</param>
        /// <returns>The graph in GraphData format</returns>
    public static GraphData Convert(IEnumerable<Interaction> edges)
    //public static GraphData Convert(List<Interaction> edges)
    {
        GraphData gr = new GraphData();
        foreach (Interaction e in edges)
        {
            if(!gr.graph.ContainsKey(e.startNode.name))
                gr[e.startNode.name] = new Dictionary<string, double>();

            if (!gr[e.startNode.name].ContainsKey(e.endNode.name))
                   gr[e.startNode.name][e.endNode.name]=e.weight;
            else
                gr[e.startNode.name][e.endNode.name] += e.weight;
        }
        return gr;
    }
    /// <summary>
    /// Clustering data with condition weight >0 (error if weight =-1)
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="modularity"></param>
    /// <returns></returns>
    //public static Dictionary<Node, int> ClusterGraph(BasicNetwork Net, GraphData graph, ref double modularity)
    //{
    //    //To avoid weight = -1
    //    graph = graph.Clone() as GraphData;
    //    foreach(string start in graph.graph.Keys)
    //        for (int i=0;i<graph.graph[start].Keys.Count;i++)
    //            if (graph.graph[start][graph.graph[start].Keys.ElementAt(i)] < 0)
    //                graph.graph[start][graph.graph[start].Keys.ElementAt(i)] = 1;
    //    //To avoid weight = -1

    //    graph = makeSymmetricGraph(graph);
    //    Dictionary<string, Node> nameToNode = makeNodes(Net,graph);
    //    List<Node> nodes = new List<Node>(nameToNode.Values);
    //    List<Interaction> edges = makeEdges(graph, nameToNode);
    //    Dictionary<Node, double[]> nodeToPosition = makeInitialPositions(nodes, true);
    //    // see class MinimizerBarnesHut for a description of the parameters;
    //    // for classical "nice" layout (uniformly distributed nodes), use
        
    //    //new MinimizerBarnesHut(nodes, edges, 0.0, 1.0, 0.05).minimizeEnergy(nodeToPosition, 100);

    //    OptimizerModularity optimizer = new OptimizerModularity();
    //    Dictionary<Node, int> nodeToCluster =
    //        optimizer.execute(Net, nodes, edges, false, ref modularity);

    //    /*
    //    //writePositions(nodeToPosition, nodeToCluster, args[2]);

        
    //    (new GraphFrame(nodeToPosition, nodeToCluster)).setVisible(true);
    //     */
    //    return nodeToCluster;
    //}
    public static Dictionary<Node, int> ClusterGraph2(BasicNetwork Net, ref double modularity)
    {
        //graph = makeSymmetricGraph(graph);
        Dictionary<string, Node> nameToNode = makeNodes2(Net);
        List<Node> nodes = new List<Node>(nameToNode.Values);
        List<Interaction> edges = makeEdges2(Net, nameToNode);
        //List<Interaction> edges = Net.Arcs.ToList();
        //Dictionary<Node, double[]> nodeToPosition = makeInitialPositions(nodes, true);
        // see class MinimizerBarnesHut for a description of the parameters;
        // for classical "nice" layout (uniformly distributed nodes), use
        //new MinimizerBarnesHut(nodes, edges, -1.0, 2.0, 0.05).minimizeEnergy(nodeToPosition, 100);
        //new MinimizerBarnesHut2(nodes, edges, 0.0, 1.0, 0.05).minimizeEnergy(nodeToPosition, 100);
        // see class OptimizerModularity for a description of the parameters
        OptimizerModularityDirected optimizer = new OptimizerModularityDirected();
        Dictionary<Node, int> nodeToCluster =
            optimizer.execute(Net, nodes, edges, false, ref modularity);

        /*
        //writePositions(nodeToPosition, nodeToCluster, args[2]);

        
    //    (new GraphFrame(nodeToPosition, nodeToCluster)).setVisible(true);
    //     */
        return nodeToCluster;
    }
    static double MultiMatrixOperator(double a, double b)
    {
        return a * b;
    }
     public static double modularity(Dictionary<Node, int> Clusters, GraphData graph)
    {
        
        Matrix am = graph.CreateAdjacencyMatrix(), dv = graph.CreateDegreeVector();
        Matrix K = dv * Matrix.Transpose(dv) / (2 * graph.NoLink);
        Matrix B = am - K;
        //double modularity=0.0;
        //Dictionary<Node, int> Clusters = GraphData.ClusterGraph(graph, ref modularity);

        Dictionary<string, int> NameList = graph.NodeIndexFromName();
        Matrix Kronecker = GraphData.CreateKroneckerDelta(NameList, Clusters);
        B = B.ArrayFormula(Kronecker, new Matrix.TwoOperandAction(MultiMatrixOperator));

        double Q = B.SumCell() / (2 * graph.NoLink);
        return Q;
    }
     public static Dictionary<int,int> DegreeDistribution(GraphData graph)
     {
         List<string> nodenames = graph.GetNodeNames();
         Dictionary<string, int> NodeDegrees = new Dictionary<string, int>();
         foreach (string n in nodenames)
         {
             NodeDegrees[n] = graph.GetDegree(n);
         }



         var nodequery = (from p in NodeDegrees
                   group p by p.Value into g select new {Degree=g.Key, Count=g.Count()} );
         
         

         return nodequery.ToDictionary(g => g.Degree, p=>p.Count);
     }
}

}
