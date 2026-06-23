using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetSimulation.Lib;
using Mathutil;
using Fuzzy;


namespace BasicNet
{
    public class SpinNode: Node
    {
        protected double _state = 0;
        protected double _prestate = 0;
        public SpinNode(String name, double weight = 1.0)
            : base(name, weight)
        {
            
            InitNode();
        }

        public override NetBased CreateObject()
        {
            return new SpinNode("null");
        }
        public override void Assign(object Source)
        {
            SpinNode Src = Source as SpinNode;
            base.Assign(Src);

            this._state = Src._state;
            this._prestate = Src._prestate;
        }
        public double PreviousState
        {
            get { return _prestate; }
        }
        public double State
        {
            get { return _state; }
        }
        public double SetNextState(double Val)
        {
            _prestate = _state;
            _state = Val;
            return Val;
        }
        public double ResetState(double Val)
        {
            _prestate = _state = Val;
            return _state;
        }
        protected void InitNode()
        {
            ResetRandomState();
        }
        public void ResetRandomState()
        {
            float V = NumericMath.RandomCraft.Next(-1, 2);// -1, 0, 1
            this.ResetState(V);
        }
        /// <summary>
        /// Calculate the next state of the node from neighbours' states 
        /// The formula is from paper titled "Competitive Dynamics on Complex Networks"
        /// </summary>
        /// <param name="E">The epsilon in the formula, normally being in the range of (0, 1/D) where D is the largest in-degree of nodes in the network</param>
        /// <param name="IsDirected">=true: the directed links are used, otherwise: undirected links are used</param>
        /// <returns>The next state</returns>
        public double Spin_GoToNextState(float E, bool IsDirected)
        {
            List<LinkingNode> srcNodes = IsDirected?GetSrcLinkingNodes():GetUndirectedLinkingNodes();

            if (srcNodes.Count <= 0)
                return this._state;

            
            double sum = 0;
            int i = 0;
            for (; i < srcNodes.Count; i++)
            {
                sum += srcNodes[i].InteractionWeight * ((srcNodes[i].Node as SpinNode).PreviousState - this.PreviousState);
            }

            this._state = this._state+E*sum;
            return this._state;
        }
    }
}
