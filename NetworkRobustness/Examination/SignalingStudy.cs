using BasicNet;
using MathNet;
using MathNet.Numerics.Statistics;
using Mathutil;
using NetSimulation.Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using static BasicNet.Interaction;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace BasicNet.Examination
{

    public enum DiSCStrategy
    {
        Dominant,       // D: Preferential Attachment — ưu tiên node degree CAO
        Influential,    // I: Centrality-based — ưu tiên node ảnh hưởng nhất
        Steady,         // S: Random — kết nối ngẫu nhiên đều
        Conscientious   // C: Anti-preferential — ưu tiên node degree THẤP
    }
    public class SignalingStudy
    {
        #region Robustness
        /// <summary>
        /// Calculate Pvalue of robustness estimation
        /// </summary>
        /// <param name="nNet">The number of network</param>
        /// <param name="nSample">The number of robustness sample for a network</param>
        /// <param name="nNodeFrom"></param>
        /// <param name="nNodeTo"></param>
        /// <param name="fileName"></param>
        /// <param name="IsSparseNets">true if sparse network is considered</param>
        public static void CalculateRobustnessPvalue(int nNet, int nSample, int nNodeFrom, int nNodeTo, string fileName, bool IsSparseNets)
        {
            int link = 0;

            string exportFileDeviation = "Dev." + fileName;
            string exportFileRo = "Ro." + fileName;



            User.One.MessageToUser("Started at: " + DateTime.Now.ToString());

            ComplexNetGenerator GeneratingNet = new BasicNet.ComplexNetGenerator();
            double[] Ro = new double[nSample];
            TextDB.WriteTextFile(new string[] { "Node", "Edge", "Network", "Robustness" }, exportFileRo);
            TextDB.WriteTextFile(new string[] { "Node", "Edge", "Robustness Mean", "Robustness Deviation", "Samples", }, exportFileDeviation);
            BooleanNetwork temp = new BooleanNetwork();
            for (int idx = 1; idx <= nNet; idx++)
            {
                User.One.ShowWaitIndicator(idx, nNet);
                int node = NumericMath.RandomCraft.Next(nNodeFrom, nNodeTo + 1);

                if (IsSparseNets)
                    link = ComplexNetGenerator.nMinSFLink(node);
                else
                {
                    int nQuard = (ComplexNetGenerator.nMaxSFDLink(node) - ComplexNetGenerator.nMinSFLink(node)) / 4;
                    int seeds = NumericMath.RandomCraft.Next(0, 100); //40, 30, 20, 10

                    if (seeds < 40)
                        link = NumericMath.RandomCraft.Next(ComplexNetGenerator.nMinSFLink(node), nQuard + 1);
                    else if (40 <= seeds && seeds < 70)
                        link = NumericMath.RandomCraft.Next(nQuard, nQuard * 2 + 1);
                    else if (70 <= seeds && seeds < 90)
                        link = NumericMath.RandomCraft.Next(nQuard * 2, nQuard * 3 + 1);
                    else
                        link = NumericMath.RandomCraft.Next(nQuard * 3, ComplexNetGenerator.nMaxSFDLink(node));
                }

                BooleanNetwork net = (BooleanNetwork)GeneratingNet.generateDirectedNetworkByPreferentialAttachment(temp, node, link);
                for (int j = 0; j < nSample; j++)
                {
                    Ro[j] = net.NetworkRobustnessWithRandomInitiation(new Perturbation());
                    TextDB.WriteTextFile(new string[] { net.Nodes.Count().ToString(), net.Edges.Count().ToString(), net.ObjectID.ToString(), Ro[j].ToString() }, exportFileRo);
                }
                double mean = Ro.Average();

                double sigma = Math.Sqrt(
                    Ro.Sum(x => (x - mean) * (x - mean)) / Ro.Length
                );

                int count = Ro.Length;

                TextDB.WriteTextFile(new string[] {
                    node.ToString(),
                    net.Edges.Count().ToString(),
                    mean.ToString(),
                    sigma.ToString(),
                    count.ToString()
                    }, exportFileDeviation);

            }
            User.One.MessageToUser("Finish OnRandomRobustnessCmd on " + DateTime.Now.ToString());
        }
        #endregion
        #region Relationship between Modularity and Robustness
        /// <summary>
        /// Show the relationship between modularity and robustness
        /// </summary>
        /// <param name="nNet"></param>
        /// <param name="nNodeFrom"></param>
        /// <param name="nNodeTo"></param>
        /// <param name="nMinLink"></param>
        /// <param name="nMaxLink"></param>
        /// <param name="ReportFileName"></param>
        public static void AnalyzeModularityRobustnessRelationship(int nNet, int nNodeFrom, int nNodeTo, int nMinLink, int nMaxLink, string ReportFileName)
        {

            if (nNodeFrom > nNodeTo)
                throw new Exception("The node range is invalid!");
            if (nMinLink > nMaxLink)
                throw new Exception("The link range is invalid!");

            ComplexNetGenerator GeneratingNet = new BasicNet.ComplexNetGenerator();
            User.One.MessageToUser("Start generating network data on " + DateTime.Now.ToString());
            try
            {
                int i = 0;
                //foreach node
                BooleanNetwork Net = null;
                TextDB.WriteTextFile(new string[] {"NetID", "Node #", "Edge #", "Link#",//"Module mixing rate",
                    //"Centrality", 
                    //"Module amount", 
                    "Modularity", "Robustness", "In-module robustness", "Out-module robustness"//, "In-power law", "In-R", "In-Pvalue", "Out-power law","Out-R","Out-Pvalue"
                    //,"Controllability" 
                }, ReportFileName);
                BooleanNetwork temp = new BooleanNetwork();
                for (int j = nNodeFrom; j <= nNodeTo; j = j >= nNodeTo ? nNodeFrom : (j + 1))
                {
                    if (i > nNet) break;


                    for (int k = nMinLink; k <= nMaxLink; k++)
                    {
                        if (++i > nNet) break;

                        try
                        {
                            Net = (BooleanNetwork)GeneratingNet.generateDirectedNetworkByPreferentialAttachment(temp, j, k);
                            //Net = (BooleanNetwork)GeneratingNet.generateScaleFreeDirectedNetwork(j, k);
                        }
                        catch (Exception)
                        {
                            i--;
                            continue;
                        }
                        //double mixingRateOfModule = 0;
                        double multationRobustness = 0;
                        double modularity = 0;
                        double inModuleRo = 0, outModuleRo = 0;
                        multationRobustness = Net.NetworkMutantRobustnessParalell();//Net.NetworkRobustness(new Perturbation(Perturbation.Kind.Mutation));
                        Dictionary<Node, int> Cluster = null;
                        modularity = Net.modularity(ref Cluster);
                        //mixingRateOfModule = Net.MixingRateOfModule(Cluster);
                        Net.InOutModuleRobustnessParalell(Cluster, new Perturbation(Perturbation.Kind.Mutation), ref inModuleRo, ref outModuleRo);

                        int nNode = Net.Nodes.Count();
                        int nEdge = Net.EdgesWithoutSelfLoops.Count();
                        int nLink = Net.Arcs.Count();


                        //double inGamma = 0, outGamma = 0, inR = 0, outR = 0;
                        //double inPvalue = 0, outPvalue = 0;
                        //Netutil.FitDegreeDistribution(Net, ref inGamma, ref inR, ref inPvalue, ref outGamma, ref outR, ref outPvalue);





                        string[] f = new string[] {Net.ObjectID.ToString(), nNode.ToString(), nEdge.ToString(), nLink.ToString(),
                            //mixingRateOfModule.ToString(),
                            //centrality.ToString(), nCluster.Count().ToString(), 
                            modularity.ToString(), multationRobustness.ToString(),inModuleRo.ToString(),outModuleRo.ToString()
                            //inGamma.ToString(),inR.ToString(), inPvalue.ToString(),
                            //outGamma.ToString(),outR.ToString(),outPvalue.ToString()
                            //,Net.driverNodes.Count().ToString()
                        };

                        TextDB.WriteTextFile(f, ReportFileName);

                        User.One.ShowWaitIndicator(i, nNet);

                    }

                    if (i == 0)
                    {
                        User.One.MessageToUser("Can not create any network");
                        break;
                    }
                }
                User.One.MessageToUser("Finish generating network data on " + DateTime.Now.ToString());

            }
            catch (Exception ex)
            {
                User.One.SendErrorToUser(ex);
            }
        }

        class DoubleEqualityComparer : IEqualityComparer<Double>
        {

            public bool Equals(Double b1, Double b2)
            {
                if (Math.Abs(b1 - b2) < 0.00000001)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }


            public int GetHashCode(Double bx)
            {
                return bx.GetHashCode();
            }

        }
        /// <summary>
        /// Randomly select the same size groups of relation(FirstColumn, SecondColumn) where the first column is the ID of object, the second column is the non-same size group
        /// </summary>
        /// <param name="fileName">The file name of the relation compose of two columns without header where the first column is object ID and the second is group ID</param>
        /// <param name="outFile">The destination file to save object relation where the number of objects in groups are the same</param>
        /// <param name="groupSize">The group size to randomly select</param>
        public static void SelectRandomObjectByGroup(string fileName, string outFile, int groupSize = 100)
        {
            fileName = Netutil.InPutDirector + "\\" + fileName;

            List<KeyValuePair<string, string>> first2Second = new List<KeyValuePair<string, string>>();
            StreamReader file = new StreamReader(fileName);

            string line;
            string[] token = null;
            try
            {
                String FirstCol = null;
                string SecondCol = "";
                while ((line = file.ReadLine()) != null)
                {


                    token = line.Split(new char[] { '\t' });


                    if (token == null) continue;

                    FirstCol = token[0].Trim();
                    SecondCol = token[1].Trim();
                    first2Second.Add(new KeyValuePair<string, string>(FirstCol, SecondCol));
                }


                Dictionary<string, List<string>> second2first = new Dictionary<string, List<string>>();
                IEnumerable<string> modularity = from p in first2Second group p by p.Value into g where g.Count() >= groupSize select g.Key;
                foreach (string mo in modularity)
                    second2first.Add(mo, new List<string>());


                Netutil.Shuffle<KeyValuePair<string, string>>(first2Second);
                foreach (KeyValuePair<string, string> e in first2Second)
                {
                    if (second2first.ContainsKey(e.Value) && second2first[e.Value].Count < groupSize)
                        second2first[e.Value].Add(e.Key);
                }
                foreach (var second in second2first.Keys)
                {
                    foreach (var first in second2first[second])
                    {
                        TextDB.WriteTextFile(string.Format("{0}\t{1}", first, second), outFile);
                    }
                }
            }
            catch (Exception ex)
            {
                User.One.SendErrorToUser(ex);
            }
            finally
            {
                file.Close();

            }

        }
        public static void AnalyzeModularityRobustnessRelationship_FixedModularity(int nNet, int nNodeFrom, int nNodeTo, int nMinLink, int nMaxLink, string ReportFileName)
        {

            if (nNodeFrom > nNodeTo)
                throw new Exception("The node range is invalid!");
            if (nMinLink > nMaxLink)
                throw new Exception("The link range is invalid!");

            ComplexNetGenerator GeneratingNet = new BasicNet.ComplexNetGenerator();
            User.One.MessageToUser("Start generating network data on " + DateTime.Now.ToString());


            Dictionary<double, int> nModularityNetwork = new Dictionary<double, int>();
            const int nNetWorkPin = 80;

            double[] moPins = new double[] { 0.73, 0.72, 0.66, 0.69, 0.7, 0.67, 0.71, 0.01, 0.65, 0.68, 0.6, 0.61, 0.62, 0.64, 0.63, 0.57, 0.59, 0.53, 0.56, 0.54, 0.58, 0.55, 0.46, 0.52, 0.51, 0.5, 0.48, 0.43, 0.49, 0.11, 0.45, 0.47, 0.4, 0.1, 0.12, 0.44, 0.42, 0.09 };

            for (int i = 0; i < moPins.Count(); i++)
            {

                nModularityNetwork[moPins[i]] = 0;

            }
            try
            {
                int i = 0;
                //foreach node
                BooleanNetwork Net = null;
                TextDB.WriteTextFile(new string[] {"NetID", "Node #", "Edge #", "Link#","Module mixing rate",
                    //"Centrality", 
                    //"Module amount", 
                    "Modularity", "Robustness", "In-module robustness", "Out-module robustness", "In-power law", "In-R", "In-Pvalue", "Out-power law","Out-R","Out-Pvalue"
                    //,"Controllability" 
                }, ReportFileName);
                BooleanNetwork temp = new BooleanNetwork();
                nNet = nNetWorkPin * nModularityNetwork.Count;
                for (int j = nNodeFrom; j <= nNodeTo; j = j >= nNodeTo ? nNodeFrom : (j + 1))
                {
                    if (i > nNet) break;


                    for (int k = nMinLink; k <= nMaxLink; k++)
                    {
                        if (++i > nNet) break;


                        try
                        {
                            Net = (BooleanNetwork)GeneratingNet.generateDirectedNetworkByPreferentialAttachment(temp, j, k);
                        }
                        catch (Exception)
                        {
                            i--;
                            continue;
                        }

                        Dictionary<Node, int> Cluster = null;
                        double modularity = Math.Round(Net.modularity(ref Cluster), 2);

                        if (!nModularityNetwork.ContainsKey(modularity))
                        {
                            i--;
                            continue;
                        }
                        else
                        {
                            if (nModularityNetwork[modularity] > nNetWorkPin)
                            {
                                i--;
                                continue;
                            }
                            else
                                nModularityNetwork[modularity]++;
                        }

                        double mixingRateOfModule = 0;
                        double multationRobustness = 0;

                        double inModuleRo = 0, outModuleRo = 0;
                        multationRobustness = Net.NetworkMutantRobustnessParalell();//Net.NetworkRobustness(new Perturbation(Perturbation.Kind.Mutation));

                        mixingRateOfModule = Net.MixingRateOfModule(Cluster);
                        Net.InOutModuleRobustnessParalell(Cluster, new Perturbation(Perturbation.Kind.Mutation), ref inModuleRo, ref outModuleRo);

                        int nNode = Net.Nodes.Count();
                        int nEdge = Net.EdgesWithoutSelfLoops.Count();
                        int nLink = Net.Arcs.Count();


                        double inGamma = 0, outGamma = 0, inR = 0, outR = 0;
                        double inPvalue = 0, outPvalue = 0;
                        Netutil.FitDegreeDistribution(Net, ref inGamma, ref inR, ref inPvalue, ref outGamma, ref outR, ref outPvalue);





                        string[] f = new string[] {Net.ObjectID.ToString(), nNode.ToString(), nEdge.ToString(), nLink.ToString(),
                            mixingRateOfModule.ToString(),
                            //centrality.ToString(), nCluster.Count().ToString(), 
                            modularity.ToString(), multationRobustness.ToString(),inModuleRo.ToString(),outModuleRo.ToString(),
                            inGamma.ToString(),inR.ToString(), inPvalue.ToString(),
                            outGamma.ToString(),outR.ToString(),outPvalue.ToString()
                            //,Net.driverNodes.Count().ToString()
                        };

                        TextDB.WriteTextFile(f, ReportFileName);

                        User.One.ShowWaitIndicator(i, nNet);

                    }

                    //if (i == 0)
                    //{
                    //    User.One.MessageToUser("Can not create any network");
                    //    break;
                    //}
                }
                User.One.MessageToUser("Finish generating network data on " + DateTime.Now.ToString());

            }
            catch (Exception ex)
            {
                User.One.SendErrorToUser(ex);
            }
        }

        private static readonly string[] DISC_TYPES = { "D", "I", "S", "C" };
 
    
    public class KLevelStats
    {
        public int KLevel { get; set; }
        public int CountD { get; set; }
        public int CountI { get; set; }
        public int CountS { get; set; }
        public int CountC { get; set; }
        public double RatioD { get; set; }
        public double RatioI { get; set; }
        public double RatioS { get; set; }
        public double RatioC { get; set; }
        public int TotalNodes { get; set; }
        public int NetworkId { get; set; }
        public int NumberOfNodes { get; set; }
        public int LinksPerStep { get; set; }
        public double DeviationD { get; set; }
        public double DeviationI { get; set; }
        public double DeviationS { get; set; }
        public double DeviationC { get; set; }
    }
 
    
    public static void DiSCNetworkSimulation(
        int nNet,
        int nNodeFrom,
        int nNodeTo,
        int nMinLink,
        int nMaxLink,
        string reportFileName)
    {
        Random rnd = new Random((int)DateTime.Now.Ticks);
        string outputPath = Path.Combine(Netutil.OutPutDirector, reportFileName);
 
        // Header TSV — 8 cột, mỗi network 1 dòng (chỉ lấy innermost k-core)
        using (StreamWriter sw = new StreamWriter(outputPath, false))
        {
            sw.WriteLine("Id mạng\tSố node\tSố cạnh\tKcore\t" +
                         "Tỉ lệ D trong lõi\tTỉ lệ I trong lõi\tTỉ lệ C trong lõi\tTỉ lệ S trong lõi");
        }
 
        var allResults = new List<KLevelStats>();
        var kshellComputer = new KShellComputer();
        var analyzer = new RatioAnalyzer();
 
        int successCount = 0;
        int attemptCount = 0;
        int maxAttempts = nNet * 10;
 
        
 
        while (successCount < nNet && attemptCount < maxAttempts)
        {
            attemptCount++;
            try
            {
                int nNodes = rnd.Next(nNodeFrom, nNodeTo + 1);
                int linksPerStep = rnd.Next(nMinLink, nMaxLink + 1);
 
                // B1: Xây mạng theo chiến lược DISC
                BasicNetwork net = BuildDISCNetwork(nNodes, linksPerStep, rnd);
 
                if (net == null || !net.Nodes.Any())
                    continue;
 
                // Bỏ qua nếu không đạt 60% số node mục tiêu
                if (net.Nodes.Count() < nNodes * 6 / 10)
                {
                    
                    continue;
                }
 
                int nodeCount = net.Nodes.Count();
                int edgeCount = net.Edges.Count();
 
                // B2: K-shell centrality (dùng BasicNetwork API)
                Dictionary<Node, int> kshell;
                try
                {
                    kshell = net.K_ShellCentrality();
                }
                catch (Exception kEx)
                {
                    
                    continue;
                }
 
                // B3: Phân tích tỉ lệ DISC theo từng k-level
                var results = analyzer.AnalyzeRatios(kshell, successCount + 1, nodeCount, edgeCount, linksPerStep);
                allResults.AddRange(results);
 
                // Chỉ ghi innermost k-core (k_max) — 1 dòng mỗi network, 8 cột
                var innermostStat = results.OrderByDescending(x => x.KLevel).First();
                using (StreamWriter sw = new StreamWriter(outputPath, true))
                {
                    sw.WriteLine(string.Format(
                        "{0}\t{1}\t{2}\t{3}\t{4:F4}\t{5:F4}\t{6:F4}\t{7:F4}",
                        innermostStat.NetworkId, nodeCount, edgeCount, innermostStat.KLevel,
                        innermostStat.RatioD, innermostStat.RatioI,
                        innermostStat.RatioC, innermostStat.RatioS));
                }
 
                successCount++;
                User.One.ShowWaitIndicator(successCount, nNet);
            }
            catch (Exception ex)
            {
                    Console.WriteLine($"[ERROR] Network {attemptCount}: {ex.Message}");
                
            }
        }
 
        if (successCount < nNet)
            User.One.MessageToUser($"Chỉ tạo được {successCount}/{nNet} network thành công");
 
        
    }
 
    
    private static BasicNetwork BuildDISCNetwork(int totalNodes, int linksPerStep, Random rnd)
    {
        BasicNetwork net = new BasicNetwork();
 
        // Seed: vòng tròn nhỏ để đảm bảo connectivity ban đầu
        int seedSize = Math.Max(linksPerStep + 1, 3);
        var seedNodes = new List<Node>();
 
        for (int i = 0; i < seedSize; i++)
        {
            string nodeType = SelectNodeTypeByStrategy(net, rnd);
            Node n = net.AddNode(nodeType + "_" + i);
            seedNodes.Add(n);
        }
 
        // Nối seed thành vòng để có connectivity tối thiểu
        for (int i = 0; i < seedNodes.Count; i++)
        {
            Node a = seedNodes[i];
            Node b = seedNodes[(i + 1) % seedNodes.Count];
            if (!net.hasEdge(a, b))
                net.AddArc(new Interaction(a, b, 0, "", 1, Interaction.DirectionType.undirected));
        }
 
        int nodeCounter = seedSize;
        int failCounter = 0;
        const int maxConsecFail = 2000;
        const double triangleProb = 0.3;
 
        // Cache láng giềng local — tránh gọi net.hasEdge O(E) liên tục
        // Key: node name, Value: set tên láng giềng
        var neighborCache = new Dictionary<string, HashSet<string>>();
        foreach (Node sn in seedNodes)
            neighborCache[sn.name] = new HashSet<string>();
        // Seed ring edges vào cache
        for (int i = 0; i < seedNodes.Count; i++)
        {
            Node a = seedNodes[i];
            Node b = seedNodes[(i + 1) % seedNodes.Count];
            if (!neighborCache[a.name].Contains(b.name))
            {
                neighborCache[a.name].Add(b.name);
                neighborCache[b.name].Add(a.name);
            }
        }
 
        while (net.Nodes.Count() < totalNodes && failCounter < maxConsecFail)
        {
            string newType = SelectNodeTypeByStrategy(net, rnd);
            string newName = newType + "_" + nodeCounter++;
            Node newNode = net.AddNode(newName);
            neighborCache[newName] = new HashSet<string>();
 
            var candidates = net.Nodes.Where(n => n.name != newName).ToList();
            if (candidates.Count == 0) { failCounter++; continue; }
 
            failCounter = 0;

             // Tính scores 1 lần — dùng cache degree thay vì gọi centrality mỗi node
             double[] scores = ComputeAttachmentScores(net, candidates, newType);
             double totalScore = scores.Sum();
 
            var chosen = new HashSet<int>();
            var chosenNodes = new List<Node>();
            int maxLinks = Math.Min(linksPerStep, candidates.Count);
            int attempts = 0;
            int maxAttempts = candidates.Count * 20;
 
            while (chosen.Count < maxLinks && attempts < maxAttempts)
            {
                attempts++;
                int idx = WeightedRandomSelect(scores, totalScore, rnd);
                if (idx < 0) break;
 
                Node target = candidates[idx];
                if (!chosen.Contains(idx) && !neighborCache[newName].Contains(target.name))
                {
                    chosen.Add(idx);
                    chosenNodes.Add(target);
                    net.AddArc(new Interaction(newNode, target, 0, "", 1, Interaction.DirectionType.undirected));
                    neighborCache[newName].Add(target.name);
                    neighborCache[target.name].Add(newName);
                }
            }
 
            // Triadic closure: dùng neighborCache — O(degree) thay vì O(N×E)
            foreach (Node target in chosenNodes)
            {
                foreach (string neighborName in neighborCache[target.name].ToList())
                {
                    if (neighborName != newName
                        && !neighborCache[newName].Contains(neighborName)
                        && rnd.NextDouble() < triangleProb)
                    {
                        Node neighbor = candidates.FirstOrDefault(n => n.name == neighborName);
                        if (neighbor != null)
                        {
                            net.AddArc(new Interaction(newNode, neighbor, 0, "", 1,
                                Interaction.DirectionType.undirected));
                            neighborCache[newName].Add(neighborName);
                            neighborCache[neighborName].Add(newName);
                        }
                    }
                }
            }
        }
 
        return net;
    }
 
    
    private static string SelectNodeTypeByStrategy(BasicNetwork net, Random rnd)
    {
        var existingNodes = net.Nodes.ToList();
 
        if (existingNodes.Count < 5)
            return DISC_TYPES[rnd.Next(4)];
 
        int sampleSize = Math.Min(10, existingNodes.Count);
        var sample = existingNodes.OrderBy(_ => rnd.Next()).Take(sampleSize).ToList();
 
        var typeCounts = new Dictionary<string, int> { { "D", 0 }, { "I", 0 }, { "S", 0 }, { "C", 0 } };
        foreach (var node in sample)
        {
            string t = GetNodeType(node);
            if (typeCounts.ContainsKey(t)) typeCounts[t]++;
        }
 
        int total = typeCounts.Values.Sum();
        if (total == 0)
            return DISC_TYPES[rnd.Next(4)];
 
        int roll = rnd.Next(total);
        int cumulative = 0;
        foreach (var kvp in typeCounts)
        {
            cumulative += kvp.Value;
            if (roll < cumulative) return kvp.Key;
        }
 
        return DISC_TYPES[rnd.Next(4)];
    }
 
    
    private static double[] ComputeAttachmentScores(
        BasicNetwork net,
        List<Node> candidates,
        string discType)
    {
        int n = candidates.Count;
        double[] scores = new double[n];
 
        switch (discType)
        {
            case "D":
            {
                var betweenness = net.BetweenessCentrality();
                for (int i = 0; i < n; i++)
                {
                    float bw = betweenness.TryGetValue(candidates[i], out float b)
                                ? Math.Max(b, 1e-10f) : 1e-10f;
                    float deg = Math.Max(candidates[i].TotalDegree, 1);
                    scores[i] = Math.Pow(0.6 * bw + 0.4 * deg, 1.5);
                }
                break;
            }
 
            case "I":
            {
                for (int i = 0; i < n; i++)
                {
                    double deg = Math.Max(candidates[i].TotalDegree, 1);
                    scores[i] = Math.Pow(deg, 3.0);
                }
                break;
            }

                case "S":
                    {
                        for (int i = 0; i < n; i++)
                        {
                            Node node = candidates[i];

                            double clustering = ComputeLocalClusteringCoefficient(net,node);

                            double degree = Math.Max(node.TotalDegree, 1);

                            // S = trusted cohesive local core
                            scores[i] = Math.Pow(clustering * degree,2.5);
                        }

                        break;
                    }

                case "C":
            {
                for (int i = 0; i < n; i++)
                {
                    double deg = Math.Max(candidates[i].TotalDegree, 1);
                    scores[i] = Math.Pow(1.0 / deg, 2.0);
                }
                break;
            }
 
            default:
                for (int i = 0; i < n; i++) scores[i] = 1.0;
                break;
        }
 
        return scores;
    }

        private static double ComputeLocalClusteringCoefficient(
        BasicNetwork net,
        Node node)
        {
            var neighbors = new HashSet<Node>();

            foreach (var edge in net.Edges)
            {
                if (edge._startNode == node)
                    neighbors.Add(edge._endNode);

                else if (edge._endNode == node)
                    neighbors.Add(edge._startNode);
            }

            int k = neighbors.Count;

            if (k < 2)
                return 0;

            int triangleLinks = 0;

            var neighborList = neighbors.ToList();

            for (int i = 0; i < neighborList.Count; i++)
            {
                for (int j = i + 1; j < neighborList.Count; j++)
                {
                    if (net.hasEdge(
                            neighborList[i],
                            neighborList[j]))
                    {
                        triangleLinks++;
                    }
                }
            }

            double possibleLinks =
                k * (k - 1) / 2.0;

            return triangleLinks / possibleLinks;
        }


        public class KShellComputer
    {
        
        public Dictionary<int, List<Node>> GetKCoreLevels(Dictionary<Node, int> kshell)
        {
            var result = new Dictionary<int, List<Node>>();
 
            foreach (var kvp in kshell)
            {
                if (!result.ContainsKey(kvp.Value))
                    result[kvp.Value] = new List<Node>();
                result[kvp.Value].Add(kvp.Key);
            }
 
            return result.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }
    }
 
 
    public class RatioAnalyzer
    {
        private readonly Dictionary<string, double> _inputRatios = new Dictionary<string, double>
        {
            { "D", 0.25 }, { "I", 0.25 }, { "S", 0.25 }, { "C", 0.25 }
        };
 
        public List<KLevelStats> AnalyzeRatios(
            Dictionary<Node, int> kshell,
            int networkId, int nodeCount, int edgeCount, int linksPerStep)
        {
            var kshellComputer = new KShellComputer();
            var kCoreLevels = kshellComputer.GetKCoreLevels(kshell);
            var results = new List<KLevelStats>();
 
            foreach (var kvp in kCoreLevels)
            {
                int kLevel = kvp.Key;
                var nodesAtK = kvp.Value;
 
                var counts = new Dictionary<string, int>
                    { { "D", 0 }, { "I", 0 }, { "S", 0 }, { "C", 0 } };
 
                foreach (var node in nodesAtK)
                {
                    string type = GetNodeType(node);
                    if (counts.ContainsKey(type))
                        counts[type]++;
                }
 
                int total = nodesAtK.Count;
                var ratios = new Dictionary<string, double>();
                var deviations = new Dictionary<string, double>();
 
                foreach (string type in DISC_TYPES)
                {
                    double ratio = total > 0 ? (double)counts[type] / total : 0;
                    ratios[type] = ratio;
                    deviations[type] = ratio - _inputRatios[type];
                }
 
                results.Add(new KLevelStats
                {
                    KLevel = kLevel,
                    CountD = counts["D"], CountI = counts["I"],
                    CountS = counts["S"], CountC = counts["C"],
                    RatioD = ratios["D"], RatioI = ratios["I"],
                    RatioS = ratios["S"], RatioC = ratios["C"],
                    DeviationD = deviations["D"], DeviationI = deviations["I"],
                    DeviationS = deviations["S"], DeviationC = deviations["C"],
                    TotalNodes = total,
                    NetworkId = networkId,
                    NumberOfNodes = nodeCount,
                    LinksPerStep = linksPerStep
                });
            }
 
            return results;
        }
    }

 
    public class StatisticsCalculator
    {
        public class SummaryStats
        {
            public int KLevel { get; set; }
            public double MeanRatioD { get; set; }
            public double StdRatioD { get; set; }
            public double MeanRatioI { get; set; }
            public double StdRatioI { get; set; }
            public double MeanRatioS { get; set; }
            public double StdRatioS { get; set; }
            public double MeanRatioC { get; set; }
            public double StdRatioC { get; set; }
            public int ObservationCount { get; set; }
        }
 
        public List<SummaryStats> CalculateSummaryStats(List<KLevelStats> allResults)
        {
            var grouped = allResults.GroupBy(x => x.KLevel).OrderBy(x => x.Key);
            var results = new List<SummaryStats>();
 
            foreach (var group in grouped)
            {
                var ratios = group.ToList();
                results.Add(new SummaryStats
                {
                    KLevel = group.Key,
                    MeanRatioD = ratios.Average(x => x.RatioD),
                    StdRatioD  = CalculateStdDev(ratios.Select(x => x.RatioD)),
                    MeanRatioI = ratios.Average(x => x.RatioI),
                    StdRatioI  = CalculateStdDev(ratios.Select(x => x.RatioI)),
                    MeanRatioS = ratios.Average(x => x.RatioS),
                    StdRatioS  = CalculateStdDev(ratios.Select(x => x.RatioS)),
                    MeanRatioC = ratios.Average(x => x.RatioC),
                    StdRatioC  = CalculateStdDev(ratios.Select(x => x.RatioC)),
                    ObservationCount = ratios.Count
                });
            }
 
            return results;
        }
 
        private double CalculateStdDev(IEnumerable<double> values)
        {
            var list = values.ToList();
            if (list.Count < 2) return 0;
            double mean = list.Average();
            double sumOfSquares = list.Sum(x => Math.Pow(x - mean, 2));
            return Math.Sqrt(sumOfSquares / (list.Count - 1));
        }
 
        
    }
 
 

    private static int WeightedRandomSelect(double[] weights, double totalWeight, Random rnd)
    {
        if (weights.Length == 0) return -1;
        if (totalWeight <= 0)    return rnd.Next(weights.Length);
 
        double r = rnd.NextDouble() * totalWeight;
        double cumulative = 0;
 
        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (r <= cumulative) return i;
        }
 
        return weights.Length - 1;
    }
 
    /// <summary>
    /// Trích tiền tố D/I/S/C từ tên node (format: "TYPE_index").
    /// Trả về chuỗi rỗng nếu tiền tố không hợp lệ.
    /// </summary>
    private static string GetNodeType(Node node)
    {
        if (string.IsNullOrEmpty(node.name)) return "";
        string prefix = node.name.Split('_')[0];
        return DISC_TYPES.Contains(prefix) ? prefix : "";
    }

        //AnalyzeMoRoMutipleMutations
        public static void AnalyzeMoRoMutipleMutations(int nNet, int nNodeFrom, int nNodeTo, int nMinLink, int nMaxLink, string ReportFileName)
        {

            if (nNodeFrom > nNodeTo)
                throw new Exception("The node range is invalid!");
            if (nMinLink > nMaxLink)
                throw new Exception("The link range is invalid!");

            ComplexNetGenerator GeneratingNet = new BasicNet.ComplexNetGenerator();
            User.One.MessageToUser("Start generating network data on " + DateTime.Now.ToString());
            try
            {
                int i = 0;
                //foreach node
                BooleanNetwork Net = null;
                TextDB.WriteTextFile(new string[] {"NetID", "Node #", "Edge #", "Link#","Module mixing rate",
                    //"Centrality", 
                    //"Module amount", 
                    "Modularity", "Module size","Robustness", "Same-module robustness", "Diff-module robustness", "In-power law", "In-R", "In-Pvalue", "Out-power law","Out-R","Out-Pvalue"
                    //,"Controllability" 
                }, ReportFileName);
                BooleanNetwork temp = new BooleanNetwork();
                for (int j = nNodeFrom; j <= nNodeTo; j = j >= nNodeTo ? nNodeFrom : (j + 1))
                {
                    if (i > nNet) break;


                    for (int k = nMinLink; k <= nMaxLink; k++)
                    {
                        if (++i > nNet) break;

                        try
                        {
                            Net = (BooleanNetwork)GeneratingNet.generateDirectedNetworkByPreferentialAttachment(temp, j, k);
                        }
                        catch (Exception)
                        {
                            i--;
                            continue;
                        }
                        double mixingRateOfModule = 0;
                        double multationRobustness = 0;
                        double modularity = 0;
                        double inModuleRo = 0, outModuleRo = 0;
                        multationRobustness = Net.NetworkMutantRobustnessParalell();//Net.NetworkRobustness(new Perturbation(Perturbation.Kind.Mutation));
                        Dictionary<Node, int> Cluster = null;
                        modularity = Net.modularity(ref Cluster);
                        int nCluster = (from p in Cluster group p by p.Value into g select g).Count();
                        mixingRateOfModule = Net.MixingRateOfModule(Cluster);
                        //Net.InOutModuleRobustnessParalell(Cluster, new Perturbation(Perturbation.Kind.Mutation), ref inModuleRo, ref outModuleRo);
                        Net.multiplePerturbationRobustness(Cluster, new Perturbation(Perturbation.Kind.Mutation), ref inModuleRo, ref outModuleRo);

                        int nNode = Net.Nodes.Count();
                        int nEdge = Net.EdgesWithoutSelfLoops.Count();
                        int nLink = Net.Arcs.Count();


                        double inGamma = 0, outGamma = 0, inR = 0, outR = 0;
                        double inPvalue = 0, outPvalue = 0;
                        Netutil.FitDegreeDistribution(Net, ref inGamma, ref inR, ref inPvalue, ref outGamma, ref outR, ref outPvalue);





                        string[] f = new string[] {Net.ObjectID.ToString(), nNode.ToString(), nEdge.ToString(), nLink.ToString(),
                            mixingRateOfModule.ToString(),

                            modularity.ToString(), nCluster.ToString(), multationRobustness.ToString(),inModuleRo.ToString(),outModuleRo.ToString(),
                            inGamma.ToString(),inR.ToString(), inPvalue.ToString(),
                            outGamma.ToString(),outR.ToString(),outPvalue.ToString()
                            //,Net.driverNodes.Count().ToString()
                        };

                        TextDB.WriteTextFile(f, ReportFileName);

                        User.One.ShowWaitIndicator(i, nNet);

                    }

                    if (i == 0)
                    {
                        User.One.MessageToUser("Can not create any network");
                        break;
                    }
                }
                User.One.MessageToUser("Finish generating network data on " + DateTime.Now.ToString());

            }
            catch (Exception ex)
            {
                User.One.SendErrorToUser(ex);
            }
        }
        public static void AnalyzeNetworkDegreeDistribution(string network)
        {
            BooleanNetwork Net = BooleanNetwork.ReadSignalingNetworkFile(network);
            string Report = Netutil.ExtractMainFileName(network) + ".degreedistribution.txt";
            foreach (Node n in Net.Nodes)
            {
                TextDB.WriteTextFile(string.Format("{0}\t{1}\t{2}", n.name, n.InDegree, n.OutDegree), Report);
            }

        }
        public static void AnalyzeMoRoHC(int nNet, int nNodeFrom, int nNodeTo, int nMinLink, int nMaxLink, string ReportFileName)
        {

            if (nNodeFrom > nNodeTo)
                throw new Exception("The node range is invalid!");
            if (nMinLink > nMaxLink)
                throw new Exception("The link range is invalid!");

            ComplexNetGenerator GeneratingNet = new BasicNet.ComplexNetGenerator();
            User.One.MessageToUser("Start generating network data on " + DateTime.Now.ToString());
            try
            {
                int i = 0;
                //foreach node
                BooleanNetwork Net = null;
                TextDB.WriteTextFile(new string[] {"NetID", "Node #", "Edge #", "Link#","Module mixing rate",
                    //"Centrality", 
                    //"Module amount", 
                    "Modularity", "Robustness", "In-power law", "In-R", "In-Pvalue", "Out-power law","Out-R","Out-Pvalue",
                    "HC entropy", "Reachability entropy","Closeness entropy"
                }, ReportFileName);
                BooleanNetwork temp = new BooleanNetwork();
                for (int j = nNodeFrom; j <= nNodeTo; j = j >= nNodeTo ? nNodeFrom : (j + 1))
                {
                    if (i > nNet) break;


                    for (int k = nMinLink; k <= nMaxLink; k++)
                    {
                        if (++i > nNet) break;

                        try
                        {
                            //Net = (BooleanNetwork)GeneratingNet.generateDirectedNetworkByPreferentialAttachment(temp, j, k);
                            Net = (BooleanNetwork)GeneratingNet.generateScaleFreeDirectedNetwork(j, k);
                        }
                        catch (Exception)
                        {
                            i--;
                            continue;
                        }
                        double mixingRateOfModule = 0;
                        double multationRobustness = 0;
                        double modularity = 0;

                        multationRobustness = Net.NetworkMutantRobustnessParalell();//Net.NetworkRobustness(new Perturbation(Perturbation.Kind.Mutation));
                        Dictionary<Node, int> Cluster = null;
                        modularity = Net.modularity(ref Cluster);
                        mixingRateOfModule = Net.MixingRateOfModule(Cluster);


                        int nNode = Net.Nodes.Count();
                        int nEdge = Net.EdgesWithoutSelfLoops.Count();
                        int nLink = Net.Arcs.Count();


                        double inGamma = 0, outGamma = 0, inR = 0, outR = 0;
                        double inPvalue = 0, outPvalue = 0;
                        Netutil.FitDegreeDistribution(Net, ref inGamma, ref inR, ref inPvalue, ref outGamma, ref outR, ref outPvalue);

                        Dictionary<string, double> HC = new Dictionary<string, double>();
                        Dictionary<string, double> Closeness = new Dictionary<string, double>();
                        Dictionary<string, double> Reachability = new Dictionary<string, double>();

                        Dictionary<string, Triple<double>> HCana = Net.HierarchicalClosenessCentralityAnalysis();
                        foreach (var p in HCana)
                        {
                            HC.Add(p.Key, p.Value.A);
                            Reachability.Add(p.Key, p.Value.B);
                            Closeness.Add(p.Key, p.Value.C);
                        }
                        double HCent = BasicNetwork.EntropyOfNodes(HC),
                            ReachabilityEnt = BasicNetwork.EntropyOfNodes(Reachability),
                            ClosenessEnt = BasicNetwork.EntropyOfNodes(Closeness);

                        string[] f = new string[] {Net.ObjectID.ToString(), nNode.ToString(), nEdge.ToString(), nLink.ToString(),
                            mixingRateOfModule.ToString(),
                            //centrality.ToString(), nCluster.Count().ToString(), 
                            modularity.ToString(), multationRobustness.ToString(),
                            inGamma.ToString(),inR.ToString(), inPvalue.ToString(),
                            outGamma.ToString(),outR.ToString(),outPvalue.ToString(),
                            HCent.ToString(),
                            ReachabilityEnt.ToString(),
                            ClosenessEnt.ToString()
                        };

                        TextDB.WriteTextFile(f, ReportFileName);

                        User.One.ShowWaitIndicator(i, nNet);

                    }

                    if (i == 0)
                    {
                        User.One.MessageToUser("Can not create any network");
                        break;
                    }
                }
                User.One.MessageToUser("Finish generating network data on " + DateTime.Now.ToString());

            }
            catch (Exception ex)
            {
                User.One.SendErrorToUser(ex);
            }
        }
        public static void AnalyzeMoRoByConfigureModel(int nNet, int nNodeFrom, int nNodeTo, double inDegPw, double outDegPw, string ReportFileName)
        {
            NumericMath.RandomCraft Rnd = new NumericMath.RandomCraft(NumericMath.RandomCraft.Next(1, int.MaxValue - 1));
            if (nNodeFrom > nNodeTo)
                throw new Exception("The node range is invalid!");
            bool IsRandomInDeg = false;
            if (inDegPw == 0.0) IsRandomInDeg = true;
            bool IsRandomOutDeg = false;
            if (outDegPw == 0.0) IsRandomOutDeg = true;

            ComplexNetGenerator GeneratingNet = new BasicNet.ComplexNetGenerator();
            User.One.MessageToUser("Start generating network data on " + DateTime.Now.ToString());
            try
            {
                int i = 0;
                //foreach node
                BooleanNetwork Net = new BooleanNetwork();
                TextDB.WriteTextFile(new string[] {"NetID", "Node #", "Edge #","Link #","In-degree gamma", "Out-degree gamma", "Module Mixing rate",
                    //"Centrality", 
                    //"Module amount", 
                    "Modularity", "Robustness", "In-module robustness", "Out-module robustness",
                    "In-power law", "In-R", "In-Pvalue", "Out-power law","Out-R","Out-Pvalue"
                    //,"Controllability" 
                }, ReportFileName);

                for (int j = nNodeFrom; j <= nNodeTo; j = j >= nNodeTo ? nNodeFrom : (j + 1))
                {
                    if (i > nNet) break;



                    if (++i > nNet) break;

                    inDegPw = IsRandomInDeg ? NumericMath.RandomCraft.dRandBetween(1, 10) : inDegPw;
                    outDegPw = IsRandomOutDeg ? NumericMath.RandomCraft.dRandBetween(1, 10) : outDegPw;

                    Net = GeneratingNet.GenConfModel4Directed(Net, j, inDegPw, outDegPw, Rnd) as BooleanNetwork;
                    if (!Net.IsConnected)
                    {
                        i--;
                        continue;
                    }



                    double modularity = 0;
                    double inModuleRo = 0, outModuleRo = 0;



                    int nNode = Net.Nodes.Count();
                    int nEdge = Net.Edges.Count();
                    int nLink = Net.Arcs.Count();

                    double multationRobustness = 0;
                    double mixingRate = 0;
                    Dictionary<Node, int> Cluster = null;
                    modularity = Net.modularity(ref Cluster);
                    multationRobustness = Net.NetworkMutantRobustnessParalell();
                    Net.InOutModuleRobustnessParalell(Cluster, new Perturbation(Perturbation.Kind.Mutation), ref inModuleRo, ref outModuleRo);
                    mixingRate = Net.MixingRateOfModule(Cluster);

                    double inGamma = 0, outGamma = 0, inR = 0, outR = 0;
                    double inPvalue = 0, outPvalue = 0;
                    Netutil.FitDegreeDistribution(Net, ref inGamma, ref inR, ref inPvalue, ref outGamma, ref outR, ref outPvalue);


                    string[] f = new string[] {Net.ObjectID.ToString(), nNode.ToString(), nEdge.ToString(), nLink.ToString(),
                            inDegPw.ToString(),
                            outDegPw.ToString(),
                            mixingRate.ToString(),
                            //centrality.ToString(), nCluster.Count().ToString(), 
                            modularity.ToString(), multationRobustness.ToString(),inModuleRo.ToString(),outModuleRo.ToString(),
                            inGamma.ToString(),inR.ToString(), inPvalue.ToString(),
                            outGamma.ToString(),outR.ToString(),outPvalue.ToString()
                            //,Net.driverNodes.Count().ToString()
                        };

                    TextDB.WriteTextFile(f, ReportFileName);

                    User.One.ShowWaitIndicator(i, nNet);


                    if (i == 0)
                    {
                        User.One.MessageToUser("Can not create any network");
                        break;
                    }
                }
                User.One.MessageToUser("Finish generating network data on " + DateTime.Now.ToString());

            }
            catch (Exception ex)
            {
                User.One.SendErrorToUser(ex);
            }
        }
        /// <summary>
        /// Analysis the modularity & robustness relationship in the case of network density fixed
        /// </summary>
        /// <param name="Startdensity">Begin of network density</param>
        /// <param name="Enddensity">End of network density </param>
        /// <param name="fromNode">Network size from</param>
        /// <param name="toNode">Network size to</param>
        /// <param name="NetNum">The number of networks randomly generated</param>
        public static void AnalyzeModularityRobustnessWithFixedDensity(decimal Startdensity, decimal Enddensity, int fromNode, int toNode, int NetNum)
        {
            ComplexNetGenerator GeneratingNet = new BasicNet.ComplexNetGenerator();
            try
            {
                string ReportFileName = "NetByDensity." + Startdensity.ToString() + "." + Enddensity.ToString() + "." + fromNode.ToString() + "." + toNode.ToString() + ".txt";
                TextDB.WriteTextFile(new string[] { "NetID", "Node", "Edge #", "Link #", "Module Mixing rate",
                    "Modularity", "Robustness", "In-module robustness", "Out-module robustness",
                "In-power law", "In-R", "In-Pvalue", "Out-power law","Out-R","Out-Pvalue"}, ReportFileName);

                User.One.MessageToUser(string.Format("Create networks with Density = [{0}, {1}], Node range = [{2}..{3}], The number of networks = {4}. Start time of {5}", Startdensity, Enddensity, fromNode, toNode, NetNum, DateTime.Now.ToString()));
                User.One.MessageToUser(string.Format("Networks will be saved in file {0}", ReportFileName));
                //foreach node
                decimal density = 0.0M;
                int net = 1;
                BooleanNetwork temp = new BooleanNetwork();
                for (density = Startdensity; net <= NetNum; density = (density > Enddensity ? Startdensity : density + 0.005M))
                {
                    for (int node = fromNode;
                        net <= NetNum;
                        node = (node > toNode ? fromNode : node + 1), net++)
                    {

                        decimal link = Math.Round((node * node - node) * density, 0);
                        BooleanNetwork sf = null;
                        try
                        {
                            sf = (BooleanNetwork)GeneratingNet.generateDirectedNetworkByPreferentialAttachment(temp, node, (int)link);
                        }
                        catch (Exception)
                        {
                            net--;
                            continue;
                        }
                        //float centrality = sf.Centrality;



                        double multationRobustness = sf.NetworkMutantRobustnessParalell(); //sf.NetworkRobustness(new Perturbation(Perturbation.Kind.Mutation));



                        Dictionary<Node, int> Cluster = null;
                        double modularity = sf.modularity(ref Cluster);
                        int nNode = sf.Nodes.Count();
                        int nEdge = sf.Edges.Count();
                        int nArc = sf.Arcs.Count();

                        double inMoRo = 0, outMoRo = 0;
                        double mixingRate = sf.MixingRateOfModule(Cluster);

                        sf.InOutModuleRobustnessParalell(Cluster, new Perturbation(), ref inMoRo, ref outMoRo);//.InOutModuleRobustness(Cluster, new Perturbation(), ref inMoRo, ref outMoRo);

                        double inGamma = 0, outGamma = 0, inR = 0, outR = 0;
                        double inPvalue = 0, outPvalue = 0;
                        Netutil.FitDegreeDistribution(sf, ref inGamma, ref inR, ref inPvalue, ref outGamma, ref outR, ref outPvalue);

                        string[] f = new string[] { sf.ObjectID.ToString(), nNode.ToString(), nEdge.ToString(),nArc.ToString(), mixingRate.ToString(),
                            modularity.ToString(), multationRobustness.ToString(), inMoRo.ToString(), outMoRo.ToString(),
                            inGamma.ToString(),inR.ToString(), inPvalue.ToString(),
                            outGamma.ToString(),outR.ToString(),outPvalue.ToString()
                        };

                        TextDB.WriteTextFile(f, ReportFileName);

                        User.One.ShowWaitIndicator(net, NetNum);
                    }
                }
                User.One.MessageToUser("Create a file " + ReportFileName + " on " + DateTime.Now.ToString());

            }
            catch (Exception ex)
            {

                User.One.SendErrorToUser(ex);
            }
        }

        public static void AnalyzeModularityRobustnessWithRewiredNet(string FileName, float fromRate, float toRate, int NetNum)
        {
            ComplexNetGenerator GeneratingNet = new BasicNet.ComplexNetGenerator();
            try
            {
                string ReportFileName = "Rewire network.MoRo" + Netutil.ExtractMainFileName(FileName) + "." + fromRate.ToString() + "." + toRate.ToString() + ".txt";
                TextDB.WriteTextFile(new string[] { "NetID", "Node", "Link", "Modularity", "Robustness", "In-module robustness", "Out-module robustness" }, ReportFileName);

                User.One.MessageToUser(string.Format("Create rewired networks from \"{0}\" with perturbed rate [{1}, {2}], total of networks ={3}, at time {4}", FileName, fromRate, toRate, NetNum, DateTime.Now.ToString()));
                BooleanNetwork originalNet = BooleanNetwork.ReadSignalingNetworkFile(FileName);
                BooleanNetwork perturbedNet = null;
                User.One.MessageToUser(string.Format("File \"{0}\" is loaded", FileName));
                //foreach node
                double rate = 0.0;

                for (int i = 0; i < NetNum; i++)
                {

                    rate = fromRate + Mathutil.NumericMath.RandomCraft.NextDouble() * (toRate - fromRate);

                    perturbedNet = originalNet.ShufflePreservingDegree((int)(rate * originalNet.Arcs.Count())) as BooleanNetwork;


                    double multationRobustness = perturbedNet.NetworkMutantRobustnessParalell(); //sf.NetworkRobustness(new Perturbation(Perturbation.Kind.Mutation));



                    Dictionary<Node, int> Cluster = null;
                    double modularity = perturbedNet.modularity(ref Cluster);
                    int nNode = perturbedNet.Nodes.Count();
                    int nLink = perturbedNet.Arcs.Count();

                    double inMoRo = 0, outMoRo = 0;
                    perturbedNet.InOutModuleRobustnessParalell(Cluster, new Perturbation(), ref inMoRo, ref outMoRo);//.InOutModuleRobustness(Cluster, new Perturbation(), ref inMoRo, ref outMoRo);

                    string[] f = new string[] { perturbedNet.ObjectID.ToString(), nNode.ToString(), nLink.ToString(), modularity.ToString(), multationRobustness.ToString(), inMoRo.ToString(), outMoRo.ToString() };

                    TextDB.WriteTextFile(f, ReportFileName);

                    User.One.ShowWaitIndicator(i, NetNum);
                }
                User.One.MessageToUser("Create a file " + ReportFileName + " on " + DateTime.Now.ToString());

            }
            catch (Exception ex)
            {

                User.One.SendErrorToUser(ex);
            }
        }
        public static void RBN_AnalyzeModuleRobustness(int nNet, int nNodeFrom, int nNodeTo, int nMinLink, int nMaxLink, string ReportFileName)
        {
            if (nNodeFrom > nNodeTo)
                throw new Exception("The node range is invalid!");
            if (nMinLink > nMaxLink)
                throw new Exception("The link range is invalid!");

            ComplexNetGenerator GeneratingNet = new BasicNet.ComplexNetGenerator();
            User.One.MessageToUser("Start generating network data on " + DateTime.Now.ToString());
            try
            {
                int i = 0;
                BooleanNetwork Net = null;
                TextDB.WriteTextFile(new string[] {
                    "NetID", "Node", "Link", "Centrality", "Network modularity", "Network robustness","Network robustness at module-level",
                    "ModuleID", "In-Module Modularity", "In-Module Robustness", "Out-Module Robustness", "Group node Robustness",
                    "Subnet node", "Subnet edge", "Isolate subnet robustness","Isolate subnet Modularity",
                    "Module-node Degree", "Module-node In-Degree","Module-node Out-Degree","Module-node robustness"
                }, ReportFileName);
                #region code
                BooleanNetwork temp = new BooleanNetwork();
                for (int j = nNodeFrom; j <= nNodeTo; j = j >= nNodeTo ? nNodeFrom : (j + 1))
                {
                    if (i > nNet) break;


                    for (int k = nMinLink; k <= nMaxLink; k++)
                    {
                        if (++i > nNet) break;

                        try
                        {
                            Net = (BooleanNetwork)GeneratingNet.generateDirectedNetworkByPreferentialAttachment(temp, j, k);
                        }
                        catch (Exception)
                        {
                            i--;
                            continue;
                        }
                        float centrality = Net.DegreeCentrality;
                        double multationRobustness = Net.NetworkRobustness(new Perturbation(Perturbation.Kind.Mutation));

                        Dictionary<Node, int> Cluster = null, pTemp = null;
                        double modularity = Net.modularity(ref Cluster);

                        int nNode = Net.Nodes.Count();
                        int nLink = Net.Arcs.Count();

                        Dictionary<int, double> InModuleRo = Net.InModuleRobustness(Cluster, new Perturbation());
                        Dictionary<int, double> OutModuleRo = Net.OutModuleRobustness(Cluster, new Perturbation());
                        Dictionary<int, double> ModuleMo = Net.ModuleModularity(Cluster);

                        BooleanNetwork clusterNetwork = Net.CreateClusterNework(Cluster) as BooleanNetwork;
                        //clusterNetwork.CreateClusterNework(Net, Cluster);
                        double cRo = clusterNetwork.NetworkRobustness(new Perturbation());
                        #endregion
                        foreach (int t in InModuleRo.Keys)
                        {
                            BooleanNode cnode = (clusterNetwork.Nodes.Where(e => e.name == t.ToString())).Select(e => e).ElementAt(0) as BooleanNode;

                            string[] f = new string[] {
                                Net.ObjectID.ToString(), nNode.ToString(), nLink.ToString(), centrality.ToString(), modularity.ToString(), multationRobustness.ToString(),cRo.ToString(),
                                cnode.name, ModuleMo[t].ToString(), InModuleRo[t].ToString(),OutModuleRo[t].ToString(), Net.NodeGroupRobustness((from p in Cluster where p.Value==t select p.Key),new Perturbation()).ToString(),
                                cnode.SubNetwork.Nodes.Count().ToString(),cnode.SubNetwork.Edges.Count().ToString(), (cnode.SubNetwork as BooleanNetwork).NetworkRobustness(new Perturbation()).ToString(), cnode.SubNetwork.modularity(ref pTemp).ToString(),
                                cnode.EdgeDegree.ToString(),cnode.InDegree.ToString(),cnode.OutDegree.ToString(),clusterNetwork.NodeRobustness(cnode, new Perturbation()).ToString()
                            };

                            TextDB.WriteTextFile(f, ReportFileName);

                        }



                        User.One.ShowWaitIndicator(i, nNet);

                    }

                    if (i == 0)
                    {
                        User.One.MessageToUser("Can not create any network");
                        break;
                    }
                }
                User.One.MessageToUser("Finish generating network data on " + DateTime.Now.ToString());

            }
            catch (Exception ex)
            {
                User.One.SendErrorToUser(ex);
            }
        }
        /// <summary>
        /// Examine in Random Boolean Network (RBN) how the negative relationships between Mo & Ro are, respectively in structural modules and random groups
        /// </summary>
        /// <param name="nNet">The number of RBNs</param>
        /// <param name="nNodeFrom">Node size is from</param>
        /// <param name="nNodeTo">Node size is to</param>
        /// <param name="nMinLink">Link size is from</param>
        /// <param name="nMaxLink">Link size is to</param>
        /// <param name="ReportFileName">The file to save the result</param>
        public static void RBN_MoRoOnModuleAndRandomGroup(int nNet, int nNodeFrom, int nNodeTo, int nMinLink, int nMaxLink, string ReportFileName)
        {

            if (nNodeFrom > nNodeTo)
                throw new Exception("The node range is invalid!");
            if (nMinLink > nMaxLink)
                throw new Exception("The link range is invalid!");

            ComplexNetGenerator GeneratingNet = new BasicNet.ComplexNetGenerator();
            User.One.MessageToUser("Start generating network data on " + DateTime.Now.ToString());
            try
            {
                int i = 0;
                BooleanNetwork Net = null;
                TextDB.WriteTextFile(new string[] {
                        "NetID", "Node", "Link", "Network modularity", "Network robustness",
                        "ModuleID","Module Node", "Module Interaction", "Module modularity","Independent Module robustness", "Dependent Module robustness",
                        "RandomGroupID", "RandomGroup Node","RandomGroup Interaction", "RandomGroup modularity", "Independent RandomGroup robustness", "Dependent RandomGroup robustness",
                    }, ReportFileName);
                BooleanNetwork temp = new BooleanNetwork();
                for (int j = nNodeFrom; j <= nNodeTo; j = j >= nNodeTo ? nNodeFrom : (j + 1))
                {
                    if (i > nNet) break;


                    for (int k = nMinLink; k <= nMaxLink; k++)
                    {
                        if (++i > nNet) break;

                        try
                        {
                            Net = GeneratingNet.generateDirectedNetworkByPreferentialAttachment(temp, j, k) as BooleanNetwork;
                        }
                        catch (Exception)
                        {
                            i--;
                            continue;
                        }


                        Dictionary<Node, int> mainCluster = null, subCluster = null, randCluster = null;
                        double netMo = Net.modularity(ref mainCluster);
                        double netRo = Net.NetworkMutantRobustnessParalell();
                        int nNode = Net.Nodes.Count();
                        int nLink = Net.Arcs.Count();
                        BooleanNetwork moduleNet = Net.CreateClusterNework(mainCluster) as BooleanNetwork;
                        //moduleNet.CreateClusterNework(Net,mainCluster);

                        foreach (BooleanNode node in moduleNet.Nodes)
                        {
                            BooleanNetwork randomSubnet = (BooleanNetwork)Net.CreateNewRandomSubnetwork(node.SubNetwork.Nodes.Count());


                            TextDB.WriteTextFile(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}\t{14}\t{15}\t{16}",
                                Net.ObjectID, nNode, nLink, netMo, netRo,
                                node.name, node.SubNetwork.Nodes.Count(), node.SubNetwork.Arcs.Count(), node.SubNetwork.modularity(ref subCluster), (node.SubNetwork as BooleanNetwork).NetworkRobustness(new Perturbation()), Net.NetworkRobustness(Net.SelectNode(node.SubNetwork.Nodes), new Perturbation()),
                                randomSubnet.ObjectID, randomSubnet.Nodes.Count(), randomSubnet.Arcs.Count(), randomSubnet.modularity(ref randCluster), randomSubnet.NetworkRobustness(new Perturbation()), Net.NetworkRobustness(Net.SelectNode(randomSubnet.Nodes), new Perturbation())
                            ), ReportFileName);

                        }
                        User.One.ShowWaitIndicator(i, nNet);
                    }

                    if (i == 0)
                    {
                        User.One.MessageToUser("Can not create any network");
                        break;
                    }
                }
                User.One.MessageToUser("Finish generating network data on " + DateTime.Now.ToString());

            }
            catch (Exception ex)
            {
                User.One.SendErrorToUser(ex);
            }
        }
        #endregion

        #region Tran's centrality
        /// <summary>
        /// Compute Tran's centrality of a signaling network stored in a file
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <param name="isDirected">That is directed or undirected network</param>
        public static void ComputeCentrality(string fileFolderName)
        {
            if (fileFolderName.LastIndexOf(".txt") > 0 || fileFolderName.LastIndexOf(".xls") > 0
                || fileFolderName.LastIndexOf(".xlsx") > 0 || fileFolderName.LastIndexOf(".xml") > 0)// not a folder
            {
                string ReportFileName = "Centrality." + fileFolderName;
                BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(fileFolderName);

                WriteCentralityToFile(net, ReportFileName);
            }
            else
            {
                string OutputFolder = "Centrality." + fileFolderName;
                IEnumerable<string> files = Directory.EnumerateFiles(Netutil.InPutDirector + (fileFolderName != "" ? "\\" + fileFolderName : ""));
                int i = 1;

                Netutil.CreateOutputFolder(OutputFolder);

                foreach (string fileName in files)
                {
                    string ReportFileName = "Centrality." + Netutil.ExtractMainFileName(Netutil.ExtractFileNameFromPath(fileName)) + ".txt";
                    BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(fileName);
                    WriteCentralityToFile(net, OutputFolder + "\\" + ReportFileName);


                    User.One.ShowWaitIndicator(i++, files.Count());
                }
            }


        }

        public static void FindDriverNode(string fileFolderName)
        {
            if (fileFolderName.LastIndexOf(".txt") > 0 || fileFolderName.LastIndexOf(".xls") > 0
                || fileFolderName.LastIndexOf(".xlsx") > 0 || fileFolderName.LastIndexOf(".xml") > 0)// not a folder
            {
                string ReportFileName = "DriverNode." + fileFolderName;
                BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(fileFolderName);

                WriteDriverNodeToFile(net, ReportFileName);
            }
        }

        public static void FociNetwork(string fileFolderName)
        {
            if (fileFolderName.LastIndexOf(".txt") > 0 || fileFolderName.LastIndexOf(".xls") > 0
                || fileFolderName.LastIndexOf(".xlsx") > 0 || fileFolderName.LastIndexOf(".xml") > 0)// not a folder
            {
                string ReportFileName = "Foci." + fileFolderName;
                BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(fileFolderName);

                WriteFociToFile(net, ReportFileName);
            }
            else
            {
                string OutputFolder = "Foci." + fileFolderName;
                IEnumerable<string> files = Directory.EnumerateFiles(Netutil.InPutDirector + (fileFolderName != "" ? "\\" + fileFolderName : ""));
                int i = 1;

                Netutil.CreateOutputFolder(OutputFolder);

                foreach (string fileName in files)
                {
                    string ReportFileName = "Foci." + Netutil.ExtractMainFileName(Netutil.ExtractFileNameFromPath(fileName)) + ".txt";
                    BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(fileName);
                    WriteFociToFile(net, OutputFolder + "\\" + ReportFileName);
                    User.One.ShowWaitIndicator(i++, files.Count());
                }
            }
        }

        public static void GraphFociNetwork(string fileFolderName)
        {
            if (fileFolderName.LastIndexOf(".txt") > 0 || fileFolderName.LastIndexOf(".xls") > 0
                || fileFolderName.LastIndexOf(".xlsx") > 0 || fileFolderName.LastIndexOf(".xml") > 0)// not a folder
            {
                string ReportFileName = "SocialNetwork." + fileFolderName;
                BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(fileFolderName);

                WriteGraphFociToFile(net, ReportFileName);
            }
            else
            {
                string OutputFolder = "SocialNetwork." + fileFolderName;
                IEnumerable<string> files = Directory.EnumerateFiles(Netutil.InPutDirector + (fileFolderName != "" ? "\\" + fileFolderName : ""));

                int i = 1;

                Netutil.CreateOutputFolder(OutputFolder);

                foreach (string fileName in files)
                {
                    string ReportFileName = "SocialNetwork." + Netutil.ExtractMainFileName(Netutil.ExtractFileNameFromPath(fileName)) + ".txt";
                    BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(fileName);
                    WriteGraphFociToFile(net, OutputFolder + "\\" + ReportFileName);
                    User.One.ShowWaitIndicator(i++, files.Count());
                }
            }
        }

        public static void ConvertCommucation(string fileFolderName)
        {
              if (fileFolderName.LastIndexOf(".txt") > 0 || fileFolderName.LastIndexOf(".xls") > 0
                || fileFolderName.LastIndexOf(".xlsx") > 0 || fileFolderName.LastIndexOf(".xml") > 0)
            {
                string ReportFileName = "Convert." + fileFolderName;
                BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(fileFolderName);
                WriteConvertToFile(net, ReportFileName);
            }
            else
            {
                string OutputFolder = "Convert." + fileFolderName;
                IEnumerable<string> files = Directory.EnumerateFiles(Netutil.InPutDirector
                    + (fileFolderName != "" ? "\\" 
                    + fileFolderName : ""));
                int i = 1;
                Netutil.CreateOutputFolder(OutputFolder);
                foreach (string fileName in files)
                {
                    string ReportFileName = "Balance."
                        + Netutil.ExtractMainFileName(Netutil.ExtractFileNameFromPath(fileName))
                        + ".txt";
                    BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(fileName);
                    WriteConvertToFile(net, OutputFolder + "\\" + ReportFileName);
                    User.One.ShowWaitIndicator(i++, files.Count());
                }
            }
        }
        
        public static void BalanceCommucation(string fileFolderName)
        {
              if (fileFolderName.LastIndexOf(".txt") > 0 || fileFolderName.LastIndexOf(".xls") > 0
                || fileFolderName.LastIndexOf(".xlsx") > 0 || fileFolderName.LastIndexOf(".xml") > 0)// not a folder
            {
                BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(fileFolderName);
                WriteBalanceToFile(net);
            }
            else
            {
                string OutputFolder = "Balance." + fileFolderName;
                IEnumerable<string> files = Directory.EnumerateFiles(Netutil.InPutDirector + (fileFolderName != "" ? "\\" 
                    + fileFolderName : ""));
                int i = 1;
                Netutil.CreateOutputFolder(OutputFolder);
                foreach (string fileName in files)
                {
                    BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(fileName);
                    WriteBalanceToFile(net);
                    User.One.ShowWaitIndicator(i++, files.Count());
                }
            }
        }

         public static void ComputeConnected(string fileFolderName)
        {
            if (fileFolderName.LastIndexOf(".txt") > 0 || fileFolderName.LastIndexOf(".xls") > 0
                || fileFolderName.LastIndexOf(".xlsx") > 0 || fileFolderName.LastIndexOf(".xml") > 0)// not a folder
            {
                string ReportFileName = "Connected." + fileFolderName;
                BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(fileFolderName);

                WriteSCCToFile(net, ReportFileName);
            }
            else
            {
                string OutputFolder = "Connected" + fileFolderName;
                IEnumerable<string> files = Directory.EnumerateFiles(Netutil.InPutDirector + (fileFolderName != "" ? "\\" + fileFolderName : ""));
                int i = 1;

                Netutil.CreateOutputFolder(OutputFolder);

                foreach (string fileName in files)
                {
                    string ReportFileName = "Connected." + Netutil.ExtractMainFileName(Netutil.ExtractFileNameFromPath(fileName)) + ".txt";
                    BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(fileName);
                    WriteSCCToFile(net, OutputFolder + "\\" + ReportFileName);


                    User.One.ShowWaitIndicator(i++, files.Count());
                }
            }
        }
            public static void CheckGateKeeper(string fileFolderName)
        {
            
            if (fileFolderName.LastIndexOf(".txt") > 0 || fileFolderName.LastIndexOf(".xls") > 0
                || fileFolderName.LastIndexOf(".xlsx") > 0 || fileFolderName.LastIndexOf(".xml") > 0)// not a folder
            {
                string ReportFileName = "CheckGatekeeper." + fileFolderName;
                BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(fileFolderName);

                WriteToFileGateKepper(net, ReportFileName);
            }
            else
            {
                string OutputFolder = "CheckGatekeeper." + fileFolderName;
                IEnumerable<string> files = Directory.EnumerateFiles(Netutil.InPutDirector + (fileFolderName != "" ? "\\" + fileFolderName : ""));
                int i = 1;

                Netutil.CreateOutputFolder(OutputFolder);

                foreach (string fileName in files)
                {
                    string ReportFileName = "CheckGatekeeper." + Netutil.ExtractMainFileName(Netutil.ExtractFileNameFromPath(fileName)) + ".txt";
                    BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(fileName);
                    WriteToFileGateKepper(net, OutputFolder + "\\" + ReportFileName);


                    User.One.ShowWaitIndicator(i++, files.Count());
                }
            }
        }

        public static void CoreDecomposition(string fileFolderName)
        {
            if (fileFolderName.LastIndexOf(".txt") > 0 || fileFolderName.LastIndexOf(".xls") > 0
                || fileFolderName.LastIndexOf(".xlsx") > 0 || fileFolderName.LastIndexOf(".xml") > 0)// not a folder
            {
                string ReportFileName = "Core." + fileFolderName;
                BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(fileFolderName);

                WriteCoreDecompositionToFile(net, ReportFileName);
            }
            else
            {
                string OutputFolder = "Core." + fileFolderName;
                IEnumerable<string> files = Directory.EnumerateFiles(Netutil.InPutDirector + (fileFolderName != "" ? "\\" + fileFolderName : ""));
                int i = 1;

                Netutil.CreateOutputFolder(OutputFolder);

                foreach (string fileName in files)
                {
                    string ReportFileName = "Core." + Netutil.ExtractFileNameFromPath(fileName);
                    BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(fileName);
                    WriteCoreDecompositionToFile(net, OutputFolder + "\\" + ReportFileName);


                    User.One.ShowWaitIndicator(i++, files.Count());
                }
            }


        }
        private static void WriteCoreDecompositionToFile(BooleanNetwork net, string ReportFileName)
        {

            Dictionary<Node, int> kShell = net.K_ShellCentrality();
            Dictionary<Node, int> rShell = net.R_ShellCentrality();

            TextDB.WriteTextFile(new string[] { "Node name", "K-shell", "R-shell" }, ReportFileName);

            foreach (KeyValuePair<Node, int> e in kShell)
            {
                TextDB.WriteTextFile(new string[] {
                    e.Key.name,
                    kShell[e.Key].ToString(),
                    rShell[e.Key].ToString(),
                },

                    ReportFileName);
            }
            User.One.MessageToUser("The result is saved in file " + ReportFileName);
        }
