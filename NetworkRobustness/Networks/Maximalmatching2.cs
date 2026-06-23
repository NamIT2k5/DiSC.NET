using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
namespace BasicNet
{
    /// <summary>
    /// Find maximal matching by Hopcroft–Karp algorithm, see http://en.wikipedia.org/wiki/Hopcroft%E2%80%93Karp_algorithm
    /// </summary>
    public class Maximalmatching
    {
        private IEnumerable<Node> G1 = null, G2 = null;
        private HashSet<Node> G = new HashSet<Node>();
        private Dictionary<Node, float> Dist = new Dictionary<Node, float>();
        public Dictionary<Node, Node> Pair_G1 = new Dictionary<Node, Node>();
        public  Dictionary<Node, Node> Pair_G2 = new Dictionary<Node, Node>();
        private Node NULL = new Node("null");
        public int nMatchingEdges = 0;
        public Maximalmatching(IEnumerable<Node> L, IEnumerable<Node> R)
        {
            G.UnionWith(L);
            G.UnionWith(R);
            this.G1 = L;
            this.G2 = R;
            nMatchingEdges = Hopcroft_Karp();
        }
        bool BFS()
        {
            Queue<Node> Q = new Queue<Node>();
            foreach (Node v in G1)
                if (Pair_G1[v] == NULL)
                {
                    Dist[v] = 0;
                    Q.Enqueue(v);
                }
                else
                    Dist[v] = float.PositiveInfinity;
            Dist[NULL] = float.PositiveInfinity;
            while (Q.Count > 0)
            {
                Node v = Q.Dequeue();
                foreach (Node u in v.Neighbours)
                //foreach (Node u in v.OutNeighbours)
                    if (Dist[Pair_G2[u]] == float.PositiveInfinity)
                    {
                        Dist[Pair_G2[u]] = Dist[v] + 1;
                        Q.Enqueue(Pair_G2[u]);
                    }
            }
            return Dist[NULL] != float.PositiveInfinity;
        }
        bool DFS(Node v)
        {
            if (v != NULL)
            {
                foreach (Node u in v.Neighbours)
                //foreach (Node u in v.OutNeighbours)
                    if (Dist[Pair_G2[u]] == Dist[v] + 1)
                        if (DFS(Pair_G2[u]) == true)
                        {
                            Pair_G2[u] = v;
                            Pair_G1[v] = u;
                            return true;
                        }
                Dist[v] = float.PositiveInfinity;
                return false;
            }
            return true;
        }
        private int Hopcroft_Karp()
        {
            foreach(Node v in G)
            {
                Pair_G1[v] = NULL;
                Pair_G2[v] = NULL;
            }
            int matching = 0;
            while (BFS() == true)
                foreach (Node v in G1)
                    if (Pair_G1[v] == NULL)
                        if (DFS(v) == true)
                            matching = matching + 1;
            //DumpPairs(Pair_G1);
            //DumpPairs(Pair_G2);
            return matching;
        }
        public IEnumerable<KeyValuePair<Node, Node>> MatchingEdges
        {
            get
            {
                var matching= from e in Pair_G1 where !(e.Key.name == "null" || e.Value.name == "null") select e;
                foreach(var p in matching)
                    yield return p;
            }
        }
        public bool IsMathchingEdge(Interaction arc)
        {
            foreach (var e in MatchingEdges)
            {
                if ((e.Key.name == arc.startNode.name && e.Value.name == arc.endNode.name) ||
                    (e.Key.name == arc.endNode.name && e.Value.name == arc.startNode.name))
                    return true;
            }
            return false;
        }
        private void DumpPairs(Dictionary<Node, Node> G)
        {
            Debug.WriteLine("---------");
            foreach(var e in G)
            {
                Debug.WriteLine(string.Format("{0}\t{1}",e.Key.name, e.Value.name));
            }

        }

    }
}
