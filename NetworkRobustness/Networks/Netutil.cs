using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Collections;
using System.Linq;
using Fuzzy;
using System.IO;
using NetSimulation.Lib;
using Mathutil;
// optional MathNet dependency removed to avoid requiring MathNet NuGet during build
// using MathNet.Numerics.Statistics;
using System.Threading.Tasks;
//using MathNet.Numerics;

namespace BasicNet
{
    public static class Netutil
    {
        /// <summary>
        /// Return the similarity between two states of a network
        /// </summary>
        /// <param name="states1">The first the network's state</param>
        /// <param name="states2">The second the network's state</param>
        /// <returns>The degree of defference
        /// 1: Same
        /// 0: Completely different</returns>
        public static float NetStateSimilarity(float[] states1, float[] states2)
        {
            
            Debug.Assert(states1.Length == states2.Length);
            
            int count = states1.Length;
            int nSimilarBit = 0;

            for (int i = 0; i < count; i++)
            {
                if (states1[i] == states2[i])
                    nSimilarBit++;
            }
            return (float)nSimilarBit / count;
        }
        /// <summary>
        /// The paper: Shyi Meng Chen, New methods for subjective mental workload assessment and fuzzy risk analysis
        /// </summary>
        /// <param name="states1"></param>
        /// <param name="states2"></param>
        /// <returns></returns>
        public static float NetStateFuzzySimilarity(float[] states1, float[] states2)
        {
            Debug.Assert(states1.Length == states2.Length);

            int count = states1.Length;
            
            float temp = 0;
            for (int i = 0; i < count; i++)
            {
                temp+=Math.Abs(states1[i] - states2[i]);
            }
            temp = temp / count;
            return 1-temp;
        }
        /// <summary>
        /// Reorder, basing on the feedbak loop, network state in the attractor from minimum element to others
        /// </summary>
        /// <param name="attractor">The attrator needs to reorder</param>
        /// <returns>The input reordered-attrator</returns>
        public static List<float[]> ReorderAttractor(List<float[]> attractor)
         {
             List<float[]> orderedAttr = new List<float[]>();

             int Idx = MinElementIndex<float>(attractor);
             for (int i = 0; i < attractor.Count; i++)
             {
                 orderedAttr.Add(attractor[Idx]);
                 Idx = (Idx + 1) % attractor.Count;
             }
             for (int i = 0; i < attractor.Count; i++)
                 attractor[i] = orderedAttr[i];
             return attractor;
         }
        /// <summary>
        /// Calculate gamma coefficient of in- and out- power law degree distribution
        /// </summary>
        /// <param name="Net">The network</param>
        /// <param name="inGamma">Returning in-gamma degree distribution</param>
        /// <param name="inR">Correlation coefficient between of in-power law</param>
        /// <param name="inRPvalue">P-value of correlation coefficient between of in-power law</param>
        /// <param name="outGamma">Returning out-gamma degree distribution</param>
        /// <param name="outR">Correlation coefficient between of out-power law</param>
        /// <param name="outRPvalue">P-value of correlation coefficient between of out-power law</param>
        public static void FitDegreeDistribution(BasicNetwork Net, ref double inGamma, ref double inR, ref double inRPvalue, ref double outGamma, ref double outR, ref double outRPvalue)
        {
            double inA = 0, outA = 0; inGamma = outGamma = 0;
            var indegSequece = from p in Net.Nodes
                               group p by p.InDegree into gj
                               where gj.Key>0
                               select new Pair<float,float>(gj.Key, gj.Count());

            var outdegSequece = from p in Net.Nodes
                               group p by p.OutDegree into gj
                                where gj.Key > 0
                                select new Pair<float, float>(gj.Key, gj.Count());

            Mathutil.NumericMath.LestSquareFittingPowerLaw(indegSequece, ref inA, ref inGamma, ref inR);

            Mathutil.NumericMath.LestSquareFittingPowerLaw(outdegSequece, ref outA, ref outGamma, ref outR);
            inRPvalue = Mathutil.NumericMath.Pvalue4PearsonCC(inR, indegSequece.Count());
            outRPvalue = Mathutil.NumericMath.Pvalue4PearsonCC(outR, outdegSequece.Count());
        }
        public static void FitTotalDegreeDistribution(BasicNetwork Net, ref double Gamma, ref double R, ref double pValue)
        {
            double A = 0;
            var totaldegSequece = from p in Net.Nodes
                               group p by p.TotalDegree into gj
                               where gj.Key > 0
                               select new Pair<float, float>(gj.Key, gj.Count());



            Mathutil.NumericMath.LestSquareFittingPowerLaw(totaldegSequece, ref A, ref Gamma, ref R);
            pValue=Mathutil.NumericMath.Pvalue4PearsonCC(R, totaldegSequece.Count());

        }
        /// <summary>
        /// Return the minimum elements index of an array
        /// </summary>
        /// <typeparam name="T">Type of element in the array</typeparam>
        /// <param name="list">The array data</param>
        /// <returns>The index of the minimum element</returns>
        public static int MinElementIndex<T>(IList<T[]> list) where T : System.IComparable<T>
         {
             int minIdx = 0;

             for (int i=0;i<list.Count;i++)
             {
                 if (CompareTwoArrayElements<T>(list[i], list[minIdx]) < 0)
                     minIdx = i;
             }
             return minIdx;
         }
        /// <summary>
         ///  Return the maximum elements index of an array
        /// </summary>
         /// <typeparam name="T">Type of element in the array</typeparam>
         /// <param name="list">The array data</param>
         /// <returns>The index of the maxmimum element</returns>
        public static int MaxElementIndex<T>(IList<T[]> list) where T : System.IComparable<T>
         {
             int maxIdx = 0;
             for (int i = 0; i < list.Count; i++)
             {
                 if (CompareTwoArrayElements<T>(list[i], list[maxIdx]) > 0)
                     maxIdx = i;
             }
             return maxIdx;
         }

