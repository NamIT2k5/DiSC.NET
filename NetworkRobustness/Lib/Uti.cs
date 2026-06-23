using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSimulation.Lib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

   
    public static class Uti
    {
        /// <summary>
        /// Does a deep copy of a dictionary, optimizing for value types.  If 
        /// the key or value is not a value type (excluding strings), this method
        /// relies on the type's serialization capabilities.  If the type is not
        /// serializable, this method will fail.
        /// </summary>
        public static Dictionary<K, V> CloneDictionary<K, V>(Dictionary<K, V> dict)
        {
            Dictionary<K, V> newDict = null;

            // The clone method is immune to the source dictionary being null.
            if (dict != null)
            {
                // If the key and value are value types, clone without serialization.
                if (((typeof(K).IsValueType || typeof(K) == typeof(string)) &&
                    (typeof(V).IsValueType) || typeof(V) == typeof(string)))
                {
                    newDict = new Dictionary<K, V>();
                    // Clone by copying the value types.
                    foreach (KeyValuePair<K, V> kvp in dict)
                    {
                        newDict[kvp.Key] = kvp.Value;
                    }
                }
                else
                {
                    // Clone by serializing to a memory stream, then deserializing.
                    // Don't use this method if you've got a large objects, as the
                    // BinaryFormatter produces bloat, bloat, and more bloat.
                    BinaryFormatter bf = new BinaryFormatter();
                    MemoryStream ms = new MemoryStream();
                    bf.Serialize(ms, dict);
                    ms.Position = 0;
                    newDict = (Dictionary<K, V>)bf.Deserialize(ms);
                }
            }

            return newDict;
        }
        public static Dictionary<TKey, TValue> CloneDictionaryCloningValues<TKey, TValue>(Dictionary<TKey, TValue> original) where TValue : ICloneable 
        { 
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(original.Count, original.Comparer); 
            foreach (KeyValuePair<TKey, TValue> entry in original) 
            { 
                ret.Add(entry.Key, (TValue)entry.Value.Clone()); 
            } 
            return ret; 
        }
        public static HashSet<T> CloneHashSetCloningValues<T>(HashSet<T> original) where T : ICloneable
        {
            HashSet<T> ret = new HashSet<T>();
            foreach (T entry in original)
            {
                ret.Add((T)entry.Clone());
            }
            return ret;
        } 
        
        public static object CheckNull(object originalVal, object nullCase)
        {
            return originalVal != null ? originalVal : nullCase;
        }
        public static bool Equal(object obj1, object obj2, Type compareType)
        {
            if (obj1 == null && obj2 == null) return true;
            if (obj1 == null || obj2 == null) return false;
            return Convert.ChangeType(obj1, compareType).Equals(Convert.ChangeType(obj2, compareType));
        }

        #region Methods for guard

        /// <summary>
        /// Checks a string argument to ensure it isn't null or empty.
        /// </summary>
        /// <param name="argumentValue">The argument value to check.</param>
        /// <param name="argumentName">The name of the argument.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argumentValue"/> is a null reference.</exception>
        /// <exception cref="ArgumentException"><paramref name="argumentValue"/> is <see cref="string.Empty"/>.</exception>
        public static void ArgumentNotNullOrEmptyString(string argumentValue, string argumentName)
        {
            ArgumentNotNull(argumentValue, argumentName);

            if (argumentValue.Length == 0)
            {
                throw new ArgumentException("String cannot be empty.", argumentName);
            }
        }


        /// <summary>
        /// Checks an argument to ensure it isn't null.
        /// </summary>
        /// <param name="argumentValue">The argument value to check.</param>
        /// <param name="argumentName">The name of the argument.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argumentValue"/> is a null reference.</exception>
        public static void ArgumentNotNull(object argumentValue, string argumentName)
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        #endregion

    }

}
