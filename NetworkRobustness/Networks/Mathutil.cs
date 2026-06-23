using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using BasicNet;
using MathNet.Numerics;
using System.Threading;
using MathNet.Numerics.Statistics;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra;
namespace Mathutil
{
   
    #region Numeric functions
        public class NumericMath
        {
            /// <summary>
            /// Generate an enumerating number initialized from a given value in a range of [a..b]. 
            /// The function will return initValue if nextValue = 0o and the function will return a steadily increased number by the increase of nextValue
            /// </summary>
            /// <param name="start">The value to start the enumeration in the range [a..b]</param>
            /// <param name="a">The inclusive value a in the range [a..b]</param>
            /// <param name="b">The inclusive value b in the range [a..b]</param>
            /// <param name="nextValue">The value's index in the range of [a..b] from the start value</param>
            /// <returns></returns>
            public static float EnumerateNumberInRange(float start, int a, int b, int nextValue)
            {
                // The inclusive range of initState
                // return 3 values that started at initState and will be enumed in the range [a, b] respectively 
                return a + (start - a + nextValue) % (b - a + 1); // the next value

            }
            public const float zeroEpsionf = 0.000001f;
            /// <summary>
            /// Return double random between a range [min, max]
            /// </summary>
            /// <param name="fMin">Inclusive Min</param>
            /// <param name="fMax">Inclusive max</param>
            /// <returns></returns>
            

            /// <summary>
            /// Fit power law distribution with function y=Ax^B
            /// http://mathworld.wolfram.com/LeastSquaresFittingPowerLaw.html
            /// </summary>
            /// <param name="degSeq">degree sequence where x is degree (first) and y is the number of nodes (second)</param>
            /// <param name="A">A in y = Ax^B</param>
            /// <param name="B">B in y = Ax^B</param>
            public static void LestSquareFittingPowerLaw(IEnumerable<Pair<float, float>> degSeq, ref double A, ref double B)
            {
                int n = degSeq.Count();
                double lnxlny=0,lnx=0,lny=0,lnx2=0;
                for (int i = 0; i < n; i++)
                {
                    lnxlny += Math.Log(degSeq.ElementAt(i).First) * Math.Log(degSeq.ElementAt(i).Second);
                    lnx += Math.Log(degSeq.ElementAt(i).First);
                    lny += Math.Log(degSeq.ElementAt(i).Second);
                    lnx2 += Math.Log(degSeq.ElementAt(i).First) * Math.Log(degSeq.ElementAt(i).First);
                    
                }
                double b = (n * lnxlny - lnx * lny) / (n * lnx2 - lnx * lnx);
                double a = (lny - b * lnx) / n;
                B = b;
                A = Math.Exp(a);
            }
            /// <summary>
            /// Fit power law distribution with function y=Ax^B
            /// http://mathworld.wolfram.com/LeastSquaresFittingPowerLaw.html
            /// </summary>
            /// <param name="degSeq">degree sequence where x is degree (first) and y is the number of nodes (second)</param>
            /// <param name="A">A in y = Ax^B</param>
            /// <param name="B">B in y = Ax^B</param>
            /// <param name="R">Correlation coefficient (y, yy)</param>
            public static void LestSquareFittingPowerLaw(IEnumerable<Pair<float, float>> degSeq, ref double A, ref double B, ref double R)
            {
                
                LestSquareFittingPowerLaw(degSeq, ref A, ref B);
                var y = from e in degSeq select (double)e.Second;
                double a = A, b = B;
                var yy = from e in degSeq select a*Math.Pow(e.First,b);

                R=MathNet.Numerics.Statistics.Accumulator.CorrelationCoefficient_S(y.ToArray(), yy.ToArray());

            }
            /// <summary>
            /// P-value for Pearson's Correlation Coefficient
            /// http://www.minitab.com/support/documentation/answers/pearsoncorrelationcoefficientp.pdf
            /// </summary>
            /// <param name="r">the correlation coefficient</param>
            /// <param name="n">the number of pairs of data</param>
            /// <returns>-1 if there is an error</returns>
            public static double Pvalue4PearsonCC(double r, int n)
            {
                if (n <= 2)
                    return 1;
                double s = r * Math.Sqrt(n - 2) / Math.Sqrt(1 - r * r);
                try
                {
                    MathNet.Numerics.RandomSources.SystemRandomSource rnd = new MathNet.Numerics.RandomSources.SystemRandomSource(Mathutil.NumericMath.RandomCraft.Next());
                    MathNet.Numerics.Distributions.StudentsTDistribution tDistribution = new MathNet.Numerics.Distributions.StudentsTDistribution(rnd);

                    tDistribution.SetDistributionParameters(n - 2);
                    double x = tDistribution.CumulativeDistribution(Math.Abs(s));

                    return 2 * (1 - x);
                }
                catch (ArgumentException)
                {
                    
                    return -1;
                }

            }
            public static void Swap<T>(ref T a, ref T b)
            {
                T c = a;
                a = b;
                b = c;
            }
            #region special functions
            /////////////////////////////////////////////////
            // Special-Functions
            void GammaPSeries/*gser*/(
             ref double gamser, double a, double x, ref double gln){
              const int ITMAX=100;
              const double EPS=3.0e-7;
              int n;
              double sum, del, ap;

              gln=LnGamma(a);
              if (x <= 0.0){
                Debug.Assert(x>=0); /*if (x < 0.0) nrerror("x less than 0 in routine gser");*/
                gamser=0.0;
                return;
              } else {
                ap=a;
                del=sum=1.0/a;
                for (n=1; n<=ITMAX; n++){
                  ++ap;
                  del *= x/ap;
                  sum += del;
                  if (Math.Abs(del) < Math.Abs(sum)*EPS){
                    gamser=sum*Math.Exp(-x+a*Math.Log(x)-(gln));
                    return;
                  }
                }
                /*nrerror("a too large, ITMAX too small in routine gser");*/
                throw new Exception("Fail in NumericMath.GammaPSeries function.");
              }
            }