        /// <summary>
        /// Calculate the different degree between attractors
        /// if two attractors have different lengths, the longer one is the length to match them, 
        /// then the shorter one is assumed that it has null states, with similarity zero, at the end
        /// </summary>
        /// <param name="attractor1">The first attractor</param>
        /// <param name="attractor2">The second attractor</param>
        /// <returns>The different degree between two attractors</returns>
        public static float AttratorSimilarity(List<float[]> attractor1, List<float[]> attractor2)
        {
            float nSimilarity = 0;
            int overlapState = Math.Min(attractor1.Count, attractor2.Count);
            int totalState = Math.Max(attractor1.Count, attractor2.Count);
            
            //Reorder attractors
            Netutil.ReorderAttractor(attractor1);
            Netutil.ReorderAttractor(attractor2);


            for (int i = 0; i < overlapState; i++)
                nSimilarity += NetStateSimilarity(attractor1[i], attractor2[i]);

            return nSimilarity / totalState;
        }
        public static float AttratorFuzzySimilarity(List<float[]> attractor1, List<float[]> attractor2)
        {
            float nSimilarity = 0;
            int overlapState = Math.Min(attractor1.Count, attractor2.Count);
            int totalState = Math.Max(attractor1.Count, attractor2.Count);

            //Reorder attractors
            Netutil.ReorderAttractor(attractor1);
            Netutil.ReorderAttractor(attractor2);


            for (int i = 0; i < overlapState; i++)
            {
                nSimilarity += NetStateFuzzySimilarity(attractor1[i], attractor2[i]);
            }

            return nSimilarity / totalState;
        }
        public static double Prob2NodeInModule(Dictionary<Node, int> Cluster)
        {
            var nCluster = from p in Cluster
                           group p by p.Value into g
                           select new { Id = g, Count = g.Count() };
            

            double nCouple = 0.0f;
            foreach(var c in nCluster)
            {
                nCouple+=Mathutil.NumericMath.Combin(c.Count, 2);

            }
            return nCouple / Mathutil.NumericMath.Combin(Cluster.Count, 2);
            
        }


        public static float[] GetNodeState(IEnumerable<Node> nodes)
        {
            float[] currentStates = new float[nodes.Count()];
            for (int i = 0; i < nodes.Count(); i++)
                currentStates[i] = (nodes.ElementAt(i)as BooleanNode).State;
            return currentStates;
        }

        public static void SetNodeState(IEnumerable<Node> nodes, float[] states)
        {
            Debug.Assert(nodes.Count() == states.Length);

            for (int i = 0; i < nodes.Count(); i++)
                (nodes.ElementAt(i) as BooleanNode).ResetState(states[i]);
        }

