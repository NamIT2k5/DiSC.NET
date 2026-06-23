#define DEGREE_MODE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetSimulation.Lib;
using Mathutil;
using Fuzzy;


namespace BasicNet
{
    public class BooleanNode: Node
    {
        protected float _state = 0;
        protected float _prestate = 0;
        protected FunctionType _functionType;
        public override string ToString()
        {
            //return string.Format("[{0}]\tname:{1,5}\tstate:{2}", ObjectID, name,_state);
            return string.Format("name:{0} \t id:{1} \t state: {2}", name,id, _state);
            //return this.name + " (id=" + id +" state="+_state+ ")";
            
        }
        public override NetBased CreateObject()
        {
            return new BooleanNode("null", BooleanNode.ArbitraryFunctionType);
        }
        public override void Assign(Object Source)
        {
            BooleanNode Src = Source as BooleanNode;
            base.Assign(Src);

            this._state = Src._state;
            this._prestate = Src._prestate;
            this._functionType = Src._functionType;
        }
        public override object Perturb(Perturbation.Kind perturbType)
        {
            
            if (perturbType == Perturbation.Kind.Mutation)
            {
                float[] oldState = new float[1];
                oldState[0]=this.State;
                this.ResetState(FLogic.not(this.State));
                return oldState;
                
            }
            else if (perturbType == Perturbation.Kind.ChangedFunction)// For essential node?
            {
                FunctionType[] oldType = new FunctionType[1];
                oldType[0]=this.Type;
                this.Type = (this.Type == FunctionType.AND ? FunctionType.OR : FunctionType.AND);
                return oldType;
            }
            return null;
        }

        public override void Unperturb(Perturbation.Kind perturbType, object state)
        {
            if (perturbType == Perturbation.Kind.Mutation)
            {
                float[] oldState = state as float[];
                this.ResetState(oldState[0]);

            }
            else if (perturbType == Perturbation.Kind.ChangedFunction)// For essential node?
            {
                FunctionType[] oldType = state as FunctionType[];

                this.Type = oldType[0];
            }
        }

        public FunctionType Type
        {
            get
            {
                
#if DEGREE_MODE
                return (this.TotalDegree % 2) == 0 ? FunctionType.AND : FunctionType.OR;
#else
                return _functionType;
#endif
            }
            set
            {
                _functionType = value;
            }
        }
        public static FunctionType ArbitraryFunctionType
        {
            get
            {
                return NumericMath.RandomCraft.Next(0, 2) == 0 ? FunctionType.AND : FunctionType.OR;
            }
        }
        public static FunctionType DefaultFunctionType
        {
            get
            {
                return FunctionType.AND;
            }
        }
        public BooleanNode(String name, FunctionType fType, double weight = 1.0):base(name,weight)
        {
            
            _functionType = fType;
            InitNode();
        }
       
