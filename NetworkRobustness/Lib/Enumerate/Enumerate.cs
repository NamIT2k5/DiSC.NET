using System;
using System.Collections.Generic;
using System.Text;

namespace Mathutil
{
    public static class Enumerate<T>
    {
        public static IEnumerable<T> Backward(IEnumerable<T> e)
        {
            List<T> list = new List<T>(e);
            for (int i = list.Count - 1; i >= 0; i--)
                yield return list[i];
        }
        public static IEnumerable<T> RoundTrip(IEnumerable<T> e)
        {
            List<T> list = new List<T>(e);
            for (int i = 0; i < list.Count; i++)
                yield return list[i];
            for (int i = list.Count - 1; i >= 0; i--)
                yield return list[i];
        }
        public static IEnumerable<T> Cat(IEnumerable<T> e1, IEnumerable<T> e2)
        {
            foreach (T t in e1)
                yield return t;
            foreach (T t in e2)
                yield return t;
        }
        public static IEnumerable<T> Cat(IEnumerable<T> e1, IEnumerable<T> e2, IEnumerable<T> e3)
        {
            foreach (T t in e1)
                yield return t;
            foreach (T t in e2)
                yield return t;
            foreach (T t in e3)
                yield return t;
        }
        public static IEnumerable<T> Cat(params IEnumerable<T>[] es)
        {
            foreach (IEnumerable<T> e in es)
                foreach (T t in e)
                    yield return t;
        }

        #region Pair
        public static IEnumerable<Pair<T>> Bind(IEnumerable<T> ea, IEnumerable<T> eb)
        {
            return Bind(ea.GetEnumerator(), eb.GetEnumerator());
        }

        public static IEnumerable<Pair<T>> Bind(IEnumerator<T> ear, IEnumerator<T> ebr)
        {
            while (ear.MoveNext() && ebr.MoveNext())
                yield return new Pair<T>(ear.Current, ebr.Current);
        }

        public static IEnumerable<Pair<T>> Combination(IEnumerable<T> ea, IEnumerable<T> eb)
        {
            for (IEnumerator<T> ear = ea.GetEnumerator(); ear.MoveNext(); )
                for (IEnumerator<T> ebr = eb.GetEnumerator(); ebr.MoveNext(); )
                    yield return new Pair<T>(ear.Current, ebr.Current);
        }

        public static IEnumerable<Pair<T>> Combination2(IEnumerable<T> e)
        {
            return Combination(e, e);
        }

        public static IEnumerable<Pair<T>> Permutation(IEnumerable<T> ea, IEnumerable<T> eb, IEqualityComparer<T> cmpr)
        {
            if (cmpr == null)
                cmpr = EqualityComparer<T>.Default;
            for (IEnumerator<T> ear = ea.GetEnumerator(); ear.MoveNext(); )
                for (IEnumerator<T> ebr = eb.GetEnumerator(); ebr.MoveNext(); )
                    if(!cmpr.Equals(ear.Current, ebr.Current))
                        yield return new Pair<T>(ear.Current, ebr.Current);
        }

        public static IEnumerable<Pair<T>> Permutation(IEnumerable<T> ea, IEnumerable<T> eb)
        {
            return Permutation(ea, eb, (IEqualityComparer<T>)null);
        }
        
        public static IEnumerable<Pair<T>> Permutation2(IEnumerable<T> e, IEqualityComparer<T> cmpr)
        {
            return Permutation(e, e, cmpr);
        }

        public static IEnumerable<Pair<T>> Permutation2(IEnumerable<T> e)
        {
            return Permutation(e, e, (IEqualityComparer<T>)null);
        }

        #endregion

        #region Triple

        public static IEnumerable<Triple<T>> Bind(IEnumerable<T> ea, IEnumerable<T> eb, IEnumerable<T> ec)
        {
            return Bind(ea.GetEnumerator(), eb.GetEnumerator(), ec.GetEnumerator());
        }

        public static IEnumerable<Triple<T>> Bind(IEnumerator<T> ear, IEnumerator<T> ebr, IEnumerator<T> ecr)
        {
            while (ear.MoveNext() && ebr.MoveNext() && ecr.MoveNext())
                yield return new Triple<T>(ear.Current, ebr.Current, ecr.Current);
        }

        public static IEnumerable<Triple<T>> Combination(IEnumerable<T> ea, IEnumerable<T> eb, IEnumerable<T> ec)
        {
            for (IEnumerator<T> ear = ea.GetEnumerator(); ear.MoveNext(); )
                for (IEnumerator<T> ebr = eb.GetEnumerator(); ebr.MoveNext(); )
                    for (IEnumerator<T> ecr = ec.GetEnumerator(); ecr.MoveNext(); )
                        yield return new Triple<T>(ear.Current, ebr.Current, ecr.Current);
        }

