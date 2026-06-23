using System;
using System.Collections.Generic;
using System.Text;
using Fuzzy;
namespace Mathutil
{
    public static class For<T>
    {
        public static IEnumerable<T> Inc(T begin, T end)
        {
            return Inc(begin, end, false, EqualityComparer<T>.Default);
        }
        public static IEnumerable<T> Inc(T begin, T end, IEqualityComparer<T> comparer)
        {
            return Inc(begin, end, false, comparer);
        }
        public static IEnumerable<T> Inc(T begin, T end, bool withEnd)
        {
            return Inc(begin, end, withEnd, EqualityComparer<T>.Default);
        }

        public static IEnumerable<T> Inc(T begin, T end, bool withEnd, IEqualityComparer<T> comparer)
        {
            T t = begin;
            while (!comparer.Equals(t, end))
            {
                yield return t;
                t = OperatorDelegates<T>.Increment(t);
            }
            if (withEnd && comparer.Equals(t, end))
                yield return t;
        }

        public static IEnumerable<T> Dec(T begin, T end)
        {
            return Dec(begin, end, false, EqualityComparer<T>.Default);
        }
        public static IEnumerable<T> Dec(T begin, T end, IEqualityComparer<T> comparer)
        {
            return Dec(begin, end, false, comparer);
        }
        public static IEnumerable<T> Dec(T begin, T end, bool withEnd)
        {
            return Dec(begin, end, withEnd, EqualityComparer<T>.Default);
        }

        public static IEnumerable<T> Dec(T begin, T end, bool withEnd, IEqualityComparer<T> comparer)
        {
            T t = begin;
            while (!comparer.Equals(t, end))
            {
                yield return t;
                t = OperatorDelegates<T>.Decrement(t);
            }
            if(withEnd && comparer.Equals(t, end))
                yield return t;
        }
    }
    /// <summary>
    /// Define: 
    /// 1) The domain of elements for enumerating
    /// 2) The direction of sorting, increase or decrease
    /// </summary>
    public static class For
    {
        public static IEnumerable<bool> Dec()
        {
            yield return true;
            yield return false;
            yield break;

        }
        public static IEnumerable<bool>Logic (bool initState)
        {
            yield return initState;
            yield return !initState;
            yield break;

        }
        public static IEnumerable<float> FLogic(float initState)
        {
            yield return initState;
            yield return Fuzzy.FLogic.not(initState);
            yield break;

        }
        /// <summary>
        /// Return a spin integer value in the range of [-1, 1]
        /// </summary>
        /// <param name="initState">The initial value in {-1, 0, 1}</param>
        /// <returns>a enumerated value in {-1, 0, 1} that is started from the initial value</returns>
        public static IEnumerable<float> Spin_Value(float initState)
        {
            yield return Mathutil.NumericMath.EnumerateNumberInRange(initState, -1, 1, 0);
            yield return Mathutil.NumericMath.EnumerateNumberInRange(initState, -1, 1, 1);
            yield return Mathutil.NumericMath.EnumerateNumberInRange(initState, -1, 1, 2);
            yield break; 
            //const int a = -1, b = 1;// The inclusive range of initState
            //// return 3 values that started at initState and will be enumed in the range [a, b] respectively 
            //yield return a + (initState -a) % (b - a + 1); //first value = initState
            //yield return a + (initState -a + 1)%(b-a+1); // the next value
            //yield return a + (initState -a + 2) % (b - a + 1);// the value after next
            //yield break; 

        }

        public static IEnumerable<float>[] FLogic(params float[] initState)
        {
            IEnumerable<float>[] ret = new IEnumerable<float>[initState.Length];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = FLogic(initState[i]);
            return ret;

        }
        public static IEnumerable<float>[] Spin_FLogic(params float[] initState)
        {
            IEnumerable<float>[] ret = new IEnumerable<float>[initState.Length];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = Spin_Value(initState[i]);
            return ret;

        }
        public static IEnumerable<bool>[] Logic(params bool[] initState)
        {
            IEnumerable<bool>[] ret = new IEnumerable<bool>[initState.Length];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = Logic(initState[i]);
            return ret;

        }
       
        public static IEnumerable<Int32> Inc(Int32 begin, Int32 end)
        {
            for (Int32 i = begin; i < end; i++)
                yield return i;
        }
        public static IEnumerable<Int32> Inc(Int32 begin, Int32 end, bool withEnd)
        {
            Int32 i;
            for (i = begin; i < end; i++)
                yield return i;
            if (withEnd && i == end)
                yield return i;
        }
        public static IEnumerable<byte> Dec(byte begin, byte end)
        {
            for (byte i = begin; i > end; i--)
                yield return i;

        }
        public static IEnumerable<Int32> Dec(Int32 begin, Int32 end)
        {
            for (Int32 i = begin; i > end; i--)
                yield return i;
        
        }
        public static IEnumerable<Int32> Dec(Int32 begin, Int32 end, bool withEnd)
        {
            Int32 i;
            for (i = begin; i > end; i--)
                yield return i;
            if (withEnd && i == end)
                yield return i;
        }
        public static IEnumerable<Int64> Inc(Int64 begin, Int64 end)
        {
            for (Int64 i = begin; i < end; i++)
                yield return i;
        }
        public static IEnumerable<Int64> Inc(Int64 begin, Int64 end, bool withEnd)
        {
            Int64 i;
            for (i = begin; i < end; i++)
                yield return i;
            if (withEnd && i == end)
                yield return i;
        }
        public static IEnumerable<Int64> Dec(Int64 begin, Int64 end)
        {
            for (Int64 i = begin; i > end; i--)
                yield return i;
        }
        public static IEnumerable<Int64> Dec(Int64 begin, Int64 end, bool withEnd)
        {
            Int64 i;
            for (i = begin; i > end; i--)
                yield return i;
            if (withEnd && i == end)
                yield return i;
        }
    }

}