        public float PreviousState
        {
            get { return _prestate; }
        }
        public float State
        {
            get { return _state; }
        }
        public float SetNextState(float Val)
        {
            _prestate = _state;
            _state = Val;
            return Val;
        }
        protected void InitNode()
        {
            ResetRandomState();
        }
        public float ResetState(float Val)
        {
            _prestate = _state = Val;
            return _state;
        }
        public void ResetRandomState()
        {
            float V = NumericMath.RandomCraft.Next(0, 2);
            this.ResetState(V);
        }
        public float GoToNextStateParalell(float[] preNetworkState, Dictionary<BooleanNode, int> NodeIndices, float[] networkState)
        {


            List<LinkingNode> srcNodes = this.GetSrcLinkingNodes();

            if (srcNodes.Count <= 0)
                //return node._state;
                return networkState[NodeIndices[this]];

            //float Val = node._state;
            float Val = networkState[NodeIndices[this]];
            int i = 0;
            for (; i < srcNodes.Count; i++)
                if (srcNodes[i].InteractionType != InteractionType.NULL)
                {
                    //Val = (srcNodes[i].Interaction == InteractionType.NEGATIVE ? FLogic.not(srcNodes[i].Node.PreviousState) : srcNodes[i].Node.PreviousState);
                    Val = (srcNodes[i].InteractionType == InteractionType.NEGATIVE ? FLogic.not(preNetworkState[NodeIndices[srcNodes[i].Node as BooleanNode]]) : preNetworkState[NodeIndices[srcNodes[i].Node as BooleanNode]]);
                    i++;
                    break;
                }
            switch (this.Type)
            {
                case FunctionType.AND:
                    for (; i < srcNodes.Count; i++)
                        if (srcNodes[i].InteractionType != InteractionType.NULL)
                            //Val = FLogic.and(Val, (srcNodes[i].Interaction == InteractionType.NEGATIVE ? FLogic.not(srcNodes[i].Node.PreviousState) : srcNodes[i].Node.PreviousState));
                            Val = FLogic.and(Val, (srcNodes[i].InteractionType == InteractionType.NEGATIVE ? FLogic.not(preNetworkState[NodeIndices[srcNodes[i].Node as BooleanNode]]) : preNetworkState[NodeIndices[srcNodes[i].Node as BooleanNode]]));
                    break;

                case FunctionType.OR:
                    for (; i < srcNodes.Count; i++)
                        if (srcNodes[i].InteractionType != InteractionType.NULL)
                            //Val = FLogic.or(Val, (srcNodes[i].Interaction == InteractionType.NEGATIVE ? FLogic.not(srcNodes[i].Node.PreviousState) : srcNodes[i].Node.PreviousState));
                            Val = FLogic.or(Val, (srcNodes[i].InteractionType == InteractionType.NEGATIVE ? FLogic.not(preNetworkState[NodeIndices[srcNodes[i].Node as BooleanNode]]) : preNetworkState[NodeIndices[srcNodes[i].Node as BooleanNode]]));
                    break;
            }

           


            return Val;
        }
        public float Spin_GoToNextState(float E)
        {
            // cac not dau vao
            List<LinkingNode> srcNodes = GetSrcMixingLinkingNodes();
            // neu khong co cac not dau vao thi trang thai khong thay doi
            if (srcNodes.Count <= 0)
                return this._state;


            float sum = 0;
            int i = 0;
            // duyet cac not dau vao
            for (; i < srcNodes.Count; i++)
            {
                sum += ((float)(srcNodes[i].InteractionWeight)) * ((srcNodes[i].Node as BooleanNode).PreviousState - this.PreviousState);
            }

            this._state = this._state + E * sum;
            return this._state;
        }
        public float GoToNextState()
        {
            List<LinkingNode> srcNodes = GetSrcLinkingNodes();

            if (srcNodes.Count <= 0)
                return this._state;

            float Val = this._state;
            int i = 0;
            for (; i < srcNodes.Count; i++)
                if (srcNodes[i].InteractionType != InteractionType.NULL)
                {
                    Val = (srcNodes[i].InteractionType == InteractionType.NEGATIVE ? FLogic.not((srcNodes[i].Node as BooleanNode).PreviousState) : (srcNodes[i].Node as BooleanNode).PreviousState);
                    i++;
                    break;
                }
            switch (this.Type)
            {
                case FunctionType.AND:
                    for (; i < srcNodes.Count; i++)
                        if (srcNodes[i].InteractionType != InteractionType.NULL)
                            Val = FLogic.and(Val, (srcNodes[i].InteractionType == InteractionType.NEGATIVE ? FLogic.not((srcNodes[i].Node as BooleanNode).PreviousState) : (srcNodes[i].Node as BooleanNode).PreviousState));
                    break;

                case FunctionType.OR:
                    for (; i < srcNodes.Count; i++)
                        if (srcNodes[i].InteractionType != InteractionType.NULL)
                            Val = FLogic.or(Val, (srcNodes[i].InteractionType == InteractionType.NEGATIVE ? FLogic.not((srcNodes[i].Node as BooleanNode).PreviousState) : (srcNodes[i].Node as BooleanNode).PreviousState));
                    break;
            }
            
            this._state = Val;
            return this._state;
        }
    }
}