#if !PROCESS_NET
        /*
        private static void WriteCentralityToFile(BooleanNetwork net, string ReportFileName)
        {
            Dictionary<string, double> hcCentrality = net.HierarchicalClosenessCentrality();
            //Dictionary<string, double> closenessCentrality = net.ClosenessCentrality();
            Dictionary<Node, int> kShell = net.K_ShellCentrality();
            //Dictionary<Node, int> rShell = net.R_ShellCentrality();
            //Dictionary<Node, float> betweenessCentrality = net.BetweenessCentrality();
            //Dictionary<Node, float> pageRankInCentrality = net.PageRankCentralityInLink();
            //Dictionary<Node, float> pageRankOutCentrality = net.PageRankCentralityOutLink();
            //Dictionary<Node, float> katzCentrality = net.KatzCentrality();
            //Dictionary<Node, int> corenessCentrality = net.K_CorenessCentrality();
            //Dictionary<Node, float> epidemicSIR = net.EpidemicBySIR_Centrality(-1, 0.8f);
            //Dictionary<Node, float> rurmorSpreading = net.RumorSpeader_Centrality(1, -1, 0.8f);
            //Dictionary<Node, float> TaxiPassenger = net.TaxiPassengerRank();
            //Dictionary<Node, float> OutsideLoyality = net.Competition_SumOutsideLoyalPoint();
            TextDB.WriteTextFile(new string[] { "Node name", "K-shell", "Hierarchical closeness"}, ReportFileName);
            double mo = 0;
            Dictionary<Node, int> pCluster = BasicNet.OptimizerModularityExactly.ClusterGraph(net, true, ref mo);// using undirected weight modularity function
            int i = 0;
            foreach (KeyValuePair<Node, int> e in pCluster)
            {
                TextDB.WriteTextFile(new string[] {
                    e.Key.name,
                    kShell[net[e.Key.name]].ToString(),
                    hcCentrality[e.Key.name].ToString(),
                    //OutsideLoyality[net[e.Key.name]].ToString()
                },

                    ReportFileName);
            }
            User.One.MessageToUser("The result is saved in file " + ReportFileName);
        }
        */
        private static void WriteCentralityToFile(BooleanNetwork net, string ReportFileName)
        {
            Dictionary<string, double> hcCentrality = net.HierarchicalClosenessCentrality();
            Dictionary<string, double> closenessCentrality = net.ClosenessCentrality();
            Dictionary<Node, int> kShell = net.K_ShellCentrality();
            Dictionary<Node, int> rShell = net.R_ShellCentrality();
            Dictionary<Node, float> betweenessCentrality = net.BetweenessCentrality();
            Dictionary<Node, float> pageRankInCentrality = net.PageRankCentralityInLink();
            Dictionary<Node, float> pageRankOutCentrality = net.PageRankCentralityOutLink();
            Dictionary<Node, float> katzCentrality = net.KatzCentrality();
            Dictionary<Node, int> corenessCentrality = net.K_CorenessCentrality();
            Dictionary<Node, float> epidemicSIR = net.EpidemicBySIR_Centrality(-1, 0.8f);
            Dictionary<Node, float> rurmorSpreading = net.RumorSpeader_Centrality(1, -1, 0.8f);
            Dictionary<Node, float> TaxiPassenger = net.TaxiPassengerRank();
            //Dictionary<Node, float> OutsideLoyality = net.Competition_SumOutsideLoyalPoint();
            TextDB.WriteTextFile(new string[] { "Node name", "K-shell", "R-shell", "K-Coreness", "Hierarchical closeness", "Closeness", "Betweeness", "Page rank in", "Total degree", "Katz", "Page rank out", "Epidemic SIR", "Rumor spreading", "Taxi passenger ranking", "UW_ModuleID"/*, "Total support"*/ }, ReportFileName);
            double mo = 0;
            Dictionary<Node, int> pCluster = BasicNet.OptimizerModularityExactly.ClusterGraph(net, true, ref mo);// using undirected weight modularity function
            int i = 0;
            foreach (KeyValuePair<Node, int> e in pCluster)
            {


                TextDB.WriteTextFile(new string[] {
                    e.Key.name,
                    kShell[net[e.Key.name]].ToString(),
                    rShell[net[e.Key.name]].ToString(),
                    corenessCentrality[net[e.Key.name]].ToString(),
                    hcCentrality[e.Key.name].ToString(),
                    closenessCentrality[e.Key.name].ToString(),
                    betweenessCentrality[net[e.Key.name]].ToString(),
                    pageRankInCentrality[net[e.Key.name]].ToString(),
                    net[e.Key.name].TotalDegree.ToString(),
                    katzCentrality[net[e.Key.name]].ToString(),
                    pageRankOutCentrality[net[e.Key.name]].ToString(),
                    epidemicSIR[net[e.Key.name]].ToString(),
                    rurmorSpreading[net[e.Key.name]].ToString(),
                    TaxiPassenger[net[e.Key.name]].ToString(),
                    pCluster[e.Key].ToString(),
                    //OutsideLoyality[net[e.Key.name]].ToString()
                },

                    ReportFileName);
            }
            User.One.MessageToUser("The result is saved in file " + ReportFileName);
        }

        private static void WriteDriverNodeToFile(BooleanNetwork net, string ReportFileName)
        {
            Dictionary<string, double> hcCentrality = net.HierarchicalClosenessCentrality();
            Dictionary<string, double> closenessCentrality = net.ClosenessCentrality();
            Dictionary<Node, int> kShell = net.K_ShellCentrality();
            Dictionary<Node, int> rShell = net.R_ShellCentrality();
            Dictionary<Node, float> betweenessCentrality = net.BetweenessCentrality();
            Dictionary<Node, float> pageRankInCentrality = net.PageRankCentralityInLink();
            Dictionary<Node, float> pageRankOutCentrality = net.PageRankCentralityOutLink();
            Dictionary<Node, float> katzCentrality = net.KatzCentrality();
            Dictionary<Node, int> corenessCentrality = net.K_CorenessCentrality();
            Dictionary<Node, float> epidemicSIR = net.EpidemicBySIR_Centrality(-1, 0.8f);
            Dictionary<Node, float> rurmorSpreading = net.RumorSpeader_Centrality(1, -1, 0.8f);
            Dictionary<Node, float> TaxiPassenger = net.TaxiPassengerRank();

            var sortedCorenessCentrality = (from e in corenessCentrality orderby e.Value descending select e).Take(3).ToDictionary(pair => pair.Key, pair => pair.Value);
            TextDB.WriteTextFile(new string[] { "Top 3 node có ảnh hưởng lớn nhất trong mạng: "}, ReportFileName);
            TextDB.WriteTextFile(new string[] { "Node name", "K-shell", "R-shell", "K-Coreness", "Hierarchical closeness", "Closeness", "Betweeness", "Page rank in", "Total degree", "Katz", "Page rank out", "Epidemic SIR", "Rumor spreading", "Taxi passenger ranking" }, ReportFileName);
            double mo = 0;
            Dictionary<Node, int> pCluster = BasicNet.OptimizerModularityExactly.ClusterGraph(net, true, ref mo);// using undirected weight modularity function
            int i = 0;

            string txtOut = "Top 3 driver node found: ";
            foreach (KeyValuePair<Node, int> e in sortedCorenessCentrality)
            {
                txtOut += e.Key.name + ", ";
                TextDB.WriteTextFile(new string[] {
                    e.Key.name,
                    kShell[net[e.Key.name]].ToString(),
                    rShell[net[e.Key.name]].ToString(),
                    corenessCentrality[net[e.Key.name]].ToString(),
                    hcCentrality[e.Key.name].ToString(),
                    closenessCentrality[e.Key.name].ToString(),
                    betweenessCentrality[net[e.Key.name]].ToString(),
                    pageRankInCentrality[net[e.Key.name]].ToString(),
                    net[e.Key.name].TotalDegree.ToString(),
                    katzCentrality[net[e.Key.name]].ToString(),
                    pageRankOutCentrality[net[e.Key.name]].ToString(),
                    epidemicSIR[net[e.Key.name]].ToString(),
                    rurmorSpreading[net[e.Key.name]].ToString(),
                    TaxiPassenger[net[e.Key.name]].ToString(),
                },

                    ReportFileName);
            }

            User.One.MessageToUser(txtOut);
            User.One.MessageToUser("The result is saved in file " + ReportFileName);
        }