            void GammaQContFrac/*gcf*/(
             ref double gammcf, double a, double x, ref double gln){
              const int ITMAX=100;
              const double EPS=3.0e-7;
              const double  FPMIN=1.0e-30;
              int i;
              double an, b, c, d, del, h;

              gln=LnGamma(a);
              b=x+1.0-a;
              c=1.0/FPMIN;
              d=1.0/b;
              h=d;
              for (i=1;i<=ITMAX;i++){
                an = -i*(i-a);
                b += 2.0;
                d=an*d+b;
                if (Math.Abs(d) < FPMIN) d=FPMIN;
                c=b+an/c;
                if (Math.Abs(c) < FPMIN) c=FPMIN;
                d=1.0/d;
                del=d*c;
                h *= del;
                if (Math.Abs(del-1.0) < EPS) break;
              }
              Debug.Assert(i<=ITMAX);
              /*if (i > ITMAX) nrerror("a too large, ITMAX too small in gcf");*/
              gammcf=Math.Exp(-x+a*Math.Log(x)-(gln))*h;
            }

            double GammaQ/*gammq*/(double a, double x){
              Debug.Assert((x>=0)&&(a>0));
              double gamser=0, gammcf=0, gln=0;
              if (x<(a+1.0)){
                GammaPSeries(ref gamser,a,x,ref gln);
                return 1.0-gamser;
              } else {
                GammaQContFrac(ref gammcf,a,x,ref gln);
                return gammcf;
              }
            }
            readonly static double[] cof=new double[]{76.18009172947146,-86.50532032941677,
                      24.01409824083091,-1.231739572450155,
                      0.1208650973866179e-2,-0.5395239384953e-5};
              

            public static double LnGamma/*gammln*/(double xx){
              double x, y, tmp, ser;
              //const double cof[6]{76.18009172947146,-86.50532032941677,
              //        24.01409824083091,-1.231739572450155,
              //        0.1208650973866179e-2,-0.5395239384953e-5};
              int j;

              y=x=xx;
              tmp=x+5.5;
              tmp -= (x+0.5)*Math.Log(tmp);
              ser=1.000000000190015;
              for (j=0;j<=5;j++) ser += cof[j]/++y;
              return -tmp+Math.Log(2.5066282746310005*ser/x);
            }

