using System;
using System.Collections.Generic;
using System.Text;

namespace Mathutil
{
    public struct Pair<T>
    {
        public Pair(T a, T b)
        {
            A = a;
            B = b;
        }

        public T A;
        public T B;
    }

    public struct Pair<TFirst, TSecond>
    {
        public Pair(TFirst first, TSecond second)
        {
            First = first;
            Second = second;
        }

        public TFirst First;
        public TSecond Second;
    }

    public struct Triple<T>
    {
        public Triple(T a, T b, T c)
        {
            A = a;
            B = b;
            C = c;
        }

        public T A;
        public T B;
        public T C;
    }
    public struct Triple<TA, TB, TC>
    {
        public Triple(TA a, TB b, TC c)
        {
            A = a;
            B = b;
            C = c;
        }

        public TA A;
        public TB B;
        public TC C;
    }
    public struct Quad<TA, TB, TC, TD>
    {
        public Quad(TA a, TB b, TC c, TD d)
        {
            A = a;
            B = b;
            C = c;
            D= d;
            
        }

        public TA A;
        public TB B;
        public TC C;
        public TD D;
    }
}