#else

        private static void WriteCentralityToFile(BooleanNetwork net, string ReportFileName)
        {
            Dictionary<string, double> hcCentrality = net.HierarchicalClosenessCentrality();
            Dictionary<string, double> closenessCentrality = net.ClosenessCentrality();
            Dictionary<Node, int> kShell = net.K_ShellCentrality();
            Dictionary<Node, int> rShell = net.R_ShellCentrality();
            Dictionary<Node, float> betweenessCentrality = net.BetweenessCentrality();
            Dictionary<Node, float> pageRankInCentrality = net.PageRankCentralityInLink();
            Dictionary<Node, float> pageRankOutCentrality = net.PageRankCentralityOutLink();
            Dictionary<Node, int> corenessCentrality = net.K_CorenessCentrality();
            Dictionary<Node, float> epidemicSIR = net.EpidemicBySIR_Centrality(-1, 0.8f);
            Dictionary<Node, float> rurmorSpreading = net.RumorSpeader_Centrality(1, -1, 0.8f);
            Dictionary<Node, float> TaxiPassenger = net.TaxiPassengerRank();
            TextDB.WriteTextFile(new string[] { "Node", "Core level", "R-shell", "KCO", "HC", "Closeness", "BET", "PR", "Total degree", "Page rank out", "Epidemic SIR", "Rumor spreading", "TAX", "ModuleID" }, ReportFileName);
            double mo = 0;
            Dictionary<Node, int> pCluster = BasicNet.OptimizerModularityExactly.ClusterGraph(net, true, ref mo);// using undirected weight modularity function
#if TRIAL
            if(DateTime.Today< new DateTime(2019,5,6))
            { 
#endif
            foreach (KeyValuePair<Node, int> e in pCluster)
            {
                TextDB.WriteTextFile(new string[] {
                    e.Key.name,
                    kShell[net[e.Key.name]].ToString(), 
                    rShell[net[e.Key.name]].ToString(), 
                    corenessCentrality[net[e.Key.name]].ToString(),
                    hcCentrality[e.Key.name].ToString(), 
                    closenessCentrality[e.Key.name].ToString(),
                    betweenessCentrality[net[e.Key.name]].ToString(),
                    pageRankInCentrality[net[e.Key.name]].ToString(),
                    net[e.Key.name].TotalDegree.ToString(),
                    
                    pageRankOutCentrality[net[e.Key.name]].ToString(),
                    epidemicSIR[net[e.Key.name]].ToString(),
                    rurmorSpreading[net[e.Key.name]].ToString(),
                    TaxiPassenger[net[e.Key.name]].ToString(),
                    pCluster[e.Key].ToString()
                },

                    ReportFileName);
            }
#if TRIAL
            }else
                User.One.MessageToUser("The software is overdate! Please contact to the author to fix the error!");
#endif

            User.One.MessageToUser("The result is saved in file " + ReportFileName);
        }