            double LnComb(int n, int k){
              return LnGamma(n+1)-LnGamma(k+1)-LnGamma(n-k+1);
            }

            double BetaCf(double a, double b, double x)
            {
              const double MAXIT=100;
              const double EPS=3.0e-7;
              const double FPMIN=1.0e-30;
              int m,m2;
              double aa,c,d,del,h,qab,qam,qap;

              qab=a+b;
              qap=a+1.0;
              qam=a-1.0;
              c=1.0;
              d=1.0-qab*x/qap;
              if (Math.Abs(d) < FPMIN) d=FPMIN;
              d=1.0/d;
              h=d;
              for (m=1;m<=MAXIT;m++) {
                m2=2*m;
                aa=m*(b-m)*x/((qam+m2)*(a+m2));
                d=1.0+aa*d;
                if (Math.Abs(d) < FPMIN) d=FPMIN;
                c=1.0+aa/c;
                if (Math.Abs(c) < FPMIN) c=FPMIN;
                d=1.0/d;
                h *= d*c;
                aa = -(a+m)*(qab+m)*x/((a+m2)*(qap+m2));
                d=1.0+aa*d;
                if (Math.Abs(d) < FPMIN) d=FPMIN;
                c=1.0+aa/c;
                if (Math.Abs(c) < FPMIN) c=FPMIN;
                d=1.0/d;
                del=d*c;
                h *= del;
                if (Math.Abs(del-1.0) < EPS) break;
              }
              if (m > MAXIT){throw new Exception("a or b too big, or MAXIT too small in betacf");}
              return h;
            }

            double BetaI(double a, double b, double x){
              double bt;

              if (x < 0.0 || x > 1.0){throw new Exception("Bad x in routine betai");} // Bad x in routine betai
              if (x == 0.0 || x == 1.0) bt=0.0;
              else
                bt=Math.Exp(LnGamma(a+b)-LnGamma(a)-LnGamma(b)+a*Math.Log(x)+b*Math.Log(1.0-x));
              if (x < (a+1.0)/(a+b+2.0))
                return bt*BetaCf(a,b,x)/a;
              else
                return 1.0-bt*BetaCf(b,a,1.0-x)/b;
            }

     
            // MLE of the power-law coefficient
            double GetPowerCoef(Vector XValV, double MinX) {
              for (int i = 0; MinX <= 0.0 && i < XValV.Length; i++) { 
                MinX = XValV[i]; }
              Debug.Assert(MinX > 0.0);
              double LnSum=0.0;
              for (int i = 0; i < XValV.Length; i++) {
                if (XValV[i] < MinX) continue;
                LnSum += Math.Log(XValV[i] / MinX);
              }
              return 1.0 + (double)XValV.Length / LnSum;
            }

            #endregion
            /// <summary>
            /// Two-vector similarity based on Pearson correlation coefficient.
            /// Note: if X= k.Y then they are similar to each other
            /// </summary>
            /// <param name="X">First vector</param>
            /// <param name="Y">Second vector</param>
            /// <returns>a float number between [0, 1]</returns>
            public static float SimilarityVector(float[] X, float[] Y)
            {
                return (1 + Accumulator.CorrelationCoefficient_S(X, Y))/2;
            }
            /// <summary>
            /// Normalize a vector to unit vector
            /// </summary>
            /// <param name="X"></param>
            /// <returns></returns>
            public static float[] NormalizeVector(float[] X)
            {
                double sqrt = 0;
                float[] normalized = new float[X.Length];
                foreach(float x in X)
                {
                    sqrt += x *x;
                }
                sqrt = Math.Sqrt(sqrt);
                for (int i = 0; i < X.Length; i++)
                {
                    normalized[i] = X[i] / Convert.ToSingle(sqrt);
                }
                return normalized;
            }
            public static double Combin(int n, int k)
            {
                return MathNet.Numerics.Combinatorics.Combinations(n, k);
            }
            #region Random class
            public class RandomCraft
            {
                public static double dRandBetween(double fMin, double fMax)
                {
                    double f = NumericMath.RandomCraft.NextDouble();
                    return fMin + f * (fMax - fMin);
                }
                static ThreadSafeRandom ParalellRandom = new ThreadSafeRandom();
                //static Random SequenceRandom = new Random((int)DateTime.Now.Ticks);

