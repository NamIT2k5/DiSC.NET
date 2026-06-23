using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
namespace BasicNet
{
    /// <summary>
    /// ManageWork can be killed susddenly
    /// </summary>
    public enum WorkMode { PoolingWork = 0, ManagedWork = 1, TaskSchedule=2 };
    public class WorkManager<GlobalType, LocalType> : IDisposable
    {
        #region The same useage for two thread modes
        public delegate void WORK(WorkManager<GlobalType, LocalType> Context, int WorkID);
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


                return sharedVariable;

            }
            set
            {
                lock (_Lock)
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


        //~WorkManager()
        //{
        //    AbortAll();
        //}
        public void Dispose()
        {
            AbortAll();
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        #endregion


        public void AbortAll()
        {
            AbortAllManagedWork();// Just abort managed threads only
            AbortAllTask();
        }


        #region Pooling threads only

        /// <summary>
        /// Add a thread and its local variable to the dictionary of manager
        /// </summary>
        /// <param name="WorkID">The Work ID for the thread adding</param>
        /// <param name="initLocalVariable">The local variable of the thread</param>
        private void AddWorkData(int WorkID, LocalType initLocalVariable)
        {
            if (!LocalVarable.TryAdd(WorkID, initLocalVariable))
                throw new Exception("threads.TryAdd() failed when it should have succeeded!");

        }

        #endregion



        #region  Managed threads mode only

        private ConcurrentDictionary<int, Thread> threads = new ConcurrentDictionary<int, Thread>();
        public ConcurrentDictionary<int, Thread> ManagedWorks
        {
            get
            {
                
                return threads;
            }
        }
        public int TotalCompletedTask
        {
            get
            {
                return (threads.Count - threads.Where(t => t.Value.IsAlive).Count()) + tasks.Where(t => t.Value.IsCompleted ).Count();
                
            }
        }
        public int TotalTask
        {
            get
            {
                return threads.Count+tasks.Count;

            }
        }
        /// <summary>
        /// Add a thread and its local variable to the dictionary of manager
        /// </summary>
        /// <param name="WorkID">The Work ID for the thread adding</param>
        /// <param name="thread">The thread to add</param>
        /// <param name="initLocalVariable">The local variable of the thread</param>
        private void AddWorkData(int WorkID, Thread thread, LocalType initLocalVariable)
        {
            if (!threads.TryAdd(WorkID, thread))
                throw new Exception("threads.TryAdd() failed when it should have succeeded!");
            if (!LocalVarable.TryAdd(WorkID, initLocalVariable))
                throw new Exception("threads.TryAdd() failed when it should have succeeded!");

        }
        /// <summary>
        /// Remove thread an its local variable from the dictionary of manager
        /// </summary>
        /// <param name="WorkID">ID's thread to remove</param>
        private bool killManagedWork(int WorkID)
        {
            if (!threads.ContainsKey(WorkID))
                return false;
            if (threads[WorkID].IsAlive)
                threads[WorkID].Abort();
            return _removedThread(WorkID);
        }
        private bool _removedThread(int WorkID)
        {
            Thread removedThread = null;
            if (!threads.TryRemove(WorkID, out removedThread))
                return false;

            LocalType Obj;
            if (!LocalVarable.TryRemove(WorkID, out Obj))
                return false;
            return true;
        }
        public void AbortAllManagedWork()
        {
            while (threads.Count > 0)
                killManagedWork(threads.ElementAt(0).Key);
        }
        /// <summary>
        /// Abort and remove thread from system except the WorkID
        /// </summary>
        /// <param name="WorkID">The WorkID not be killed</param>
        public void AbortAllManagedWorkExcept(int WorkID)
        {
            
           while(threads.Count>0)
            {
               if(threads.LastOrDefault().Key==WorkID)
                   _removedThread(WorkID);
               else
                    killManagedWork(threads.LastOrDefault().Key);
            }

        }
        /// <summary>
        /// Abort and remove thread from system by its workID
        /// </summary>
        /// <param name="WorkID">ID of the thread to remove</param>
        /// <returns></returns>
        public bool AborManagedWork(int WorkID)
        {

            return killManagedWork(WorkID);

        }
        /// <summary>
        /// Abort and remove thread from system by its threadID
        /// </summary>
        /// <param name="ThreadID"></param>
        /// <returns></returns>
        public bool AborManagedThread(int ThreadID)
        {
           KeyValuePair<int, Thread> t= FindManagedThread(ThreadID);
            if(t.Value!=null)
            {
                killManagedWork(t.Key);
                return true;
            }
            return false;

        }
        /// <summary>
        ///  Abort and remove all threads from system except threadID
        /// </summary>
        /// <param name="ThreadID">The threadID not to be killed</param>
        /// <returns></returns>
        public bool AborAllManagedThreadExcept(int ThreadID)
        {
            KeyValuePair<int, Thread> t = FindManagedThread(ThreadID);
            if (t.Value != null)
            {
                AbortAllManagedWorkExcept(t.Key);
                return true;
            }
            return false;

        }
        /// <summary>
        /// Find the managed thead (WorkID) by its ID
        /// </summary>
        /// <param name="ThreadID">The thead ID (obtained by Thread.CurrentThread.ManagedThreadId) </param>
        /// <returns>(-1,null) if found no thread</returns>
        public KeyValuePair<int, Thread> FindManagedThread(int ThreadID)
        {
            
            foreach (var e in ManagedWorks)
            {
                if (e.Value.ManagedThreadId == ThreadID)
                {
                    return e;
                }
            }
            return new KeyValuePair<int,Thread>(-1,null);
        }
        /// <summary>
        /// Find thread by its workID
        /// </summary>
        /// <param name="WorkID">WorkID of the thread</param>
        /// <returns></returns>
        public Thread ManagedWork(int WorkID)
        {
            return threads[WorkID];
        }
        /// <summary>
        /// If the work identified by WorkID is running?
        /// </summary>
        /// <param name="WorkID">WorkID</param>
        /// <returns></returns>
        public bool isManagedWorkRunning(int WorkID)
        {
            if (threads.ContainsKey(WorkID) &&
                threads[WorkID].IsAlive)
                return true;
            else
                return false;
        }
        public bool isManagedThreadRunning(int ThreadID)
        {
            KeyValuePair<int, Thread> t = FindManagedThread(ThreadID);
            if (t.Value != null)
                return isManagedWorkRunning(t.Key);
             return false;
        }
        #endregion
        #region Task
        private ConcurrentDictionary<int, Task> tasks = new ConcurrentDictionary<int, Task>();

        private void AddWorkData(int WorkID, Task task, LocalType initLocalVariable)
        {
            if (!tasks.TryAdd(WorkID, task))
                throw new Exception("taks.TryAdd() failed when it should have succeeded!");


            if (!LocalVarable.TryAdd(WorkID, initLocalVariable))
                throw new Exception("taks.TryAdd() failed when it should have succeeded!");

        }
        public void AbortAllTask()
        {
            while (tasks.Count > 0)
                RemoveTask(tasks.ElementAt(0).Key);
        }
        private bool RemoveTask(int WorkID)
        {
            if (!tasks.ContainsKey(WorkID))
                return false;
            if (!tasks[WorkID].IsCompleted)
                throw new Exception("Task "+WorkID.ToString()+"is still running so cannot be removed!");

            Task removedTask = null;
            if (!tasks.TryRemove(WorkID, out removedTask))
                return false;
            LocalType Obj;
            if (!LocalVarable.TryRemove(WorkID, out Obj))
                return false;
            return true;

        }
        #endregion
        #region Switch between two thread mode

        
        public void Wait4WorksDone()
        {
            // Wait for pooling threads first
            poolingWorksDone.WaitOne();

            Task.WaitAll(tasks.Values.ToArray(), -1);

            // Then wait for managed threads
            foreach (KeyValuePair<int, Thread> thr in threads)
            {
                if ((thr.Value.ThreadState & System.Threading.ThreadState.Unstarted) == System.Threading.ThreadState.Unstarted)
                    throw new Exception("Chua chay!");
                else
                thr.Value.Join();
            }

        }
        //public enum WorkMode{PoolingWork=0, ManagedWork=1};
        private int numberOftasks = 0;
        ManualResetEvent poolingWorksDone = new ManualResetEvent(true);
        ManualResetEvent startWorking = new ManualResetEvent(false);
        public void Reset()
        {
            numberOftasks=0;
            poolingWorksDone.Reset();
            startWorking.Reset();
            AbortAllManagedWork();
            
        }
        int _workCounter = 0;
        /// <summary>
        /// Starting a work (a managed or pooling thread)
        /// </summary>
        /// <param name="WorkID">The ID of the work</param>
        /// <param name="WorkingFunction">The function to run or work with its prototype: void WORK(WorkManager<GlobalType, LocalType> Context, int WorkID);</param>
        /// <param name="initLocalVariable">Local variable for this work</param>
        /// <param name="workMode">Work mode; 
        /// + PoolingWork: unmanaged work (use CPU effectively but can not stop, suspend the work) </param>
        /// + ManagedWork: managed work (can stop, suspend the work through its managedthread
        /// <returns>WorkID</returns>
        public int AddWork(int WorkID, WORK WorkingFunction, LocalType initLocalVariable, WorkMode workMode = WorkMode.PoolingWork)
        {
            
            if (WorkID == -1)
                WorkID = _workCounter;

            if (workMode == WorkMode.TaskSchedule)
            {

                Task task = new Task(delegate()
                {
                    WorkingFunction(this, WorkID);
                }
                );
                AddWorkData(WorkID, task, initLocalVariable);
                

            }
            else if (workMode == WorkMode.PoolingWork)//Cases to use ThreadPool: http://blogs.msdn.com/b/pedram/archive/2007/08/05/dedicated-thread-or-a-threadpool-thread.aspx
            {
                Interlocked.Increment(ref numberOftasks);
                poolingWorksDone.Reset();// set = false to start
                startWorking.Reset();
                AddWorkData(WorkID, initLocalVariable);

                ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(Object stateInfo)
                {

                    startWorking.WaitOne();

                    WorkingFunction(this, WorkID);
                    
                    if (Interlocked.Decrement(ref numberOftasks) == 0)
                        poolingWorksDone.Set();
                })
                );
            }
            else// if workMode == ManagedWork
            {
                Thread thread = new Thread(delegate()
                {
                    WorkingFunction(this, WorkID);
                }
                    );
                thread.Priority = ThreadPriority.AboveNormal;
                thread.Name = WorkID.ToString();
                thread.IsBackground = true;//for automatically killing this background thread when the main thread is terminated

                AddWorkData(WorkID, thread, initLocalVariable);
                
                 // Thread.Sleep(haltedTimeForRun); // halted briefly to the thread run several times //http://msdn.microsoft.com/en-us/library/7a2f3ay4(v=VS.80).aspx
                
            }
            Interlocked.Increment(ref _workCounter);
            return WorkID;

        }
        
