using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using BasicNet;
using MatrixLibrary;
using System.Diagnostics;
using NetSimulation.Community;
using NetSimulation.Lib;
using Mathutil;
using Fuzzy;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using MathNet.Numerics.Statistics;
using System.Threading.Tasks;

namespace BasicNet
{
    public class BooleanNetwork:BasicNetwork
    {
        #region Methods have to be overrided in sub-classes
        public override NetBased CreateObject()
        {
            return new BooleanNetwork();
        }
        public override void Assign(object Source)
        {
            base.Assign(Source);
           
        }
        public override Node NewNode(string name, object para, double weight=1.0)
        {
            
            return new BooleanNode(name, para == null ? BooleanNode.ArbitraryFunctionType:(FunctionType)para, weight);
        }

        public override Node[,] NewNodeArray(int n, int m)
        {
            return new BooleanNode[n, m];
        }
        public override Node[] NewNodeArray(int n)
        {
            return new BooleanNode[n];
        }
        public override Node[] NewNodeArray(params Node[] node)
        {
            //BooleanNode[] pNodes = new BooleanNode[node.Length];
            //for (int i = 0; i < pNodes.Length; i++)
            //    pNodes[i] = node[i] as BooleanNode;
            //return pNodes;
            return node;
        }
        #endregion
        public Node SelectOrAddNewNode(string nodeName, FunctionType ft)
        {
            var n = this.Nodes.Where(p => p.name == nodeName);
            if (n.Count() == 0)
            {
                Node node = this.NewNode(nodeName, ft);
                this.AddNode(node);
                return node;
            }
            else
                return n.ElementAt(0);
        }
       
        public BooleanNetwork()
        {
            
            
        }
        #region Robustness with fixed network structure
        private float[] CurrentStates
        {
            get
            {
                
                return Netutil.GetNodeState(Nodes);
            }
            set
            {
                Netutil.SetNodeState(Nodes, value);
            }
        }
       
        public int iMaxRobustnessLoop
        {
            get
            {
                return Math.Max(100, Nodes.Count() * 2);
                //return 20000;
            }
        }

        /// <summary>
        /// Check whether a node is robust with a given network state
        /// </summary>
        /// <param name="node">The node needs to check robustness</param>
        /// <param name="networkState">The network state</param>
        /// <returns>Robust or not</returns>
        public bool IsRobustNodeWithState(BooleanNode node, float[] networkState, Perturbation perturbation)
        {

            CurrentStates = networkState;


            List<float[]> att1 = FindNetworkAttractor();

            CurrentStates = networkState;
            
            perturbation.Perturb(node);

            List<float[]> att2 = FindNetworkAttractor();

            perturbation.Recover();

            return Netutil.IsEqualAttractors(att1, att2);
        }
        public double NodeMutantRobustnessForStateParalell(Dictionary<BooleanNode, int> NodeIndices, BooleanNode perturbedNode, IEnumerable<BooleanNode> Cluster, float[] networkState)
        {
            List<float[]> att1 = FindNetworkAttractorParalell(NodeIndices, networkState);
            networkState[NodeIndices[perturbedNode]] = FLogic.not(networkState[NodeIndices[perturbedNode]]);

            //Netutil.DumpState(networkState);

            List<float[]> att2 = FindNetworkAttractorParalell(NodeIndices, networkState);

            att1 = ExtractNodesAttrator(att1, Cluster);
            att2 = ExtractNodesAttrator(att2, Cluster);

            return Netutil.AttratorSimilarity(att1, att2);
        }
        public bool IsMutantRobustNodeWithStateParalell(Dictionary<BooleanNode, int> NodeIndices, BooleanNode perturbedNode, float[] networkState)
        {
            //float[] CurrentStates=Netutil.GetNodeState(net.Nodes);

            //Dictionary<BooleanNode, float> netStateDict = new Dictionary<BooleanNode, float>();
            //for (int i = 0; i < net.Nodes.Count(); i++)
            //    netStateDict.Add(net.Nodes.ElementAt(i), networkState[i]);

            //float[] sndnetworkState = (float[])networkState.Clone();
            //Debug.WriteLine("Before");
            //Netutil.DumpState(networkState);
            List<float[]> att1 = FindNetworkAttractorParalell(NodeIndices, networkState);
            //Debug.WriteLine("After");
            //Netutil.DumpState(networkState);
            //CurrentStates = networkState;

            //perturbation.Perturb(node);

            networkState[NodeIndices[perturbedNode]] = FLogic.not(networkState[NodeIndices[perturbedNode]]);
            //networkState[NodeIndices[perturbedNode.name]] = FLogic.not(networkState[NodeIndices[perturbedNode.name]]);

            //Netutil.DumpState(networkState);

            List<float[]> att2 = FindNetworkAttractorParalell(NodeIndices, networkState);

            //perturbation.Recover(node);

            return Netutil.IsEqualAttractors(att1, att2);
        }

        public class DiseaseDictionary
        {

            //public Dictionary<int, KeyValuePair<string, int>> Diseases = new Dictionary<int, KeyValuePair<string, int>>();
            public Dictionary<int, KeyValuePair<string, string>> AttractorPair = new Dictionary<int, KeyValuePair<string, string>>();
            //The dictionary of attractor pairs in which the first attractor is the key
            int _nDisease = 0;
            public int nDisease
            {
                get
                {
                    return _nDisease;
                }
            }
            /// <summary>
            /// Check and update the pair of attractors in the dictionary
            /// </summary>
            /// <param name="hashFirstAttractor">The fisrt attractor</param>
            /// <param name="hashSecondAttractor">The second attractor</param>
            /// <returns></returns>
            public bool CheckExistentDisease(string hashFirstAttractor, string hashSecondAttractor)
            {
                var result = from p in AttractorPair
                             where p.Value.Key == hashFirstAttractor && p.Value.Value == hashSecondAttractor
                             select p;
                if (result.Count() > 0)
                    return true;
                if (hashFirstAttractor != hashSecondAttractor)
                    AttractorPair.Add(++_nDisease, new KeyValuePair<string, string>(hashFirstAttractor, hashSecondAttractor));
                return false;
            }
            public HashSet<string> KeySet = new HashSet<string>();

            public bool CheckExistentDisease(string HashValue)
            {

                if (KeySet.Contains(HashValue))
                    return true;

                KeySet.Add(HashValue);

                return false;
            }

        }
        
        

        /// <summary>
        /// Get recovery degree of a node in a module
        /// </summary>
        /// <param name="perturbedNode">The node that needs get recovery degree</param>
        /// <param name="Cluster">The node module containing the node</param>
        /// <param name="networkState">The state of the network</param>
        /// <param name="perturbation">Perturbation type</param>
        /// <returns>The recovery degree of the node in the module</returns>
        public double NodeRobustnessForState(BooleanNode perturbedNode, IEnumerable<BooleanNode> Cluster, float[] networkState, Perturbation perturbation)
        {
            //Debug.Assert(Cluster.Contains(node));
            CurrentStates = networkState;
            List<float[]> att1 = FindNetworkAttractor();

            CurrentStates = networkState;
            perturbation.Perturb(perturbedNode);
            List<float[]> att2 = FindNetworkAttractor();
            perturbation.Recover();

            att1 = ExtractNodesAttrator(att1, Cluster);
            att2 = ExtractNodesAttrator(att2, Cluster);

            return Netutil.AttratorSimilarity(att1, att2);
        }

        public double NodeRobustnessForStateWith2Mutation(BooleanNode perturbedNode1, BooleanNode perturbedNode2, float[] networkState, Perturbation perturbation)
        {
            //Debug.Assert(Cluster.Contains(node));
            CurrentStates = networkState;
            List<float[]> att1 = FindNetworkAttractor();
            
            CurrentStates = networkState;

            perturbation.Perturb(perturbedNode1);
            perturbation.Perturb(perturbedNode2);
           
            List<float[]> att2 = FindNetworkAttractor();
            //perturbation.Recover();

            //att1 = ExtractNodesAttrator(att1, Cluster);
            //att2 = ExtractNodesAttrator(att2, Cluster);

            return Netutil.AttratorSimilarity(att1, att2);
            //return Netutil.IsEqualAttractors(att1, att2)?1:0;
        }
        /// <summary>
        /// Cacluate node recovery of a module
        /// </summary>
        /// <param name="perturbedNode">The node needs calculation</param>
        /// <param name="GroupNode">The module as list of node that contains the node inside</param>
        /// <param name="perturbation">Perturbation type</param>
        /// <returns>BooleanNode recovery degree</returns>
        public double GroupNodeRobustness(BooleanNode perturbedNode, IEnumerable<BooleanNode> GroupNode, Perturbation perturbation)
        {

            double drobustness = 0;
            int nLoop = 0;
            IEnumerable<float>[] e = For.FLogic(InitStandardState());
            float[] buffer = new float[e.Length];
            foreach (float[] netState in Enumerate<float>.Combination(buffer, e))
            {
                if (nLoop > iMaxRobustnessLoop) { nLoop--; break; }
                drobustness += NodeRobustnessForState(perturbedNode, GroupNode, netState, perturbation);
                nLoop++;
            }

            return (double)drobustness / nLoop;
        }
        /// <summary>
        /// Robustness of nodes inside a node group
        /// </summary>
        /// <param name="Group">The node group</param>
        /// <param name="perturbation">Perturbation</param>
        /// <returns>Robustness</returns>
        public double NodeGroupRobustness(IEnumerable<Node> Group, Perturbation perturbation)
        {

            double ro = 0;
            foreach (BooleanNode node in Group)
                ro += NodeRobustness(node, perturbation);
            return ro / Group.Count();
        }
        /// <summary>
        /// Get recovery degree of a node in overall network
        /// </summary>
        /// <param name="node">The node to get the recovery degree</param>
        /// <param name="networkState">The network state</param>
        /// <param name="perturbation">Perturbation type</param>
        /// <returns>Recovery degree of the node in overall network</returns>
        public double NodeRobustnessForState(BooleanNode node, float[] networkState, Perturbation perturbation)
        {

            CurrentStates = networkState;
            List<float[]> att1 = FindNetworkAttractor();

            CurrentStates = networkState;
            perturbation.Perturb(node);
            List<float[]> att2 = FindNetworkAttractor();
            perturbation.Recover();

            return Netutil.AttratorSimilarity(att1, att2);
        }
        class REC
        {
            public double moduleRecovery = 0;
            public double networkRecovery = 0;
        }
        //public Dictionary<BooleanNode, int> SelectCluster(Dictionary<BooleanNode, int> Cluster)
        //{