                /// <summary>Returns a nonnegative random number.</summary>
                /// <returns>A 32-bit signed integer greater than or equal to zero and less than MaxValue.</returns>
                public static int Next()
                {
                    return ParalellRandom.Next();
                }

                /// <summary>Returns a nonnegative random number less than the specified maximum.</summary>
                /// <param name="maxValue">
                /// The exclusive upper bound of the random number to be generated. maxValue must be greater than or equal to zero. 
                /// </param>
                /// <returns>
                /// A 32-bit signed integer greater than or equal to zero, and less than maxValue; 
                /// that is, the range of return values ordinarily includes zero but not maxValue. However, 
                /// if maxValue equals zero, maxValue is returned.
                /// </returns>
                public static int Next(int maxValue)
                {
                    return ParalellRandom.Next(maxValue);
                }

                /// <summary>Returns a random number within a specified range.</summary>
                /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
                /// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
                /// <returns>
                /// A 32-bit signed integer greater than or equal to minValue and less than maxValue; 
                /// that is, the range of return values includes minValue but not maxValue. 
                /// If minValue equals maxValue, minValue is returned.
                /// </returns>
                public static int Next(int minValue, int maxValue)
                {
                    return ParalellRandom.Next(minValue, maxValue);
                }

                /// <summary>Returns a random number between 0.0 and 1.0.</summary>
                /// <returns>A double-precision floating point number greater than or equal to 0.0, and less than 1.0.</returns>
                public static double NextDouble()
                {
                    return ParalellRandom.NextDouble();
                }

                /// <summary>Fills the elements of a specified array of bytes with random numbers.</summary>
                /// <param name="buffer">An array of bytes to contain random numbers.</param>
                public static void NextBytes(byte[] buffer)
                {
                    ParalellRandom.NextBytes(buffer);
                }

                ///--
                

                const int RndSeed=0;

                const int a=16807, m=2147483647, q=127773// m DIV a
                , r=2836;// m MOD a


                int Seed;
                int GetNextSeed()
                {
                if ((Seed=a*(Seed%q)-r*(Seed/q))>0)
                    return Seed;
                else return Seed+=m;
                }

                public RandomCraft(int _Seed=1, int Steps=0)
                {
    
                    PutSeed(_Seed); 
                    Move(Steps);
                }
                void PutSeed(int _Seed)
                {
                    Debug.Assert(_Seed>=0);
                    if (_Seed==0){
                        Seed=(int)Math.Abs(DateTime.Now.Ticks);
                    } 
                    else 
                    {
                        Seed=_Seed;
                    }
                }
                int GetSeed() {return Seed;}
                void Randomize(){PutSeed(RndSeed);}
                void Move(int Steps)
                {
                    for (int StepN=0; StepN<Steps; StepN++){GetNextSeed();}
                }
                public RandomCraft(int seed)
                {
                    this.Seed=seed;
                
                }

                public RandomCraft Assign(RandomCraft Rnd) { Seed = Rnd.Seed; return this; }

