using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;
namespace BasicNet
{
    public class WorkManager<GlobalType, LocalType>:IDisposable
    {
        #region Property
        public delegate void WORK(WorkManager<GlobalType,LocalType> Context, int WorkID);
        private ConcurrentDictionary<int, Thread> threads = new ConcurrentDictionary<int, Thread>();
        /// <summary>
        /// Variables for each thread identified by WorkID
        /// </summary>
        public ConcurrentDictionary<int, LocalType> LocalVarable = new ConcurrentDictionary<int, LocalType>();
        
        public GlobalType sharedVariable;
        private readonly Object _Lock = new Object();
        /// <summary>
        /// The variable shared between threads
        /// </summary>
        public GlobalType GlobalVariable
        {
            get
            {
                
                lock (_Lock)
                {
                    return sharedVariable;
                }
            }
            set
            {
                lock(_Lock)
                {
                    sharedVariable = value;
                }
            }
        }
        public ConcurrentDictionary<int, LocalType> LocalVariables
        {
            get
            {
                return this.LocalVarable;
            }
        }

        public LocalType GetLocalVariable(int WorkID)
        {
            lock (_Lock)
            {
                return LocalVarable[WorkID];
            }
        }
        public void SetLocalVariable(int WorkID, LocalType Val)
        {
            lock (_Lock)
            {
                LocalVarable[WorkID] = Val;
            }
        }

        public Thread GetWork(int WorkID)
        {
            return threads[WorkID];
        }
        public IEnumerable<Thread> GetWork(string WorkName)
        {
            return (from p in threads where p.Value.Name == WorkName select p.Value);
        }

        #endregion

        ~WorkManager()
        {
            AbortAllWork();
        }
        public void Dispose()
        {
            AbortAllWork();
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }


        public void StartAllWorks()
        {

            foreach (KeyValuePair<int, Thread> thr in threads)
            {
                thr.Value.Start();
            }
        }

        //private const int haltedTimeForRun = 100;
        public void Wait4WorksDone()
        {

            foreach (KeyValuePair<int, Thread> thr in threads)
            {
                thr.Value.Join();
            }
            //foreach (KeyValuePair<int, LocalType> val in LocalVarable)
            //{
            //    Console.Write(string.Format("(WorkID ={0} Val ={1})\t",val.Key,val.Value));

            //}
        }
        public int StartWork(int ThreadID, WORK WorkingFunction, LocalType initLocalVariable, bool IsStart = true)
        {
           
           
            
            Thread thread = new Thread(delegate()
            {
                WorkingFunction(this, ThreadID);
            }
            );
            thread.Priority = ThreadPriority.AboveNormal;
            thread.Name = "Worker.tiendzung";
            thread.IsBackground = true;//for automatically killing this background thread when the main thread is terminated


            AddWork(ThreadID, thread, initLocalVariable);
            if (IsStart)
            {
                //Debug.WriteLine("Thread started with local var: " + initLocalVariable.ToString() + "  WorkID =" + ThreadID.ToString());
                thread.Start();
                //while (!thread.IsAlive) ; //waiting for the thread is initialized
               // Thread.Sleep(haltedTimeForRun); // halted briefly to the thread run several times //http://msdn.microsoft.com/en-us/library/7a2f3ay4(v=VS.80).aspx
            }



            return ThreadID;
            
        }
        
        /// <summary>
        /// Add a thread and its local variable to the dictionary of manager
        /// </summary>
        /// <param name="WorkID">The Work ID for the thread adding</param>
        /// <param name="thread">The thread to add</param>
        /// <param name="initLocalVariable">The local variable of the thread</param>
        private void AddWork(int WorkID, Thread thread, LocalType initLocalVariable)
        {
            if(!threads.TryAdd(WorkID, thread))
                throw new Exception("threads.TryAdd() failed when it should have succeeded!");
            if(!LocalVarable.TryAdd(WorkID, initLocalVariable))
                throw new Exception("threads.TryAdd() failed when it should have succeeded!");

        }
        /// <summary>
        /// Remove thread an its local variable from the dictionary of manager
        /// </summary>
        /// <param name="WorkID">ID's thread to remove</param>
        private void RemoveWork(int WorkID)
        {
            if(threads[WorkID].IsAlive)
                threads[WorkID].Abort();
            
            Thread removedThread = null;
            if (!threads.TryRemove(WorkID, out removedThread))
                throw new Exception("threads.TryRemove() failed when it should have succeeded!");
            LocalType Obj;
            if(!LocalVarable.TryRemove(WorkID, out Obj))
                throw new Exception("threads.TryRemove() failed when it should have succeeded!");

        }
        /// <summary>
        /// Abort and remove thread from system
        /// </summary>
        /// <param name="WorkID">ID of the thread to remove</param>
        /// <returns></returns>
        public bool AbortWork(int WorkID)
        {
            if(threads.ContainsKey(WorkID))
            {
                RemoveWork(WorkID);
                return true;
            }
            return false;
        }
        public void AbortAllWork()
        {
            while (threads.Count > 0)
                RemoveWork(threads.ElementAt(0).Key);
            
        }
    }
}
