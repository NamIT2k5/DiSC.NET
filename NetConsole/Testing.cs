using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BasicNet;
using NetSimulation.Lib;
using NetSimulation.Community;
using MathNet;
using Mathutil;
using Fuzzy;
using MathNet.Numerics.Statistics;
using BasicNet.Examination;
using NetworkRobustness.Lib;
using System.Diagnostics;
namespace NetConsole
{
    public class Testing
    {
        public static void TestBenchmark()
        {
            int k, i, j, n = 8; int[,] seq;
            

            seq = new int[n, 2];
	
	        seq[0,0] = 3;
	        seq[0,1] = 0;
	        seq[1,0] = 3;
	        seq[1,1] = 0;
	        seq[2,0] = 1;
	        seq[2,1] = 2;
	        seq[3,0] = 1;
	        seq[3,1] = 2;
	        seq[4,0] = 1;
	        seq[4,1] = 2;
	        seq[5,0] = 1;
	        seq[5,1] = 2;
	        seq[6,0] = 1;
	        seq[6,1] = 2;
	        seq[7,0] = 1;
	        seq[7,1] = 2;
            DiNetConfigureModel Netgen = new DiNetConfigureModel(DiNetConfigureModel.ConvertToDegSequenceList(seq));
            DiNetConfigureModel.Digraph G;
	
            Mathutil.NumericMath.RandomCraft rnd=new NumericMath.RandomCraft((int)DateTime.Now.Ticks);
    
	        Console.WriteLine("Sampling with uniform choice on the allowed nodes");
	        for (k=0; k<5; k++) 
            {

                G = Netgen.digsam(rnd, 0);

                Console.WriteLine("Adjacency list");
		        for (i=0; i<n; i++) {
                    Console.Write(string.Format("{0}: ", i));
			        for (j=0; j<seq[i,1]; j++) //printf("%d ",G.list[i][j]);
                        Console.Write(string.Format("{0} ", G.list[i][j]));
                    Console.WriteLine();
		        }
                Console.WriteLine();
		        Console.Write(string.Format("log(W): {0}\n\n\n",G.weight));
	        }
            Console.WriteLine("\nSampling with uniform choice of the allowed stubs");
	        for (k=0; k<5; k++) {
                G = Netgen.digsam(rnd, 1);
                Console.WriteLine("Adjacency list");
		        for (i=0; i<n; i++) {
                    Console.Write(string.Format("{0}: ", i));
                    for (j = 0; j < seq[i, 1]; j++) Console.Write(string.Format("{0} ", G.list[i][j]));
                    Console.WriteLine();
		        }
                Console.WriteLine();
                Console.Write(string.Format("log(W): {0}\n\n\n", G.weight));
	        }

            BooleanNetwork Net=new BooleanNetwork();
            Net = Netgen.CreateNetwork(Net, rnd, 0) as BooleanNetwork;
            Netutil.DumpNet(Net);
            BooleanNetwork Net2 = Netgen.CreateNetwork(Net, rnd, 0) as BooleanNetwork;
            Netutil.DumpNet(Net2);
        }
        public static void TestClustering()
        {
            //DiNetConfigureModel.Test();

            ComplexNetGenerator CNG = new ComplexNetGenerator();
            //const int nNode = 20;
            //for (int k = 0; k < 1000; k++)
            //{
            //    BasicNetwork net2 = CNG.generateScaleFreeDirectedNetwork(new BasicNetwork(), nNode, NumericMath.RandomCraft.Next(ComplexNetGenerator.nMinSFLink(nNode), ComplexNetGenerator.nMaxSFLink(nNode)));
            //    User.One.ShowWaitIndicator(k, 1000);
            //}

            NumericMath.RandomCraft Rnd = new NumericMath.RandomCraft((int)DateTime.Now.Ticks);
        

            
            //Netutil.DumpNet(n2);
            //n2.WriteToFile("KQ.2.2.txt");
            
            //BooleanNetwork n1= CNG.GenRndPowerLaw(new BooleanNetwork(), 50, 2, false, new NumericMath.RandomCraft((int)DateTime.Now.Ticks)) as BooleanNetwork;

            //Netutil.DumpNet(n1);
            
            //n1.WriteToFile("DegSeq11.txt");
            //string FileName = "STKEnn3.xls";


            //BooleanNetwork Net = BooleanNetwork.ReadSignalingNetworkFile(FileName);

            //int n = Net.Nodes.Count();
            //double density = Net.Density;
            //double meanIndeg = Net.AverageInDeg;
            //int maxIndeg = Net.MaxInDeg;
            //bool isValid = Net.IsValid();
            

            BooleanNetwork Net = new BooleanNetwork();
            //Net = BooleanNetwork.ReadSignalingNetworkFile("HSNLargestCo2013.NodeXL.txt");//BooleanNetwork.ReadSignalingNetworkFile("STKEnn3.txt");//BooleanNetwork.ReadSignalingNetworkFile("CancerHSNLargestCo2013.txt");
            Net = BooleanNetwork.ReadSignalingNetworkFile("STKEnn3.txt");
            double avrIndeg = Net.AverageInDeg;
            double MaxInDeg = Net.MaxInDeg;
            //var Solid=from i in Net.Nodes where i.TotalDegree==i.ArcTypeLink(-1).Count() orderby i.TotalDegree descending select i;
            //foreach (var i in Solid)
            //{
            //    Debug.WriteLine(string.Format("{0}\t{1}", i.name, i.TotalDegree));
            //}
            double dens = Net.ArcDensity;
            string DiseaseFile = "LargestCoHSN2013disease.txt";
            BasicNetwork DisNet = BasicNet.Examination.CreateGraphTool.CreateDiseaseFullGraphFromFile(DiseaseFile);

            var Dash = from i in DisNet.Nodes where i.TotalDegree == i.ArcName("OutModule.Dash").Count() orderby i.typeOfLink.Count() descending, i.TotalDegree descending select i;
            foreach (var i in Dash)
            {
                Debug.WriteLine(string.Format("{0}\t{1}\t{2}", i.name, i.TotalDegree,i.typeOfLink.Count()));
            }



            //Dictionary<Node,int> InDeg=new Dictionary<Node,int>();
            //for(int i=0;i<Net.Nodes.Count();i++)
            //    InDeg.Add(Net.Nodes.ElementAt(i),0);

            //for (int i = 0; i < 1000; i++)
            //    InDeg[CNG.SelectPreferentialNodeByOutDegree(Net.Nodes)]++;
            //IEnumerable<Node> sortNodes = from p in Net.Nodes orderby p.OutDegree select p;
            //for (int i = 0; i < sortNodes.Count(); i++)
            //{
            //    Debug.WriteLine(string.Format("{0}\tOutDeg={1}\tCount={2}", sortNodes.ElementAt(i).name, sortNodes.ElementAt(i).OutDegree, InDeg[sortNodes.ElementAt(i)]));
            //}
            double inGamma = 0, outGamma = 0, inR=0, outR=0, inRPvalue=0, outRPvalue=0;
            Netutil.FitDegreeDistribution(Net, ref inGamma, ref inR, ref inRPvalue, ref outGamma, ref outR, ref outRPvalue);
            double totalGamma = 0, totalR = 0, totalRPvalue = 0;
            Netutil.FitTotalDegreeDistribution(Net, ref totalGamma, ref totalR, ref totalRPvalue);

            Netutil.DumpInteraction(Net.EdgeWithMultipleOppositeArcs.ToArray());
            Net.AddNodeAndArc(new Interaction(Net.NewNode("A", FunctionType.AND), Net.NewNode("B", FunctionType.AND), Interaction.ArbitraryValue));
            //Net.AddNodeAndArc(new Interaction(new BooleanNode("A", FunctionType.AND), new BooleanNode("A", FunctionType.AND), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.NewNode("C", FunctionType.AND), Net.NewNode("B", FunctionType.AND), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.NewNode("C", FunctionType.AND), Net.NewNode("D", FunctionType.AND), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.NewNode("D", FunctionType.AND), Net.NewNode("A", FunctionType.AND), Interaction.ArbitraryValue));
            int i1 = Net.Edges.Count();
            int i2 = Net.EdgesWithoutSelfLoops.Count();
            int i3 = Net.EdgeWithMultipleOppositeArcs.Count();

            Dictionary<Node, int> pCluster1 = null,pCluster2=null;
            double Mo = Net.modularityWeightedDirected(ref pCluster1);
            Debug.WriteLine("Mo1=" + Mo.ToString());
            

            Mo = Net.modularity(ref pCluster2, true);
            Debug.WriteLine("Mo2=" + Mo.ToString());
            Netutil.DumpCluster(pCluster1);
            Netutil.DumpCluster(pCluster2);

            double mix1=Net.MixingRateOfModule(pCluster1);
            double mix2 = Net.MixingRateOfModule(pCluster2);



            //Netutil.MeasureExecutionTime(ref sw1, true);

            //Net.InOutModuleRobustnessParalell(pCluster1, new Perturbation(), ref  inModuleRobustness, ref outModuleRobustness);
            //Debug.WriteLine(string.Format("Paralell: Time={0} \t InRo ={1}\t OutRo={2}", Netutil.MeasureExecutionTime(ref sw1, false), inModuleRobustness, outModuleRobustness));

            //double inModuleRobustness2 = 0, outModuleRobustness2 = 0;
            //Netutil.MeasureExecutionTime(ref sw2, true);
            //Net.InOutModuleRobustnessParalellOld(pCluster1, new Perturbation(), ref  inModuleRobustness2, ref outModuleRobustness2);
            //Debug.WriteLine(string.Format("Paralell: Time={0} \t InRo ={1}\t OutRo={2}", Netutil.MeasureExecutionTime(ref sw2, false), inModuleRobustness2, outModuleRobustness2));

            //double inModuleRobustness3 = 0, outModuleRobustness3 = 0;

            //Netutil.MeasureExecutionTime(ref sw1, true);
            //Net.InOutModuleRobustness(pCluster1, new Perturbation(), ref  inModuleRobustness3, ref outModuleRobustness3);
            //Debug.WriteLine(string.Format("Paralell: Time={0} \t InRo ={1}\t OutRo={2}", Netutil.MeasureExecutionTime(ref sw1, false), inModuleRobustness3, outModuleRobustness3));
            //Netutil.WriteClusterToTextFile(Mo, pCluster1, "ABC." + FileName + ".txt");
          


        }
        public static void TestControllability()
        {
            //DoubleNetwork DNet = new DoubleNetwork();
            //DNet.Test();

            BasicNet.BooleanNetwork Net = new BooleanNetwork();

            Net.AddNodeAndArc(new Interaction(Net.AddNode("D"), Net.AddNode("C"), Interaction.ArbitraryValue));
            //Net.AddNodeAndArc(new Interaction(Net.AddNode("D"), Net.AddNode("C"), Interaction.ArbitraryValue));
            //Net.AddNodeAndArc(new Interaction(Net.AddNode("C"), Net.AddNode("D"), Interaction.ArbitraryValue));
            
            Net.AddNodeAndArc(new Interaction(Net.AddNode("C"), Net.AddNode("B"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("B"), Net.AddNode("A"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("C"), Net.AddNode("F"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("F"), Net.AddNode("E"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("E"), Net.AddNode("F"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("C"), Net.AddNode("J"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("J"), Net.AddNode("I"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("I"), Net.AddNode("P"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("P"), Net.AddNode("Q"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("Q"), Net.AddNode("J"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("D"), Net.AddNode("Q"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("D"), Net.AddNode("G"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("G"), Net.AddNode("C"), Interaction.ArbitraryValue));//
            Net.AddNodeAndArc(new Interaction(Net.AddNode("G"), Net.AddNode("H"), Interaction.ArbitraryValue));//
            Net.AddNodeAndArc(new Interaction(Net.AddNode("H"), Net.AddNode("T"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("T"), Net.AddNode("G"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("G"), Net.AddNode("L"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("L"), Net.AddNode("M"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("M"), Net.AddNode("N"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("N"), Net.AddNode("O"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("O"), Net.AddNode("L"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("D"), Net.AddNode("O"), Interaction.ArbitraryValue));

            Net.AddNodeAndArc(new Interaction(Net.AddNode("X"), Net.AddNode("Y"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("X"), Net.AddNode("Z"), Interaction.ArbitraryValue));
            Net.AddNode("XXX");
            Net.AddNode("XXY");
            
            
            List<IEnumerable<Node>> Comp = Net.ConnectedComponents;
            foreach (var l in Comp)
                Netutil.DumpNode(l.ToArray());
            bool IsConnected = Net.IsConnected;
            Netutil.DumpNet(Net);
            
            //HashSet<Interaction> interLink = new HashSet<Interaction>(), exterLink = new HashSet<Interaction>();
            //Net.SelectInOutGroupInteraction(Net.SelectNode(new string[] { "A", "B", "C", "D" }), ref interLink, ref exterLink);
            //Netutil.DumpInteraction(interLink.ToArray());
            //Netutil.DumpInteraction(exterLink.ToArray());

            IEnumerable<Interaction> ta = Net.GetArcNonAdjNode(new Node[] { Net["B"], Net["A"], Net["C"] });
            Netutil.DumpInteraction(ta.ToArray());

            Dictionary<Node, int> refClus = null;
            double mo = Net.modularity(ref refClus);
            double Ro = Net.NetworkRobustness(new Perturbation());
            Netutil.DumpCluster(refClus);
            BasicNetwork ClusterNet = Net.CreateClusterNework(refClus);
            Net.NodeRobustness(Net.Nodes.ElementAt(0) as BooleanNode, new Perturbation());
            ComplexNetGenerator Gen = new ComplexNetGenerator();
            
            BooleanNetwork n2 = Gen.generateDirectedNetworkByPreferentialAttachment(Net, 10, 9) as BooleanNetwork;
            //OrderedVisitor<Node>.OrderType orderType = OrderedVisitor<Node>.OrderType.PreOrder;
            //OrderedVisitor<Node> visitor = new OrderedVisitor<Node>(orderType);
            //IEnumerable<Node> N1= Net.DepthFirstTraversal(visitor, Net.Nodes.ElementAt(0), false);

            //IEnumerable<Node> N2 = Net.DepthFirstTraversalStack(visitor,orderType, Net.Nodes.ElementAt(0), false);

            BiNetwork biNet = new BiNetwork(Net);
            //Netutil.DumpNet(Net);
            //Netutil.DumpNode(biNet.Source.ToArray());
            //Netutil.DumpNode(biNet.Dest.ToArray());


            BasicNetwork CactiNet = null;
            HashSet<Node> driverNodes = biNet.findDriverNodes(out CactiNet);
            Netutil.DumpNet(CactiNet);
            //Netutil.DumpNode(N1.ToArray());
            //Netutil.DumpNode(N2.ToArray());







            // IList<Pair<IEnumerable<Node>,IEnumerable<Node>>> BiGraphs=Net.SelectBipartiteGraph();
            //int i=0;
            // foreach (var graph in BiGraphs)
            // {
            //     Debug.WriteLine("----- Graph " + (++i).ToString());
            //     Debug.WriteLine("L set ");
            //     Netutil.DumpNode(graph.First.ToArray());
            //     Debug.WriteLine("R set ");
            //     Netutil.DumpNode(graph.Second.ToArray());

            // }



        }
        public static void TestTranCentrality2()
        {

            BasicNet.BooleanNetwork Net = new BooleanNetwork();
           
            Net.AddNodeAndArc(new Interaction(Net.AddNode("A"), Net.AddNode("B"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("B"), Net.AddNode("C"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("C"), Net.AddNode("A"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("B"), Net.AddNode("E"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("C"), Net.AddNode("D"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("D"), Net.AddNode("F"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("E"), Net.AddNode("F"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("D"), Net.AddNode("G"), Interaction.ArbitraryValue));

            Net.AddNodeAndArc(new Interaction(Net.AddNode("H"), Net.AddNode("I"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("I"), Net.AddNode("J"), Interaction.ArbitraryValue));
            Net.AddNode(new Node("K"));


            IEnumerable<KeyValuePair<string, double>> closenessCentrality = Net.ClosenessCentrality();
            Dictionary<string, Triple<double>> tranClosenessCentrality = Net.HierarchicalClosenessCentralityAnalysis();
          
            closenessCentrality = from p in closenessCentrality orderby p.Key select p;


            Debug.WriteLine("Closeness in ..");
            foreach (var n in closenessCentrality)
                Debug.WriteLine(n.Key + "\t" + n.Value);

            Debug.WriteLine("Tran closeness in ..");
            foreach (var n in tranClosenessCentrality)
                Debug.WriteLine(n.Key + "\t" + n.Value.A + "\t\t" + n.Value.B + "\t\t" + n.Value.C);


            
        }

        public static void TestTranCentrality3()
        {

            BasicNet.BooleanNetwork Net = new BooleanNetwork();

            Net.AddNodeAndArc(new Interaction(Net.AddNode("H"), Net.AddNode("A"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("H"), Net.AddNode("G"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("H"), Net.AddNode("I"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("H"), Net.AddNode("F"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("G"), Net.AddNode("B"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("B"), Net.AddNode("C"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("C"), Net.AddNode("A"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("C"), Net.AddNode("D"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("D"), Net.AddNode("E"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("E"), Net.AddNode("A"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("A"), Net.AddNode("B"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("I"), Net.AddNode("J"), Interaction.ArbitraryValue));
            


            IEnumerable<KeyValuePair<string, double>> closenessCentrality = Net.ClosenessCentrality();
            Dictionary<string, Triple<double>> tranClosenessCentrality = Net.HierarchicalClosenessCentralityAnalysis();

            closenessCentrality = from p in closenessCentrality orderby p.Key select p;


            Debug.WriteLine("Closeness in ..");
            foreach (var n in closenessCentrality)
                Debug.WriteLine(n.Key + "\t" + n.Value);

            Debug.WriteLine("Tran closeness in ..");
            foreach (var n in tranClosenessCentrality)
                Debug.WriteLine(n.Key + "\t" + n.Value.A + "\t\t" + n.Value.B + "\t\t" + n.Value.C);



        }

        public static void TestTranCentrality()
        {

            BasicNet.BooleanNetwork Net = new BooleanNetwork();
            Interaction ar1 = new Interaction(Net.AddNode("A"), Net.AddNode("B"), InteractionType.NEGATIVE);
            Interaction ar2 = new Interaction(Net.AddNode("A"), Net.AddNode("B"), InteractionType.POSITIVE);
            Interaction ar3 = new Interaction(Net.AddNode("B"), Net.AddNode("A"), InteractionType.POSITIVE);
            Net.AddNodeAndArc(ar1);
            Net.AddNodeAndArc(ar2);
            //Net.AddNodeAndArc(new Interaction(Net.AddNode("A"), Net.AddNode("D"), Interaction.ArbitraryValue));//New
            Net.AddNodeAndArc(new Interaction(Net.AddNode("B"), Net.AddNode("C"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("C"), Net.AddNode("D"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("D"), Net.AddNode("F"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("F"), Net.AddNode("A"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("C"), Net.AddNode("A"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("G"), Net.AddNode("B"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("H"), Net.AddNode("G"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("H"), Net.AddNode("A"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("H"), Net.AddNode("T"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("H"), Net.AddNode("K"), Interaction.ArbitraryValue));
            Net.AddNodeAndArc(new Interaction(Net.AddNode("K"), Net.AddNode("J"), Interaction.ArbitraryValue));

            Netutil.DumpInteraction(Net.Edges.ToArray());
            Netutil.DumpInteraction(Net.Arcs.ToArray());
            Netutil.DumpInteraction(Net.GetNodeFromName("A").ArcsBetween(Net.GetNodeFromName("B")).ToArray());
            bool x = Net.GetNodeFromName("A").hasLinkFrom(Net.GetNodeFromName("J"));
            Netutil.DumpNet(Net);

            Netutil.DumpInteraction(Net.EdgeWithMultipleOppositeArcs.ToArray());
            Net.RemoveArc(ar1);
            Netutil.DumpNet(Net);
            Net.RemoveArc(ar2);
            Netutil.DumpNet(Net);
            IEnumerable<KeyValuePair<Node, float>> pageRankInCentrality1 = Short(Net.PageRankCentralityInLink());

            string[] a = Net.ClosestVertices(Net.SelectNode(new string[] { "A" }).ElementAt(0), Net.SelectNode(new string[] { "C", "B" }));

            BasicNet.BooleanNetwork Net2 = (BasicNet.BooleanNetwork)Net.CreateInvertedLinkGraph();
            IEnumerable<KeyValuePair<Node, float>> pageRankOutCentrality = Short(Net2.PageRankCentralityOutLink());



            Debug.WriteLine("PageRank in ..");
            foreach (var n in pageRankInCentrality1)
                Debug.WriteLine(n.Key.name + "\t" + n.Value.ToString());


            Debug.WriteLine("PageRank out of inverted graph..");
            foreach (var n in pageRankOutCentrality)
                Debug.WriteLine(n.Key.name + "\t" + n.Value.ToString());
        }
        static IEnumerable<KeyValuePair<Node, float>> Short(Dictionary<Node, float> centrality)
        {
            return from p in centrality orderby p.Value ascending select p;

        }
        public static void TestCommand()
        {

            List<object> Parameter = new List<object>();

            Parameter.Add("Mo");

            //Parameter.Add("CancerHSNLargestCo2013.xls");
            //Parameter.Add(5);
            //Parameter.Add(0.6f);
            //Parameter.Add(1);


            //List<object> Parameter = new List<object>();
            //Parameter.Add(100);
            //Parameter.Add(20);
            //Parameter.Add(80);
            //Parameter.Add(19);
            //Parameter.Add(200);
            //Parameter.Add("abc.txt");
            //App.OnMoRoOnModuleAndRandomGroup(Parameter);

        }



        static void Test2()
        {
            string fileName = "test2.txt";
            float perturbedRate = 0.5f;
            BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(fileName);
            string rewiredFile = fileName.Substring(0, fileName.LastIndexOf('.')) + ".Rewire." + Math.Round(perturbedRate, 2).ToString() + ".txt";
            BooleanNetwork perturbNetwork = net.ShufflePreservingDegree(1, true) as BooleanNetwork;
            perturbNetwork.WriteToFile(rewiredFile);
            var Node1 = from p in net.Nodes orderby p.name select p;
            var Node2 = from p in perturbNetwork.Nodes orderby p.name select p;
            for (int i = 0; i < Node1.Count(); i++)
            {
                if (Node1.ElementAt(i).name != Node2.ElementAt(i).name)
                {
                    throw new Exception("Loi");

                }
                if (Node1.ElementAt(i).TotalDegree != Node2.ElementAt(i).TotalDegree)
                {
                    throw new Exception("Loi");

                }
                if (Node1.ElementAt(i).InDegree != Node2.ElementAt(i).InDegree)
                {
                    throw new Exception("Loi");

                }
            }
            Netutil.DumpNet(net);
            Netutil.DumpNet(perturbNetwork);
        }

    }
}