                public double GetUniDev(){ return GetNextSeed()/(double)m; }
                public int GetUniDevInt(int Range=0)  
                {
                  int Seed=GetNextSeed();
                  if (Range==0){return Seed;}
                  else {return Seed%Range;}
                }
                public int GetUniDevInt(int MnVal, int MxVal)
                {
                    Debug.Assert(MnVal<=MxVal); 
                    return MnVal+GetUniDevInt(MxVal-MnVal+1);
                }
                public uint GetUniDevUInt(uint Range=0)
                {
                    uint Seed=(uint)(GetNextSeed()%0x10000)*0x10000+(uint)(GetNextSeed()%0x10000);
                    if (Range==0){return Seed;}
                    else {return Seed%Range;}
                }
                //public Int64 GetUniDevInt64(Int64 Range=0)
                //{
                //    Int64 RndVal = (Int64)(((UInt64)GetUniDevInt()<<32) | (UInt64)GetUniDevInt());
                //    if (Range==0){return RndVal;}
                //    else {return RndVal%Range;}
                //}
                public UInt64 GetUniDevUInt64(UInt64 Range=0)
                {
                    UInt64 RndVal = (UInt64)((UInt64)(GetUniDevInt()<<32) | (UInt64)GetUniDevInt());
                    if (Range==0){return RndVal;}
                    else {return RndVal%Range;}
                }
                public double GetNrmDev()
                {
                    double v1, v2, rsq;
                    do {
                    v1=2.0*GetUniDev()-1.0; // pick two uniform numbers in the square
                    v2=2.0*GetUniDev()-1.0; // extending from -1 to +1 in each direction
                    rsq=v1*v1+v2*v2; // see if they are in the unit cicrcle
                    } while ((rsq>=1.0)||(rsq==0.0)); // and if they are not, try again
                    double fac=Math.Sqrt(-2.0* Math.Log(rsq)/rsq); // Box-Muller transformation
                    return v1*fac;
            
                }
                public double GetNrmDev(double Mean, double SDev, double Mn, double Mx)
                {
                    double Val=Mean+GetNrmDev()*SDev;
                    if (Val<Mn){Val=Mn;}
                    if (Val>Mx){Val=Mx;}
                    return Val;
                }
                public double GetExpDev()
                {
                    double UniDev;
                    do {
                    UniDev=GetUniDev();
                    } while (UniDev==0.0);
                    return -Math.Log(UniDev);
                }
                public double GetExpDev(double Lambda){return GetExpDev()/Lambda;}
                public double GetGammaDev(int Order)
                {  
                    int j;
                    double am,e,s,v1,v2,x,y;
                    if (Order<1){throw new Exception("Fail at Random.GetGammaDev function");}
                    if (Order<6) {
                    x=1.0;
                    for (j=1;j<=Order;j++) x *=GetUniDev();
                    x = -Math.Log(x);
                    } else {
                    do {
                        do {
                        do {
                            v1=2.0*GetUniDev()-1.0;
                            v2=2.0*GetUniDev()-1.0;
                        } while (v1*v1+v2*v2 > 1.0);
                        y=v2/v1;
                        am=Order-1;
                        s=Math.Sqrt(2.0*am+1.0);
                        x=s*y+am;
                        } while (x <= 0.0);
                        e=(1.0+y*y)*Math.Exp(am*Math.Log(x/am)-s*y);
                    } while (GetUniDev()>e);
                    }
                    return x;
                }

                //public double GetPoissonDev(double Mean)
                //{
                //    //static double sq,alxm,g,oldm=(-1.0);
                //    double sq,alxm,g,oldm=(-1.0);
                //    double em,t,y;
                //    if (Mean < 12.0) {
                //    if (Mean != oldm) {
                //        oldm=Mean;
                //        g=Math.Exp(-Mean);
                //    }
                //    em = -1;
                //    t=1.0;
                //    do {
                //        ++em;
                //        t *= GetUniDev();
                //    } while (t>g);
                //    } else {
                //    if (Mean != oldm) {
                //        oldm=Mean;
                //        sq=Math.Sqrt(2.0*Mean);
                //        alxm=Math.Log(Mean);
                //        g=Mean*alxm-LnGamma(Mean+1.0);
                //    }
                //    do {
                //        do {
                //        y=Math.Tan(Math.PI*GetUniDev());
                //        em=sq*y+Mean;
                //        } while (em < 0.0);
                //        em=Math.Floor(em);
                //        t=0.9*(1.0+y*y)*Math.Exp(em*alxm-LnGamma(em+1.0)-g);
                //    } while (GetUniDev()>t);
                //    }
                //    return em;
                //}
             //   double GetBinomialDev(double Prb, int Trials)
             //   {
             //       int j;
             //       int nold=(-1);
             //       double am,em,g,angle,p,bnl,sq,t,y;
             //       double pold=(-1.0),pc,plog,pclog,en,oldg;