        public static bool IsEqualNetStates(float[] states1, float[] states2)
        {
            if (states1.Length != states2.Length)
            {
                return false;
            }

            for (int i = 0; i < states1.Length; i++)
            {
                if (Math.Abs(states1[i] - states2[i]) >= 0.01)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsEqualNetStatesParallel(float[] states1, float[] states2)
        {
            bool a1IsNullOrEmpty = ReferenceEquals(states1, null) || states1.Length == 0;
            bool a2IsNullOrEmpty = ReferenceEquals(states2, null) || states2.Length == 0;
            if (a1IsNullOrEmpty) return a2IsNullOrEmpty;
            if (a2IsNullOrEmpty || states1.Length != states2.Length) return false;

            var areEqual = true;
            Parallel.ForEach(states1,
                (i, s, x) =>
                {
                    if (Math.Abs(states1[x] - states2[x]) >= 0.01)
                    {
                        areEqual = false;
                        s.Stop();
                    }
                });

            return areEqual;
        }

        public static bool IsEqualNetStates(IEnumerable<BooleanNode> states1, IEnumerable<BooleanNode> states2)
        {
            if (states1.Count() != states2.Count())
            {
                return false;
            }

            for (int i = 0; i < states1.Count(); i++)
            {
                if (states1.ElementAt(i).State != states2.ElementAt(i).State)
                {
                    return false;
                }
            }
            return true;
        }
        private static IEnumerable<int> ElementComparer<T>(T[] x, T[] y) where T : System.IComparable<T>
        {
            for (int i = 0; i < x.Length; i++)
            {
                yield return x[i].CompareTo(y[i]);
            }
            

        }
        public static IEnumerable<Node> UniqueNodes(IEnumerable<Node> nodes)
        {
            return (from p in nodes group p by p.name into g select g.ElementAt(0));
        }
        public static IEnumerable<Node> Union2nodeListByName(IEnumerable<Node> left, IEnumerable<Node> right)
        {
            left = UniqueNodes(left);
            right = UniqueNodes(right);
            var result = from p in left where !right.Any(t => t.name == p.name) select p;
            
            foreach (Node n in result)
            {
                yield return n;
            }
            foreach (Node n in right)
            {
                yield return n;
            }
        }
        //public static IEnumerable<Interaction> CloneInteraction(IEnumerable<Interaction> interactions)
        //{
        //    if (interactions == null) return null;
        //    HashSet<Interaction> pNews = new HashSet<Interaction>();
        //    foreach (Interaction i in interactions)
        //    {
        //        pNews.Add((Interaction)i.Clone());
        //    }
        //    return pNews;
        //}
        public static List<Node> CloneNode(IEnumerable<Node> nodes)
        {
            if (nodes == null) return null;
            List<Node> pNews = new List<Node>();
            foreach (Node i in nodes)
            {
                pNews.Add((Node)i.Clone());
            }
            return pNews;
        }
        /// <summary>
        /// Compare to generic array
        /// </summary>
        /// <typeparam name="T">A comparable type</typeparam>
        /// <param name="left">The first array</param>
        /// <param name="right">The second array</param>
        /// <returns>
        /// 1: left greater than right
        /// 0: left = right
        /// -1: left lower than right
        /// </returns>
        public static int CompareTwoArrayElements<T>(T[] left, T[] right) where T : System.IComparable<T>
        {
            IEnumerable<int> Pairs = ElementComparer(left, right);
            foreach (int e in Pairs)
            {
                if (e > 0)
                    return 1; 
                else if (e < 0)
                    return -1;
            }
            return 0;
        }
        
        public static bool IsEqualAttractors(List<float[]> attractor1, List<float[]> attractor2)
        {
            if (attractor1.Count != attractor2.Count)
            {
                return false;
            }
            //Reorder attractors before comparing

            ReorderAttractor(attractor1);
            ReorderAttractor(attractor2);
           
            for (int i = 0; i < attractor1.Count; i++)
            {
                if (!IsEqualNetStates(attractor1[i], attractor2[i]))
                {
                    return false;
                }
            }
            return true;
        }
       
        
        public static Dictionary<string, Dictionary<string, double>> CreateGraph(List<Interaction> interactions)
        {
            Dictionary<string, Dictionary<string, double>> graph = new Dictionary<string, Dictionary<string, double>>();
            foreach (Interaction edge in interactions)
            {
                if (graph.ContainsKey(edge.startNode.name) && graph.ContainsKey(edge.endNode.name))
                {
                    graph[edge.startNode.name][edge.endNode.name] += edge.weight;
                    continue;
                }
                if (!graph.ContainsKey(edge.startNode.name))
                    graph[edge.startNode.name] = new Dictionary<string, double>();

                graph[edge.startNode.name][edge.endNode.name] = edge.weight;

            }
            return graph;
        }
        public static string DumpList<T>(params IEnumerable<T>[] list)
        {
            string buffer = "Dumping the list of single objects...\n";
            for (int i = 0; i < list.Length; i++)
            {
                buffer += string.Format("{0}- ", i + 1);
                for (int j = 0; j < list[i].Count(); j++)
                    buffer += string.Format("{0}\t", list[i].ElementAt(j));
                buffer += "\n";
            }
            Debug.WriteLine(buffer);
            return buffer;
        }
        public static string DumpAttractor(List<float[]> States)
        {
            string buffer = "Attractor dumping...\n";
            for (int i = 0; i < States.Count; i++)
            {
                for (int j = 0; j < States[i].Length; j++)
                    buffer += string.Format("{0}", States[i][j] == FLogic.True ? "1" : "0");
                buffer += "-";
            }
            buffer= buffer.TrimEnd('-');
            Debug.WriteLine(buffer);
            return buffer;
        }
        /// <summary>
        /// Dump all content of network to a string.
        /// </summary>
        /// <returns></returns>
        public static string DumpNet(BooleanNetwork net)
        {
            string buffer = string.Format("Network with ID=[{0}] dumping...\n", net.ObjectID);
            int nEdge = 0;
            foreach (Interaction intr in net.Arcs)
            {

                buffer += string.Format("{0}- [{1}]\t{2,3}\t---"+(intr.Type<0?"|":">")+" {3,3}\t\ttype:{4,5}\tweight:{5}\t[id={6}, f={7}, s={8}]->[id={9}, f={10}, s={11}]",
                    ++nEdge,
                    intr.ObjectID,
                    intr.startNode,
                    intr.endNode,
                    intr.Type, intr.weight,
                    intr.startNode.ObjectID, (intr.startNode as BooleanNode).Type, FLogic.True == (intr.startNode as BooleanNode).State ? 1 : 0,
                    intr.endNode.ObjectID, (intr.endNode as BooleanNode).Type, FLogic.True == (intr.endNode as BooleanNode).State ? 1 : 0) + "\n";
            }
            Debug.WriteLine(buffer);

            IEnumerable<Node> isolateNodes = net.IsolateNodes;
            if (isolateNodes.Count() > 0)
            {
                buffer = "Isolating nodes:\n";
                foreach (BooleanNode n in isolateNodes)
                {
                    buffer += string.Format("{0}-\t[{1}]\tname:{2,5}\ttype:{3,3}\tstate:{4,1}\n", ++nEdge, n.ObjectID, n.name, n.Type, FLogic.True == n.State ? 1 : 0);
                }
                Debug.WriteLine(buffer);
            }


            return buffer;
        }
        public static string DumpDegree(BasicNetwork net)
        {
            string buffer = string.Format("Degree distribution of Network with ID=[{0}] dumping...\n", net.ObjectID);
            
            var degreeNodes = from n in net.Nodes orderby n.TotalDegree descending select n;

            foreach (Node n in degreeNodes)
            {

                buffer += string.Format("{0}\t{1}\n", n.name, n.TotalDegree);
            }
                    
            Debug.WriteLine(buffer);

            return buffer;
        }
        /// <summary>
        /// Return index of a value as the lower_bound of [value, x)
        /// </summary>
        /// <typeparam name="T">Type of data</typeparam>
        /// <param name="sortList">The sorted list to locate the lower bound</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int lower_bound<T>(IList<T> sortedList, T value) where T : IComparable
        {
            return sortedList.IndexOf((from x in sortedList where x.CompareTo(value)>=0 select x).FirstOrDefault());
        }
        /// <summary>
        ///  Return index of a value as the upper_bound of (x, value]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sortedList"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int upper_bound<T>(IList<T> sortedList, T value) where T : IComparable
        {
            return sortedList.IndexOf((from x in sortedList where x.CompareTo(value) <= 0 select x).FirstOrDefault());
        }
        

        public static string DumpNet(BasicNetwork net)
        {
            string buffer = string.Format("Network with ID=[{0}] dumping...\n", net.ObjectID);
            int nEdge = 0;
            foreach (Interaction intr in net.Arcs)
            {

                buffer += string.Format("{0}- [{1}]\t{2,3}\t---> {3,3}\t\ttype:{4,5}\tweight:{5}\t[{6}]->[{7}]",
                    ++nEdge,
                    intr.ObjectID,
                    intr.startNode,
                    intr.endNode,
                    intr.Type, intr.weight, intr.startNode.name, intr.endNode.name
                    ) + "\n";
            }
            Debug.WriteLine(buffer);

            IEnumerable<Node> isolateNodes = net.IsolateNodes;
            if (isolateNodes.Count() > 0)
            {
                buffer = "Isolating nodes:\n";
                foreach (Node n in isolateNodes)
                {
                    buffer += string.Format("{0}-\t[{1}]\tname:{2,5}\n", ++nEdge, n.ObjectID, n.name);
                }
                Debug.WriteLine(buffer);
            }


            return buffer;
        }
        public static string DumpInteraction(params Interaction[] interactions)
        {
            string buffer = "Interaction dumping...\n";
            
            if (interactions.Count() == 0) buffer += "Empty";
            //int i = 0;
            //foreach (Interaction n in interactions)
            //{
            //    buffer += string.Format("{0}-\t[{1}]\tstart:{2,5}; ID={3}\ttype:{4,3}\tend:{5,1};ID={6}\n", ++i, n.ObjectID, n.startNode.name, n.startNode.ObjectID, n.Type, n.endNode.name, n.endNode.ObjectID);
            //}
            //Debug.WriteLine(buffer);
            //return buffer;

            int nEdge = 0;
            foreach (Interaction intr in interactions)
            {

                buffer += string.Format("{0}- [{1}]\t{2,3}\t---> {3,3}\t\ttype:{4,5}\tweight:{5}\t[{6}]->[{7}]",
                    ++nEdge,
                    intr.ObjectID,
                    intr.startNode.name,
                    intr.endNode.name,
                    intr.Type, intr.weight, intr.startNode.ObjectID, intr.endNode.ObjectID
                    ) + "\n";
            }
            Debug.WriteLine(buffer);
            return buffer;

        }
        public static string DumpNode(params Node[] nodes)
        {
            var pSort = from p in nodes orderby p.name select p;
            string buffer = "Node dumping...\n";
            
            if(nodes.Count()==0) buffer+="Empty";
            foreach (Node n in pSort)
            {
                buffer += string.Format("{0}\n",n.ToString());
            }
            Debug.WriteLine(buffer);
            return buffer;

        }
        
        public static string DumpPairs<Key,Value>(IEnumerable<KeyValuePair<Key, Value>> pairs)
        {
            string buffer = "Dumping the list of object couples...\n";
            var sortlist = from p in pairs orderby p.Key, p.Value select p;
            int i = 0;
            foreach (var e in sortlist)
            {
                buffer += string.Format("{0}-\t", ++i);
                buffer += string.Format("(Key, Value)=\t{0}\t{1}\n", e.Key, e.Value);
            }
            Debug.WriteLine(buffer);
            return buffer;
        }
        public static string DumpCluster(Dictionary<Node, int> cluster)
        {
            string buffer = "Cluster dumping...\n";
            var sortCluster = from p in cluster orderby p.Value, p.Key.name select new { p };
            int i=0;
            foreach (var e in sortCluster)
            {
                buffer += string.Format("{0}-\t", ++i);
                buffer += string.Format("[{0}]\tNode:{1}\tClusterID:{2}\n", e.p.Key.ObjectID, e.p.Key.name, e.p.Value);
            }
            Debug.WriteLine(buffer);
            return buffer;
        }
        public static string DumpClusterWithNumericNodeName(Dictionary<Node, int> cluster)
        {
            string buffer = "Cluster dumping...\n";
            var sortCluster = from p in cluster orderby p.Value, Convert.ToInt64(p.Key.name) select new {p};
            int i = 0;
            foreach (var e in sortCluster )
            {
                buffer += string.Format("{0}-\t", ++i);
                buffer += string.Format("[{0}]\tNode:{1}\tClusterID:{2}\n", e.p.Key.ObjectID, e.p.Key.name, e.p.Value);
            }
            Debug.WriteLine(buffer);
            return buffer;
        }
        public static string DumpState(params float[][] states)
        {
            //string buffer = "State dumping...\n";
            string buffer = "";
            for (int j = 0; j < states.Length; j++)
            {
                buffer += string.Format("\n{0}-\t", j + 1);
                for (int i = 0; i < states[j].Length; i++)
                {
                    //buffer += string.Format("{0}", states[j][i] == FLogic.True ? "1" : "0");
                    buffer += string.Format("{0}", states[j][i]);
                }
            }
            Debug.WriteLine(buffer);
            return buffer;
        }
        public static string DumpNodeState(IEnumerable<Node> nodes)
        {
            //string buffer = "State dumping...\n";
            string buffer = "\n";

            for (int i = 0; i < nodes.Count(); i++)
            {
                //buffer += string.Format("{0}", states[j][i] == FLogic.True ? "1" : "0");
                buffer += string.Format("{0}", (nodes.ElementAt(i) as BooleanNode).State);
            }
            
            Debug.WriteLine(buffer);
            return buffer;
        }
        public static float[] InitState(IEnumerable<Node> nodes)
        {
            float[] ret = new float[nodes.Count()];
            for (int i = 0; i < ret.Length; i++)
            {
                if (nodes.ElementAt(i).InDegree % 2 == 0)
                    ret[i] = FLogic.False;
                else
                    ret[i] = FLogic.True;
            }
            return ret;
        }
        
        //public static Random random = new Random((int)DateTime.Now.Ticks);
        public static float[] InitRandomState(int nCount)
        {
            float[] ret = new float[nCount];
            for (int i = 0; i < ret.Length; i++)
            {
                if (NumericMath.RandomCraft.Next(0, 2) == 0)
                    ret[i] = FLogic.False;
                else
                    ret[i] = FLogic.True;
            }
            //Netutil.DumpState(ret);
            return ret;
        }
        //private static Stopwatch sw = new Stopwatch();
        /// <summary>
        /// Measure the time that the code runs
        /// </summary>
        /// <param name="IsStart">command indicator; true -> Start measurement; false -> Finish measurement and return the time range from start to stop </param>
        /// <returns>string of time spand</returns>
        public static string MeasureExecutionTime(ref Stopwatch sw, Boolean IsStart)
        {

            string ExecutionTimeTaken = "Starting...";


            // Start The StopWatch ...From 000
            if (IsStart)
                sw.Start();
            else
            {


                //Stop the Timer
                sw.Stop();
                ExecutionTimeTaken = string.Format("Minutes: Seconds: Mili seconds = {0}:{1}:{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds);
            }


            return ExecutionTimeTaken;
            
        }
        public static InteractionType ConvertInteraction(int interactionNumber)
        {
            switch (interactionNumber)
            {
                case 1:
                    return InteractionType.POSITIVE;
                case -1:
                    return InteractionType.NEGATIVE;
                case 0:
                    return InteractionType.NULL;
                default:
                    return InteractionType.NULL;
            }
        }
        //public static string AnalysisModuleToTextFile(BooleanNetwork net, string filename)
        //{
        //    const int nJob = 6;
        //    Dictionary<Node, int> Cluster = null, pTemp = null;

        //    User.One.ShowWaitIndicator(0, nJob);//job #0
        //    double modularity = net.modularity(ref Cluster);

        //    Netutil.WriteClusterToTextFile(Cluster, "Modules." + filename);
        //    User.One.MessageToUser("Clusters were save to file: " + "Modules." + filename);
        //    User.One.ShowWaitIndicator(1, nJob);//job #1

        //    Dictionary<int, double> InModuleRo = net.InModuleRobustness(Cluster, new Perturbation());
        //    User.One.ShowWaitIndicator(2, nJob);//job #2

        //    Dictionary<int, double> OutModuleRo = net.OutModuleRobustness(Cluster, new Perturbation());
        //    User.One.ShowWaitIndicator(3, nJob);//job #3

        //    Dictionary<int, double> ModuleMo = net.ModuleModularity(Cluster);
        //    User.One.ShowWaitIndicator(4, nJob);//job #4


        //    BooleanNetwork ClusterNet = net.CreateClusterNework(Cluster, true);
        //    User.One.ShowWaitIndicator(5, nJob);//job #5

        //    TextDB.WriteTextFile(new string[] {"Node", "Node degree", "Node in-degree", "Node out-degree", "Node Robustness", 
        //                "Subnet node", "Subnet edge", 
        //                "Isolate subnet robustness", "Group node robustness", "In-module robustness", "Out-module robustness", 
        //                "Isolate subnet Modularity", "Module modularity", 
        //            "Subnet centrality", "Subnet cycles"}, filename);

        //    foreach (GroupNode p in ClusterNet.Nodes)
        //    {

        //        TextDB.WriteTextFile(new string[] { p.name, p.Degree.ToString(), p.InDegree.ToString(), p.OutDegree.ToString(), net.NodeRobustness(p, new Perturbation()).ToString(), 
        //                p.SubNetwork.Nodes.Count().ToString(), p.SubNetwork.Edges.Count().ToString(), 
        //                p.SubNetwork.NetworkRobustness(new Perturbation()).ToString(), net.NodeGroupRobustness((from c in Cluster where c.Value.ToString()==p.name select c.Key),new Perturbation()).ToString(), InModuleRo[Convert.ToInt32(p.name)].ToString(), OutModuleRo[Convert.ToInt32(p.name)].ToString(),
        //                p.SubNetwork.modularity(ref pTemp).ToString(), ModuleMo[Convert.ToInt32(p.name)].ToString(),
        //                p.SubNetwork.Centrality.ToString(),p.SubNetwork.FindCycles(true).Count.ToString() }, filename);

        //    }
        //    string path = null;
        //    TextDB.WriteTextFile(new string[] { "" }, filename);
        //    TextDB.WriteTextFile(new string[] { "Start", "End", "Weight", "Interaction type" }, filename);
        //    foreach (Interaction edge in ClusterNet.Arcs)
        //    {
        //        path = TextDB.WriteTextFile(new string[] { edge.startNode.name, edge.endNode.name, edge.weight.ToString(), edge.Type.ToString() }, filename);
        //    }
        //    User.One.ShowWaitIndicator(6, nJob);//job #6

        //    User.One.MessageToUser("A network of node groups was save to file: " + filename);
        //    return path;
        //}
        public static void Working(WorkManager<BooleanNetwork, WorkData> Context, int WorkID)
        {
            WorkData workdata = Context.GetLocalVariable(WorkID);
            Node p = workdata.p;
            BooleanNetwork net = Context.GlobalVariable;
            Dictionary<Node, int> pTemp = null;
            User.One.MessageToUser("On work ID =" + WorkID.ToString());

            double modularity=p.SubNetwork.modularity(ref pTemp);
            var ClusterIDs = from e in pTemp group e by e.Value into g select g.Key;



            string[] data = new string[] { p.name, p.EdgeDegree.ToString(), p.InDegree.ToString(), p.OutDegree.ToString(), //net.NodeRobustness(p,new Perturbation()).ToString(), 
                        p.SubNetwork.Nodes.Count().ToString(), p.SubNetwork.Edges.Count().ToString(), 
                        //p.SubNetwork.NetworkMutantRobustnessParalell().ToString(), 
                        modularity.ToString(), ClusterIDs.Count().ToString(), workdata.InModularity.ToString(),
                        p.SubNetwork.DegreeCentrality.ToString(),p.SubNetwork.FindCycles(true).Count.ToString() };
            lock (_Lock)
            {
                TextDB.WriteTextFile(data, workdata.filename);
            }

        }
        readonly static Object _Lock = new object();
        public struct WorkData
        {
            public double InModularity;
            public string filename;
            public Node p;
            public WorkData(double InModularity,
            string filename,
            Node p)
            {
                this.InModularity = InModularity;
                this.filename = filename;
                this.p = p;
            }
        }
        
        
        #region median calulation
        private struct AnalysisMedian
        {
            public BooleanNetwork net;
            public BooleanNetwork ClusterNet;
            public string filename;
            
            public AnalysisMedian(BooleanNetwork net,
            BooleanNetwork ClusterNet,
            string filename)
            {
                this.net = net;
                this.ClusterNet = ClusterNet;
                this.filename = filename;
            }
        }
        private static void Working2(WorkManager<AnalysisMedian, bool> Context, int WorkID)
        {
            BooleanNetwork net = Context.GlobalVariable.net;
            BooleanNetwork ClusterNet=Context.GlobalVariable.ClusterNet;
            string filename = Context.GlobalVariable.filename;

            Dictionary<string, double> directedMedianVal = net.HierarchicalClosenessCentrality();
            Dictionary<string, double> undirectedMedianVal = net.HierarchicalClosenessCentrality();
            TextDB.WriteTextFile(new string[] { "Node", "network directed median", "network undirected median", "cluster directed median", "cluster undirected median" }, filename);
            foreach (Node p in ClusterNet.Nodes)
            {
                Dictionary<string, double> directedCluster = p.SubNetwork.HierarchicalClosenessCentrality();
                Dictionary<string, double> undirectedCluster = p.SubNetwork.HierarchicalClosenessCentrality();
                foreach (KeyValuePair<string, double> e in directedCluster)
                {
                    TextDB.WriteTextFile(new string[] { e.Key, directedMedianVal[e.Key].ToString(), undirectedMedianVal[e.Key].ToString(), e.Value.ToString(), undirectedCluster[e.Key].ToString() }, filename);
                }
            }
        }
        private static WorkManager<AnalysisMedian, bool> AnalysisMedianToTextFile(BooleanNetwork net, BooleanNetwork ClusterNet, string filename)
        {
            WorkManager<AnalysisMedian, bool> Worker = new WorkManager<AnalysisMedian, bool>();
            AnalysisMedian data=new AnalysisMedian(net, ClusterNet, filename);
            Worker.GlobalVariable = data;
            Worker.Start(0, Working2, false);
            return Worker;
            
        }
        #endregion
        public static string AnalysisModuleToTextFile(BooleanNetwork net, string filename)
        {
            const int nJob=5;
            Dictionary<Node, int> Cluster = null;

            User.One.ShowWaitIndicator(0, nJob);//job #0
            double modularity = net.modularity(ref Cluster);
            
            Netutil.WriteClusterToTextFile(modularity,Cluster, "Modules." + filename);
            User.One.MessageToUser("Clusters were save to file: " + "Modules." + filename);
            User.One.ShowWaitIndicator(1, nJob);//job #1

            

            Dictionary<int, double> ModuleMo = net.ModuleModularity(Cluster);
            User.One.ShowWaitIndicator(2, nJob);//job #2


            BooleanNetwork ClusterNet = new BooleanNetwork();
            ClusterNet.CreateClusterNeworkWithWeight(net,Cluster, true);
            User.One.ShowWaitIndicator(3, nJob);//job #3

            WorkManager<AnalysisMedian, bool> Worker1 = AnalysisMedianToTextFile(net, ClusterNet, filename + ".median.txt");
            string path = null;
            
            TextDB.WriteTextFile(new string[] { "Start", "End", "Weight", "Interaction type" }, filename);
            foreach (Interaction edge in ClusterNet.Arcs)
            {
                path = TextDB.WriteTextFile(new string[] { edge.startNode.name, edge.endNode.name, edge.weight.ToString(), edge.Type.ToString() }, filename);
            }
            User.One.ShowWaitIndicator(4, nJob);//job #4

            TextDB.WriteTextFile(new string[] { "" }, filename);
            TextDB.WriteTextFile(new string[] {"Node", "Node degree", "Node in-degree", "Node out-degree", //"Node Robustness", 
                        "Subnet node", "Subnet edge", 
                        //"Isolate subnet robustness", 
                        "Isolate subnet Modularity", "subnet's module #", "In-Module modularity",
                    "Subnet centrality", "Subnet cycles"}, filename);

            int k = 0;
            WorkManager<BooleanNetwork, WorkData> Worker = new WorkManager<BooleanNetwork, WorkData>();
            Worker.GlobalVariable = net;
            
            foreach (Node p in ClusterNet.Nodes)
            {
                Worker.Start(++k, Working, new WorkData(ModuleMo[Convert.ToInt32(p.name)],filename,p));
                
            }
            
            Worker.Wait4WorksDone();
            //Worker1.Wait4WorksDone();

            // Write median values of network of modules
            Dictionary<string, double> directedMedianVal = ClusterNet.HierarchicalClosenessCentrality();
            Dictionary<string, double> undirectedMedianVal = ClusterNet.HierarchicalClosenessCentrality();
            TextDB.WriteTextFile(new string[] { "Node", "Directed median", "Undirected median" }, filename);

            foreach (KeyValuePair<string, double> e in directedMedianVal)
            {
                TextDB.WriteTextFile(new string[] { e.Key, directedMedianVal[e.Key].ToString(), undirectedMedianVal[e.Key].ToString() }, filename);
            }
            // End writing median values of network of modules

            User.One.ShowWaitIndicator(5, nJob);//job #5
            

            User.One.MessageToUser("A network of node groups was save to file: " + filename);
            return path;
        }
        public static string WriteGraphToTextFile(BasicNetwork net, string filename)
        {

            TextDB.WriteTextFile(new string[] { "start", "type", "end"}, filename);
            string path = null;
            foreach (Interaction edge in net.Arcs)
            {
                path = TextDB.WriteTextFile(new string[] { edge.startNode.name, edge.Type.ToString(), edge.endNode.name}, filename);
            }
            foreach (Node n in net.IsolateNodes)
            {
                TextDB.WriteTextFile(new string[] { n.name}, filename);
            }

            return path;
        }
       
        public static string WriteClusterToTextFile(double Modularity, Dictionary<Node, int> Cluster, string filename)
        {
            
            string path = null;
            var sortCluster = (from p in Cluster orderby p.Value, p.Key.name select p);
            if (File.Exists(Netutil.OutPutDirector + "\\" + filename))
                TextDB.WriteTextFile("***********" + DateTime.Now.ToString() + "***********", filename);

            TextDB.WriteTextFile(string.Format("Modularity={0}",Modularity), filename);
            TextDB.WriteTextFile("ModuleID\tNode", filename);
            foreach (KeyValuePair<Node, int> p in sortCluster)
            {
                path = TextDB.WriteTextFile(new string[] {p.Value.ToString(), p.Key.name}, filename);
            }
            return path;
        }
        public static string OutPutDirector
        {
            get
            {
                // Original folder name used by the application
                string folder = Directory.GetCurrentDirectory() + "\\OutPut";
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                return folder;
            }
        }
        /// <summary>
        /// Create a folder in output folder of program.
        /// </summary>
        /// <param name="folderName">The name of the creating folder</param>
        /// <returns>Full path to the created folder</returns>
        public static string CreateOutputFolder(string folderName)
        {
            string folder=OutPutDirector+"\\"+folderName;
             if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
             return folder;

        }
        /// <summary>
        /// Extract file name from full file name (with its path)
        /// </summary>
        /// <param name="fileNameWithPath">The full file name</param>
        /// <returns></returns>
        public static string ExtractFileNameFromPath(string fileNameWithPath)
        { 
            int sep=fileNameWithPath.LastIndexOf('\\');
            if (sep < 0) sep = 0;
            else
                sep++;
            return fileNameWithPath.Substring(sep, fileNameWithPath.Length - sep );
        }
        public static string ExtractMainFileName(string shortFilename)
        {
            return shortFilename.Substring(0, shortFilename.LastIndexOf('.'));
        }
        /// <summary>
        /// Shuffle a list randomly
        /// </summary>
        /// <typeparam name="T">Type of elements in the list</typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        
        public static T [] Shuffle<T>(IEnumerable<T> list)
        {
            Random rng = new Random();
            int n = list.Count();
            T []result = list.ToArray();
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = result[k];
                result[k] = result[n];
                result[n] = value;
            }
            return result;
        }
       
        /// <summary>
        /// Substract two lists of nodes: Left - Right
        /// </summary>
        /// <param name="Left">The Left list</param>
        /// <param name="Right">The Right list</param>
        /// <returns>Nodes are in the Left list but not in the Right list</returns>
        public static IEnumerable<T> SubstractNodeListByID<T>(IEnumerable<T> Left, IEnumerable<T> Right) where T: Node
        {
            return from p in Left
                   join q in Right on p.id equals q.id into groupJoin
                   from subNode in groupJoin.DefaultIfEmpty()
                   where subNode == null
                   select p;
        }
        /// <summary>
        /// Substract two lists of nodes: Left - Right
        /// </summary>
        /// <param name="Left">The Left list</param>
        /// <param name="Right">The Right list</param>
        /// <returns>Nodes are in the Left list but not in the Right list</returns>
        public static IEnumerable<T> SubstractNodeListByName<T>(IEnumerable<T> Left, IEnumerable<T> Right) where T : Node
        {
            return from p in Left
                   join q in Right on p.name equals q.name into groupJoin
                   from subNode in groupJoin.DefaultIfEmpty()
                   where subNode == null
                   select p;
        }
        /// <summary>
        /// Select nodes in the node Left not in the node Right (Left-Right). NOTE: Left is BooleanNode instances actually
        /// </summary>
        /// <param name="Left">The Left list</param>
        /// <param name="Right">The Right list</param>
        /// <returns>Nodes are in the Left list but not in the Right list</returns>
        public static IEnumerable<BooleanNode> SubstractNodeListByID(IEnumerable<Node> Left, IEnumerable<BooleanNode> Right)
        {
            return from p in Left
                   join q in Right on p.id equals q.id into groupJoin
                   from subNode in groupJoin.DefaultIfEmpty()
                   where subNode == null
                   select p as BooleanNode;
        }

        public static IEnumerable<T> SubstractInteractionList<T>(IEnumerable<T> Left, IEnumerable<T> Right) where T : Interaction
        {
            return from p in Left
                   join q in Right on p.startNode.name+p.endNode.name equals q.startNode.name+q.endNode.name into groupJoin
                   from subNode in groupJoin.DefaultIfEmpty()
                   where subNode == null
                   select p;
        }
        
        public static string GetFullInputFileName(string filename)
        {
            if (!File.Exists(filename))
                filename = Netutil.InPutDirector + "\\" + filename;
            return filename;
        }
        public static string GetFullOutputFileName(string filename)
        {
            if (!File.Exists(filename))
                filename = Netutil.OutPutDirector + "\\" + filename;
            return filename;
        }
        public static string InPutDirector
        {
            get
            {
                
                string folder = Directory.GetCurrentDirectory() + "\\InPut";
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                return folder;
            }
        }
    }
}
