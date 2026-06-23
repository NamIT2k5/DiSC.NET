using System;
using System.Collections.Generic;
using System.Text;
using Mathutil;
namespace Mathutil.Test { 
    public class EnumerateTest { 
        public void Test() { 
            int n = new Random().Next(10, 20); 
            IEnumerable<int> e = For.Inc(0, n); 
            int i = 0; 
            foreach (Pair<int> pair in Enumerate<int>.Bind(e, e)) 
            { 
                //Assert.AreEqual(i, pair.A); 
                //Assert.AreEqual(i, pair.B); 
                i++; 
            } 
            i = 0; 
            foreach (Pair<int> pair in Enumerate<int>.Combination(e, e)) 
            { 
                //Assert.AreEqual(i, pair.A * n + pair.B); 
                i++; 
            } 
            i = 0; 
            foreach (Pair<int> pair in Enumerate<int>.Permutation(e, e)) 
            { 
                //Assert.AreNotEqual(pair.A, pair.B); 
                i++; 
            } 
                //Assert.AreEqual(n * (n - 1), i); 
            i = 0; 
            foreach (Triple<int> triple in Enumerate<int>.Bind(e, e, e)) 
            { 
                //Assert.AreEqual(i, triple.A); 
                //Assert.AreEqual(i, triple.B); 
                //Assert.AreEqual(i, triple.C); 
                i++; 
            } 
            i = 0; 
            foreach (Triple<int> triple in Enumerate<int>.Combination(e, e, e)) 
            { 
                //Assert.AreEqual(i, triple.A * n * n + triple.B * n + triple.C); 
                i++; 
            } 
            i = 0; 
            foreach (Triple<int> triple in Enumerate<int>.Permutation(e, e, e)) 
            { 
                //Assert.AreNotEqual(triple.A, triple.B); 
                //Assert.AreNotEqual(triple.A, triple.C); 
                //Assert.AreNotEqual(triple.B, triple.C); 
                i++; 
            } 
            //Assert.AreEqual(n * (n - 1) * (n - 2), i); 
            i = 0; 
            foreach (int[] ints in Enumerate<int>.Combination(4, e)) 
            { 
                int x = ints[0] * n * n * n + ints[1] * n * n + ints[2] * n + ints[3]; 
                //Assert.AreEqual(i, x); 
                i++; 
            } 
            i = 0; 
            foreach (int[] ints in Enumerate<int>.Permutation(4, e)) 
            { 
                bool[] bs = new bool[n]; 
                for (int j = 0; j < 4; j++) 
                { 
                    //Assert.IsTrue(0 <= ints[j]); 
                    //Assert.IsTrue(n > ints[j]); 
                    //Assert.IsFalse(bs[ints[j]]); 
                    bs[ints[j]] = true; 
                } 
            } 
            int[] buffer = new int[4]; 
            i = 0; 
            foreach (int[] ints in Enumerate<int>.Combination(buffer, e)) 
            { 
                int x = ints[0] * n * n * n + ints[1] * n * n + ints[2] * n + ints[3]; 
                //Assert.AreEqual(i, x); 
                i++; 
            } 
            i = 0; 
            foreach (int[] ints in Enumerate<int>.Permutation(buffer, e)) 
            { 
                bool[] bs = new bool[n]; for (int j = 0; j < 4; j++)
                { 
                    //Assert.IsTrue(0 <= ints[j]); 
                    //Assert.IsTrue(n > ints[j]); 
                    //Assert.IsFalse(bs[ints[j]]); 
                    bs[ints[j]] = true; 
                } 
            } 
        } 
    } 
}