        //    var nodes = from p in Nodes where Cluster.Any(e => e.Key.name == p.name) select p;
        //    Dictionary<BooleanNode, int> retDic = new Dictionary<BooleanNode, int>();
        //    foreach (BooleanNode n in nodes)
        //    {
        //        retDic.Add(n, Cluster.Where(e => e.Key.name == n.name).Select(e => e).ElementAt(0).Value );
        //    }
        //    return retDic;
        //}
        /// <summary>
        /// Calculate the average recovery of nodes in modules based on Arcs (not Edges)
        ///     Requirement: All node on the network are captured in the Clusters
        /// </summary>
        /// <param name="Clusters">The modules to module recovery calculation</param>
        /// <param name="perturbation">Perturbation kind</param>
        /// <param name="inModuleRobustness">In-module robustness</param>
        /// <param name="outModuleRobustness">Out-module robustness</param>
        /// </returns>
        public void InOutModuleRobustness(Dictionary<Node, int> Clusters, Perturbation perturbation, ref double inModuleRobustness, ref double outModuleRobustness)
        {
            //Netutil.DumpCluster(Clusters);
            inModuleRobustness = 0; outModuleRobustness = 0;
            int nCount = 0;

            IEnumerable<int> clusterIndexes = Clusters.Values.Distinct();

            //Dictionary<int, Pair<IEnumerable<BooleanNode>, IEnumerable<BooleanNode>>> ClusterList = new Dictionary<int, Pair<IEnumerable<BooleanNode>, IEnumerable<BooleanNode>>>();
            //foreach (int cIndex in clusterIndexes)
            //{
            //    IEnumerable<BooleanNode> aCluster = (from p in this.Nodes join q in (from c in Clusters where c.Value == cIndex select c.Key) on p.name equals q.name select p);
            //    IEnumerable<BooleanNode> noneCluster = (from p in Nodes where !aCluster.Any(q => q.name == p.name) select p);
            //    ClusterList.Add(cIndex, new Pair<IEnumerable<BooleanNode>, IEnumerable<BooleanNode>>(aCluster, noneCluster));


            //}

            IEnumerable<float>[] e = For.FLogic(InitStandardState());
            int nLoop = 0;
            float[] buffer = new float[e.Length];
            foreach (float[] netState in Enumerate<float>.Combination(buffer,e))
            {
                if (++nLoop > iMaxRobustnessLoop) { nLoop--; break; }

                //Find node recovery by modules containing it
                foreach (int cIndex in clusterIndexes)
                {

                    IEnumerable<BooleanNode> aCluster = (from p in this.Nodes join q in (from c in Clusters where c.Value == cIndex select c.Key) on p.name equals q.name select p as BooleanNode);
                    IEnumerable<BooleanNode> noneCluster = (from p in Nodes where !aCluster.Any(q => q.name == p.name) select p as BooleanNode);

                    //IEnumerable<BooleanNode> aCluster = ClusterList[cIndex].First;
                    //IEnumerable<BooleanNode> noneCluster = ClusterList[cIndex].Second;

                    foreach (BooleanNode node in aCluster)
                    {

                        inModuleRobustness = inModuleRobustness + NodeRobustnessForState(node, aCluster, netState, perturbation);
                        //Netutil.DumpNode(aCluster.ToArray());

                        //Netutil.DumpNode(noneCluster.ToArray());
                        if (noneCluster.Count() > 0)
                            outModuleRobustness = outModuleRobustness + NodeRobustnessForState(node, noneCluster, netState, perturbation);
                        nCount++;
                    }
                }

            }
            
            inModuleRobustness = inModuleRobustness / nCount;
            outModuleRobustness = outModuleRobustness / nCount;
            //return dModuleRecovery / nCount;
        }
        IEnumerable<Node> outGroupNode(Dictionary<Node, int> pClusters, int theGroupIdx)
        {
            return from p in pClusters where p.Value != theGroupIdx select p.Key;
            
        }
        public void multiplePerturbationRobustness(Dictionary<Node, int> Clusters, Perturbation perturbation, ref double inModuleRobustness, ref double outModuleRobustness)
        {
            Clusters = Clustering.SelectNodeFromCluster(this, Clusters);
            inModuleRobustness = 0;
            outModuleRobustness = 0;
            

            int nCountIn = 0, nCountOut=0;
            Dictionary<int,List<Node>> pCls= Clustering.ConvertCluster(Clusters);

            IEnumerable<Node> hubs= this.SelectHubs();


            IEnumerable<float>[] e = For.FLogic(InitStandardState());
            int nLoop = 0;
            float[] buffer = new float[e.Length];
            foreach (float[] netState in Enumerate<float>.Combination(buffer, e))
            {
                if (++nLoop > iMaxRobustnessLoop) { nLoop--; break; }

                //foreach group
                foreach (int cIndex in pCls.Keys)
                {
                    IEnumerable<Node> outGroup = outGroupNode(Clusters, cIndex);
                    //for each node in the group
                    for(int i=0;i<pCls[cIndex].Count -1;i++)
                    {
                        
                        BooleanNode node1 = pCls[cIndex].ElementAt(i) as BooleanNode;
                        if (!hubs.Contains(node1)) continue;
                        
                        
                        for (int j = i+1; j < pCls[cIndex].Count; j++)
                        {
                            BooleanNode node2 = pCls[cIndex].ElementAt(j) as BooleanNode;
                            if (!hubs.Contains(node2)) continue;
                                
                            inModuleRobustness = inModuleRobustness + NodeRobustnessForStateWith2Mutation(node1, node2, netState, perturbation);
                            nCountIn++;
                          
                        }
                        foreach (BooleanNode node2 in outGroup)
                        {
                            if (!hubs.Contains(node2)) continue;
                            outModuleRobustness = outModuleRobustness + NodeRobustnessForStateWith2Mutation(node1, node2, netState, perturbation);
                            nCountOut++;
                        }
                    }


                }

            }

            inModuleRobustness = inModuleRobustness / nCountIn;
            outModuleRobustness = outModuleRobustness / nCountOut;
            //return dModuleRecovery / nCount;
        }
        
        #region Module calculation
        /// <summary>
        /// Calculate in-module robustness of clusters or modules
        /// </summary>
        /// <param name="Clusters">The modules to calculate robustness
        /// Have to sure the node in the modules on the network</param>
        /// <param name="perturbation">Perturbation kind</param>
        /// <returns>Robustness list of modules identified by module ID</returns>
        public Dictionary<int, double> InModuleRobustness(Dictionary<Node, int> Clusters, Perturbation perturbation)
        {
            Dictionary<int, double> dModuleRobustness = new Dictionary<int, double>();
            Dictionary<int, int> counter = new Dictionary<int, int>();
            //Clusters = SelectCluster(Clusters);


            IEnumerable<int> clusterIndexes = Clusters.Values.Distinct();

            IEnumerable<float>[] e = For.FLogic(InitStandardState());
            int nLoop = 0;
            float[] buffer = new float[e.Length];
            foreach (float[] netState in Enumerate<float>.Combination(buffer,e))
            {
                if (++nLoop > iMaxRobustnessLoop) { nLoop--; break; }

                //Find node recovery by modules containing it
                foreach (int cIndex in clusterIndexes)
                {
                    //IEnumerable<BooleanNode> aCluster = (from p in Clusters where p.Value == cIndex select p.Key);
                    IEnumerable<BooleanNode> aCluster = (from p in this.Nodes join q in (from c in Clusters where c.Value == cIndex select c.Key) on p.name equals q.name select p as BooleanNode);

                    //Perturn on nodes inside the cluster to calculate its robustness
                    foreach (BooleanNode node in aCluster)
                    {
                        if (!dModuleRobustness.Keys.Contains(cIndex))
                        {
                            dModuleRobustness.Add(cIndex, 0);
                            counter.Add(cIndex, 0);
                        }
                        dModuleRobustness[cIndex] = dModuleRobustness[cIndex] + NodeRobustnessForState(node, aCluster, netState, perturbation);
                        counter[cIndex]++;
                    }
                }


            }
            foreach (int cIndex in clusterIndexes)
            {
                dModuleRobustness[cIndex] = dModuleRobustness[cIndex] / counter[cIndex];
            }

            return dModuleRobustness;
        }

        public Dictionary<int, double> OutModuleRobustness(Dictionary<Node, int> Clusters, Perturbation perturbation)
        {
            Dictionary<int, double> dModuleRobustness = new Dictionary<int, double>();
            Dictionary<int, int> counter = new Dictionary<int, int>();
            //Clusters = SelectCluster(Clusters);


            IEnumerable<int> clusterIndexes = Clusters.Values.Distinct();

            IEnumerable<float>[] e = For.FLogic(InitStandardState());
            int nLoop = 0;
            float[] buffer = new float[e.Length];
            foreach (float[] netState in Enumerate<float>.Combination(buffer,e))
            {
                if (++nLoop > iMaxRobustnessLoop) { nLoop--; break; }

                //Find node recovery by modules containing it
                foreach (int cIndex in clusterIndexes)
                {
                    //IEnumerable<BooleanNode> aCluster = (from p in Clusters where p.Value == cIndex select p.Key);
                    IEnumerable<BooleanNode> aCluster = (from p in this.Nodes join q in (from c in Clusters where c.Value == cIndex select c.Key) on p.name equals q.name select p as BooleanNode);

                    IEnumerable<BooleanNode> outNodes = (from p in this.Nodes where !aCluster.Any(t => t.name == p.name) select p as BooleanNode);//BooleanNode outside the cluster
                    //IEnumerable<BooleanNode> noneCluster = (from p in Nodes where aCluster.All(q => q.name != p.name) select p);

                    //Perturn on nodes inside the cluster to calculate the its outside area

                    foreach (BooleanNode node in aCluster)
                    {
                        if (!dModuleRobustness.Keys.Contains(cIndex))
                        {
                            dModuleRobustness.Add(cIndex, 0);
                            counter.Add(cIndex, 0);
                        }
                        //if (noneCluster.Count() > 0)
                        //    outModuleRobustness = outModuleRobustness + NodeRobustnessForState(node, noneCluster, netState, perturbation);
                        if (outNodes.Count() > 0)
                        {
                            dModuleRobustness[cIndex] = dModuleRobustness[cIndex] + NodeRobustnessForState(node, outNodes, netState, perturbation);
                            counter[cIndex]++;
                        }
                    }
                }


            }
            foreach (int cIndex in clusterIndexes)
            {
                dModuleRobustness[cIndex] = dModuleRobustness[cIndex] / counter[cIndex];
            }

            return dModuleRobustness;
        }

        

        #endregion

        /// <summary>
        /// Calculate the average recovery of nodes in overall network.
        /// </summary>
        /// <param name="perturbation">Perturbation kind</param>
        /// <returns>The network recovery degree</returns>
        public double NetworkRecovery(Perturbation perturbation)
        {
            double dRecovery = 0;
            int nCount = 0;
            IEnumerable<float>[] e = For.FLogic(InitStandardState());
            int nLoop = 0;
            float[] buffer = new float[e.Length];
            foreach (float[] netState in Enumerate<float>.Combination(buffer,e))
            {
                if (++nLoop > iMaxRobustnessLoop) { nLoop--; break; }

                //Find node recovery by modules containing it

                foreach (BooleanNode node in Nodes)
                {
                    dRecovery = dRecovery + NodeRobustnessForState(node, netState, perturbation);
                    nCount++;
                }
            }
            return dRecovery / nCount;// (nLoop * Nodes.Count());
        }
        /// <summary>
        /// Return nodes are soure of a node group
        /// </summary>
        /// <param name="nodeCluster">The node group needs find out its source nodes</param>
        /// <returns></returns>
        public IEnumerable<Node> GetClusterCover(IEnumerable<Node> nodeCluster)
        {
            return ((from p in Nodes where p.DesNodes.Any(e => nodeCluster.Any(i => i.name == e.name)) select p).Union(nodeCluster)).Distinct();
        }

        /// <summary>
        /// Calculate the network robustness based on Arcs (not Edges)
        /// </summary>
        /// <returns>The robustness of the network</returns>
        public double NetworkRobustness(Perturbation perturbation)
        {
            double nCount = 0;

            IEnumerable<float>[] e = For.FLogic(InitStandardState());
            int nLoop = 0;
            float[] buffer = new float[e.Length];
            foreach (float[] netState in Enumerate<float>.Combination(buffer,e))
            {
                if (++nLoop > iMaxRobustnessLoop) { nLoop--; break; }

                foreach (BooleanNode node in Nodes)
                {
                    if (IsRobustNodeWithState(node, netState, perturbation))
                        nCount += 1;
                }
            }
            return nCount / (nLoop * Nodes.Count());
        }
        public double NetworkRobustness(IEnumerable<Node> groupNode, Perturbation perturbation)
        {
            double nCount = 0;

            IEnumerable<float>[] e = For.FLogic(InitStandardState());
            int nLoop = 0;
            float[] buffer = new float[e.Length];
            foreach (float[] netState in Enumerate<float>.Combination(buffer,e))
            {
                if (++nLoop > iMaxRobustnessLoop) { nLoop--; break; }

                foreach (BooleanNode node in groupNode)
                {
                    if (IsRobustNodeWithState(node, netState, perturbation))
                        nCount += 1;
                }
            }
            return nCount / (nLoop * Nodes.Count());
        }
        
        #region Paralell functions
        private struct NodeWorkerData
        {
            public NodeWorkerData(BooleanNode node, IEnumerable<BooleanNode> pCluster)
            {
                this.node = node;
                this.Ro = 0;
                this.pCluster = pCluster;
            }
            public BooleanNode node;
            public double Ro;
            public IEnumerable<BooleanNode> pCluster;
        }
        private Dictionary<BooleanNode, object> CreateDictionaryOfStates(float[] buffer)
        {

            Dictionary<BooleanNode, object> theDict = new Dictionary<BooleanNode, object>();
            for (int i = 0; i < this.Nodes.Count(); i++)
            {
                theDict.Add(this.Nodes.ElementAt(i) as BooleanNode, buffer[i]);
            }
            return theDict;

        }
        /// <summary>
        /// This function for paralell call
        /// </summary>
        /// <param name="Context">worker context</param>
        /// <param name="WorkID">worker ID</param>
        private void NodeMutantRobustnessWoker(WorkManager<Dictionary<BooleanNode, int>, NodeWorkerData> Context, int WorkID)
        {


            NodeWorkerData pWorkData = Context.GetLocalVariable(WorkID);
            Dictionary<BooleanNode, int> NodeIndices = Context.GlobalVariable;
            //int iMaxRobustnessLoop = net.iMaxRobustnessLoop;
            //IEnumerable<BooleanNode> Nodes = net.Nodes;
            //Debug.WriteLine("Working !!");

            IEnumerable<float>[] e = For.FLogic(InitStandardState());
            int nLoop = 0;
            int nCount = 0;
            float[] buffer = new float[e.Length];
            Dictionary<BooleanNode, object> Dictionary = CreateDictionaryOfStates(buffer);
            foreach (float[] netState in Enumerate<float>.Combination(buffer,e))
            {
                if (++nLoop > iMaxRobustnessLoop) { nLoop--; break; }

                if (IsMutantRobustNodeWithStateParalell(NodeIndices, pWorkData.node, netState))
                    nCount++;
            }


            pWorkData.Ro = nCount / (double)nLoop;
            //Netutil.DumpNode(pWorkData.node);
            //Debug.WriteLine("BooleanNode="+pWorkData.node.name+" Ro="+pWorkData.Ro.ToString());
            Context.SetLocalVariable(WorkID, pWorkData);
        }



