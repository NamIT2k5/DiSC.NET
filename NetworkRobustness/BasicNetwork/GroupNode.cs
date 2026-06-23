using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace BasicNet
{
    public class GroupNode : BooleanNode
    {
        public override NetBased CreateObject()
        {
            return new GroupNode(ModuleID, null, null);
        }
        public override void Assign(object Source)
        {
            base.Assign(Source);
            GroupNode o = Source as GroupNode;
            this.net = o.net.Clone() as BooleanNetwork;
        }

        private BooleanNetwork net = new BooleanNetwork();// the sub-network inside a node
        /// <summary>
        /// Create a GroupNode that contains a sub network with nodes cloned from the mother network
        /// </summary>
        /// <param name="GroupID">The ID or name of GroupNode</param>
        /// <param name="internalInteractions">Links of the subnetwork</param>
        /// <param name="nodes">Nodes of the subnetwork, if null, nodes are selected from the link list</param>
        public GroupNode(int GroupID, IEnumerable<Interaction> internalInteractions, IEnumerable<Node> nodes = null)
            : base(GroupID.ToString(), BooleanNode.ArbitraryFunctionType, 1.0)
        {
            net.Name = GroupID.ToString();

            if (internalInteractions != null)
            {
                var newint = Netutil.CloneInteraction(internalInteractions);

                if (nodes == null)
                    net.AddNode(Netutil.Union2nodeListByName(
                        (from p in newint select p.startNode),
                        (from p in newint select p.endNode)
                        ).ToArray());
                else//in the case the number of nodes > the number of nodes in the internalInteractions
                {
                    net.AddNode(Netutil.Union2nodeListByName(
                        (from p in newint select p.startNode),
                        (from p in newint select p.endNode)
                        ).ToArray());

                    var newNodes = from p in nodes where !net.Nodes.Any(t => t.name == p.name) select (Node)p.Clone();
                    net.AddNode(newNodes.ToArray());
                }

                net.AddArc(newint.ToArray());
            }
            else if (nodes != null)
            {
                net.AddNode(nodes.ToArray());
            }
            //Netutil.DumpNode(net.Nodes.ToArray());
            
            
            //Netutil.DumpInteraction(net.Arcs.ToArray());
            //subInteractions.UnionWith(internalInteractions);
            //subNodes.UnionWith((from p in internalInteractions select p.startNode).Union(from p in internalInteractions select p.endNode));
        }
        public int ModuleID
        {
            get
            {
                return Convert.ToInt32(net.Name);
            }
        }
        public BooleanNetwork SubNetwork
        {
            get
            {
                return net;
            }
        }
        
       

    }
}
