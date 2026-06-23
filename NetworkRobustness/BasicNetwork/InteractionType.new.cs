namespace BasicNet
{
    public class InteractionType
    {
        public static readonly int NULL = 0, // undirected interaction
        NEGATIVE = -1, // directed and negative/inhibited interaction
        POSITIVE = 1; //directed and positive/activated interaction
       
        private int type = NULL;
        public InteractionType(int value)
        {
            this.type = value;
        }
        public static implicit operator int(InteractionType f)
        {
            return (int)f.type;
        }
        public static implicit operator InteractionType(int i)
        {

            return new InteractionType(i);
        }

    }
    
}