        public static IEnumerable<Triple<T>> Combination3(IEnumerable<T> e)
        {
            return Combination(e, e,e);
        }

        public static IEnumerable<Triple<T>> Permutation(IEnumerable<T> ea, IEnumerable<T> eb, IEnumerable<T> ec, IEqualityComparer<T> cmpr)
        {
            if (cmpr == null)
                cmpr = EqualityComparer<T>.Default;
            for (IEnumerator<T> ear = ea.GetEnumerator(); ear.MoveNext(); )
                for (IEnumerator<T> ebr = eb.GetEnumerator(); ebr.MoveNext(); )
                    if (!cmpr.Equals(ear.Current, ebr.Current))
                        for (IEnumerator<T> ecr = ec.GetEnumerator(); ecr.MoveNext(); )
                            if (!cmpr.Equals(ear.Current, ecr.Current) && !cmpr.Equals(ebr.Current, ecr.Current))
                                yield return new Triple<T>(ear.Current, ebr.Current, ecr.Current);
        }

        public static IEnumerable<Triple<T>> Permutation(IEnumerable<T> ea, IEnumerable<T> eb, IEnumerable<T> ec)
        {
            return Permutation(ea, eb, ec, (IEqualityComparer<T>)null);
        }

        public static IEnumerable<Triple<T>> Permutation3(IEnumerable<T> e, IEqualityComparer<T> cmpr)
        {
            return Permutation(e, e, e, cmpr);
        }

        public static IEnumerable<Triple<T>> Permutation3(IEnumerable<T> e)
        {
            return Permutation(e, e, e, (IEqualityComparer<T>)null);
        }

        #endregion

        #region a lot
        private static IEnumerable<T>[] Es(IEnumerable<T> e, int dim)
        {
            IEnumerable<T>[] es = new IEnumerable<T>[dim];
            for (int i = 0; i < dim; i++)
                es[i] = e;
            return es;
        }

        private static IEnumerator<T>[] Ers(IEnumerable<T>[] es)
        {
            IEnumerator<T>[] ers = new IEnumerator<T>[es.Length];
            for (int i = 0; i < es.Length; i++)
                if(!(ers[i] = es[i].GetEnumerator()).MoveNext())
                    return null;
            return ers;
        }
      

        public static IEnumerable<T[]> Bind(params IEnumerable<T>[] es)
        {
            return Bind(null, es);
        }

        public static IEnumerable<T[]> Bind(params IEnumerator<T>[] ers)
        {
            return Bind(null, ers);
        }

        public static IEnumerable<T[]> Bind(T[] buffer, params IEnumerable<T>[] es)
        {
            return Bind(buffer, Ers(es));
        }

        public static IEnumerable<T[]> Bind(T[] buffer, params IEnumerator<T>[] ers)
        {
            if (ers == null)
                yield break;
            while (true)
            {
                T[] ret = buffer;
                if(ret == null)
                    ret = new T[ers.Length];
                for (int i = 0; i < ers.Length; i++)
                    ret[i] = ers[i].Current;
                yield return ret;
                foreach (IEnumerator<T> er in ers)
                    if (!er.MoveNext())
                        yield break;
            }
        }
        /// <summary>
        /// Enumerate combination
        /// </summary>
        /// <param name="es">Element enumeration</param>
        /// <returns>The combination</returns>
        public static IEnumerable<T[]> Combination(params IEnumerable<T>[] es)
        {
            return Combination(null, es);
        }

        public static IEnumerable<T[]> Combination(T[] buffer, params IEnumerable<T>[] es)
        {
            IEnumerator<T>[] ers = Ers(es);
            if (ers == null)
                yield break;
            while (true)
            {
                int i;
                T[] ret = buffer;
                if (ret == null)
                    ret = new T[ers.Length];
                for (i = 0; i < ers.Length; i++)
                    ret[i] = ers[i].Current;
                yield return ret;
                i--;
                while (!ers[i].MoveNext())
                    if (!(ers[i] = es[i].GetEnumerator()).MoveNext() || --i < 0)
                        yield break;
            }
        }
               
        public static IEnumerable<T[]> Combination(int dim, IEnumerable<T> e)
        {
            return Combination(null, Es(e, dim));
        }

        public static IEnumerable<T[]> Combination(T[] buffer, IEnumerable<T> e)
        {
            return Combination(buffer, Es(e, buffer.Length));
        }