             //       p=(Prb <= 0.5 ? Prb : 1.0-Prb);
             //       am=Trials*p;
             //       if (Trials < 25) {
             //       bnl=0.0;
             //       for (j=1;j<=Trials;j++)
             //           if (GetUniDev() < p) ++bnl;
             //       } else if (am < 1.0) {
             //       g=Math.Exp(-am);
             //       t=1.0;
             //       for (j=0;j<=Trials;j++) {
             //           t *= GetUniDev();
             //           if (t < g) break;
             //       }
             //       bnl=(j <= Trials ? j : Trials);
             //       } else {
             //       if (Trials != nold) {
             //           en=Trials;
             //           oldg=LnGamma(en+1.0);
             //           nold=Trials;
             //       } if (p != pold) {
             //           pc=1.0-p;
             //           plog=Math.Log(p);
             //           pclog=Math.Log(pc);
             //           pold=p;
             //       }
             //       sq=Math.Sqrt(2.0*am*pc);
             //       do {
             //           do {
             //           angle=Math.PI*GetUniDev();
             //           y=Math.Tan(angle);
             //           em=sq*y+am;
             //           } while (em < 0.0 || em >= (en+1.0));
             //           em=Math.Floor(em);
             //           t=1.2*sq*(1.0+y*y)*Math.Exp(oldg-(em+1.0)
             //           -LnGamma(en-em+1.0)+em*plog+(en-em)*pclog);
             //       } while (GetUniDev() > t);
             //       bnl=em;
             //       }
             //       if (p != Prb) bnl=Trials-bnl;
             //       return bnl;
             //}
            public int GetGeoDev(double Prb){return 1+(int)Math.Floor(Math.Log(1.0-GetUniDev())/Math.Log(1.0-Prb));}
            public double GetPowerDev(double AlphaSlope){ // power-law degree distribution (AlphaSlope>0)
                Debug.Assert(AlphaSlope>1.0);
                return Math.Pow(1.0-GetUniDev(), -1.0/(AlphaSlope-1.0));
            }
           
            public double GetRayleigh(double Sigma) { // 1/sqrt(alpha) = sigma
                Debug.Assert(Sigma>0.0);
                return Sigma*Math.Sqrt(-2*Math.Log(1-GetUniDev()));
            }
            public double GetWeibull(double K, double Lambda) { // 1/alpha = lambda
            Debug.Assert(Lambda>0.0 && K>0.0);
            return Lambda*Math.Pow(-Math.Log(1-GetUniDev()), 1.0/K);}
            //void GetSphereDev(const int& Dim, TFltV& ValV);

  
            public bool Check(){ int PSeed=Seed; Seed=1;
            for (int SeedN=0; SeedN<10000; SeedN++){GetNextSeed();}
            bool Ok=Seed==1043618065; Seed=PSeed; return Ok; }

            public static double GetUniDevStep(int Seed, int Steps)
            {
                RandomCraft Rnd = new RandomCraft(Seed); Rnd.Move(Steps); return Rnd.GetUniDev();
            }
            public double GetNrmDevStep(int Seed, int Steps)
            {
                RandomCraft Rnd = new RandomCraft(Seed); Rnd.Move(Steps); return Rnd.GetNrmDev();
            }
            public static double GetExpDevStep(int Seed, int Steps)
            {
                RandomCraft Rnd = new RandomCraft(Seed); Rnd.Move(Steps); return Rnd.GetExpDev();
            }

             ///---
            }
            #endregion
            #region Hashing
            