        /// <summary>
        /// Calculate network robustness with mutant perturbation (paralell run)
        /// </summary>
        /// <returns>Robustness of this network</returns>
        public double NetworkMutantRobustnessParalell()
        {
            WorkManager<Dictionary<BooleanNode, int>, NodeWorkerData> Worker = new WorkManager<Dictionary<BooleanNode, int>, NodeWorkerData>();
            //Dictionary<string, int> NodeIndices = new Dictionary<string, int>();
            //for (int j = 0; j < Nodes.Count(); j++)
            //    NodeIndices.Add(Nodes.ElementAt(j).name, j);

            Dictionary<BooleanNode, int> NodeIndices = new Dictionary<BooleanNode, int>();
            for (int j = 0; j < Nodes.Count(); j++)
                NodeIndices.Add(Nodes.ElementAt(j) as BooleanNode, j);

            Worker.GlobalVariable = NodeIndices;
            int i = 0;
            foreach (BooleanNode node in Nodes)
            {
                Worker.AddWork(++i, NodeMutantRobustnessWoker, new NodeWorkerData(node, null));
            }
            Worker.Start();
            Worker.Wait4WorksDone();

            double Ro = Worker.LocalVariables.Select(t => t.Value.Ro).Average();
            Worker.Dispose();

            return Ro;
        }
        public double NetworkMutantRobustnessParalell(IEnumerable<BooleanNode> groupNodes)
        {
            WorkManager<Dictionary<BooleanNode, int>, NodeWorkerData> Worker = new WorkManager<Dictionary<BooleanNode, int>, NodeWorkerData>();
            Dictionary<BooleanNode, int> NodeIndices = new Dictionary<BooleanNode, int>();
            for (int j = 0; j < groupNodes.Count(); j++)
                NodeIndices.Add(Nodes.ElementAt(j) as BooleanNode, j);
            Worker.GlobalVariable = NodeIndices;
            int i = 0;
            foreach (BooleanNode node in groupNodes)
            {
                Worker.AddWork(++i, NodeMutantRobustnessWoker, new NodeWorkerData(node, null));
            }
            Worker.Start();
            Worker.Wait4WorksDone();

            double Ro = Worker.LocalVariables.Select(t => t.Value.Ro).Average();
            Worker.Dispose();

            return Ro;
        }
        private struct GroupNodeWorkerData
        {
            public GroupNodeWorkerData(IEnumerable<BooleanNode> cluster, IEnumerable<BooleanNode> nonCluster)
            {
                this.cluster = cluster;
                this.nonCluster = nonCluster;
                this.inRo = this.outRo = 0;
            }
            public IEnumerable<BooleanNode> cluster, nonCluster;
            public double inRo, outRo;
        }

        //-----------------
        private void GroupNodeMutantRobustnessWoker(WorkManager<Dictionary<BooleanNode, int>, NodeWorkerData> Context, int WorkID)
        {


            NodeWorkerData pWorkData = Context.GetLocalVariable(WorkID);
            IEnumerable<BooleanNode> pCluster = pWorkData.pCluster;
            Dictionary<BooleanNode, int> NodeIndices = Context.GlobalVariable;
            //int iMaxRobustnessLoop = net.iMaxRobustnessLoop;
            //IEnumerable<BooleanNode> Nodes = net.Nodes;
            //Debug.WriteLine("Working !!");

            IEnumerable<float>[] e = For.FLogic(InitStandardState());
            int nLoop = 0;

            double Ro = 0;
            float[] buffer = new float[e.Length];
            foreach (float[] netState in Enumerate<float>.Combination(buffer,e))
            {
                if (++nLoop > iMaxRobustnessLoop) { nLoop--; break; }

                Ro = Ro + NodeMutantRobustnessForStateParalell(NodeIndices, pWorkData.node, pCluster, netState);
            }


            pWorkData.Ro = Ro / (double)nLoop;
            //Netutil.DumpNode(pWorkData.node);
            //Debug.WriteLine("BooleanNode="+pWorkData.node.name+" Ro="+pWorkData.Ro.ToString());
            Context.SetLocalVariable(WorkID, pWorkData);
        }
        private void GroupNodeMutantRobustnessWorker(WorkManager<Dictionary<BooleanNode, int>, GroupNodeWorkerData> Context, int WorkID)
        {
            GroupNodeWorkerData pWorkData = Context.GetLocalVariable(WorkID);
            IEnumerable<BooleanNode> aCluster = pWorkData.cluster;
            IEnumerable<BooleanNode> noneCluster = pWorkData.nonCluster;

            
            WorkManager<Dictionary<BooleanNode, int>, NodeWorkerData> inWorker = new WorkManager<Dictionary<BooleanNode, int>, NodeWorkerData>();
            WorkManager<Dictionary<BooleanNode, int>, NodeWorkerData> outWorker = new WorkManager<Dictionary<BooleanNode, int>, NodeWorkerData>();
            //Dictionary<BooleanNode, int> NodeIndices = new Dictionary<BooleanNode, int>();
            //for (int j = 0; j < Nodes.Count(); j++)
            //    NodeIndices.Add(Nodes.ElementAt(j) as BooleanNode, j);
            inWorker.GlobalVariable = Context.GlobalVariable;//NodeIndices;
            outWorker.GlobalVariable = Context.GlobalVariable;// NodeIndices;

            int i = 0;
            foreach (BooleanNode node in aCluster)
            {
                ++i;
                inWorker.AddWork(i, GroupNodeMutantRobustnessWoker, new NodeWorkerData(node, aCluster));
                outWorker.AddWork(i, GroupNodeMutantRobustnessWoker, new NodeWorkerData(node, noneCluster));
            }
            inWorker.Start();
            outWorker.Start();

            inWorker.Wait4WorksDone();
            outWorker.Wait4WorksDone();

            double inModuleRobustness = inWorker.LocalVariables.Select(t => t.Value.Ro).Average();
            double outModuleRobustness = outWorker.LocalVariables.Select(t => t.Value.Ro).Average();
            inWorker.Dispose();
            outWorker.Dispose();

            //int nCount = 0;
            //foreach (BooleanNode node in aCluster)
            //{
            //    int nLoop = 0;
            //    IEnumerable<float>[] e = For.FLogic(InitStandardState());
            //      float[] buffer = new float[e.Length];
            //    foreach (float[] netState in Enumerate<float>.Combination(buffer,e))
            //    {
            //        if (++nLoop > iMaxRobustnessLoop) { nLoop--; break; }
            //        inModuleRobustness = inModuleRobustness + NodeRobustnessForState(node, aCluster, netState, new Perturbation() );

            //        if (noneCluster.Count() > 0)
            //            outModuleRobustness = outModuleRobustness + NodeRobustnessForState(node, noneCluster, netState, new Perturbation());
            //        nCount++;
            //    }
            //}

            //inModuleRobustness = inModuleRobustness / nCount;
            //outModuleRobustness = outModuleRobustness / nCount;

            pWorkData.inRo = inModuleRobustness;
            pWorkData.outRo = outModuleRobustness;
            Context.SetLocalVariable(WorkID, pWorkData);
        }
        public void InOutModuleRobustnessParalellOld(Dictionary<Node, int> Clusters, Perturbation perturbation, ref double inModuleRobustness, ref double outModuleRobustness)
        {

            WorkManager<Dictionary<BooleanNode, int>, GroupNodeWorkerData> Worker = new WorkManager<Dictionary<BooleanNode, int>, GroupNodeWorkerData>();
            Dictionary<BooleanNode, int> NodeIndices = new Dictionary<BooleanNode, int>();
            for (int j = 0; j < Nodes.Count(); j++)
                NodeIndices.Add(Nodes.ElementAt(j) as BooleanNode, j);
            Worker.GlobalVariable = NodeIndices;

            int i = 0;
            IEnumerable<int> clusterIndexes = Clusters.Values.Distinct();
            foreach (int cIndex in clusterIndexes)
            {
                IEnumerable<BooleanNode> aCluster = (from p in this.Nodes join q in (from c in Clusters where c.Value == cIndex select c.Key) on p.name equals q.name select p as BooleanNode);
                IEnumerable<BooleanNode> noneCluster = (from p in Nodes where !aCluster.Any(q => q.name == p.name) select p as BooleanNode);

                Worker.AddWork(++i, GroupNodeMutantRobustnessWorker, new GroupNodeWorkerData(aCluster, noneCluster));
            }
            Worker.Start();
            Worker.Wait4WorksDone();

            inModuleRobustness = Worker.LocalVariables.Select(t => t.Value.inRo).Average();
            outModuleRobustness = Worker.LocalVariables.Select(t => t.Value.outRo).Average();

            Worker.Dispose();


        }
        /// <summary>
        /// Create a list of nodes that are not included in a given cluster
        /// </summary>
        /// <param name="exclusiveClusterID">ID of the given cluster</param>
        /// <param name="Clusters">The cluster list contains all nodes</param>
        /// <returns>The list in the cluster list without the given clusterID</returns>
        private IEnumerable<BooleanNode> GetNonCluster(int exclusiveClusterID, Dictionary<int, List<BooleanNode>> Clusters)
        {
            List<BooleanNode> nonClusterID=new List<BooleanNode>();
            foreach(var cls in Clusters.Keys)
            {
                if (cls != exclusiveClusterID)
                    nonClusterID.AddRange(Clusters[cls]);
            }
            return nonClusterID;
        }
        public void InOutModuleRobustnessParalell(Dictionary<Node, int> pClustering, Perturbation perturbation, ref double inModuleRobustness, ref double outModuleRobustness)
        {
            Dictionary<int, List<BooleanNode>> Clusters = this.SelectNodeFromCluster(pClustering);

            WorkManager<Dictionary<BooleanNode, int>, GroupNodeWorkerData> Worker = new WorkManager<Dictionary<BooleanNode, int>, GroupNodeWorkerData>();
            Dictionary<BooleanNode, int> NodeIndices = new Dictionary<BooleanNode, int>();
            for (int j = 0; j < Nodes.Count(); j++)
                NodeIndices.Add(Nodes.ElementAt(j) as BooleanNode, j);
            Worker.GlobalVariable = NodeIndices;

            int i = 0;
            IEnumerable<int> clusterIndexes = Clusters.Keys;
            foreach (int cIndex in clusterIndexes)
            {
                IEnumerable<BooleanNode> aCluster = Clusters[cIndex];
                IEnumerable<BooleanNode> noneCluster = GetNonCluster(cIndex, Clusters);//(from p in Nodes where !aCluster.Any(q => q.name == p.name) select p as BooleanNode);

                Worker.AddWork(++i, GroupNodeMutantRobustnessWorker, new GroupNodeWorkerData(aCluster, noneCluster));
            }
            Worker.Start();
            Worker.Wait4WorksDone();

            inModuleRobustness = Worker.LocalVariables.Select(t => t.Value.inRo).Average();
            outModuleRobustness = Worker.LocalVariables.Select(t => t.Value.outRo).Average();

            Worker.Dispose();


        }
        #endregion
        public double NetworkRobustnessWithRandomInitiation(Perturbation perturbation)
        {
            double nCount = 0;

            IEnumerable<float>[] e = For.FLogic(Netutil.InitRandomState(this.Nodes.Count()));
            int nLoop = 0;
            float[] buffer = new float[e.Length];
            foreach (float[] netState in Enumerate<float>.Combination(buffer,e))
            {
                if (++nLoop > iMaxRobustnessLoop) { nLoop--; break; }

                foreach (BooleanNode node in Nodes)
                {
                    if (IsRobustNodeWithState(node, netState, perturbation))
                        nCount += 1;
                }
            }
            return nCount / (nLoop * Nodes.Count());
        }

        



