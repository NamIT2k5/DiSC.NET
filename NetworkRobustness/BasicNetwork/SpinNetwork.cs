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
namespace BasicNet
{
    public class SpinNetwork : BasicNetwork
    {
        #region Methods have to be overrided in sub-classes
        public override void Assign(object Source)
        {

            BasicNetwork o = Source as BasicNetwork;
            this.name = o.name;
            this._networkType = o._networkType;
            for (int i = 0; i < o._arcs.Count; i++)
            {
                Interaction arc = o._arcs.ElementAt(i);

                Node start = this.AddNode((Node)arc.startNode.Clone());
                Node end = this.AddNode((Node)arc.endNode.Clone());
                this.AddArc(new Interaction(start, end, arc.Type, arc.weight, arc.Direction));
            }
        }
        public override NetBased CreateObject()
        {
            return new BasicNetwork();
        }
        public virtual Node NewNode(string name, object para, double weight = 1.0)
        {
            return new Node(name, weight);

        }
        public virtual Node[,] NewNodeArray(int n, int m)
        {
            return new Node[n, m];

        }
        public virtual Node[] NewNodeArray(int n)
        {
            return new Node[n];
        }
        public virtual Node[] NewNodeArray(params Node[] node)
        {
            return node;
        }
        /// <summary>
        /// The node in the network
        /// </summary>
        /// <param name="NodeName">name of the node</param>
        /// <returns></returns>
        public Node this[string NodeName]
        {
            get
            {
                return nodeNameDictionary[NodeName];
            }

        }
        public Node this[int id]
        {
            get
            {
                return nodeIdDictionary[id];
            }

        }
        public IEnumerable<Interaction> this[string start, string end]
        {
            get
            {
                return ArcDictionary[BasicNetwork.ArcKey(nodeNameDictionary[start].id, nodeNameDictionary[end].id)];
            }

        }
        #endregion

    }
}