            /// <summary>
            /// Mapping two integers to one, in a unique and deterministic way
            /// http://stackoverflow.com/questions/919612/mapping-two-integers-to-one-in-a-unique-and-deterministic-way
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static long HashTwoNumber(int a, int b)
            {
                var A = (ulong)(a >= 0 ? 2 * (long)a : -2 * (long)a - 1);
                var B = (ulong)(b >= 0 ? 2 * (long)b : -2 * (long)b - 1);
                var C = (long)((A >= B ? A * A + A + B : A + B * B) / 2);
                return a < 0 && b < 0 || a >= 0 && b >= 0 ? C : -C - 1;
                //return (a * b + 3 * a + 2 * a * b + b + b * b) / 2;

            }
            /// <summary>
            /// Mapping two integers to one, in a unique and deterministic way
            /// http://stackoverflow.com/questions/919612/mapping-two-integers-to-one-in-a-unique-and-deterministic-way
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static int HashTwoNumber(short a, short b)
            {
                var A = (uint)(a >= 0 ? 2 * a : -2 * a - 1);
                var B = (uint)(b >= 0 ? 2 * b : -2 * b - 1);
                var C = (int)((A >= B ? A * A + A + B : A + B * B) / 2);
                return a < 0 && b < 0 || a >= 0 && b >= 0 ? C : -C - 1;
            }
            public static long HashTwoNumberRegardlessOrder(int a, int b)
            {
                return a > b ? HashTwoNumber(a, b) : HashTwoNumber(b, a);
            }
            #endregion
        }
    #endregion

