using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fuzzy;
namespace BasicNet
{
    /// <summary>
    /// Mutation perturbation to detect stative robustness or disease genes
    /// Locking perturbation to...
    /// Functional perturbation to detect essentiality?
    /// </summary>
    public class Perturbation : NetBased
    {
        Kind kind = Kind.Mutation;
        protected Node perturbedNode = null;
        object nodeState = null;

        public enum Kind { Mutation, ChangedFunction };
        public override NetBased CreateObject()
        {
            return new Perturbation();
        }

        //public override Object Clone()
        //{
        //    Perturbation per = new Perturbation(this.kind);
        //    per.perturbedNodeState = this.perturbedNodeState;
        //    return per;
        //}
        public override void Assign(object Source)
        {
            Perturbation o = Source as Perturbation;
            this.kind = o.kind;
            this.perturbedNode = o.perturbedNode;
            this.nodeState = o.nodeState;
        }
        public Perturbation(Kind kind = Kind.Mutation)
        {
            this.kind = kind;
        }
        /// <summary>
        /// Perturb on a node
        /// </summary>
        /// <param name="node">The node needs perturbing</param>
        public void Perturb(Node node)
        {
            nodeState = node.Perturb(this.kind);
            perturbedNode = node;
        }
       
        /// <summary>
        /// Recover a node from perturbation
        /// This function is always go after the PERTURB function (they are a pair of function)
        /// </summary>
        /// <param name="node">The node needs recovery</param>
        public void Recover()
        {
            perturbedNode.Unperturb(this.kind, nodeState);
        }
    }
}