        /// <summary>
        /// Create the initial network standard state (only one state for a network), for the robustness measure on the network is stable
        /// </summary>
        /// <returns></returns>
        //public float[] InitStandardUnlockedState()
        //{
        //    return Netutil.InitState(UnlockedNodes);
        //}
        public float[] InitStandardState()
        {
            return Netutil.InitState(Nodes);
        }
        

        /// <summary>
        /// Calculate the robustness of a node with a intial network state being specific (use Arcs for update functions, not using Edges)
        /// </summary>
        /// <param name="node">The node</param>
        /// <returns></returns>
        public double NodeRobustness(BooleanNode node, Perturbation perturbation)
        {

            int nRobustCount = 0;
            int nLoop = 0;
            IEnumerable<float>[] e = For.FLogic(InitStandardState());
            float[] buffer = new float[e.Length];
            foreach (float[] netState in Enumerate<float>.Combination(buffer,e))
            {
                if (++nLoop > iMaxRobustnessLoop) { nLoop--; break; }

                if (IsRobustNodeWithState(node, netState, perturbation))
                    nRobustCount++;
            }

            return (double)nRobustCount / nLoop;

        }
        

        public double NodeRobustness_Fuzzy(BooleanNode node, Perturbation perturbation)
        {

            double similarity = 0;
            int nLoop = 0;
            IEnumerable<float>[] e = For.FLogic(InitStandardState());
            float[] fuzzyState;
            float[] buffer = new float[e.Length];
            foreach (float[] netState in Enumerate<float>.Combination(buffer,e))
            {
                fuzzyState = Fuzzy.FLogic.Randomize(netState);
                if (++nLoop > iMaxRobustnessLoop) { nLoop--; break; }
                similarity += NodeRobustnessForState_Fuzzy(node, fuzzyState, perturbation);
                
            }
            
            return (double)similarity / nLoop;

        }
        //This functions work with both directed and undirected links in a network. Undirected links are considered as two-ending directed links
        
        public double NodeRobustnessForState_Fuzzy(BooleanNode node, float[] networkState, Perturbation perturbation)
        {

            CurrentStates = networkState;
            List<float[]> att1 = FindNetworkAttractor();

            CurrentStates = networkState;
            perturbation.Perturb(node);
            List<float[]> att2 = FindNetworkAttractor();
            perturbation.Recover();

            return Netutil.AttratorFuzzySimilarity(att1, att2);
        }


        /// <summary>
        /// Tạo ra trạng thái tiếp theo của cả mạng
        /// </summary>
        /// <returns>
        /// Trả về trạng thái hiện tại (trạng thái cuối cùng)
        /// </returns>
        private static float[] GoToNextStates(IEnumerable<Node> nodes)
        {
            //Assign the old state by the current state
            GotoHere(nodes);

            foreach (BooleanNode node in nodes)
            {
                node.GoToNextState();
            }
            return Netutil.GetNodeState(nodes);
        }

        private static float[] Spin_GoToNextStates(IEnumerable<Node> nodes,float E)
        {
            //Assign the old state by the current state
            GotoHere(nodes);
            
            foreach (BooleanNode node in nodes)
            {
                node.Spin_GoToNextState(E);
            }
            return Netutil.GetNodeState(nodes);
        }

        private void GoToNextStatesParalell(Dictionary<BooleanNode, int> NodeIndices, float[] networkState)
        {
            //Assign the old state by the current state
            float[] preNetworkState = (float[])networkState.Clone();

            //float[] newState = new float[networkState.Length];

            for (int i = 0; i < networkState.Length; i++)
            {
                networkState[i] = (Nodes.ElementAt(i) as BooleanNode).GoToNextStateParalell(preNetworkState, NodeIndices, networkState);
            }
            //return newState;
        }


        private static float[] GotoHere(IEnumerable<Node> nodes)
        {
            foreach (BooleanNode node in nodes)
            {
                node.ResetState(node.State);
            }
            return Netutil.GetNodeState(nodes);
        }

        /// <summary>
        /// Find the acttractor of the network
        /// </summary>
        /// <returns>the acttractor as a list of network states, which may be a feedback loop or only a state</returns>
        private List<float[]> FindNetworkAttractor()
        {
            var statesLists = new List<float[]> { CurrentStates };

            do
            {
                float[] st = GoToNextStates(Nodes);

                for (int i = statesLists.Count - 1; i >= 0; i--)
                {
                    if (Netutil.IsEqualNetStates(st, statesLists[i]))
                        return statesLists.GetRange(i, statesLists.Count - i); // the network state at position i is the state, in the attractor, directly converged from CurrentStates 
                    // ( zero index in the return result)
                }
                statesLists.Add(st);

            } while (true);
        }

        public List<float[]> FindNetworkAttractorParalell(Dictionary<BooleanNode, int> NodeIndices, float[] networkState)
        {
            var statesLists = new List<float[]> { (float[])networkState.Clone() };

            do
            {

                GoToNextStatesParalell(NodeIndices, networkState);

                for (int i = statesLists.Count - 1; i >= 0; i--)
                {
                    if (Netutil.IsEqualNetStates(networkState, statesLists[i]))
                        return statesLists.GetRange(i, statesLists.Count - i); // the network state at position i is the state, in the attractor, directly converged from CurrentStates 
                }
                statesLists.Add((float[])networkState.Clone());

            } while (true);
        }
        /// <summary>
        /// Extract attractor of nodes from a network's attractor
        /// </summary>
        /// <param name="netAttractor">The network's attractor</param>
        /// <param name="nodes">BooleanNode in the network needing to extract the their attractor from the network attractor</param>
        /// <returns>BooleanNode's attractor</returns>
        private List<float[]> ExtractNodesAttrator(List<float[]> netAttractor, IEnumerable<BooleanNode> nodes)
        {
            Dictionary<BooleanNode, int> IndexMap = new Dictionary<BooleanNode, int>();
            for (int i = 0; i < Nodes.Count(); i++)
                IndexMap.Add(Nodes.ElementAt(i) as BooleanNode, i);

            //clusterIndex contains indexes of element in the Cluster
            IEnumerable<int> clusterIndex = from e1 in IndexMap join e2 in nodes on e1.Key.name equals e2.name select e1.Value; //from p in IndexMap where nodes.Any(q => q.name == p.Key.name) select p.Value;

            List<float[]> ClusterAttrator = new List<float[]>();
            foreach (float[] state in netAttractor)
            {
                float[] ClusState = new float[clusterIndex.Count()];
                for (int j = 0; j < clusterIndex.Count(); j++)
                {
                    ClusState[j] = state[clusterIndex.ElementAt(j)];
                }
                ClusterAttrator.Add(ClusState);
            }

            return ClusterAttrator;
        }


        private bool IsConnect(BooleanNode a, BooleanNode b)
        {
            if (!(Nodes.Contains(a) && Nodes.Contains(b)))
            {
                throw new Exception("not exist");
            }

            if (a.SrcNodes.Contains(b) || a.DesNodes.Contains(b))
            {
                return true;
            }
            return false;
        }
        #region Read network file
       
        public static BooleanNetwork ReadSignalingNetworkFile(string filename)
        {
            if(!File.Exists(filename))
                filename =Netutil.InPutDirector + "\\" + filename;

            BooleanNetwork Net = new BooleanNetwork();
            Net.Name = filename;
            if (filename.Contains(".txt") || filename.Contains(".sif"))
            {
                Net.readTextFile(filename);
                
            }
            else if (filename.Contains(".xml"))
            {
                //Net.readFromGraphML(filename);

                //BasicNetwork net = new BasicNetwork();
                BasicNetwork.ReadNetworkFromKeggXML(Net, filename);

            }
            else
            {
                Net.readExcelFile(filename);
            }
            User.One.MessageToUser("Loaded network data from " + filename);
            return Net;
        }
        private bool readMatrixFile(string filename)
        {
            Dictionary<string, List<Pair<string, Quad<int, double, string, Interaction.DirectionType>>>> result = new Dictionary<string, List<Pair<string, Quad<int, double, string, Interaction.DirectionType>>>>();
            
            StreamReader file = new StreamReader(filename);
            string line;
            string[] token = null;
            try
            {

                
                string startMark = "matrix"; //startMakr ="matrix(i) where i is interaction type


                Interaction.DirectionType DirectionType = Interaction.DirectionType.undirected;
                const int nMaxHeaderLine = 3;
                int iLine = 0;

                //Detect header for maxtri data
                string[] Header = null;
                bool IsMatrix = false;
                while ((line = file.ReadLine()) != null)
                {
                    if (++iLine > nMaxHeaderLine)//greater than maximum lines for header, this is not matrix data
                        return false;

                    token = line.Split(new char[] { '\t' });

                    //------------Detect text file header
                    if (token.Length > 0 && token[0].ToLower().IndexOf(startMark) > -1)
                    {
                        IsMatrix = true;
                        string txtinteraction = token[0].Split(new char[] { '(', ')', '[', ']' })[1].Trim();// Extract direction in the marker of matrix, with format matrix(interaction)
                        DirectionType = Convert.ToInt32(txtinteraction) == 0 ? Interaction.DirectionType.undirected : Interaction.DirectionType.directed;

                        Header = new string[token.Length];
                        Header[0] = token[0].ToLower();
                        for (int i = 1; i < token.Length; i++)
                        {
                            Header[i] = token[i].ToLower();
                            result[token[i].ToLower()] = null;//Add isolate nodes
                        }
                        
                        break;
                        
                    }

                }
                if (!IsMatrix)
                    return false;

                while ((line = file.ReadLine()) != null)
                {
                    token = line.Split(new char[] { '\t' });


                    if (token == null) continue;


                    if (!result.ContainsKey(token[0].ToLower()))// an isolate node
                        result.Add(token[0].ToLower(), null);
                    if (token.Length > 1)
                        result[token[0].ToLower()] = new List<Pair<string, Quad<int, double, string, Interaction.DirectionType>>>();
                    for (int j = 1; j < token.Length; j++)
                    {
                        string weight = token[j].Trim();
                        if (weight == "") continue; // having no weight => no link
                        if (Convert.ToDouble(weight) == 0) continue; // having zero weight => two becoming one => no link
                        result[token[0].ToLower()].Add(new Pair<string, Quad<int, double, string, Interaction.DirectionType>>(Header[j], new Quad<int, double, string, Interaction.DirectionType>(0, Convert.ToDouble(weight), "",DirectionType)));
                    }

                }
                this.ImportUniqueArc(result);
                return true;
            }
            catch (Exception ex)
            {
                User.One.SendErrorToUser(ex);
                return false;
            }
            finally
            {
                file.Close();
            }   
        }
        private void readTextFile(string filename)
        {
            if (readMatrixFile(filename)) return;

            Dictionary<string, List<Pair<string, Quad<int, double, string, Interaction.DirectionType>>>> result = new Dictionary<string, List<Pair<string, Quad<int, double, string, Interaction.DirectionType>>>>();
            int srcIndex = -1, tarIndex = -1, typeIdx = -1, weightIdx = -1, edgeNameIdx=-1, directionIdx=-1;
            StreamReader file = new StreamReader(filename);
            string line;
            string[] token = null;
            try
            {
                string[] startHeader = { "start","from","source","src","vertex 1", "begin", "vertex1" };
                string[] endHeader = { "end","target", "destination","des","to", "vertex 2", "finish", "vertex2" };
                string[] interactionHeader = {"interaction", "type", "interaction type" };//1: activate, -1: inhibition, 0: neutral
                string[] weightHeader = { "weight"};
                string[] directionHeader = { "direction" };//1: directed 0: undirected
                string[] edgenameHeader = { "name", "edge name", "link name", "interaction name" };
                string[] startMark = startHeader.Union(endHeader).Union(interactionHeader).Union(weightHeader).Union(edgenameHeader).Union(directionHeader).ToArray();
                bool isDetectedHeader = false;
                String source = null, target = null;
                int interaction = 1;
                const int defaultInteraction=1;
                const Interaction.DirectionType defaultDirection = Interaction.DirectionType.undirected;
                double weight = 1.0;
                string name ="";
                Interaction.DirectionType direction = 0;
                const double defaultWeight=1.0;
                //Detect header
                while ((line = file.ReadLine()) != null)
                {

                    //token = line.Split(new char[] { ' ', ';', '\t' });
                    token = line.Split(new char[] { '\t' });

                    //------------Detect text file header
                    if (token.Length > 0 && startMark.Contains(token[0].ToLower()))
                    {
                        for (int i = 0; i < token.Length; i++)
                        {
                            if (startHeader.Contains(token[i].ToLower()))
                                srcIndex = i;
                            else if (endHeader.Contains(token[i].ToLower()))
                                tarIndex = i;
                            else if (interactionHeader.Contains(token[i].ToLower()))
                                typeIdx = i;
                            else if (weightHeader.Contains(token[i].ToLower()))
                                weightIdx = i;
                            else if (directionHeader.Contains(token[i].ToLower()))
                                directionIdx = i;
                            else if (edgenameHeader.Contains(token[i].ToLower()))
                                edgeNameIdx = i;
                            else if (i<3)
                                throw new Exception(string.Format("file \"{0}\" has an invalide header!", filename));
                        }
                        isDetectedHeader = true;
                        break;
                    }
                }
                if (!isDetectedHeader)
                    User.One.SendErrorToUser(new Exception("Cannot detect the header (start, end, type, weight, name) in file \"" + filename+"\""));
                while ((line = file.ReadLine()) != null)
                {
                    token = line.Split(new char[] { '\t' });
                    

                    if (token == null) continue;
                    else if (token.Length == 1) //1 node only
                    {
                        result.Add(token[0], null);
                        continue;
                    }
                    source = token[srcIndex].Trim();
                   
                    target = token[tarIndex].Trim();
                    if (source == "")
                        continue;
                    if (target == "") //source !="" and target=="" 1 node only
                    {
                        result.Add(source, null);
                        continue;
                    }
                    try
                    {
                        direction = directionIdx == -1 ? defaultDirection : (Convert.ToInt32(token[directionIdx].Trim()) == 0 ? Interaction.DirectionType.undirected : Interaction.DirectionType.directed);
                    }
                    catch
                    {
                        direction = defaultDirection;
                    }
                    try
                    {
                        interaction = typeIdx == -1 ? defaultInteraction : Convert.ToInt32(token[typeIdx].Trim());
                    }catch
                    {
                        interaction = defaultInteraction;
                    }
                    try
                    {
                        weight = weightIdx == -1 ? defaultWeight : Convert.ToDouble(token[weightIdx].Trim());
                    }
                    catch
                    {
                        weight = defaultWeight;
                    }
                    try
                    {
                        name = edgeNameIdx == -1 ? "" : token[edgeNameIdx].Trim();
                    }catch
                    {
                        name = "";
                    }
                    

                    if (!result.ContainsKey(source)) result[source] = new List<Pair<string, Quad<int, double, string, Interaction.DirectionType>>>();
                  
                    result[source].Add(new Pair<string, Quad<int, double, string, Interaction.DirectionType>>(target, new Quad<int, double, string, Interaction.DirectionType>
                        (interaction, weight, name, direction)));
                }
                this.Import(result);
            }finally
            {
                file.Close();
            }   
            //}
            //catch (Exception e)
            //{
            //    Debug.WriteLine("Exception while reading the graph for invalid data format:");
            //    Debug.WriteLine(e.Message);
                
            //}
        }
        /// <summary>
        /// Import a list of links to the network
        /// </summary>
        /// <param name="source">The list of links to import(start: string, end: string, type:int, weight:double, name: string)</param>