    public class Set<T>:HashSet<T> where T : System.IComparable<T> 
    {
        public Set(params T[] Values):base()
        {
            base.UnionWith(Values);
        }
        public Set(IEnumerable<T> set):base(set)
        {
        }
        public void Add(IEnumerable<T> set)
        {
            base.UnionWith(set);
        }
        public Set<T> Assign(IEnumerable<T> set)
        {
            this.Clear();
            this.Add(set);
            return this;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        /// <summary>
        /// Find the closure of the set
        /// </summary>
        /// <param name="Laws">The dependent functions</param>
        /// <returns>The closure whose elements are this set and others expanded from the set by the laws</returns>
        public Set<T> Closure(IEnumerable<Pair<Set<T>, Set<T>>> Laws)
        {
            Set<T> Kplus = this.ToList();
            bool IsFoundoneMore = true;

            while (IsFoundoneMore)
            {
                IsFoundoneMore = false;
                for (int i = 0; i < Laws.Count(); i++)
                {
                    if (Laws.ElementAt(i).First.In(Kplus) &&
                        !Laws.ElementAt(i).Second.In(Kplus))
                    {
                        Kplus.UnionWith(Laws.ElementAt(i).Second);
                        IsFoundoneMore = true;
                    }
                }
            }
            return Kplus;
        }
        /// <summary>
        /// Modifies the current Set object to contain all elements that present in its closure
        /// </summary>
        /// <param name="Laws">The dependent functions</param>
        /// <returns>The closure whose elements are this set and others expanded from the set by the laws</returns>
        public void ClosureWith(IEnumerable<Pair<Set<T>, Set<T>>> Laws)
        {
            this.UnionWith(this.Closure(Laws));
        }
        /// <summary>
        /// Find smallest keys of a dependent functions/laws
        /// </summary>
        /// <param name="Laws">The laws whose elements are dependent functions, which are relations between set pairs</param>
        /// <returns>Set of smallest keys</returns>
        //public static IEnumerable<Set<T>> FindSmallestKeySet(IEnumerable<Pair<Set<T>, Set<T>>> Laws)
        //{
        //    //First, find a super key
        //    Set<T> SuperKey=new Set<T>(),K=null;
        //    foreach (Pair<Set<T>, Set<T>> law in Laws)
        //        SuperKey.UnionWith(law.First);

        //    int count = 0;
        //    List<Set<T>> SmallestKeys = new List<Set<T>>();
            
        //    for (int start = 0; start < Laws.Count(); start++)
        //    {
        //        count = 0;
        //        //Intialize with the super key
        //        K = SuperKey.ToList();

        //        //Second, compact the super key K to find a smallest key
        //        for (int i = start; count++ < Laws.Count(); i = (i >= Laws.Count()-1 ? 0 : i + 1))
        //        {
        //            Pair<Set<T>, Set<T>> law = Laws.ElementAt(i);
        //            if (law.First.In(K))
        //            {
        //                int ibefore = K.Count;
        //                K.ExceptWith(law.First.Closure(Laws) ^ K);
        //                K.UnionWith(law.First);
        //                if (ibefore != K.Count)
        //                {
        //                    count = 0;
        //                    i = start;
        //                    continue;
        //                }
        //            }
        //        }
        //        SmallestKeys.Add(K);
        //    }
        //    return SmallestKeys.Distinct();
        //}

        public static IEnumerable<Set<T>> FindSmallestKeySet(IEnumerable<Pair<Set<T>, Set<T>>> Laws)
        {
            //First, find a super key
            Set<T> SuperKey = new Set<T>(), K = null;
            foreach (Pair<Set<T>, Set<T>> law in Laws)
                SuperKey.UnionWith(law.First);

            int count = 0;
            List<Set<T>> SmallestKeys = new List<Set<T>>();

            for (int start = 0; start < Laws.Count(); start++)
            {
                count = 0;
                //Intialize with the super key
                K = SuperKey.ToList();

                //Second, compact the super key K to find a smallest key
                for (int i = start; count++ < Laws.Count(); i = (i >= Laws.Count() - 1 ? 0 : i + 1))
                {
                    Pair<Set<T>, Set<T>> law = Laws.ElementAt(i);
                    if (law.First.In(K))
                    {
                        int ibefore = K.Count;
                        K.ExceptWith(law.First.Closure(Laws) ^ K);
                        K.UnionWith(law.First);
                        if (ibefore != K.Count)
                        {
                            count = 0;
                            i = start;
                            continue;
                        }
                    }
                }
                SmallestKeys.Add(K);
            }
            return SmallestKeys.Distinct();
        }
       
        public override bool Equals(object obj)
        {
            return base.SetEquals((Set<T>)obj);
        }
       
        #region User-defined Operators
        public T this[int index]
        {
            get
            {
                return this.ElementAt(index);
            }
        }
        public bool In(Set<T> ParentSet)
        {
            return this.IsSubsetOf(ParentSet);
        }
        public static implicit operator List<T>(Set<T> set)
        {
            return set.ToList();
        }
        public static implicit operator Set<T>(List<T> set)
        {
            return new Set<T>(set);
        }
       
        public static bool operator==(Set<T> set1, Set<T> set2) 
        {
            return set1.Equals(set2);
        }
        public static bool operator !=(Set<T> set1, Set<T> set2)
        {
            return !set1.Equals(set2);
        }
        /// <summary>
        /// Intersect two sets
        /// </summary>
        /// <param name="set1">The first set</param>
        /// <param name="set2">The second set</param>
        /// <returns>The set whose elments are from the fist or the second set</returns>
        public static Set<T> operator ^(Set<T> set1, Set<T> set2)
        {
            return new Set<T>(set1.Intersect(set2));
        }
        /// <summary>
        /// Union two sets
        /// </summary>
        /// <param name="set1">The first set</param>
        /// <param name="set2">The second set</param>
        /// <returns>The set whose elments are from the fist and the second set</returns>
        public static Set<T> operator +(Set<T> set1, Set<T> set2)
        {
            return new Set<T>(set1.Union(set2) );
        }
        public static Set<T> operator +(Set<T> set1, T element)
        {
            return new Set<T>(set1.Union(new List<T>(){element}));
        }
        /// <summary>
        /// Substract a set with another
        /// </summary>
        /// <param name="set1">The fist whose elements need to remove from</param>
        /// <param name="set2">The second whose elements is chosen to remove from the first</param>
        /// <returns>The set whose elements are not in the first set1 but the second set2</returns>
        public static Set<T> operator -(Set<T> set1, Set<T> set2)
        {
            return new Set<T>(set1.Except(set2));
        }
        public static Set<T> operator -(Set<T> set1, T element)
        {
            return new Set<T>(set1.Except(new List<T>(){element} ));
        }
       
        #endregion

       
    }

}