        /// <summary>
        /// Start a work immediately 
        /// </summary>
        /// <param name="WorkID">Work ID </param>
        /// <param name="WorkingFunction">Work to do</param>
        /// <param name="initLocalVariable">Variable for the work</param>
        /// <param name="workMode">Work mode:</param>
        /// + PoolingWork: unmanaged work (use CPU effectively but can not stop, suspend the work) </param>
        /// + ManagedWork: managed work (can stop, suspend the work through its managedthread
        /// <returns>WorkID</returns>
        public void Start(int WorkID, WORK WorkingFunction, LocalType initLocalVariable, WorkMode workMode = WorkMode.PoolingWork)
        {
            if (workMode == WorkMode.TaskSchedule)
            {
                
                Task task = new Task(delegate()
                {
                    WorkingFunction(this, WorkID);
                }
                );
                AddWorkData(WorkID, task, initLocalVariable);
                task.Start();
                
            }
            else if (workMode == WorkMode.PoolingWork)//Cases to use ThreadPool: http://blogs.msdn.com/b/pedram/archive/2007/08/05/dedicated-thread-or-a-threadpool-thread.aspx
            {

                Interlocked.Increment(ref numberOftasks);
                poolingWorksDone.Reset();// set = false to start
                AddWorkData(WorkID, initLocalVariable);

                ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(Object stateInfo)
                {
                    WorkingFunction(this, WorkID);

                    if (Interlocked.Decrement(ref numberOftasks) == 0)
                        poolingWorksDone.Set();
                })
                );
            }
            else
            {
                Thread thread = new Thread(delegate()
                {
                    WorkingFunction(this, WorkID);
                }
                    );
                thread.Priority = ThreadPriority.AboveNormal;
                thread.Name = "Worker.tiendzung";
                thread.IsBackground = true;//for automatically killing this background thread when the main thread is terminated


                AddWorkData(WorkID, thread, initLocalVariable);

                thread.Start();
                // Thread.Sleep(haltedTimeForRun); // halted briefly to the thread run several times //http://msdn.microsoft.com/en-us/library/7a2f3ay4(v=VS.80).aspx

            }

           

        }
        /// <summary>
        /// Start threads in the queue of works
        /// </summary>
        public void Start()
        {
            foreach (Task task in this.tasks.Values)
                 task.Start();
            


            startWorking.Set();

            foreach (Thread thr in this.threads.Values)
                if ((thr.ThreadState & System.Threading.ThreadState.Unstarted) == System.Threading.ThreadState.Unstarted)
                    thr.Start();

        }
        
        
        #endregion
    }
}
