using System;
using System.Collections.Generic;
using System.Linq;
using NetworkRobustness.Elements;
using ComplexNetGeneratorLib;
namespace NetworkRobustness.Elements
{
    public abstract class BasicNetwork
    {
        //private List<Node> _nodes;
        //private List<Interaction> _interactions;

//         private const int NumberOfAttractor = 100;
//         private const int RobustnessStandard = 70;
        

        public abstract List<Node> Nodes { get; }

        public abstract List<Interaction> Interactions { get; }

       
        /*
        public void GetAllElements(ref List<Node> nodes, ref List<Interaction> interactions)
        {
            nodes = Nodes;
            interactions = Interactions;
        }
         */
        #region Compute Robustness
        private List<bool> CurrentStates
        {
            get
            {
                List<bool> currentStates = new List<bool>();
                foreach (Node node in Nodes)
                {
                    currentStates.Add(node.States[node.States.Count - 1]);
                }
                return currentStates;
            }

        }
        /// <summary>
        /// Tính Robustness của một mạng
        /// </summary>
        /// <returns></returns>
        public float Robustness()
        {
            float rbn = 0;
            const int iLoop = 100;
            foreach (Node node in Nodes)
            {
                int iNodeRbnCount = 0;
                for (int i = 0; i < iLoop; i++)
                {
                    List<bool> initState = ResetStates(); 

                    List<List<bool>> att1 = FindAttractor();

                    SetStates(initState);

                    node.States[node.States.Count - 1] = node.States[node.States.Count - 1] ? false : true;
                    List<List<bool>> att2 = FindAttractor();
                    if (NetworkUtil.CompareAttractor(att1, att2)) iNodeRbnCount++;
                }

                float nodeRobustness = (float) iNodeRbnCount/iLoop;

                rbn += nodeRobustness;
            }

            float rt = rbn / Nodes.Count;
            return rt;

            
        }

        /// <summary>
        /// Tạo ra trạng thái tiếp theo của cả mạng
        /// </summary>
        /// <returns>
        /// Trả về trạng thái hiện tại (trạng thái cuối cùng)
        /// </returns>
        private List<bool> NextStates()
        {
            foreach (Node node in Nodes)
            {
                node.UpdateFuction();
            }
            return CurrentStates;
        }

        /// <summary>
        /// Khởi tạo một trạng thái mới hoàn toàn
        /// </summary>
        /// <returns></returns>
        private List<bool> ResetStates()
        {
            foreach (Node node in Nodes)
            {
                node.ResetState();
            }
            return CurrentStates;
        }

        private void SetStates(List<bool> states)
        {
            for (int i = 0; i < states.Count; i++)
            {
                Nodes[i].States = new List<bool> { states[i] };
            }
        }

        /// <summary>
        /// Tìm Attractor của mạng
        /// </summary>
        /// <returns></returns>
        private List<List<bool>> FindAttractor()
        {
            var statesLists = new List<List<bool>> { CurrentStates };
            do
            {
                List<bool> st = NextStates();

                for (int i = statesLists.Count - 1; i >= 0; i--)
                {
                    if (NetworkUtil.CompareState(st, statesLists[i]))
                    {
                        return statesLists.GetRange(i, statesLists.Count - i);
                    }
                }
                statesLists.Add(st);

            } while (true);
        }

        private bool IsConnect(Node a, Node b)
        {
            if (!(Nodes.Contains(a) && Nodes.Contains(b)))
            {
                throw new Exception("not exist");
            }

            if (a.GetSrcNodes().Contains(b) || a.GetDesNodes().Contains(b))
            {
                return true;
            }
            return false;
        }

        public void FixInteraction(int maxInteractions)
        {

            Random rd = new Random((int)DateTime.Now.Ticks);

            while (Interactions.Count > maxInteractions)
            {
                int iItr = rd.Next(0, Interactions.Count);
                Interactions[iItr].Dispose();

                Interactions.RemoveAt(iItr);
            }

            while (Interactions.Count < maxInteractions)
            {
                int iSrc = rd.Next(0, Nodes.Count);
                int iDsc = rd.Next(0, Nodes.Count);
                if (iSrc == iDsc)
                {
                    continue;
                }

                if (!IsConnect(Nodes[iSrc], Nodes[iDsc]))
                {
                    Interactions.Add(new Interaction(Nodes[iSrc], Nodes[iDsc], InteractionType.POSITIVE));
                }
            }
        }
        #endregion

        #region Centrality
        public float Centrality()
        {

            int nodeCount = Nodes.Count;
            foreach (Node node in Nodes)//tính Cd từng đỉnh
            {
                node.Cd = (float)(node.InLink.Count + node.OutLink.Count) / (nodeCount - 1);
            }

            float maxCd = 0;
            foreach (Node node in Nodes) //lấy ra max Cd
            {
                if (node.Cd > maxCd)
                {
                    maxCd = node.Cd;
                }
            }

            float CdG = Nodes.Sum(node => (maxCd - node.Cd) / ((nodeCount - 1) * (nodeCount - 2)));

            return CdG;
        }
        #endregion

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
        public void WriteGraph(string filename)
        {
            foreach (Interaction edge in this.Interactions)
            {
                TextDB.WriteTextFile(new string[] { edge.startNode.name, edge.endNode.name, edge.weight.ToString() }, filename);
            }
        }
        #region Modularity calculation
        public double modularity(ref Dictionary<Node, int> Cluster)
        {
            GraphBase graph = GraphBase.Convert(Interactions);
            double modularity = 0.0;
            Cluster = GraphBase.ClusterGraph(graph, ref modularity);
            return modularity;
        }
        #endregion

    }
}