using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BasicNet
{
    public abstract class NetBased
    {
        /// <summary>
        /// To new an object of NetBased inside a method, must call CreateObject rather than new operator
        /// </summary>
        private Int64 _objectID = 0;
        private static Int64 _currentId = 0;
        //to support paralell programming
        private Object Locking = new object();
        public Int64 ObjectID
        {
            get { return _objectID; }
        }
        public NetBased()
        {
            lock (Locking)
            {
                _objectID = _currentId++;
            }
            if (_currentId >= Int64.MaxValue) _currentId = Int64.MinValue;
        }
        /// <summary>
        /// CreateObject must be called to create new object rather than usage of operator new in implementation of method of derived classes
        /// </summary>
        /// <returns>The new object of the derived class</returns>
        public abstract NetBased CreateObject();
        /// <summary>
        /// if the derived class has defined CreateObject and Assign method, it doesn't need define Clone method
        /// </summary>
        /// <returns></returns>
        public virtual NetBased Clone()
        {
            NetBased o = CreateObject();
            o.Assign(this);
            return o;
        }
        /// <summary>
        /// Assign two derived class objects
        /// </summary>
        /// <param name="Source"></param>
        public abstract void Assign(Object Source);
    }
}
