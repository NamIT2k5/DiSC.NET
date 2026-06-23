using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetSimulation.Lib;
using BasicNet;
using System.Threading;
namespace NetworkRobustness.Lib
{
    public class TextCommand
    {
        public bool isSysTask = false;
        public string Command = "";
        public string Comment = "";
        /// <summary>
        /// Dictionary with Order of the variable in the list of parameter, a pair of (variable name, its value)
        /// </summary>
        public Dictionary<int, KeyValuePair<string, object>> Parameter = new Dictionary<int, KeyValuePair<string, object>>();
        public KeyValuePair<string, object>[] defaultParameter = null;
        public delegate void HandleFunction(Dictionary<int,KeyValuePair<string, object>> Parameter);
        HandleFunction f = null;
        public static WorkManager<int, TextCommand> threadManager = new WorkManager<int, TextCommand>();
        static int workID = 0;
        /// <summary>
        /// Construct a command object
        /// </summary>
        /// <param name="Command">Command name</param>
        /// <param name="Comment">Comment for the comman</param>
        /// <param name="f">The function corresponding to the command</param>
        /// <param name="defaultParameter">Default parameter that is assigned to the function for running</param>
        public TextCommand(bool isSysTask, string Command, string Comment, HandleFunction f, KeyValuePair<string, object>[] defaultParameter)
        {
            this.Command = Command;
            this.Comment = Comment;
            this.f = f;
            this.isSysTask = isSysTask;
            this.defaultParameter = defaultParameter;
        }
        /// <summary>
        /// Function is call parallely
        /// </summary>
        /// <param name="Context">Data container</param>
        /// <param name="WorkID">ID of the thread</param>
        void WorkingFunction(WorkManager<int, TextCommand> Context, int WorkID)
        {
            TextCommand cmd = Context.LocalVariables[WorkID];
            Dictionary<int, KeyValuePair<string, object>> Params = cmd.Parameter;

            try
            {

                if (Params.Count > 0)
                {

                    User.One.MessageToUser("\n\tParameters have been inputed:");
                    int i = 0;
                    foreach (var entry in Params)
                    {
                        User.One.MessageToUser(string.Format("\t\t{0}- {1} = '{2}'", ++i, entry.Value.Key, entry.Value.Value));
                    }
                    User.One.MessageToUser("\nStarting...");
                }
                if (!cmd.isSysTask)
                {
                    string buffer = string.Format("\n\"{0}\" starting at {1} with task ID = {2}...\n", cmd.Command, DateTime.Now.ToString(), Thread.CurrentThread.ManagedThreadId);
                    User.One.MessageToUser(buffer);
                    Console.Title = buffer;
                }

                User.One.MessageToUser("\n");
                User.One.ShowWaitIndicator(0, -1);
                f(Params);
            }
            catch (ThreadAbortException threadAbort)
            {
                User.One.MessageToUser("-> "+ threadAbort.Message);
            }
            catch (Exception ex)
            {
                User.One.SendErrorToUser(ex);
            }
            finally
            {
                if (!cmd.isSysTask)
                {

                    string buffer = "\n\"" + cmd.Command + string.Format("\" finished at {0} !", DateTime.Now.ToString());
                    User.One.MessageToUser(buffer);
                    Console.Title = buffer;
                }
                if(cmd.Command!="clear")
                    User.One.MessageToUser("<----- End of [" + cmd.Command + "] ----->");
            }
        }
        /// <summary>
        /// To support for command show task
        /// </summary>
        public static Dictionary<int, string> DoneTasks = new Dictionary<int, string>();
        public void DoCommand()
        {
            try
            {
                
                User.One.MessageToUser("\n<----- Begin of [" + Command + "] ----->");
                if (defaultParameter != null)
                {
                    object temp = null;
                    if (defaultParameter.Length > 0)
                        User.One.MessageToUser("Type command's parameter:");
                    int i = 0;
                    foreach (var Para in defaultParameter)
                    {
                        User.One.AskUserAnValue("\t"+(1 + i).ToString(), Para.Key, Para.Value.GetType(), Para.Value, ref temp);
                        if (Parameter.ContainsKey(i))
                            Parameter[i] = new KeyValuePair<string, object>(Para.Key, temp);
                        else
                            Parameter.Add(i, new KeyValuePair<string, object>(Para.Key, temp));
                        i++;
                    }
                }
                DoneTasks.Add(workID, this.Command);
                threadManager.AddWork(workID++, WorkingFunction, this, WorkMode.ManagedWork);
           
                threadManager.Start();
                
            }
            catch (ThreadAbortException)
            {

                User.One.MessageToUser("[" + this.Command + "] was aborted");
            }
            catch(KeyNotFoundException key)
            {
                User.One.MessageToUser(key.Message);
            }
            catch (Exception ex)
            {
                User.One.SendErrorToUser(ex);
            }
        }
    }
}