        public void Import(Dictionary<string, List<Pair<string, Quad<int, double, string,Interaction.DirectionType >>>> source)
        {
            Dictionary<string, List<Pair<string, Quad<int, double, string, Interaction.DirectionType>>>> g = source;
            foreach (string start in g.Keys)
            {
                if (g[start] == null)
                {
                    this.AddNode(start);
                }else if (g[start].Count == 0)
                {
                    this.AddNode(start);
                }
                else
                {
                    foreach (var end in g[start])
                    {


                        Node nodesrc = this.AddNode(start);
                        Node nodetar = this.AddNode(end.First);
                        

                        Interaction intract = null;
                        //int interactionType = Netutil.ConvertInteraction(end.Second.First);
                        int interactionType = Netutil.ConvertInteraction(end.Second.A);
                        //if (interactionType == InteractionType.NULL)
                            intract = new Interaction(nodesrc, nodetar, interactionType,end.Second.C, end.Second.B, end.Second.D);
                        //else
                        //    intract = new Interaction(nodesrc, nodetar, interactionType, end.Second.C, end.Second.B);
                        AddArc(intract);
                        
                    }
                }
            }
            User.One.MessageToUser(string.Format("A network with {0} nodes and {1} links was imported", Nodes.Count(), Arcs.Count()));
        }
        /// <summary>
        /// Import a list of links to the network so that there is only 1 arc between a node pair
        /// </summary>
        /// <param name="source">The list of links to import(start: string, end: string, type:int, weight:double</param>
        public void ImportUniqueArc(Dictionary<string, List<Pair<string, Quad<int, double, string, Interaction.DirectionType>>>> source)
        {
            Dictionary<string, List<Pair<string, Quad<int, double, string, Interaction.DirectionType>>>> g = source;
            foreach (string start in g.Keys)
            {
                if (g[start] == null)
                {
                    this.AddNode(start);
                }
                else if (g[start].Count == 0)
                {
                    this.AddNode(start);
                }else
                {
                    foreach (var end in g[start])
                    {


                        Node nodesrc = this.AddNode(start);
                        Node nodetar = this.AddNode(end.First);

                        Interaction intract = null;
                        int interactionType = Netutil.ConvertInteraction(end.Second.A);
                        
                        //if (interactionType == InteractionType.NULL)
                        //{

                        IEnumerable<Interaction> it = GetArcsBetween2Node(nodesrc, nodetar);
                        if (it.Count() > 0)
                        {
                            it.ElementAt(0).weight += end.Second.B;
                        }else
                        {
                            intract = new Interaction(nodesrc, nodetar, interactionType, end.Second.C,end.Second.B, end.Second.D);
                            AddArc(intract);
                        }

                        //}
                        //else
                        //{
                        //    IEnumerable<Interaction> it = GetArcsFromStartToEnd(nodesrc, nodetar);
                        //    if (it.Count() > 0)
                        //    {
                        //        it.ElementAt(0).weight += end.Second.B;
                        //    }
                        //    else
                        //    {
                        //        intract = new Interaction(nodesrc, nodetar, interactionType, end.Second.C,end.Second.B);
                        //        AddArc(intract);
                        //    }
                        //}

                    }
                }
            }
            User.One.MessageToUser(string.Format("A network with {0} nodes and {1} links was imported", Nodes.Count(), Arcs.Count()));
        }
       
