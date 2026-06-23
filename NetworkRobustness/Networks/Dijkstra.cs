using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace BasicNet
{
    class Dijkstra
    {
        /* Takes adjacency matrix in the following format, for a directed graph (2-D array)
                 * Ex. node 1 to 3 is accessible at a cost of 4
                 *        0  1  2  3  4
                 *   0  { 0, 2, 5, 0, 0},
                 *   1  { 0, 0, 0, 4, 0},
                 *   2  { 0, 6, 0, 0, 8},
                 *   3  { 0, 0, 0, 0, 9},
                 *   4  { 0, 0, 0, 0, 0}
                 */
       
        #region Common members
        private Dictionary<int,int> previous=new Dictionary<int,int>();
        public Dictionary<int, int> Previous
        {
            get { return previous; }
            private set { previous = value; }
        }
        /// <summary>
        /// Get the sortest path from the start to the target vertex (including the start and the target)
        /// </summary>
        /// <param name="target">the target vertex</param>
        /// <returns>
        /// if null, there is no path between the start to the target
        /// else an array contains the vertices from the start (index 0) to the target (including both vertices)</returns>
        public int[] GetSortestPathToTarget(int target)
        {
            if (!Previous.ContainsKey(target))
                return null;
            Queue<int> S = new Queue<int>();
            
            int idx = target; ;
            while (Previous[idx] != -1)
            {
                S.Enqueue(idx);
                idx = Previous[idx];
            }
            S.Enqueue(idx);
            return S.Reverse().ToArray();
        }
        //If the distance between two node A and B is an unreachable distance, there is no path from A to B
        public const float UnreachableDistance = float.MaxValue;
        /* Holds queue for the nodes to be evaluated */
        private readonly List<int> queue = new List<int>();

       
        #endregion

        #region function for big network

        /* Sets up initial settings */
        //private void Initialize(int start, int len)
        //{
        //    Distance.Clear();
        //    Previous.Clear();
        //    queue.Clear();

        //    /* Set distance to all nodes to infinity - alternatively use Int.MaxValue for use of Int type instead */
        //    for (int i = 0; i < len; i++)
        //    {
        //        queue.Add(i);
        //    }

        //    /* Set distance to 0 for starting point and the previous node to null (-1) */
        //    Distance[start] = 0;
        //    Previous[start] = -1;
        //}

        private void Initialize(Dictionary<int, Dictionary<int, double>> graph, int start)
        {
            Distance.Clear();
            Previous.Clear();
            queue.Clear();

            /* Set distance to all nodes to infinity - alternatively use Int.MaxValue for use of Int type instead */
            //for (int i = 0; i < len; i++)
            foreach(int i in graph.Keys)
            {
                queue.Add(i);
            }

            /* Set distance to 0 for starting point and the previous node to null (-1) */
            Distance[start] = 0;
            Previous[start] = -1;
        }

        private Dictionary<int, double> distance = new Dictionary<int, double>();
        /// <summary>
        /// The shortest distances from the start vertex to every vertices
        /// Distance.Keys: The index of the vertex
        /// Distance.Values: The distance to the vertex (key)
        /// </summary>
        public Dictionary<int, double> Distance
        {
            get
            {
                return distance;
            }
            
        }

        /* Retrives next node to evaluate from the queue */
        private int GetNextVertex()
        {
            double min = double.PositiveInfinity;
            int vertex = -1;

            /* Search through queue to find the next node having the smallest distance */
            foreach (int j in queue)
            {
                if (getDistance(j) <= min)
                {
                    min = getDistance(j);
                    vertex = j;
                }
            }

            queue.Remove(vertex);

            return vertex;
        }
        public int[] GetClosestVertex()
        {
            var closestDis=(from e in Distance where e.Value>0 select e);
            if (closestDis.Count() == 0) return null;

            var closestVertices = from p in Distance where p.Value == closestDis.Min(t => t.Value) select p.Key;
            if (closestVertices.Count() == 0)
                return null;
            else if (closestVertices.ElementAt(0) == Dijkstra.UnreachableDistance)
                return null;
            else
                return closestVertices.ToArray();

        }
        /// <summary>
        /// Find the shortest paths from a given start node
        /// </summary>
        /// <param name="graph">The adjacency list (zero-based index) that is composed of tuples (start, end, weight)</param>
        /// <param name="start">The index of the start node</param>
        /// <returns>
        /// 1. The shortest distances from vertices to the start in property Distance
        /// 2. The shortest paths in property Previous
        /// </returns>
        public void FindShortestPathAndDistance(Dictionary<int, Dictionary<int, double>> graph, int start)
        {
           
            /* Check graph format and that the graph actually contains something */
            if (graph.Keys.Count < 1)
            {
                return;
                //throw new ArgumentException("Graph error, wrong format or no nodes to compute");
            }

            //int len = graph.Keys.Count;

            //Initialize(start, len);
            Initialize(graph,start);
            
            if (!graph.ContainsKey(start))
                return;

            while (queue.Count > 0)
            {
                int u = GetNextVertex();

                /* Find the nodes that u connects to and perform relax */
                //for (int v = 0; v < graph[u].Keys.Count; v++)
                foreach(int v in graph[u].Keys)
                {
                    /* Checks for edges with negative weight */
                    if (graph[u][v] < 0)
                    {
                        throw new ArgumentException("Graph contains negative edge(s)");
                    }

                    /* Check for an edge between u and v */
                    if (graph[u][v] > 0)
                    {
                        /* Edge exists, relax the edge */
                        if (getDistance(v) > getDistance(u) + graph[u][v])
                        {
                            Distance[v] = Distance[u] + graph[u][v];
                            Previous[v] = u;
                        }
                    }
                }
            }
        }
        private double getDistance(int idx)
        {
            if (Distance.ContainsKey(idx))
                return Distance[idx];
            else
                return UnreachableDistance;
        }
        /// <summary>
        /// Find the shortest paths from start to target nodes
        /// </summary>
        /// <param name="graph">The adjacency list (zero-based index) that is composed of tuples (start, end, weight)</param>
        /// <param name="start">the index of the start vertex</param>
        /// <param name="target">the index of the target vertex</param>
        /// <returns>The shortest path from start to target (its distance is stored in property Distance)</returns>
        public int[] FindShortestPathToTarget(Dictionary<int, Dictionary<int, double>> graph, int start, int target)
        {
            /* Check graph format and that the graph actually contains something */
            if (graph.Keys.Count < 1)
            {
                throw new ArgumentException("Graph error, wrong format or no nodes to compute");
            }

            //int len = graph.Keys.Count;

            Initialize(graph,start);

            if (!graph.ContainsKey(start))
                return null;

            while (queue.Count > 0)
            {
                int u = GetNextVertex();
                if (u == target)
                    break;
                /* Find the nodes that u connects to and perform relax */
                //for (int v = 0; v < graph[u].Keys.Count; v++)
                foreach (int v in graph[u].Keys)
                {
                    /* Checks for edges with negative weight */
                    if (graph[u][v] < 0)
                    {
                        throw new ArgumentException("Graph contains negative edge(s)");
                    }

                    /* Check for an edge between u and v */
                    if (graph[u][v] > 0)
                    {
                        /* Edge exists, relax the edge */
                        if (getDistance(v) > getDistance(u) + graph[u][v])
                        {
                            Distance[v] = Distance[u] + graph[u][v];
                            Previous[v] = u;
                        }
                    }
                }
            }
            return GetSortestPathToTarget(target);
        }
        #endregion
        //#region Small network

        //private float[] dist;
        ///// <summary>
        ///// The sortest distances from the start vertex to every vertices
        ///// The distance to a vertex indicated by the index of array is the element corresponding to the vertex index
        ///// if the distance of UnreachableDistance, there is no connection from start to that vertex
        ///// </summary>
        //public float[] Dist
        //{
        //    get { return dist; }
        //    private set { dist = value; }
        //}
        //private int GetNextVertex4SmallNetwork()
        //{
        //    float min = float.PositiveInfinity;
        //    int vertex = -1;

        //    /* Search through queue to find the next node having the smallest distance */
        //    foreach (int j in queue)
        //    {
        //        if (Dist[j] <= min)
        //        {
        //            min = Dist[j];
        //            vertex = j;
        //        }
        //    }

        //    queue.Remove(vertex);

        //    return vertex;
        //}
        //private void Initialize4SmallNetwork(int start, int len)
        //{
        //    Dist = new float[len];
        //    //Previous = new int[len];
        //    Previous.Clear();
        //    queue.Clear();

        //    /* Set distance to all nodes to infinity - alternatively use Int.MaxValue for use of Int type instead */
        //    for (int i = 0; i < len; i++)
        //    {
        //        Dist[i] = UnreachableDistance;

        //        queue.Add(i);
        //    }

        //    /* Set distance to 0 for starting point and the previous node to null (-1) */
        //    Dist[start] = 0;
        //    Previous[start] = -1;
        //}
        ///// <summary>
        ///// Find the shortest path from a given start node
        ///// </summary>
        ///// <param name="graph">The adjacency matrix (zero-based index)</param>
        ///// <param name="start">The index of the start node</param>
        ///// <param name="end">The index of the target node</param>
        //public int[] GetSortestPath4SmallNetwork(Dictionary<int, Dictionary<int, float>> graph, int start, int end)
        //{
        //    /* Check graph format and that the graph actually contains something */
        //    if (graph.Keys.Count < 1)
        //    {
        //        throw new ArgumentException("Graph error, wrong format or no nodes to compute");
        //    }

        //    int len = graph.Keys.Count;

        //    Initialize4SmallNetwork(start, len);

        //    while (queue.Count > 0)
        //    {
        //        int u = GetNextVertex4SmallNetwork();
        //        if (u == end)/*terminate if finding out the target*/
        //            break;

        //        /* Find the nodes that u connects to and perform relax */
        //        //for (int v = 0; v < graph[u].Keys.Count; v++)
        //        foreach (int v in graph[u].Keys)
        //        {
        //            /* Checks for edges with negative weight */
        //            if (graph[u][v] < 0)
        //            {
        //                throw new ArgumentException("Graph contains negative edge(s)");
        //            }

        //            /* Check for an edge between u and v */
        //            if (graph[u][v] > 0)
        //            {
        //                /* Edge exists, relax the edge */
        //                if (Dist[v] > Dist[u] + graph[u][v])
        //                {
        //                    Dist[v] = Dist[u] + graph[u][v];
        //                    Previous[v] = u;
        //                }
        //            }
        //        }
        //    }
        //    return GetSortestPathToTarget(end);
        //}

        ///* Takes a graph as input an adjacency matrix (see top for details) and a starting node */
        ///// <summary>
        ///// Find the shortest path from a given start node
        ///// </summary>
        ///// <param name="graph">an adjacency matrix (see top for details) </param>
        ///// <param name="start">starting node</param>
        //public float[] FindSortestPathToVertices4SmallNetwork(float[,] graph, int start)
        //{
        //    /* Check graph format and that the graph actually contains something */
        //    if (graph.GetLength(0) < 1 || graph.GetLength(0) != graph.GetLength(1))
        //    {
        //        throw new ArgumentException("Graph error, wrong format or no nodes to compute");
        //    }

        //    int len = graph.GetLength(0);

        //    Initialize4SmallNetwork(start, len);

        //    while (queue.Count > 0)
        //    {
        //        int u = GetNextVertex4SmallNetwork();

        //        /* Find the nodes that u connects to and perform relax */
        //        for (int v = 0; v < len; v++)
        //        {
        //            /* Checks for edges with negative weight */
        //            if (graph[u, v] < 0)
        //            {
        //                throw new ArgumentException("Graph contains negative edge(s)");
        //            }

        //            /* Check for an edge between u and v */
        //            if (graph[u, v] > 0)
        //            {
        //                /* Edge exists, relax the edge */
        //                if (Dist[v] > Dist[u] + graph[u, v])
        //                {
        //                    Dist[v] = Dist[u] + graph[u, v];
        //                    Previous[v] = u;
        //                }
        //            }
        //        }
        //    }
        //    return Dist;
        //}
        //#endregion
    }
}
