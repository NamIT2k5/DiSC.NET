/*  
  Copyright 2007-2010 The NGenerics Team
 (http://code.google.com/p/ngenerics/wiki/Team)

 This program is licensed under the GNU Lesser General Public License (LGPL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://www.gnu.org/copyleft/lesser.html.
*/


using System.Collections.Generic;
using NetSimulation.Lib;
using System.Diagnostics;

namespace BasicNet
{
    #region IVisitor interface
    public interface IVisitor<T>
    {
        /// <summary>
        /// Gets a value indicating whether this instance is done performing it's work..
        /// </summary>
        /// <value><c>true</c> if this instance is done; otherwise, <c>false</c>.</value>
        bool HasCompleted { get; }

        /// <summary>
        /// Visits the specified object.
        /// </summary>
        /// <param name="obj">The object to visit.</param>
        void Visit(T obj);
    }
    #endregion
    #region TrackingVisitor class
    /// <summary>
    /// A visitor that tracks (stores) objects in the order they were visited.
    /// Handy for demonstrating and testing different ordered visits implementations on
    /// data structures.
    /// </summary>
    /// <typeparam name="T">The type of objects to be visited.</typeparam>
    public sealed class TrackingVisitor<T> : IVisitor<T>
    {
        #region Globals

        private readonly List<T> tracks;

        #endregion

        #region Construction


        /// <inheritdoc/>
        public TrackingVisitor()
        {
            tracks = new List<T>();
        }

        #endregion

        #region IVisitor<T> Members
        /// <inheritdoc />
        public void Visit(T obj)
        {
            tracks.Add(obj);
        }

        /// <inheritdoc />
        public bool HasCompleted
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Gets the tracking list.
        /// </summary>
        /// <value>The tracking list.</value>        
        public IList<T> TrackingList
        {
            get
            {
                return tracks;
            }
        }

        #endregion
    }
    #endregion
    #region OrderedVisitor class
    /// <summary>
    /// A visitor that visits objects in order (PreOrder, PostOrder, or InOrder).
    /// Used primarily as a base class for Visitors specializing in a specific order type.
    /// </summary>
    /// <typeparam name="T">The type of objects to be visited.</typeparam>
    public class OrderedVisitor<T> : IVisitor<T>
    {
        #region Globals

        //private readonly IVisitor<T> visitorToUse;

        #endregion

        #region Construction
        public enum OrderType { PreOrder=0, PostOrder=1 };
        OrderType orderType = OrderType.PreOrder;
        /// <param name="visitorToUse">The visitor to use when visiting the object.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitorToUse"/> is a null reference (<c>Nothing</c> in Visual Basic).</exception>
        public OrderedVisitor(OrderType orderType)
        {
            this.orderType = orderType;
            
        }

        #endregion

        #region IOrderedVisitor<T> Members

        /// <summary>
        /// Determines whether this visitor is done.
        /// </summary>
        /// <value></value>
        /// <returns>
        ///     <c>true</c> if this visitor is done; otherwise, <c>false</c>.
        /// </returns>
        public bool HasCompleted
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Visits the object in pre order.
        /// Return false if stopping the search
        /// parent is the parent node on the link (not in the order)
        /// </summary>
        /// <param name="obj">The obj.</param>         
        
        public virtual bool VisitPreOrder(T parent,T obj)
        {
            return true;
        }

        /// <summary>
        /// Visits the object in post order.
        /// Return false if stopping the search
        /// parent is the parent node on the link (not in the order)
        /// </summary>
        /// <param name="obj">The obj.</param>        
        public virtual bool VisitPostOrder(T parent, T obj)
        {
            return true;
        }

        public void Visit(T obj)
        {
        }

        #endregion

    }
    #endregion
    #region DummyVisitor classs
    /// <summary>
    /// A dummy visitor - that does absolutely nothing with visits.
    /// Believe it or not, it's actually useful in some situations.
    /// </summary>
    /// <typeparam name="T">The type of item to visit.</typeparam>
    public class DummyVisitor<T> : IVisitor<T>
    {
        #region IVisitor<T> Members

        /// <inheritdoc />
        public bool HasCompleted
        {
            get { return false; }
        }

        /// <inheritdoc />
        public void Visit(T obj)
        {
            
        }

        #endregion
    }
#endregion
    #region DebugVisitor classs
    /// <summary>
    /// A dummy visitor - that does absolutely nothing with visits.
    /// Believe it or not, it's actually useful in some situations.
    /// </summary>
    /// <typeparam name="T">The type of item to visit.</typeparam>
    public class DebugVisitor : IVisitor<Node>
    {
        #region IVisitor<T> Members

        /// <inheritdoc />
        public bool HasCompleted
        {
            get { return false; }
        }

        /// <inheritdoc />
        public void Visit(Node obj)
        {
            Debug.Write(obj.name);
        }

        #endregion
    }
    #endregion
}
