using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetSimulation.Lib;
using BasicNet;
namespace NetConsole
{
    public class TextCommand
    {
        public string Command = "";
        public string Comment = "";
        public Dictionary<string, object> Parameter = new Dictionary<string,object>();
        public KeyValuePair<string, object>[] defaultParameter = null;
        public delegate void HandleFunction(Dictionary<string, object> Parameter);
        HandleFunction f = null;
        public static WorkManager<int, TextCommand> threadManager = new WorkManager<int, TextCommand>();
        static int threadID = 0;
        public TextCommand(string Command, string Comment, HandleFunction f, KeyValuePair<string, object>[] defaultParameter)
        {
            this.Command = Command;
            this.Comment = Comment;
            this.f = f;
            this.defaultParameter = defaultParameter;
        }
        void WorkingFunction(WorkManager<int, TextCommand> Context, int WorkID)
        {
            TextCommand cmd = Context.LocalVariables[WorkID];
            User.One.MessageToUser("\nParameters have been inputed:");
            int i = 0;
            foreach (var entry in Parameter)
            {
                User.One.MessageToUser(string.Format("{0}. {1} = {2}",++i, entry.Key, entry.Value));
            }
            string buffer = string.Format("\n\"{0}\" starting at {1} ...\n",cmd.Command,DateTime.Now.ToString());
            User.One.MessageToUser(buffer);
            Console.Title = buffer;
            try
            {
                f(Parameter);
            }
            finally
            {
                buffer = "\n\"" + cmd.Command + string.Format("\" finished at {0} !",DateTime.Now.ToString());
                User.One.MessageToUser(buffer);
                Console.Title = buffer;
            }
        }
        public void DoCommand()
        {
            User.One.MessageToUser("Command \"" + Command + "\" is selected!");
            object temp = null;
            if (defaultParameter.Length > 0)
                User.One.MessageToUser("Input parameters:");
            int i = 0;
            foreach (var Para in defaultParameter)
            {
                User.One.AskUserAnValue((++i).ToString(), Para.Key, Para.Value.GetType(), Para.Value, ref temp);
                if(Parameter.ContainsKey(Para.Key))
                    Parameter[Para.Key]= temp;
                else
                    Parameter.Add(Para.Key, temp);
            }
            threadManager.AddWork(threadID++, WorkingFunction, this);
            threadManager.Start();
        }
    }
}
