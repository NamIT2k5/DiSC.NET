using System;
using BasicNet;
using Mathutil;
namespace BasicNet
{
    public class Interaction : NetBased
    {
        // Always _startNode to _endNode
        public double weight=1.0f;
        public double density;
        public Node _startNode;
        public Node _endNode;
        public string Name = null;
        private DirectionType _directionType = DirectionType.directed;
        //private InteractionType _type;
        private int _type;

        public Node StartNode { get { return _startNode; } }
        public Node EndNode { get { return _endNode; } }
        public override NetBased CreateObject()
        {
            throw new Exception("CreateObject of Interaction has not been implemented yet!");
        }
        public override string ToString()
        {
            if(weight==1)
                return (this.Direction == DirectionType.directed ? "[d]\t" : "[u]\t" + " ") + _startNode.name + " -> " + _endNode.name + "  " + this.Name;
            else
                return (this.Direction == DirectionType.directed ? "[d]\t" : "[u]\t" + " ") + _startNode.name + " --[" + weight.ToString() + "]->" + _endNode.name + "  " + this.Name;
        }
        public override NetBased Clone()
        {
            Interaction newInter = new Interaction((Node)this._startNode.Clone(), (Node)_endNode.Clone(), this._type,this.Name, this.weight, this._directionType);
            newInter.density = this.density;
            return newInter;
        }
        public override void Assign(object Source)
        {
            
            Interaction o = Source as Interaction;

            this._startNode = o._startNode as Node;
            this._endNode = o._endNode as Node;

            //this._startNode = o._startNode.Clone() as Node;
            //this._endNode = o._endNode.Clone() as Node;
            
            this._type = o._type;
            this.weight = o.weight;
            this._directionType = o._directionType;
            this.density = o.density;
            this.Name = o.Name;
        }

        public Node startNode
        {
            get
            {
                return _startNode;
            }
            set
            {
                _startNode = value;
            }
        }
        public Node endNode
        {
            get
            {
                return _endNode;
            }
            set
            {
                _endNode = value;
            }
        }
        public static Interaction RandomInteraction(Node startNode, Node endNode, int type, string name="", double weight = 1.0f, DirectionType directionType = DirectionType.directed)
        {
            Interaction inte = null;
            if (NumericMath.RandomCraft.Next(0, 2) == 0)
                inte = new Interaction(startNode, endNode, type, name, weight, directionType);
            else
                inte = new Interaction(endNode, startNode, type, name, weight, directionType);
            return inte;
        }
        public static Interaction RandomInteraction(Random random, Node startNode, Node endNode, int type, string name="", double weight = 1.0f, DirectionType directionType = DirectionType.directed)
        {
            Interaction inte = null;
            if (random.Next(0, 2) == 0)
                inte = new Interaction(startNode, endNode, type,name, weight, directionType);
            else
                inte = new Interaction(endNode, startNode, type,name, weight, directionType);
            return inte;
        }
        //public Interaction(Node startNode, Node endNode, int type, string Name, double weight = 1, DirectionType directionType = DirectionType.directed)
        //    : this(startNode, endNode, type, weight, directionType)
        //{
        //    this.Name = Name;
        //}
        public Interaction(Node startNode, Node endNode, int type, string name="", double weight = 1.0f, DirectionType directionType = DirectionType.directed, bool isManaged = true)
        {

            _type = type;
            this.startNode = startNode;
            this.endNode = endNode;
            this.Name = name;
            this.weight = weight;
            this.density = weight / (startNode.weight * endNode.weight);
            this._directionType = directionType;

            //here only
            if (isManaged)
            {
                this.startNode.AddArc(true, this);
                this.endNode.AddArc(false, this);
            }

        }
        
        ~Interaction()
         {
             Dispose();
         }
        public void Dispose()
        {
            _startNode.RemoveArc(this);
            _endNode.RemoveArc(this);

        }
        //-----
        //static Random rd = BasicNetwork.random;
        public static int ArbitraryValue
        {
            get
            {

                if (NumericMath.RandomCraft.NextDouble() <= 0.5)
                    return InteractionType.NEGATIVE;
                else
                    return InteractionType.POSITIVE;
            }
        }
        public static InteractionType DefaultValue
        {
            get
            {

              
                return InteractionType.POSITIVE;
            }
        }
        public enum DirectionType { undirected=0, directed=1};
        
        public DirectionType Direction
        {
            get
            {
                return _directionType;
            }
        }
        
        /// <summary>
        /// Returning true, Positive; else Negative
        /// </summary>
        public int Type
        {
            get
            {
                return _type;
            }
        }
        

        //public Node GetDesNode(Node sourceNode)
        //{
        //    if (!sourceNode.Equals(_startNode))
        //    {
        //        return null;
        //    }
        //    return _endNode;
        //}

        //public Node GetSrcNode(Node destinationNode)
        //{
        //    if (!destinationNode.Equals(_endNode))
        //    {
        //        return null;
        //    }
        //    return _startNode;
        //}

        public Node GetPartnerVertex(Node vertex)
        {
            if (this.startNode.name == vertex.name)
            {
                return this.endNode;
            }

            if (this.endNode.name == vertex.name)
            {
                return this.startNode;
            }

            throw new ArgumentException("The vertex specified does not form part of this edge.", "vertex");
        }


    }
}
