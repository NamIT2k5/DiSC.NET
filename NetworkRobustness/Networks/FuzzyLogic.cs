using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fuzzy
{
    public class FLogic
    {
        float val=0;
        public const float True = 1.0f;
        public const float False = 0.0f;
        public const float Null = -1.0f;
        public static implicit operator float(FLogic f)
        {
            return f.val;
        }
        public static implicit operator FLogic(float f)
        {
            return new FLogic(f);
        }
        public FLogic(float f)
        {
            val=f;
        }
        public static float[] Randomize(float[] fVariable)
        {
            float[] randF = new float[fVariable.Length];

            for(int i=0;i<fVariable.Length;i++)
            {
                if (Math.Round(fVariable[i]) == 0.0)
                {
                    randF[i] = fVariable[i] + Mathutil.NumericMath.RandomCraft.Next(0, 4) / (float)10;
                }else
                    randF[i] = fVariable[i] - Mathutil.NumericMath.RandomCraft.Next(0, 4) / (float)10;

            }
            return randF;
        }
        
        static public float not(float fval)
        {
            return 1 - fval;
        }
        static public float and(float fval1, float fval2)
        {
            return Math.Min(fval1, fval2);
        }
        static public float or(float fval1, float fval2)
        {
            return Math.Max(fval1, fval2);
        }
        
       



    }
}