        private void readExcelFile(string filename)
        {
            Dictionary<string, List<Pair<string, Quad<int, double, string, Interaction.DirectionType>>>> result = new Dictionary<string, List<Pair<string, Quad<int, double, string, Interaction.DirectionType>>>>();
            string[] headername = new string[6];
            headername[0] = "start";
            headername[1] = "end";
            headername[2] = "interaction";
            headername[3] = "weight";
            headername[4] = "name";
            headername[5] = "direction";
            
            ExcelDB file = new ExcelDB(headername);
            String source = null, target = null;
            int interaction = -2;
            double weight = 1;
            string name = "";
            Interaction.DirectionType direction = Interaction.DirectionType.undirected;
            try
            {
                file.ReadFile(filename);
                object[] row = null;
                int nrow = file.headerRowIndex + 1;
                while ((row = file.ReadRow(nrow++)) != null)
                {
                    if (row[0] == null) break;

                    source = Uti.CheckNull(row[file.srcIdx], "").ToString().Trim().ToLower();
                    target = Uti.CheckNull(row[file.tgIdx], "").ToString().Trim().ToLower();

                    if ((source == "" && target != "") || (source != "" && target == ""))// has only one column
                    {
                        source = (source != "" ? source : target);
                        result[source] = null;
                        continue;
                    }


                    interaction = int.Parse(row[file.edgIdx].ToString().Trim());
                    direction = (int.Parse(row[file.directionIdx].ToString().Trim())==0? Interaction.DirectionType.undirected:Interaction.DirectionType.directed);
                    weight = double.Parse(row[file.weightIdx].ToString().Trim());
                    name = Uti.CheckNull(row[file.nameIdx], "").ToString().Trim().ToLower();
                    if (!result.ContainsKey(source)) result[source] = new List<Pair<string, Quad<int, double, string, Interaction.DirectionType>>>();
                    result[source].Add(new Pair<string, Quad<int, double, string, Interaction.DirectionType>>(target,
                        new Quad<int, double, string, Interaction.DirectionType>(interaction, weight, name,direction)));
                }

                this.Import(result);
              
            }
            finally
            {
                file.Dispose();
            }

        }
        private void readFromGraphML(string xmlfile)
        {
            XmlTextReader reader = new XmlTextReader(xmlfile);
            using (reader)
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element: // The node is an element.
                            Debug.Write("<" + reader.Name);
                            Debug.WriteLine(">");
                            readXmlElement(reader);
                            break;
                        case XmlNodeType.Text: //Display the text in each element.
                            Debug.WriteLine(reader.Value);
                            break;
                        case XmlNodeType.EndElement: //Display the end of the element.
                            Debug.Write("</" + reader.Name);
                            Debug.WriteLine(">");
                            break;
                    }
                }
            }


        }
        private void readXmlElement(XmlTextReader reader)
        {
            if (reader.Name == "node")
            {
                string name = null;
                float state = 0.0f;
                FunctionType type = FunctionType.AND;
                while (reader.MoveToNextAttribute()) // Read the attributes.
                {
                    switch (reader.Name)
                    {
                        case "name":
                            name = reader.Value;
                            break;
                        case "state":
                            state = Convert.ToSingle(reader.Value);
                            break;
                        case "function":
                            type = reader.Value.ToUpper().Equals("AND") ? FunctionType.AND : FunctionType.OR;
                            break;
                    }
                }
                BooleanNode n = this.NewNode(name, type) as BooleanNode;
                n.ResetState(state);
                this.AddNode(n);
            }
            if (reader.Name == "edge")
            {
                string start = null, end = null;
                float weight = 0.0f;
                InteractionType type = InteractionType.NULL;
                string name = "";

                while (reader.MoveToNextAttribute()) // Read the attributes.
                {
                    switch (reader.Name)
                    {
                        case "type":
                            type = reader.Value.ToUpper().Equals("NEGATIVE") ? InteractionType.NEGATIVE : (reader.Value.ToUpper().Equals("POSITIVE") ? InteractionType.POSITIVE : InteractionType.NULL);
                            break;
                        case "weight":
                            weight = Convert.ToSingle(reader.Value);
                            break;
                        case "start":
                            start = reader.Value;
                            break;
                        case "end":
                            end = reader.Value;
                            break;
                        case "name":
                            name=reader.Value;
                            break;
                    }
                }
                BooleanNode nstart = null;
                BooleanNode nend = null;

                IEnumerable<BooleanNode> pNode = (from p in Nodes where p.name == start select p as BooleanNode);
                if (pNode.Count() == 0)
                {
                    nstart = this.NewNode(start, BooleanNode.ArbitraryFunctionType) as BooleanNode;
                    this.AddNode(nstart);
                }
                else
                    nstart = pNode.ElementAt(0);

                pNode = (from p in Nodes where p.name == end select p as BooleanNode);
                if (pNode.Count() == 0)
                {
                    nend = this.NewNode(end, BooleanNode.ArbitraryFunctionType) as BooleanNode;
                    this.AddNode(nend);
                }
                else
                    nend = pNode.ElementAt(0);
                if(type==InteractionType.NULL)
                    this.AddArc(new Interaction(nstart, nend, type, name, weight, Interaction.DirectionType.undirected));
                else
                    this.AddArc(new Interaction(nstart, nend, type, name, weight));

            }

        }
        public Dictionary<int, List<BooleanNode>> SelectNodeFromCluster(Dictionary<Node, int> pClustering)
        {
            Dictionary<int, List<BooleanNode>> clusterSummarized = new Dictionary<int, List<BooleanNode>>();

            //Afer groupping by ModuleID => a list of {(ModuleID, node total in the module)}
            var nodeOntheNet = from e1 in this.Nodes join e2 in pClustering on e1.name equals e2.Key.name select new {node=e1, ID=e2.Value};
            var moduleClustering = from e in nodeOntheNet group e by e.ID into g select g;
            foreach (var acluster in moduleClustering)
                foreach (var node in acluster)
                {
                    if (!clusterSummarized.ContainsKey(acluster.Key))
                    {
                        clusterSummarized.Add(acluster.Key, new List<BooleanNode>());
                    }
                    clusterSummarized[acluster.Key].Add(node.node as BooleanNode);
                }
            return clusterSummarized;
        }
        #endregion
        #endregion

        #region Competitive dynamics functions
        /// <summary>
        /// Generate a new array with random value in {-1,0,1};
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        protected static float[] Competition_InitState(IEnumerable<Node> nodes)
        {
            float[] ret = new float[nodes.Count()];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = Mathutil.NumericMath.RandomCraft.Next(-1, 2);// a random value in {-1, 0, 1}

            return ret;
        }
        
       
        /// <summary>
        /// Ranking nodes by their support for competitors in a network, in which competitor's states are normally indicated by the fixed state in {-1, 1}
        /// see paper "Competitive Dynamics on Complex Networks" (http://www.nature.com/srep/2014/140728/srep05858/full/srep05858.html)
        /// </summary>
        /// <param name="competitors">The competitor nodes whose state is set at a given values {-1, 1} in advance</param>
        /// <returns>
        /// 1- the return of function: The number of supporters the winner are higher than the loser (excluse the competitors); 
        /// the sign shows which competitor wins
        /// 2- the node's state in the network: Show the bias of normal agents</returns>
        public Dictionary<BooleanNode, List<BooleanNode>> Competition_Computing(IEnumerable<BooleanNode> competitors)
        {
            if (competitors == null || (competitors != null && competitors.Count() == 0))
            {
                User.One.SendErrorToUser(new Exception("The list of competitors in the network is null!"));
                return null;
            }
            IEnumerable<BooleanNode> normalAgents = Netutil.SubstractNodeListByID(this.Nodes, competitors);

            int nCount = 0;
            int nLoop = 0;
            IEnumerable<float>[] e = For.Spin_FLogic(Competition_InitState(normalAgents));
            float[] buffer = new float[e.Length];
            float[] sumAttractor = new float[e.Length];
            float E = 1.0f / this.MaxInDegMixing;
            IEnumerable<float[]> combinations = Enumerate<float>.Combination(buffer, e);

            //var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
            //Parallel.ForEach(combinations, options,
            //    (netState, s, x) =>
            //    {
            //        if (++nLoop > iMaxRobustnessLoop) { nLoop--; s.Stop(); }
            //        Netutil.SetNodeState(normalAgents, netState);


            //        List<float[]> att1 = Competition_FindNetworkAttractor(E, normalAgents);
            //        foreach (var state in att1)
            //        {
            //            for (int i = 0; i < state.Length; i++)
            //            {
            //                sumAttractor[i] += state[i];
            //            }
            //            nCount++;
            //        }
            //    });

            foreach (float[] netState in combinations)
            {
                if (++nLoop > iMaxRobustnessLoop) { nLoop--; break; }


                Netutil.SetNodeState(normalAgents, netState);


                List<float[]> att1 = Competition_FindNetworkAttractor(E, normalAgents);
                foreach (var state in att1)
                {
                    for (int i = 0; i < state.Length; i++)
                    {
                        sumAttractor[i] += state[i];
                    }
                    nCount++;
                }

            }
            for (int i = 0; i < sumAttractor.Length; i++)
            {
                sumAttractor[i] /= nCount;

            }

            Netutil.SetNodeState(normalAgents, sumAttractor);
            return Competition_GetSupporters(competitors, this.Nodes);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="L1">leader 1</param>
        /// <param name="R1">Reputation of leader 1| > 1</param>
        /// <param name="S1">Stubborness of leader 1</param>
        /// <param name="A1">Appeal of leader 1</param>
        /// <param name="E1">Extremeness of leader 1</param>
        /// <param name="L2">leader 2</param>
        /// <param name="R2">Reputation of leader 2| > 1</param>
        /// <param name="S2">Stubborness of leader 2</param>
        ///  <param name="A2">Appeal of leader 2</param>
        /// <param name="E2">Extremeness of leader 2</param>
        /// <param name="T">The bound of confidence</param>
        /// <returns></returns>
        public Dictionary<BooleanNode, List<BooleanNode>> Competition_Computing(string L1, float R1, float S1, float A1, float E1,
            string L2, float R2, float S2, float A2, float E2, float T)
        {


            BooleanNode leader1 = this[L1] as BooleanNode;
            BooleanNode leader2 = this[L2] as BooleanNode;
            IEnumerable<BooleanNode> competitors= new BooleanNode[] { leader1, leader2 };
            IEnumerable<BooleanNode> normalAgents = Netutil.SubstractNodeListByID(this.Nodes, competitors);


            int nCount = 0;
            int nLoop = 0;
            IEnumerable<float>[] e = For.Spin_FLogic(Competition_InitState(normalAgents));
            float[] buffer = new float[e.Length];
            float[] sumAttractor = new float[e.Length];
            float E = 1.0f / this.MaxInDegMixing;

            foreach (float[] netState in Enumerate<float>.Combination(buffer, e))
            {
                if (++nLoop > iMaxRobustnessLoop) { nLoop--; break; }


                Netutil.SetNodeState(normalAgents, netState);


                List<float[]> att1 = Competition_FindNetworkAttractor(E, normalAgents);
                foreach (var state in att1)
                {
                    for (int i = 0; i < state.Length; i++)
                    {
                        sumAttractor[i] += state[i];
                    }
                    nCount++;
                }

            }
            for (int i = 0; i < sumAttractor.Length; i++)
            {
                sumAttractor[i] /= nCount;

            }

            Netutil.SetNodeState(normalAgents, sumAttractor);
            return Competition_GetSupporters(competitors, this.Nodes);
        }
        /// <summary>
        /// Check if an attractor of the network convergent from a networkstate is chanced because of a node perturbed
        /// </summary>
        /// <param name="node">The node to get perturbed where node is the list of normalAgents</param>
        /// <param name="networkState">The network state for the attractor</param>
        /// <param name="normalAgents">The nodes whose state will converge to attractor</param>
        /// <param name="E">Epsilon for updating function</param>
        /// <returns>true: robust; otherwise not robust</returns>
        protected bool Competition_IsRobustNodeWithState(BooleanNode node, float[] networkState, float E)
        {

            
            Netutil.SetNodeState(this.Nodes, networkState);
            //node.ResetState(1);// perturbation

            List<float[]> att1 = Competition_FindNetworkAttractor(E,this.Nodes);

            Netutil.SetNodeState(this.Nodes, networkState);


            node.ResetState(-node.State);// perturbation

            List<float[]> att2 = Competition_FindNetworkAttractor(E, this.Nodes);
            

            return Netutil.IsEqualAttractors(att1, att2);
        }
        
      
        /// <summary>
        /// Compute the supporting value of each normal node to the leader node where the normal node is being driven by an outside competitor
        /// </summary>
        /// <param name="leader">The leader node</param>
        /// <returns></returns>
        public Dictionary<Node, float> Competition_SupportTheLeaderAgainstOutside(Node leader)
        {
            Dictionary<Node, float> loyalStates = new Dictionary<Node, float>();

            Node againstLeader = this.NewNode("#oppositeLeader#", null);
            this.AddNode(againstLeader);

            IEnumerable<Node> normalAgents = Netutil.SubstractNodeListByID(this.Nodes, new Node[] { againstLeader, leader });

            foreach (Node node in normalAgents)
            {
                //Attache a node whose state is always inverted with that of the leader (weight is the highest of 1)
                Interaction tempInt = new Interaction(againstLeader, node, 0, "", 1, Interaction.DirectionType.directed);
                
                againstLeader.AddArc(true, tempInt);
                
                Dictionary<Node, float> nodeStates = this.InsideCompetition2NodeSets(new string[] { leader.name }, new string[] { againstLeader.name });

                loyalStates.Add(node, nodeStates[node]);

                againstLeader.RemoveArc(tempInt);
                node.RemoveArc(tempInt);
            }

            this.RemoveNode(againstLeader);
            loyalStates.Add(leader, 1);//

            return loyalStates;
        }
        public Dictionary<Node, float> OutsideCompetition2Nodes(Node leader)
        {
            Dictionary<Node, float> loyalStates = new Dictionary<Node, float>();

            Node againstLeader = new Node("#oppositeLeader#");
            
            int maxIterations = Convert.ToInt32(Math.Round(Math.Pow(this.Nodes.Count() * this.Arcs.Count(), 1.0 / 1.001), 0));

            //const float Epsilon = float.Epsilon;
            float Epsilon = (this.Nodes.Count()<=100)? float.Epsilon: (this.Nodes.Count() <= 200) ? 1E-5f: (this.Nodes.Count() <= 300)? 1E-4f: (this.Nodes.Count() <= 400) ? 1E-3f: 1E-2f;
            float E = 1.0f / this.MaxOutDegMixing - float.Epsilon;

            Dictionary<Node, float> Xt = new Dictionary<Node, float>();
            Dictionary<Node, float> Xt_1 = new Dictionary<Node, float>();

            IEnumerable<Node> normalAgents = Netutil.SubstractNodeListByID(this.Nodes, new Node[] { againstLeader, leader });
            Interaction tempInt = new Interaction(againstLeader, new Node("No thing"), 0, "", 1, Interaction.DirectionType.directed, false);
            double error = 0;
            float r = 0;
            int t = 0;
            Dictionary<Node, float> temp = null;
            Node v = null;
            IEnumerable<Interaction> vInteraction = null;
            foreach (Node node in normalAgents)
            {
        
                foreach (Node n in this.Nodes)
                    Xt[n]= 0;
                Xt[againstLeader] = -1;//

                Xt[leader] = Xt_1[leader] = 1;
                Xt_1[againstLeader] = -1;

                //Attache a node whose state is always inverted with that of the leader (weight is the highest of 1)
                //tempInt = new Interaction(againstLeader, node, 0, "", 1, Interaction.DirectionType.directed, false);
                tempInt.endNode = node;
                error = 0;
                r = 0;
                t = 0;
                do
                {
                    error = 0;
                    foreach (KeyValuePair<Node, float> u in Xt)
                    {
                        if (leader.name==u.Key.name|| againstLeader.name==u.Key.name) continue;
                        r = 0;

                        vInteraction = u.Key.InUnLink;//Income interaction of u
                        if (u.Key == node)
                            vInteraction = vInteraction.Union(new Interaction[] { tempInt });

                        //foreach income interaction of u
                        foreach (Interaction e in vInteraction)
                        {
                            // not dau vao
                            v = e.GetPartnerVertex(u.Key);// foreach v is a neibour of u

                            
                            if (!Xt.ContainsKey(v)) continue;

                            r += ((float)(e.weight)) * (Xt[v] - u.Value); // trang thai cu cua not dau vao tru trang thai cua not hien thoi nhan voi trong so
                        }
                        Xt_1[u.Key] = u.Value + E * r;
                        error += Math.Abs(u.Value - Xt_1[u.Key]);
                    }

                    // swap ranks
                    temp = Xt;
                    Xt = Xt_1;
                    Xt_1 = temp;

                    t++;
                    //TextDB.WriteTextFile(string.Format("{0}\t{1}\t-\t{2}\t{3}", error, Epsilon, t, maxIterations),"Kiem tra hoi tu.txt");
                } while (error > Epsilon && t < maxIterations);
                loyalStates.Add(node, Xt[node]);
            }
            loyalStates.Add(leader, 1);//
            return loyalStates;
        }

        public Dictionary<Node, float> InsideCompetition2NodeSets(IEnumerable<string> leaderNames, IEnumerable<string> againstLeaderNames)
        {
            //const int maxIterations = 10000;
            //const double Epsilon = 2 * double.Epsilon;
            //float E = 1.0f / this.MaxOutDegMixing;
            //const int maxIterations = 1000;
            int maxIterations = Convert.ToInt32(Math.Round(Math.Pow(this.Nodes.Count() * this.Arcs.Count(), 1.0 / 1.001), 0));

            //const float Epsilon = float.Epsilon;
            const float Epsilon = 1E-4f;
            float E = 1.0f / this.MaxOutDegMixing - float.Epsilon;

            Dictionary<Node, float> Xt = new Dictionary<Node, float>();
            Dictionary<Node, float> Xt_1 = new Dictionary<Node, float>();

            foreach (Node n in this.Nodes)
                Xt.Add(n, 0);


            foreach (string leaderName in leaderNames)
            {
                Node leader = this[leaderName];
                Xt[leader] = Xt_1[leader] = 1;
            }

            foreach (string againstLeaderName in againstLeaderNames)
            {
                Node againstLeader = this[againstLeaderName];
                Xt[againstLeader] = Xt_1[againstLeader] = -1;
            }

            double error = 0;
            float r = 0;
            int t = 0;
            do
            {
                error = 0;
                
                foreach (KeyValuePair<Node, float> u in Xt)
                {
                    //Node u = de.Key;
                    //float rank = de.Value;

                    if (leaderNames.FirstOrDefault(name => name.Equals(u.Key.name)) != null || againstLeaderNames.FirstOrDefault(name => name.Equals(u.Key.name)) != null) continue;

                    r = 0;
                    IEnumerable<Interaction> vInteraction = u.Key.InUnLink;

                    // voi moi canh dau vao
                    foreach (Interaction e in vInteraction)
                    {
                        // not dau vao
                        Node v = e.GetPartnerVertex(u.Key);// foreach v is a neibour of u

                        //User.One.MessageToUser(neibourNode.name);
                        if (!Xt.ContainsKey(v)) continue;

                        r += ((float)(e.weight)) * (Xt[v] - u.Value); // trang thai cu cua not dau vao tru trang thai cua not hien thoi nhan voi trong so
                    }

                    //float newRank = u.Value + E * r;
                    //Xt_1[u.Key] = newRank;
                    
                    Xt_1[u.Key] = u.Value + E * r;

                    //error += Math.Abs(u.Value - newRank);
                    error += Math.Abs(u.Value - Xt_1[u.Key]);
                }

                // swap ranks
                Dictionary<Node, float> temp = Xt;
                Xt = Xt_1;
                Xt_1 = temp;

                t++;
                //TextDB.WriteTextFile(string.Format("{0}\t{1}\t-\t{2}\t{3}", error, Epsilon, t, maxIterations),"Kiem tra hoi tu.txt");
            } while (error > Epsilon && t < maxIterations);

            return Xt;
        }
       
        public class CompetitionWork
        {
            public BasicNet.Node node = null;
            public Dictionary<Node, float> result = null;
            public CompetitionWork(BasicNet.Node node, Dictionary<Node, float> result)
            {
                this.node = node;
                this.result = result;
            }

        }
        public Dictionary<Node, Dictionary<Node,float>> Competition_Totalsupport()
        {
            Dictionary<Node, Dictionary<Node, float>> result = new Dictionary<Node, Dictionary<Node, float>>();
            

            WorkManager<BooleanNetwork, CompetitionWork> threadManager = new WorkManager<BooleanNetwork, CompetitionWork>();

            threadManager.GlobalVariable = this;

            for (int i=0;i< this.Nodes.Count();i++)
                threadManager.AddWork(i, 
                    (Context, WorkID) => 
                    {
                        Context.LocalVarable[WorkID].result = Context.GlobalVariable.OutsideCompetition2Nodes(Context.LocalVarable[WorkID].node);
                    }, 
                    new CompetitionWork(this.Nodes.ElementAt(i),null), WorkMode.TaskSchedule);

            threadManager.Start();
            while(threadManager.TotalCompletedTask< threadManager.TotalTask)
                User.One.ShowWaitIndicator(threadManager.TotalCompletedTask, threadManager.TotalTask);

      
            threadManager.Wait4WorksDone();

            for (int i = 0; i < this.Nodes.Count(); i++)
                result.Add(threadManager.GetLocalVariable(i).node, threadManager.GetLocalVariable(i).result);
            

            return result;
        }

        public Dictionary<BooleanNode, Dictionary<BooleanNode, int>> Competition_Ranking()
        { 
            // [i,j] > 0; winer else loser
            int[,] result = new int[this.Nodes.Count(), this.Nodes.Count()];
            
           
            for(int i=0;i<Nodes.Count()-1;i++)
                for (int j = i + 1; j < Nodes.Count(); j++)
                { 
                    
                    BooleanNode PostiveCompetitor = Nodes.ElementAt(i) as BooleanNode;
                    PostiveCompetitor.ResetState(1);
                    BooleanNode NegagiveCompetitor = Nodes.ElementAt(j) as BooleanNode;
                    NegagiveCompetitor.ResetState(-1);
                    Dictionary<BooleanNode, List<BooleanNode>> supporters = Competition_Computing(new BooleanNode[]{PostiveCompetitor,NegagiveCompetitor});
                    int mark = 0;
                    
                    
                    if (supporters[PostiveCompetitor].Count() > supporters[NegagiveCompetitor].Count())
                    {
                        mark =supporters[PostiveCompetitor].Count() - supporters[NegagiveCompetitor].Count();
                        result[i, j] = mark;// i is the winner
                        result[j, i] = -mark;
                    }
                    else if (supporters[PostiveCompetitor].Count() < supporters[NegagiveCompetitor].Count())
                    {
                        mark = supporters[NegagiveCompetitor].Count() - supporters[PostiveCompetitor].Count();
                        
                        result[j, i] = mark; // j is the winer
                        result[i, j] = -mark;
                    }
                    else
                    {
                        result[i, j] = result[j, i] = 0;
                    }


                }
            //for (int i = 0; i < Nodes.Count(); i++)
            //    for (int j = 0; j < Nodes.Count(); j++)
            //    {
            //        Debug.WriteLine(string.Format("[{0} wins {1}] at {2}", Nodes.ElementAt(i).name, Nodes.ElementAt(j).name, result[i, j]));
            //    }
            Dictionary<BooleanNode, Dictionary<BooleanNode, int>> cen = new Dictionary<BooleanNode, Dictionary<BooleanNode, int>>();
            for (int i = 0; i < Nodes.Count(); i++)
            {
                cen[Nodes.ElementAt(i) as BooleanNode] = new Dictionary<BooleanNode,int>();
                for (int j = 0; j < Nodes.Count(); j++)
                {
                    cen[Nodes.ElementAt(i) as BooleanNode][Nodes.ElementAt(j) as BooleanNode]=result[i,j] ;
                }
            }
            return cen;
        }
        /// <summary>
        /// Get supporters based on the state bias of nodes towards {-1, 1}
        /// </summary>
        /// <param name="competitors">The list of competitors whose state must be -1 or 1</param>
        /// <param name="nodes">The normal nodes/agents for finding supporters</param>
        /// <returns>The competiors and their supporters</returns>
        protected Dictionary<BooleanNode, List<BooleanNode>> Competition_GetSupporters(IEnumerable<BooleanNode> competitors, IEnumerable<Node> nodes)
        {
            Dictionary<BooleanNode, List<BooleanNode>> supporters = new Dictionary<BooleanNode, List<BooleanNode>>();

            List<BooleanNode> dumpcompetitors = new List<BooleanNode>();
            dumpcompetitors.AddRange(competitors);

            // Add neutral nodes to the list
            BooleanNode neutralNode = this.NewNode("Neutral", null) as BooleanNode;
            neutralNode.ResetState(0);
            dumpcompetitors.Add(neutralNode);

            //end adding the neutral node

            foreach (var c in dumpcompetitors)
            {
                supporters.Add(c, new List<BooleanNode>());
                foreach (var n in nodes)
                {
                    if (c.State == Competition_Bias((n as BooleanNode).State))
                        supporters[c].Add(n as BooleanNode);
                }
            }

            return supporters;
        }
        /// <summary>
        /// Determine bias of a value whether it is close to which of {-1, 0, 1}
        /// </summary>
        /// <param name="x">the value to evaluate its bias</param>
        /// <returns>the bias of the value</returns>
        protected int Competition_Bias(float x)
        {
            if (Math.Abs(x) < Mathutil.NumericMath.zeroEpsionf)
                return 0;
            else if (x > 0)
                return 1;
            else
                return -1;
        }
        
        /// <summary>
        /// Find attractor of a list of nodes by iteratively updating of node's states
        /// </summary>
        /// <param name="E">Epsilon for updating function</param>
        /// <param name="normalAgents">The list of nodes need updating state</param>
        /// <returns>The attractor</returns>
        protected List<float[]> Competition_FindNetworkAttractor(float E, IEnumerable<Node> normalAgents)
        {
            var statesLists = new List<float[]> { Netutil.GetNodeState(normalAgents) };

            do
            {
                float[] st = Spin_GoToNextStates(normalAgents, E);

                for (int i = statesLists.Count - 1; i >= 0; i--)
                {
                    if (Netutil.IsEqualNetStatesParallel(st, statesLists[i]))
                        return statesLists.GetRange(i, statesLists.Count - i); // the network state at position i is the state, in the attractor, directly converged from CurrentStates 
                    // ( zero index in the return result)
                }
                statesLists.Add(st);

            } while (true);
        }

        protected List<float[]> Competition_FindNetworkAttractor2(float E, IEnumerable<Node> normalAgents)
        {
            float[] statesLists = Netutil.GetNodeState(normalAgents);

            const int maxIterations = 100;
            float tolerance = 0.01f;
            float error = 0;
            int iter = 0;

            do
            {
                error = 0;
                float[] st = Spin_GoToNextStates(normalAgents, E);

                for (int i = 0; i < statesLists.Length; i++)
                {
                    error += Math.Abs(statesLists[i] - st[i]);
                }

                statesLists = st;

                iter++;
            } while (error > tolerance && iter < maxIterations);

            return new List<float[]> { statesLists };
        }

        /// <summary>
        /// Calculate the optimum branch of network
        /// R. E. Tarjan, “Finding optimum branchings,” Networks, vol. 7, no. 1, pp. 25–35, 1977.
        /// </summary>
        /// <returns>The list of interactions
        /// </returns>
        public List<Interaction> FindOptimumBranchings()
        {
            List<Interaction> interactions = new List<Interaction>();

            // TODO: Convert to BooleanNetwork

            //int MAX = 123123;
            ////n: số đỉnh (bắt đầu từ 1 -> n)
            ////m: số cạnh (bắt đầu từ 0 -> m - 1)
            //int n = this.Nodes.Count(), m = 0;
            ////a -  b - c: from - to - weight
            ////s: Lưu thông tin về thành phân liên thông mạnh
            ////w: "                                     " yếu
            //int[] a, b, c, s, w;
            ////h: lưu các cạnh chứa rẽ nhánh tối ưu
            //HashSet<int> h;
            ////enter: chứa số thứ tự của cạnh kết nối với thành phần liên thông mạnh
            //int[] enter;
            ////br[i]: chứa số thứ tự các cạnh đi đến đỉnh i
            //HashSet<int>[] br;
            ////root: sẽ lưu trữ các thành phần gốc của G(H) mà sẽ chứa các cạnh kết nối có trọng số dương.
            ////out: lưu các cạnh loại bỏ để h trở thành rẽ nhánh tối ưu
            //HashSet<int> root, outt;

            //// init
            ////khởi tạo mảng và set
            //a = new int[MAX];
            //b = new int[MAX];
            //c = new int[MAX];
            //s = new int[MAX];
            //w = new int[MAX];
            //enter = new int[MAX];
            //h = new HashSet<int>();
            //br = new HashSet<int>[MAX];
            //for (int i = 0; i < MAX; i++)
            //{
            //    br[i] = new HashSet<int>();
            //}
            //root = new HashSet<int>();
            //outt = new HashSet<int>();

            //foreach (var edge in this.Edges)
            //{
            //    a[m] = edge.startNode.id;
            //    b[m] = edge.endNode.id;
            //    c[m++] = (int) edge.weight;
            //}

            ////Khời tạo giá trị ban đầu
            //for (int i = 1; i <= n; i++)
            //{
            //    w[i] = s[i] = enter[i] = -1;
            //    root.Add(i);
            //}
            //for (int i = 0; i < m; i++)
            //{
            //    br[b[i]].Add(i);
            //}

            //// solve
            //while (root.Count != 0)
            //{
            //    //G1: Chọn một thành phần gốc S của G(H) có một cạnh chưa xét (x,v) với v ∈ S và c(x, v) > 0
            //    //(tạm thời chưa kiểm tra trọng số ở đây).
            //    int t = root.ElementAt(0);
            //    root.Remove(t);
            //    t = get(t, s);
            //    //G2: Tìm ra cạnh chưa xét(u, v) mà có trọng số lớn nhất trong số đó thỏa mãn v ∈ S.
            //    int cur = -1;
            //    //br[t] sẽ chứa STT các cạnh đi đến T
            //    foreach (int i in br[t])
            //    {
            //        if (cur == -1)
            //        {
            //            cur = i;
            //        }
            //        else if (c[cur] < c[i])
            //        {
            //            cur = i;
            //        }
            //    }
            //    //cur == -1 tức là br[t] rỗng => bỏ qua
            //    if (cur == -1)
            //    {
            //        continue;
            //    }
            //    br[t].Remove(cur);
            //    //Nếu trọng số âm thì bỏ qua
            //    if (c[cur] <= 0)
            //    {
            //        continue;
            //    }
            //    //G3: Nếu u ∈ S, loại bỏ cạnh đó .Còn không chuyển đến G4.
            //    else if (get(a[cur], s) == get(t, s))
            //    {
            //        root.Add(t);
            //        continue;
            //    }
            //    else
            //    {
            //        /*
            //        G4: u ∉ S.Giả sử W là thành phần liên thông yếu của G(H) chứa v. Nếu u ∉ W
            //        thêm (u, v) vào H và dừng lại.Còn không chuyển đến G5.
            //        */

            //        //Thêm (u,v) vào h
            //        h.Add(cur);
            //        if (get(a[cur], w) != get(b[cur], w))
            //        {
            //            merge(b[cur], a[cur], w);
            //            enter[t] = cur;
            //        }
            //        else
            //        {
            //            /*
            //             G5: u ∉ S, u ∈ W. Tìm chuỗi S1, (x1, y1), S2, (x2, y2),....,Sk,(xk,yk) sao cho mỗi Si là
            //                một thành phần liên kết mạnh của G(H), (xi,yi) ∈ H, yi ∈ Si, 
            //                và xi ∈ Si+1 với mọi i, Sk = S, (xk,yk) = (u,v) và xk ∈ S1. 
            //             G6: Tìm cạnh (xj, yj) có trọng số nhỏ nhất trong số (xi, yi)
            //             * */

            //            //val dùng để lưu trọng số nhỏ nhất trong số (xi,yi) 
            //            int val = (int)1e9;

            //            //temp dùng làm biến duyệt các cạnh, đi từ (x1,y1),...,(xk,yk) thông qua mảng enter
            //            int temp = cur;

            //            //STT cạnh có trọng số = val, hay là cạnh sẽ loại bỏ về sau
            //            int pos = -1;

            //            //vòng lặp này sẽ tìm cạnh có trọng số nhỏ nhất trong số (xi,yi)
            //            while (temp != -1)
            //            {
            //                if (c[temp] < val)
            //                {
            //                    pos = temp;
            //                    val = c[temp];
            //                }
            //                temp = enter[get(a[temp], s)];
            //            }

            //            /*
            //            G7: Với mỗi cạnh chưa xét(x, y) mà y ∈ Si, thay đổi giá trị trọng số cạnh đó như sau:
	           //                 c(x, y) := c(x, y) – c(xi, yi) + c(xj, yj).
            //            G8: Thêm cạnh (u, v) vào H. (đã thêm từ trên)                  
            //            */
            //            foreach (int i in br[t])
            //            {
            //                c[i] += val - c[cur];
            //            }

            //            //Điều này sẽ kết nối S1,…., Sk thành một thành phần liên thông mạnh đồng thời cũng là thành phần góc của G(H).
            //            //Do đó ta coi chúng là chỉ 1 thành phần liên thông, vì vậy, những cạnh đi đến các thành phân lt con kia từ bây h sẽ coi như là đi đến cha
            //            temp = enter[get(a[cur], s)];
            //            while (temp != -1)
            //            {
            //                int child = get(b[temp], s);
            //                //Thêm các cạnh đi đến từ thành phần lt con vào thành phần lt cha
            //                foreach (int i in br[child])
            //                {
            //                    c[i] += val - c[temp];
            //                    br[t].Add(i);
            //                }
            //                br[child].Clear();
            //                merge(t, get(b[temp], s), s);
            //                enter[child] = -1;
            //                temp = enter[get(a[temp], s)];
            //            }
            //            //Thêm pos vào outt để sau này loại (R3,R4)
            //            outt.Add(pos);
            //            root.Add(t);
            //        }
            //    }
            //}

            //var edges = this.Edges.ToArray();

            //foreach (int i in h)
            //{
            //    if (outt.Contains(i))
            //    {
            //        continue;
            //    }
            //    //Console.WriteLine(a[i] + " " + b[i] + " " + c[i]);
            //    interactions.Add(edges[i]);
            //}

            return interactions;
        }
        //get: trả về gốc của thành phần cần tìm
        int get(int x, int[] f)
        {
            return f[x] < 0 ? x : f[x] = get(f[x], f);
        }
        //merge: Kết hợp 2 thành phần với nhau (y nối vào x)
        void merge(int x, int y, int[] f)
        {
            x = get(x, f);
            y = get(y, f);
            if (f[x] > f[y])
            {
                Swap<int>(ref x, ref y);
            }
            f[x] += f[y];
            f[y] = x;
        }
        int max(int a, int b)
        {
            return a > b ? a : b;
        }
        void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }


        #endregion

        #region Epidemic and rumor spreadings
        /// <summary>
        /// Determine the infectiuos nodes and their average time to be infected
        /// </summary>
        /// <param name="infectedNodes">A list of initially infected nodes</param>
        /// <param name="infectedRate">The infectious rate a node transmit the disease to its neighbour</param>
        /// <param name="recoveredRate"></param>
        /// <param name="T">Total times for sampling</param>
        /// <returns>The node and its avergage time to get infectious</returns>
        public double EpidemicBySIR_Calculation(IEnumerable<Node> infectedNodes, float infectedRate = -1, float recoveredRate = 1, int T = 100)
        {
            int i = 0;//elapsed time is indicated by i

            const int susceptibleState = -1, inflectiousState = 0, recoveredState = 1;
            
            double seeds = Mathutil.NumericMath.RandomCraft.NextDouble();




            //Real infectedRate for default case
            if (infectedRate == -1)
            {
                infectedRate = this.epidemicThreshold + 0.1f;
            }


            Accumulator accRecoveredRate = new Accumulator();

            while (i++ < T)// foreach t in T samples
            {

                //Initialize all nodes with susceptible state
                foreach (BooleanNode n in this.Nodes)
                    n.ResetState(susceptibleState);

                //Initialize infectious nodes where the infectedNodes are in the network
                foreach (BooleanNode n in infectedNodes)
                    n.ResetState(inflectiousState);

               
                var inflectedNodes = from p in this.Nodes where (p as BooleanNode).State == inflectiousState select p;
                while (inflectedNodes.Count() > 0)
                {
                    foreach (BooleanNode n in inflectedNodes)
                    {
                        foreach (BooleanNode neighbor in n.OutUnNeighbours)
                        {
                            seeds = Mathutil.NumericMath.RandomCraft.NextDouble();
                            if (seeds <= infectedRate && neighbor.State == susceptibleState)
                                neighbor.ResetState(n.State);

                        }
                        seeds = Mathutil.NumericMath.RandomCraft.NextDouble();
                        if (seeds <= recoveredRate) // An infectious node recovers with probability of m
                            n.ResetState(recoveredState);

                    }
                }
               

                int nRecoveredNode = (from p in this.Nodes where (p as BooleanNode).State == recoveredState select p).Count();
                accRecoveredRate.Add((double)nRecoveredNode / this.Nodes.Count());
            }

            return accRecoveredRate.Mean;
        }
       
        public Dictionary<Node, float> EpidemicBySIR_Centrality(float infectedRate = -1, float recoveredRate = 1, int T = 100)
        {

            Dictionary<Node, float> centrality = new Dictionary<Node, float>();
            foreach (Node n in this.Nodes)
            {
               double result = EpidemicBySIR_Calculation(new Node[] { n }, infectedRate, recoveredRate, T);
               centrality[n] = Convert.ToSingle(result);
            }
            return centrality;
        }

        /// <summary>
        /// Centrality of node by rumor spreading where high centrality node is the node can create a high proportion of stifling nodes
        /// </summary>
        /// <param name="processMode">Indicate how spreaders contact with their neighbor: 
        /// + contactProcess (processMode=0): only one random neighbor of a spreader is contacted at each time step.
        /// + truncatedProcess (processMode = other values): the neighbors of a spreader are contacted in a randomway until all of them are contacted or the spreader turns into a stifler</param>
        /// <param name="speadingRate">the proportion the rumor can spread</param>
        /// <param name="stiflingRate">the proportion a spreader becomes a stifler</param>
        /// <param name="T">The number of rumor simulation on each node</param>
        /// <returns></returns>
        public Dictionary<Node, float> RumorSpeader_Centrality(int processMode = 1, float speadingRate = -1, float stiflingRate = 1, int T = 100)
        {

            Dictionary<Node, float> centrality = new Dictionary<Node, float>();
            foreach (Node n in this.Nodes)
            {
                double result = RumorSpeader_Calculation(new Node[] { n }, processMode, speadingRate, stiflingRate, T);
                centrality[n] = Convert.ToSingle(result);
            }
            return centrality;
        }
        public float epidemicThreshold
        {
            get 
            {
                float avgDeg = Convert.ToSingle((from e in Nodes select e.TotalDegree).Average());
                float avgDeg2 = Convert.ToSingle((from e in Nodes select e.TotalDegree * e.TotalDegree).Average());
                return avgDeg / avgDeg2;
            }
        }
        /// <summary>
        /// Calculate the proportion of stifling nodes in rumor speading model in an UNDIRECTED network
        /// </summary>
        /// <param name="speaders">The spreaders where are the sources of the rumor and spreaders are NOT ISOLATE nodes</param>
        /// <param name="speadingRate">spreading rate/probability where a speader transmits the rumor to its neighbor</param>
        /// <param name="stiflingRate">stifling rate/probability where a speader becomes a stifler when contacting with its neighbor</param>
        /// <param name="T">Total times for sampling</param>
        /// <param name="processMode">Indicate how spreaders contact with their neighbor: 
        /// + contactProcess (processMode=0): only one random neighbor of a spreader is contacted at each time step.
        /// + truncatedProcess (processMode = other values): the neighbors of a spreader are contacted in a randomway until all of them are contacted or the spreader turns into a stifler</param>
        /// <returns></returns>
        public double RumorSpeader_Calculation(IEnumerable<Node> speaders, int processMode=0, float speadingRate = -1, float stiflingRate = 1, int T = 100)
        {
            const int contactProcess = 0;// otherwise: truncatedProcess = 1;

            foreach (BooleanNode n in speaders)//exclude the case the speaders are isolate nodes
            {
                if (n.Neighbours.Count()==0)
                    return 0;
            }
            int i = 0;//elapsed time is indicated by i

            const int ignorantState = -1, spreadingState = 0, stiflingState = 1;

            double seeds = Mathutil.NumericMath.RandomCraft.NextDouble();

            if (speadingRate == -1)
                speadingRate = this.epidemicThreshold + 0.1f;


            Accumulator acStiflingRate = new Accumulator();

            while (i++ < T)// foreach t in T samples
            {

                //Initialize all nodes with susceptible state
                foreach (BooleanNode n in this.Nodes)
                    n.ResetState(ignorantState);

                //Initialize infectious nodes where the infectedNodes are in the network
                foreach (BooleanNode n in speaders)
                    n.ResetState(spreadingState);


                var speadingNodes = from p in this.Nodes where (p as BooleanNode).State == spreadingState select p;
                while (speadingNodes.Count() > 0)
                {
                    IEnumerable<Node> speaderList = Netutil.Shuffle<Node>(speadingNodes);
                    foreach (BooleanNode spreader in speaderList)
                    {
                        if (processMode == contactProcess)
                        {
                            BooleanNode neighbor = spreader.Neighbours.ElementAt(Mathutil.NumericMath.RandomCraft.Next(spreader.Neighbours.Count())) as BooleanNode;// contacts with its neighbor not considering the direction

                            
                            if (neighbor.State == ignorantState)
                            {
                                seeds = Mathutil.NumericMath.RandomCraft.NextDouble();
                                if (seeds <= speadingRate)
                                    neighbor.ResetState(spreader.State);//neighbor becomes a speader
                            }
                            //if a speader contacts with a neighbor who is a speader or stifler, it and their neighbor will become stiflers at probability of stiflingRate
                            else if (neighbor.State == spreadingState || neighbor.State == stiflingState)
                            {
                                seeds = Mathutil.NumericMath.RandomCraft.NextDouble();
                                if (seeds <= stiflingRate) // the spreader stop speading rumor at probability of stiflingState
                                    spreader.ResetState(stiflingState);

                                
                            }

                        }
                        else //processMode == truncatedProcess
                        {
                            Node[] neighbors = Netutil.Shuffle<Node>(spreader.Neighbours);// shuffling the neighbour
                            foreach (BooleanNode neighbor in neighbors)// contacts with its neighbor not considering the direction
                            {
                                
                                if (neighbor.State == ignorantState)
                                {
                                    seeds = Mathutil.NumericMath.RandomCraft.NextDouble();
                                    if(seeds <= speadingRate)
                                        neighbor.ResetState(spreader.State);//neighbor becomes a speader
                                }
                                //if a speader contacts with a neighbor who is a speader or stifler, it will become a stifler at probability of stiflingRate
                                else if (neighbor.State == spreadingState || neighbor.State == stiflingState)
                                {
                                    seeds = Mathutil.NumericMath.RandomCraft.NextDouble();
                                    if (seeds <= stiflingRate)
                                    {
                                        spreader.ResetState(stiflingState);// spreader stop spreading rumor
                                        break;
                                    }
                                }
                            }
                        }
                        

                    }
                }


                int nStiflingNode = (from p in this.Nodes where (p as BooleanNode).State == stiflingState select p).Count();
                acStiflingRate.Add((double)nStiflingNode / this.Nodes.Count());
            }

            return acStiflingRate.Mean;
        }
        #endregion
    }
}