#endif
        private static void WriteFociToFile(BooleanNetwork net, string ReportFileName)
        {     
              List<Node[]> foci = net.triadicClosure();
              List<Node[]> foci1 = net.Foci();
              int count = 1;

              TextDB.WriteTextFile(new string[] {"Number of foci: " + foci1.Count().ToString() }, ReportFileName);
              foreach (var item in foci1)
	          {
                    TextDB.WriteTextFile(new string[] {"Nodes of foci fc" + count.ToString() }, ReportFileName);
                    for (int i = 0; i < item.Count(); i++)
			        {
                         TextDB.WriteTextFile(new string[] {item[i].name.ToString() }, ReportFileName);   
			        }   
                    count ++;
	          }

              TextDB.WriteTextFile(new string[] {"" }, ReportFileName);
              TextDB.WriteTextFile(new string[] {"Number of triadic closure: " + foci.Count().ToString() }, ReportFileName);
              int countTriangle = 1;

              foreach (var item in foci)
	          {
                    TextDB.WriteTextFile(new string[] {"Triadic closure " + countTriangle.ToString()}, ReportFileName);
                    for (int i = 0; i < item.Count(); i++)
			        {
                         TextDB.WriteTextFile(new string[] {item[i].name.ToString() }, ReportFileName);   
			        }   
                    countTriangle ++;
	          }  
              
        }
        private static void WriteToFileGateKepper(BooleanNetwork net, string ReportFileName)
        {
           //TextDB.WriteTextFile(new string[] {"Result : "}, ReportFileName);
            int gate=0;
            List<Node> listtest = net.Nodes.ToList();
            for(int i = 0; i< listtest.Count; i++)
            {
                for(int j = 0; j < listtest.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    else
                    {
                        List<Node> result = net.Nodes.ToList();
                        net.checkGateKepperzz(listtest[i], listtest[j], new List<Node>(), result);
                        if(result.Count == listtest.Count)
                        {
                            result.Clear();
                        }
                        foreach(Node item in result)
                        {
                            TextDB.WriteTextFile(new string[] {item.name.ToString() + "\t is gatekeeper of \t" + listtest[i].name.ToString() + "\t" + listtest[j].name.ToString()}, ReportFileName);
                            gate ++;
                        }
                    }
                    
                }
            }
            if (gate == 0)
            {
                TextDB.WriteTextFile(new string[] {"The Graph hasn't gatekeeper : "}, ReportFileName);
                return;
            }
            string fileName= "OutPut"+@"\"+ReportFileName;
            string[] arr = File.ReadAllLines(fileName);
            List<string> list1 = new List<string>();
            foreach (string s in arr)
            {
                list1.Add(s);
            }
            List<string> listSort = list1.OrderBy(x => x).ToList();
            
            string[] saveNameS = new string[listSort.Count];
            string[] temp = listSort.ToArray();
            for (int i = 0; i < saveNameS.Length; i++)
            {
                saveNameS[i] = temp[i][0].ToString();
            }
            string st = saveNameS[0];
            int ct = 0;
            List<string> name = new List<string>();
            List<int> number = new List<int>();
            for(int i = 0; i < saveNameS.Length; i++)
            {
                if(saveNameS[i]==st)
                {
                    ct++;
                }
                else
                {
                    name.Add(st);
                    number.Add(ct);
                    ct = 1;
                    st = saveNameS[i];
                }
            }
            name.Add(st);
            number.Add(ct);
            Dictionary<string,int> dictionary = new Dictionary<string, int>();
            for(int i=0;i< name.Count; i++)
            {
                dictionary.Add(name[i],number[i]);
            }
            var items = from pair in dictionary
                    orderby pair.Value descending
                    select pair;
            File.WriteAllText(fileName, String.Empty);
            TextDB.WriteTextFile(new string[] {"Node\tCount(Vertex Pair)"}, ReportFileName); 
            foreach (KeyValuePair<string, int> pair in items)
            {
                TextDB.WriteTextFile(new string[] {pair.Key+"\t"+pair.Value.ToString()}, ReportFileName);
                
            }
	         TextDB.WriteTextFile(new string[] {"Detail :  "}, ReportFileName);
             foreach (var item in listSort)
             {
                TextDB.WriteTextFile(new string[] {item}, ReportFileName);
             }
            

        }

        private static void WriteGraphFociToFile(BooleanNetwork net, string ReportFileName)
        {
            var end = net.socialNetwork();
            TextDB.WriteTextFile(new string[] { "start", "end", "weight", "direction" }, ReportFileName);
            foreach (var item in end)
            {
                TextDB.WriteTextFile(new string[] {
                     item[0].name.ToString(),
                     item[1].name.ToString(),
                     "1",
                     "0",}, ReportFileName);
            }
            User.One.MessageToUser("The result is saved in file " + ReportFileName);
            User.One.MessageToUser("Nodes of social network " + net.nodesSocialnetwork().ToString());
            User.One.MessageToUser("Edges of social network " + end.Count().ToString());
        }

        private static void WriteBalanceToFile(BooleanNetwork net)
        {
            var arcOld = net.Arcs.Count();
            var node = net.Nodes.Count();
            var arcNew = (node*(node - 1))/2;
            var listNotBalance = net.triangleNotBalance();

            if (arcOld == arcNew)
            {
                if (net.Balance())
                {
                   User.One.MessageToUser("Balanced Network ");
                }
                else
                {
                   User.One.MessageToUser("Unbalanced Network ");
                   User.One.MessageToUser("List Triangle Not Unbalanced ");            
                    foreach (var item in listNotBalance)
                    {
                        User.One.MessageToUser(item[0].name.ToString() + "," + item[1].name.ToString() + "," + item[2].name.ToString());
                    }
                }
            }
            else
            {
                User.One.MessageToUser("This network is Social Network ");
            }
        }

         private static void WriteConvertToFile(BooleanNetwork net, string ReportFileName)
        {
            var list = net.Positive();
            var list1 = net.Negative();
            TextDB.WriteTextFile(new string[] { "start", "end", "weight", "direction" },
                ReportFileName);
            foreach (var item in list)
            {
                TextDB.WriteTextFile(new string[] {                 
                    item[0].name.ToString(),
                    item[1].name.ToString(),
                    "1",
                    "0",
                },
                    ReportFileName);
            }

            foreach (var item in list1)
            {
                TextDB.WriteTextFile(new string[] {                 
                    item[0].name.ToString(),
                    item[1].name.ToString(),
                    "-1",
                    "0",
                },
                    ReportFileName);
            }
            User.One.MessageToUser("The result is saved in file " + ReportFileName);          
        }

        public static void WriteSCCToFile(BooleanNetwork net, string ReportFileName)
        {

            IList<Node> scc = net.FindGiantScc();
            HashSet<Node> nodeout = net.FindAllOutput(scc);
            HashSet<Node> nodein = net.FindAllInput(scc);
            HashSet<Node> tendrils = net.FindTendrils(scc, nodein, nodeout);

            HashSet<Node> tubes = net.FindTubes(scc, tendrils, nodein, nodeout);
            IList<Node> Disconnected = net.FindDisconnected();
            HashSet<Node> tendrilnew = net.TendrilsNew(tendrils, tubes);


            int numberOfNode = net.Nodes.Count();
            int numberOfEdge = net.Arcs.Count();

            TextDB.WriteTextFile(new string[] { "***Report***" }, ReportFileName);
            TextDB.WriteTextFile(new string[] { "--Nodes:", numberOfNode.ToString() }, ReportFileName);
            TextDB.WriteTextFile(new string[] { "--Edges:", numberOfEdge.ToString() }, ReportFileName);
            TextDB.WriteTextFile(new string[] { "--Nodes in largest Strong Connected Component:", scc.Count().ToString() }, ReportFileName);
            TextDB.WriteTextFile(new string[] { "--Nodes in Output:", nodeout.Count().ToString() }, ReportFileName);
            TextDB.WriteTextFile(new string[] { "--Nodes in Input:", nodein.Count().ToString() }, ReportFileName);
            TextDB.WriteTextFile(new string[] { "--Nodes in Disconnected :", Disconnected.Count().ToString() }, ReportFileName);
            TextDB.WriteTextFile(new string[] { "--Nodes in Tendrils :", tendrilnew.Count().ToString() }, ReportFileName);
            TextDB.WriteTextFile(new string[] { "--Nodes in Tubes :", tubes.Count().ToString() }, ReportFileName);

            TextDB.WriteTextFile(new string[] { "**Giant Strong Connected Component(Scc)**" }, ReportFileName);
            foreach (var i in scc)
                TextDB.WriteTextFile(new string[] {
                i.name.ToString(),
                },
                    ReportFileName);


            TextDB.WriteTextFile(new string[] { "**Output**" }, ReportFileName);
            foreach (var i in nodeout)
            {
                TextDB.WriteTextFile(new string[] {
                i.name.ToString()
                },
                   ReportFileName);
            }
            TextDB.WriteTextFile(new string[] { "**Input**" }, ReportFileName);
            foreach (var i in nodein)
            {
                TextDB.WriteTextFile(new string[] {
                i.name.ToString()
                },
                   ReportFileName);
            }
            TextDB.WriteTextFile(new string[] { "**Tendrils**" }, ReportFileName);
            foreach (var i in tendrilnew)
            {
                TextDB.WriteTextFile(new string[] {
                i.name.ToString()
                },
                   ReportFileName);
            }
            TextDB.WriteTextFile(new string[] { "**Disconnected**" }, ReportFileName);
            foreach (var i in Disconnected)
                TextDB.WriteTextFile(new string[] {
                i.name.ToString(),
                },
                   ReportFileName);
            TextDB.WriteTextFile(new string[] { "**Tubes**" }, ReportFileName);
            foreach (var i in tubes)
            {
                TextDB.WriteTextFile(new string[] {
                i.name.ToString()
                },
                   ReportFileName);

            }
            User.One.MessageToUser("The result is saved in file " + ReportFileName);
        } 

        /// <summary>
        /// For directed network
        /// </summary>
        /// <param name="nNet"></param>
        /// <param name="nNodeFrom"></param>
        /// <param name="nNodeTo"></param>
        /// <param name="nMinLink"></param>
        /// <param name="nMaxLink"></param>
        public static void RBN_AnalyzeCentralLayersFixedDensity(decimal Startdensity, decimal Enddensity, int from, int to, int NetNum)
        {
            ComplexNetGenerator GeneratingNet = new BasicNet.ComplexNetGenerator();

            //ExcelDB excel = CreateAnalysisReport();


            //int Row = ExcelDB.DataRowStart;
            try
            {
                string ReportFileName = "CentralityByDensity." + Startdensity.ToString() + "." + Enddensity.ToString() + "." + from.ToString() + "." + to.ToString() + ".txt";
                TextDB.WriteTextFile(new string[] {
                        "NetID", "Node", "Link", "Network modularity", "Network robustness",
                        "Node name", "Node robustness","Tran centrality","Connectivity", "Closeness centrality","Total degree", "Modularity", "Betweeness centrality", "Page rank","Out degree"
                    }, ReportFileName);

                User.One.MessageToUser(string.Format("Analyze centrality with Density = [{0}, {1}], Node range = [{2}..{3}], The number of networks = {4}. Start time of {5}", Startdensity, Enddensity, from, to, NetNum, DateTime.Now.ToString()));

                //foreach node
                decimal density = 0.0M;
                int net = 1;
                BooleanNetwork template = new BooleanNetwork();
                for (density = Startdensity; net <= NetNum; density = (density > Enddensity ? Startdensity : density + 0.05M))
                {
                    for (int node = from;
                        net <= NetNum;
                        node = (node > to ? from : node + 1), net++)
                    {

                        decimal link = Math.Round((node * node - node) * density / 2, 0);
                        BooleanNetwork Net = null;
                        try
                        {
                            Net = GeneratingNet.generateDirectedNetworkByPreferentialAttachment(template, node, (int)link) as BooleanNetwork;
                        }
                        catch (Exception)
                        {
                            net--;
                            continue;
                        }

                        Dictionary<Node, int> Cluster = null, temp = null;
                        double modularity = Net.modularity(ref Cluster);
                        double robustness = Net.NetworkRobustness(new Perturbation()); //Net.NetworkMutantRobustnessParalell();
                        int nNode = Net.Nodes.Count();
                        int nLink = Net.Arcs.Count();
                        //End calculating

                        Dictionary<string, Triple<double>> tranClosenessCentrality = Net.HierarchicalClosenessCentralityAnalysis();
                        Dictionary<Node, float> betweenessCentrality = Net.BetweenessCentrality();

                        Dictionary<Node, float> pageRankCentrality = Net.PageRankCentralityInLink();


                        //Begin calculating modularity of nodes
                        BasicNetwork clusterNet = Net.CreateClusterNework(Cluster) as BasicNetwork;
                        //clusterNet.CreateClusterNework(Net, Cluster);


                        Dictionary<int, double> modularitySubmodule = new Dictionary<int, double>(); //Module ID + its modularity

                        foreach (BooleanNode n in clusterNet.Nodes)
                            modularitySubmodule.Add(n.SubnetID, n.SubNetwork.modularity(ref temp));

                        Dictionary<Node, double> nodeModularity = new Dictionary<Node, double>(); //Node name + its modularity
                        foreach (KeyValuePair<Node, int> e in Cluster)
                        {
                            nodeModularity.Add(e.Key, modularitySubmodule[e.Value]);
                        }
                        //End calculating modularity of nodes
                        string buffer = "";
                        foreach (BooleanNode e in betweenessCentrality.Keys)
                        {
                            buffer = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}\t{14}", Net.ObjectID, nNode, nLink, modularity, robustness,
                                e.name,     //node name
                                Net.NodeRobustness(e, new Perturbation()),//Robustness
                                tranClosenessCentrality[e.name].A,//Tran centrality
                                tranClosenessCentrality[e.name].B,//Connectivity centrality
                                tranClosenessCentrality[e.name].C,//Closeness centrality
                                e.TotalDegree,          //Total degree
                                (from p in nodeModularity where p.Key.name == e.name select p).ElementAt(0).Value, //Modularity
                                betweenessCentrality[e],//Betweeness
                                pageRankCentrality[e],  // page rank
                                e.OutDegree
                                );
                            TextDB.WriteTextFile(buffer, ReportFileName);
                        }
                        User.One.ShowWaitIndicator(net, NetNum);


                    }
                }
                User.One.MessageToUser("Create a file " + ReportFileName + " on " + DateTime.Now.ToString());

            }
            catch (Exception ex)
            {

                User.One.SendErrorToUser(ex);
            }
        }
        /// <summary>
        /// For directed network
        /// </summary>
        /// <param name="nNet"></param>
        /// <param name="nNodeFrom"></param>
        /// <param name="nNodeTo"></param>
        /// <param name="nMinLink"></param>
        /// <param name="nMaxLink"></param>
        public static void RBN_AnalyzeCentralLayers(int nNet, int nNodeFrom, int nNodeTo, int nMinLink, int nMaxLink, string ReportFileName)
        {
            if (nNodeFrom > nNodeTo)
                throw new Exception("The node range is invalid!");
            if (nMinLink > nMaxLink)
                throw new Exception("The link range is invalid!");

            ComplexNetGenerator GeneratingNet = new BasicNet.ComplexNetGenerator();
            User.One.MessageToUser("Start generating network data on " + DateTime.Now.ToString());
            try
            {
                int i = 0;
                BooleanNetwork Net = null;
                TextDB.WriteTextFile(new string[] {
                        "NetID", "Node", "Link", "Network modularity", "Network robustness",
                        "Node name", "Node robustness","Hierarchical closeness","Reaching", "Closeness","Total degree", "Modularity", "Betweeness", "PageRank","Out degree"
                    }, ReportFileName);
                BooleanNetwork template = new BooleanNetwork();
                #region code
                for (int j = nNodeFrom; j <= nNodeTo; j = j >= nNodeTo ? nNodeFrom : (j + 1))
                {
                    if (i > nNet) break;


                    for (int k = nMinLink; k <= nMaxLink; k++)
                    {
                        if (++i > nNet) break;
                        try
                        {
                            //Net = GeneratingNet.generateDirectedNetworkByPreferentialAttachment(template, j, k) as BooleanNetwork;
                            Net = GeneratingNet.generateScaleFreeDirectedNetwork(j, k);
                            //Net = GeneratingNet.AdjustLink(sf, 1, 1);
                        }
                        catch (Exception)
                        {
                            i--;
                            continue;
                        }

                        #endregion
                        //Begin calculating network properties
                        Dictionary<Node, int> Cluster = null, temp = null;
                        double modularity = Net.modularity(ref Cluster);
                        double robustness = Net.NetworkRobustness(new Perturbation());//Net.NetworkMutantRobustnessParalell();
                        int nNode = Net.Nodes.Count();
                        int nLink = Net.Arcs.Count();
                        //End calculating

                        Dictionary<string, Triple<double>> tranClosenessCentrality = Net.HierarchicalClosenessCentralityAnalysis();
                        Dictionary<string, double> closenessCentrality = Net.ClosenessCentrality();
                        Dictionary<Node, float> betweenessCentrality = Net.BetweenessCentrality();
                        //Dictionary<Node, double> eigenCentrality = Net.EigenCentrality(true);
                        Dictionary<Node, float> pageRankInCentrality = Net.PageRankCentralityInLink();



                        //Begin calculating modularity of nodes
                        BasicNetwork clusterNet = Net.CreateClusterNework(Cluster) as BasicNetwork;
                        //clusterNet.CreateClusterNework(Net, Cluster);


                        Dictionary<int, double> modularitySubmodule = new Dictionary<int, double>(); //Module ID + its modularity

                        foreach (BooleanNode n in clusterNet.Nodes)
                            modularitySubmodule.Add(n.SubnetID, n.SubNetwork.modularity(ref temp));

                        Dictionary<Node, double> nodeModularity = new Dictionary<Node, double>(); //Node name + its modularity
                        foreach (KeyValuePair<Node, int> e in Cluster)
                        {
                            nodeModularity.Add(e.Key, modularitySubmodule[e.Value]);
                        }
                        //End calculating modularity of nodes
                        string buffer = "";
                        foreach (BooleanNode e in betweenessCentrality.Keys)
                        {
                            buffer = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}\t{14}", Net.ObjectID, nNode, nLink, modularity, robustness,
                                e.name,     //node name
                                Net.NodeRobustness(e, new Perturbation()),//Robustness
                                tranClosenessCentrality[e.name].A,//Tran centrality
                                tranClosenessCentrality[e.name].B,//Connectivity centrality
                                                                  //tranClosenessCentrality[e.name].C,//Closeness centrality
                                closenessCentrality[e.name],
                                e.TotalDegree,          //Total degree
                                (from p in nodeModularity where p.Key.name == e.name select p).ElementAt(0).Value, //Modularity
                                betweenessCentrality[e],//Betweeness
                                pageRankInCentrality[e],  // page rank in
                                e.OutDegree
                                );
                            TextDB.WriteTextFile(buffer, ReportFileName);
                        }
                        User.One.ShowWaitIndicator(i, nNet);

                    }
                    if (i == 0)
                    {
                        User.One.MessageToUser("Can not create any network");
                        break;
                    }
                }
                User.One.MessageToUser("Finish generating network data on " + DateTime.Now.ToString());

            }
            catch (Exception ex)
            {
                User.One.SendErrorToUser(ex);
            }
        }


        public static void RBN_AnalyzeCentralLayersWithFixedReaching(int nNet, int nNodeFrom, int nNodeTo, int nMinLink, int nMaxLink, int[] reaching, string ReportFileName)
        {
            if (nNodeFrom > nNodeTo)
                throw new Exception("The node range is invalid!");
            if (nMinLink > nMaxLink)
                throw new Exception("The link range is invalid!");
            Dictionary<double, int> reachingNetworkCount = new Dictionary<double, int>();
            foreach (int r in reaching)
                reachingNetworkCount.Add(r, 0);

            ComplexNetGenerator GeneratingNet = new BasicNet.ComplexNetGenerator();
            User.One.MessageToUser("Start generating network data on " + DateTime.Now.ToString());
            try
            {
                int i = 0;
                BooleanNetwork Net = null;
                TextDB.WriteTextFile(new string[] {
                        "NetID", "Node", "Link", "Network modularity", "Network robustness",
                        "Node name", "Node robustness","Hierarchical closeness","Reaching", "Closeness","Total degree", "Modularity", "Betweeness", "PageRank","Out degree"
                    }, ReportFileName);

                #region code
                BooleanNetwork template = new BooleanNetwork();
                for (int j = nNodeFrom; j <= nNodeTo; j = j >= nNodeTo ? nNodeFrom : (j + 1))
                {
                    if (i > nNet) break;


                    for (int k = nMinLink; k <= nMaxLink; k++)
                    {
                        if (++i > nNet) break;
                        try
                        {
                            Net = GeneratingNet.generateDirectedNetworkByPreferentialAttachment(template, j, k) as BooleanNetwork;
                            //Net = GeneratingNet.AdjustLink(sf, 1, 1);
                        }
                        catch (Exception)
                        {
                            i--;
                            continue;
                        }

                        #endregion
                        var notFullReaching = from p in reachingNetworkCount where p.Value < 200 select p.Key;//Select reaching not full (<200 nets)
                        if (notFullReaching.Count() == 0)
                        {
                            i = nNet + 1;
                            break;
                        }

                        Dictionary<string, Triple<double>> tranClosenessCentrality = Net.HierarchicalClosenessCentralityAnalysis();
                        //Dictionary<int, int> reachingTemp = new Dictionary<int, int>();
                        var reachingNodes = from p in tranClosenessCentrality where notFullReaching.Contains(p.Value.B) select p;


                        //select reaching at least 10 nodes
                        var acceptedReaching = from e in reachingNodes group e by e.Value.B into g where g.Count() >= 10 select new { reaching = g, count = g.Count() };




                        var acceptedReachingNodes = from p in reachingNodes join r in acceptedReaching on p.Value.B equals r.reaching.Key select p;
                        if (acceptedReachingNodes.Count() == 0)
                            continue;

                        foreach (var r in acceptedReaching)
                            reachingNetworkCount[r.reaching.Key] += 1;

                        //Begin calculating network properties
                        Dictionary<Node, int> Cluster = null, temp = null;
                        double modularity = Net.modularity(ref Cluster);
                        double robustness = 0;// Net.NetworkRobustness(new Perturbation());//Net.NetworkMutantRobustnessParalell();
                        int nNode = Net.Nodes.Count();
                        int nLink = Net.Arcs.Count();
                        //End calculating



                        Dictionary<Node, float> betweenessCentrality = Net.BetweenessCentrality();
                        //Dictionary<Node, double> eigenCentrality = Net.EigenCentrality(true);
                        Dictionary<Node, float> pageRankInCentrality = Net.PageRankCentralityInLink();



                        //Begin calculating modularity of nodes
                        BasicNetwork clusterNet = Net.CreateClusterNework(Cluster) as BasicNetwork;
                        //clusterNet.CreateClusterNework(Net, Cluster);


                        Dictionary<int, double> modularitySubmodule = new Dictionary<int, double>(); //Module ID + its modularity

                        foreach (BooleanNode n in clusterNet.Nodes)
                            modularitySubmodule.Add(n.SubnetID, n.SubNetwork.modularity(ref temp));

                        Dictionary<Node, double> nodeModularity = new Dictionary<Node, double>(); //Node name + its modularity
                        foreach (KeyValuePair<Node, int> e in Cluster)
                        {
                            nodeModularity.Add(e.Key, modularitySubmodule[e.Value]);
                        }
                        //End calculating modularity of nodes
                        string buffer = "";

                        foreach (var e in acceptedReachingNodes)
                        {
                            BooleanNode n = Net.GetNodeFromName(e.Key) as BooleanNode;
                            buffer = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}\t{14}", Net.ObjectID, nNode, nLink, modularity, robustness,
                                e.Key,     //node name
                                Net.NodeRobustness(n, new Perturbation()),//Robustness
                                e.Value.A,//Tran centrality
                                e.Value.B,//Connectivity centrality
                                e.Value.C,//Closeness centrality
                                n.TotalDegree,          //Total degree
                                (from p in nodeModularity where p.Key.name == e.Key select p).ElementAt(0).Value, //Modularity
                                betweenessCentrality[n],//Betweeness
                                pageRankInCentrality[n],  // page rank in
                                n.OutDegree
                                );
                            TextDB.WriteTextFile(buffer, ReportFileName);
                        }
                        User.One.ShowWaitIndicator(i, nNet);

                    }
                    if (i == 0)
                    {
                        User.One.MessageToUser("Can not create any network");
                        break;
                    }
                }
                User.One.MessageToUser("Finish generating network data on " + DateTime.Now.ToString());

            }
            catch (Exception ex)
            {
                User.One.SendErrorToUser(ex);
            }
        }
        public static void CompareModularityFunctions(int nNet, int nNodeFrom, int nNodeTo, int nMinLink, int nMaxLink, string ReportFileName)
        {

            if (nNodeFrom > nNodeTo)
                throw new Exception("The node range is invalid!");
            if (nMinLink > nMaxLink)
                throw new Exception("The link range is invalid!");

            ComplexNetGenerator GeneratingNet = new BasicNet.ComplexNetGenerator();
            User.One.MessageToUser("Start generating network data on " + DateTime.Now.ToString());
            try
            {
                int i = 0;
                //foreach node
                BooleanNetwork Net = null;
                TextDB.WriteTextFile(new string[] {"", "", "",
                    "",
                    "Undirected weighted","", "Using edge without weight", "",
                    "Undirected unweighted", "", "Using edge with the number of arcs to be the weight", "",
                    "Directed unweighted", "", "Using arcs", ""
                    //,"Controllability" 
                }, ReportFileName);

                TextDB.WriteTextFile(new string[] {"NetID", "Node #", "Link #", 
                    //"Centrality", 
                    //"Module amount", 
                    "Robustness",
                    "Module number1","Modularity1", "In-module robustness1", "Out-module robustness1",
                    "Module number2", "Modularity2", "In-module robustness2", "Out-module robustness2",
                    "Module number3", "Modularity3", "In-module robustness3", "Out-module robustness3"
                    //,"Controllability" 
                }, ReportFileName);
                BooleanNetwork template = new BooleanNetwork();
                for (int j = nNodeFrom; j <= nNodeTo; j = j >= nNodeTo ? nNodeFrom : (j + 1))
                {
                    if (i > nNet) break;


                    for (int k = nMinLink; k <= nMaxLink; k++)
                    {
                        if (++i > nNet) break;

                        try
                        {
                            Net = (BooleanNetwork)GeneratingNet.generateDirectedNetworkByPreferentialAttachment(template, j, k);
                        }
                        catch (Exception)
                        {
                            i--;
                            continue;
                        }
                        //

                        //float centrality = Net.DegreeCentrality;

                        double multationRobustness = Net.NetworkMutantRobustnessParalell();//Net.NetworkRobustness(new Perturbation(Perturbation.Kind.Mutation));

                        Dictionary<Node, int> pCluster1 = null;

                        double modularity1 = Net.modularity(ref pCluster1, true);
                        int nCluster1 = Clustering.ClusterCount(pCluster1);
                        double inModuleRo1 = 0, outModuleRo1 = 0;
                        //Net.InOutModuleRobustness(Cluster, new Perturbation(Perturbation.Kind.Mutation), ref inModuleRo, ref outModuleRo);
                        Net.InOutModuleRobustnessParalell(pCluster1, new Perturbation(Perturbation.Kind.Mutation), ref inModuleRo1, ref outModuleRo1);

                        Dictionary<Node, int> pCluster2 = null;
                        double inModuleRo2 = 0, outModuleRo2 = 0;
                        double modularity2 = Net.modularity(ref pCluster2, false);
                        int nCluster2 = Clustering.ClusterCount(pCluster2);
                        Net.InOutModuleRobustnessParalell(pCluster2, new Perturbation(Perturbation.Kind.Mutation), ref inModuleRo2, ref outModuleRo2);

                        Dictionary<Node, int> pCluster3 = null;
                        double inModuleRo3 = 0, outModuleRo3 = 0;
                        double modularity3 = Net.modularityWeightedDirected(ref pCluster3);
                        int nCluster3 = Clustering.ClusterCount(pCluster3);
                        Net.InOutModuleRobustnessParalell(pCluster3, new Perturbation(Perturbation.Kind.Mutation), ref inModuleRo3, ref outModuleRo3);

                        int nNode = Net.Nodes.Count();
                        int nLink = Net.Arcs.Count();





                        string[] f = new string[] {Net.ObjectID.ToString(), nNode.ToString(), nLink.ToString(), 
                            //centrality.ToString(), nCluster.Count().ToString(), 
                            multationRobustness.ToString(),
                            nCluster1.ToString(), modularity1.ToString(), inModuleRo1.ToString(),outModuleRo1.ToString(),
                            nCluster2.ToString(), modularity2.ToString(), inModuleRo2.ToString(),outModuleRo2.ToString(),
                            nCluster3.ToString(), modularity3.ToString(), inModuleRo3.ToString(),outModuleRo3.ToString()
                            //,Net.driverNodes.Count().ToString()
                        };

                        TextDB.WriteTextFile(f, ReportFileName);

                        User.One.ShowWaitIndicator(i, nNet);

                    }

                    if (i == 0)
                    {
                        User.One.MessageToUser("Can not create any network");
                        break;
                    }
                }
                User.One.MessageToUser("Finish generating network data on " + DateTime.Now.ToString());

            }
            catch (Exception ex)
            {
                User.One.SendErrorToUser(ex);
            }
        }
        #endregion

        #region Competitive Dynamics on Complex Networks
        public static void CalculateCompetitiveNetwork(string networkName, string positiveString, string negativeString, string ReportFileName)
        {

            string[] positiveCompetitors = positiveString.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray(); ;
            string[] negativeCompetitors = negativeString.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray(); ;

            BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(networkName);

            /*//Original version
            List<BooleanNode> competitors = new List<BooleanNode>();
            BooleanNode temp = null;
            foreach (var c in positiveCompetitors)
            {
                temp = net[c] as BooleanNode;
                temp.ResetState(1);
                competitors.Add(temp);
            }

            foreach (var c in negativeCompetitors)
            {
                temp = net[c] as BooleanNode;
                temp.ResetState(-1);
                competitors.Add(temp);
            }
            Dictionary<BooleanNode, List<BooleanNode>> supporters = net.Competition_Computing(competitors);
            */
            // Begin: Extended version for two sets of competitors
            BooleanNetwork newNet = net.CreateObject() as BooleanNetwork;
            List<BooleanNode> competitors = new List<BooleanNode>();
            //BooleanNode temp = null;

            BooleanNode posNode = null, negNode = null;
            if (positiveCompetitors.Count() > 1 || negativeCompetitors.Count() > 1)
            {
                Node temp = null;
                net = net.CreateNetworkByMergedNode(net.SelectNode(positiveCompetitors), ref temp) as BooleanNetwork;
                posNode = temp as BooleanNode;
                Netutil.DumpNet(net);

                net = net.CreateNetworkByMergedNode(net.SelectNode(negativeCompetitors), ref temp) as BooleanNetwork;
                negNode = temp as BooleanNode;

                Netutil.DumpNet(net);
            }
            else
            {
                posNode = net[positiveCompetitors[0]] as BooleanNode;


                negNode = net[negativeCompetitors[0]] as BooleanNode;
            }




            (net[posNode.name] as BooleanNode).ResetState(1);
            (net[negNode.name] as BooleanNode).ResetState(-1);


            //Dictionary<BooleanNode, List<BooleanNode>> supporters = net.Competition_Computing(new BooleanNode[] { net[negNode.name] as BooleanNode,
            //    net[posNode.name] as BooleanNode });

            Dictionary<Node, float> supporters = net.InsideCompetition2NodeSets(new string[] { posNode.name }, new string[] { negNode.name });

            var positiveSupporter = supporters.Where(s => s.Value > 0).ToList();
            positiveSupporter.Sort((a, b) => b.Value.CompareTo(a.Value));

            var negativeSupporter = supporters.Where(s => s.Value < 0).ToList();
            negativeSupporter.Sort((a, b) => a.Value.CompareTo(b.Value));

            var neutralSupporter = supporters.Where(s => Math.Abs(s.Value) < Mathutil.NumericMath.zeroEpsionf).ToList();

            Dictionary<string, List<KeyValuePair<Node, float>>> sortedSupporters = new Dictionary<string, List<KeyValuePair<Node, float>>>();
            sortedSupporters.Add(posNode.name, positiveSupporter);
            sortedSupporters.Add(negNode.name, negativeSupporter);
            sortedSupporters.Add("Neutral", neutralSupporter);

            foreach (var c in sortedSupporters.Keys)
            {
                if (sortedSupporters[c].Count() > 0)
                {
                    TextDB.WriteTextFile(string.Format("{0} supporters of \"{1}\" as follows:\n", sortedSupporters[c].Count(), c), ReportFileName);

                    foreach (var n in sortedSupporters[c])
                        TextDB.WriteTextFile(string.Format("{0,5}\t{1}\n", n.Key.name, n.Value), ReportFileName);
                }
                else
                {
                    TextDB.WriteTextFile(string.Format("None supporters of \"{0}\"\n", c), ReportFileName);
                }
            }
        }
        public static void CalculateLoyalMatrixPoint(string networkName, string ReportFileName)
        {
            BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(networkName);

            foreach (Node leader in net.Nodes.ToList())
            {

                //Dictionary<BooleanNode, float> loyalNode = net.Competition_AdhereToLeader(leader as BooleanNode);
                Dictionary<Node, float> loyalNode = net.OutsideCompetition2Nodes(leader);

                TextDB.WriteTextFile(string.Format("{0}'s supporter\tLoyal point", leader.name.ToUpper()), ReportFileName);

                var sortList = from p in loyalNode orderby p.Value descending select p;

                var sumLoyalPoint = 0.0;

                foreach (var n in sortList)
                {
                    sumLoyalPoint += loyalNode[n.Key];
                    TextDB.WriteTextFile(string.Format("{0}\t{1}", n.Key.name, loyalNode[n.Key]), ReportFileName);
                }

                TextDB.WriteTextFile(string.Format("Sum loyal point of leader\t{0}\t{1}", leader.name.ToUpper(), sumLoyalPoint), ReportFileName);
            }
        }
        /// <summary>
        /// This function finds driver nodes of a network where each normal node is controlled directly by an outside opponent with a negative opinion, which is against the positive opinion of the driver nodes.
        /// </summary>
        /// <param name="networkName">The input network</param>
        /// <param name="ReportFileName">The output file</param>
        public static void CalculateTotalSupport(string networkName, string ReportFileName)
        {
            BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(networkName);

            Dictionary<Node, Dictionary<Node, float>> Totalsupport = net.Competition_Totalsupport();
            Dictionary<Node, float> IF = new Dictionary<Node, float>();
            foreach (var n in Totalsupport)
            {
                float total = 0.0f;
                foreach (var v in n.Value)
                {
                    total += v.Value;
                }
                IF.Add(n.Key, total);
            }
            //Sort nodes by total support by descending order
            IEnumerable<KeyValuePair<Node, float>> sortedByTotalsupport = IF.OrderBy(it => -it.Value);

            TextDB.WriteTextFile("Node\tTotal support\tIs driver\tDrivable count\tDrivable nodes (supporters)\tNeutral count\tNeutral\tNon-supported count\tNon-supporters", ReportFileName);
            int isDriver = 0;
            IEnumerable<string> oldControllableNodes = new List<string>(); //Total drivable nodes
            IEnumerable<string> newControllableNodes = new List<string>();
            IEnumerable<KeyValuePair<Node, float>> supporters = null;
            string supporterList = null;
            IEnumerable<KeyValuePair<Node, float>> non_supporter = null;
            string non_supporterList = null;
            IEnumerable<KeyValuePair<Node, float>> neutral = null;
            string neutralList = null;

            foreach (var n in sortedByTotalsupport)//from the highest to the lowest Totalsupport, if the number of drivable nodes is changed, the driver is marked
            {
                supporters = Totalsupport[n.Key].Where(t => t.Value > 0).OrderBy(t=> -t.Value);
                supporterList = string.Join("; ", supporters.Select(t => t.Key.name + " (" + Math.Round(t.Value, 2) + ")").ToArray());

                non_supporter = Totalsupport[n.Key].Where(t => t.Value < 0).OrderBy(t => t.Value);
                non_supporterList = string.Join("; ", non_supporter.Select(t => t.Key.name + " (" + Math.Round(t.Value, 2) + ")").ToArray());

                neutral= Totalsupport[n.Key].Where(t => t.Value == 0);
                neutralList = string.Join("; ", neutral.Select(t => t.Key.name + " (" + Math.Round(t.Value, 2) + ")").ToArray());

                newControllableNodes = newControllableNodes.Union(supporters.Select(t => t.Key.name));

                if (newControllableNodes.Count() > oldControllableNodes.Count()) //if increasing the number of drivable nodes => driver
                    isDriver = 1;
                else
                    isDriver = 0;

                TextDB.WriteTextFile(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}", n.Key.name, n.Value, isDriver, supporters.Count(), supporterList, neutral.Count(), neutralList, non_supporter.Count(), non_supporterList), ReportFileName);

                oldControllableNodes = oldControllableNodes.Union(newControllableNodes);
            }

        }

        public static void CalculateLoyalPointWithLeader(string networkName, string leaderName, string ReportFileName)
        {
            BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(networkName);

            Node leader = net[leaderName];

            //Dictionary<BooleanNode, float> loyalNode = net.Competition_AdhereToLeader(leader as BooleanNode);
            Dictionary<Node, float> loyalNode = net.OutsideCompetition2Nodes(leader);
            var sortList = from p in loyalNode orderby p.Value descending select p;
            TextDB.WriteTextFile(string.Format("{0}'s supporter\tLoyal point", leader.name.ToUpper()), ReportFileName);
            foreach (var n in sortList)
            {
                TextDB.WriteTextFile(string.Format("{0}\t{1}", n.Key.name, loyalNode[n.Key]), ReportFileName);
            }

        }
        public static void CalculateOptimumBranchings(string networkName, string ReportFileName)
        {
            BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(networkName);

            List<Interaction> interactions = net.FindOptimumBranchings();
            TextDB.WriteTextFile(string.Format("Start\tEnd"), ReportFileName);
            foreach (var n in interactions)
            {
                TextDB.WriteTextFile(string.Format("{0}\t{1}", n.startNode.name, n.endNode.name), ReportFileName);
            }

        }
        public static void CalculateCompetitiveRanking(string networkName, string ReportFileName)
        {

            BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(networkName);
            Dictionary<BooleanNode, Dictionary<BooleanNode, int>> competitiveRanking = net.Competition_Ranking();

            //bool showHeader = false;

            if (competitiveRanking.Count() == 0)
            {
                TextDB.WriteTextFile1Line(string.Format("No result!"), ReportFileName);
                return;
            }
            var sortName = from p in competitiveRanking.ElementAt(0).Value.Keys orderby p.name descending select p;

            TextDB.WriteTextFile1Line(string.Format("Row wins Col\t"), ReportFileName);
            foreach (var h in sortName)
                TextDB.WriteTextFile1Line(string.Format("{0}\t", h.name), ReportFileName);

            var compRankingKeys = from p in competitiveRanking.Keys orderby p.name descending select p;
            foreach (var r in compRankingKeys)
            {
                //var sortName = from p in competitiveRanking[r].Keys orderby p.name descending select p;
                //if (!showHeader)
                //{
                //    TextDB.WriteTextFile1Line(string.Format("Row wins Col\t"), ReportFileName);
                //    foreach(var h in sortName)
                //        TextDB.WriteTextFile1Line(string.Format("{0}\t", h.name), ReportFileName);

                //    showHeader = true;
                //}
                TextDB.WriteTextFile(string.Format(""), ReportFileName);
                TextDB.WriteTextFile1Line(string.Format("{0}\t", r.name), ReportFileName);
                foreach (var n in sortName)
                {

                    TextDB.WriteTextFile1Line(string.Format("{0}\t", competitiveRanking[r][n]), ReportFileName);

                }

            }
        }
        #endregion

    }
}