        public static IEnumerable<T[]> Permutation(T[] buffer, IEqualityComparer<T> cmpr, params IEnumerable<T>[] es)
        {
            if (cmpr == null)
                cmpr = EqualityComparer<T>.Default;
            Dictionary<T, object> dict = new Dictionary<T, object>(cmpr);
            IEnumerator<T>[] ers = Ers(es);
            if (ers == null)
                yield break;
            int i = 0;
            while (true)
            {
                for (int j = i; j < ers.Length; j++)
                {
                    while (dict.ContainsKey(ers[j].Current))
                        if (!ers[j].MoveNext())
                            yield break;
                    dict.Add(ers[j].Current, null);
                }
                T[] ret = buffer;
                if (ret == null)
                    ret = new T[ers.Length];
                for (i = 0; i < ers.Length; i++)
                    ret[i] = ers[i].Current;
                yield return ret;
                i--;
                while (true)
                {
                    dict.Remove(ers[i].Current);
                loop:
                    if (ers[i].MoveNext())
                    {
                        if (!dict.ContainsKey(ers[i].Current))
                        {
                            dict.Add(ers[i++].Current, null);
                            break;
                        }
                        goto loop;
                    }
                    (ers[i] = es[i].GetEnumerator()).MoveNext();
                    if (--i < 0)
                        yield break;
                }
            }

        }
        public static IEnumerable<T[]> Permutation(IEqualityComparer<T> cmpr, params IEnumerable<T>[] es)
        {
            return Permutation((T[]) null, cmpr, es);
        }
        public static IEnumerable<T[]> Permutation(T[] buffer, params IEnumerable<T>[] es)
        {
            return Permutation(buffer, (IEqualityComparer<T>) null, es);
        }

        public static IEnumerable<T[]> Permutation(params IEnumerable<T>[] es)
        {
            return Permutation((T[])null, (IEqualityComparer<T>)null, es);
        }

        public static IEnumerable<T[]> Permutation(int dim, IEqualityComparer<T> cmpr, IEnumerable<T> e)
        {
            return Permutation((T[])null, cmpr, Es(e, dim));
        }
        public static IEnumerable<T[]> Permutation(T[] buffer, IEnumerable<T> e)
        {
            return Permutation(buffer, (IEqualityComparer<T>)null, Es(e, buffer.Length));
        }

        public static IEnumerable<T[]> Permutation(int dim, IEnumerable<T> e)
        {
            return Permutation((T[])null, (IEqualityComparer<T>)null, Es(e, dim));
        }

        #endregion
    }

    public static class Enumerate<TA, TB>
    {
        #region Pair<A,B>

        public static IEnumerable<Pair<TA, TB>> Bind(IEnumerable<TA> ea, IEnumerable<TB> eb)
        {
            return Bind(ea.GetEnumerator(), eb.GetEnumerator());
        }

        public static IEnumerable<Pair<TA, TB>> Bind(IEnumerator<TA> ear, IEnumerator<TB> ebr)
        {
            while (ear.MoveNext() && ebr.MoveNext())
                yield return new Pair<TA, TB>(ear.Current, ebr.Current);
        }

        public static IEnumerable<Pair<TA, TB>> Combination(IEnumerable<TA> ea, IEnumerable<TB> eb)
        {
            for (IEnumerator<TA> ear = ea.GetEnumerator(); ear.MoveNext(); )
                for (IEnumerator<TB> ebr = eb.GetEnumerator(); ebr.MoveNext(); )
                    yield return new Pair<TA, TB>(ear.Current, ebr.Current);
        }

        #endregion
    }

    public static class Enumerate<TA, TB, TC>
    {
        #region Triple<TA, TB, TC>

        public static IEnumerable<Triple<TA, TB, TC>> Bind(IEnumerable<TA> ea, IEnumerable<TB> eb, IEnumerable<TC> ec)
        {
            return Bind(ea.GetEnumerator(), eb.GetEnumerator(), ec.GetEnumerator());
        }
        public static IEnumerable<Triple<TA, TB, TC>> Bind(IEnumerator<TA> ear, IEnumerator<TB> ebr, IEnumerator<TC> ecr)
        {
            while (ear.MoveNext() && ebr.MoveNext() && ecr.MoveNext())
                yield return new Triple<TA, TB, TC>(ear.Current, ebr.Current, ecr.Current);
        }
        public static IEnumerable<Triple<TA, TB, TC>> Combination(IEnumerable<TA> ea, IEnumerable<TB> eb, IEnumerable<TC> ec)
        {
            for (IEnumerator<TA> ear = ea.GetEnumerator(); ear.MoveNext(); )
                for (IEnumerator<TB> ebr = eb.GetEnumerator(); ebr.MoveNext(); )
                    for (IEnumerator<TC> ecr = ec.GetEnumerator(); ecr.MoveNext(); )
                        yield return new Triple<TA, TB, TC>(ear.Current, ebr.Current, ecr.Current);
        }

        #endregion
    }
}
