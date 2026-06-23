using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Collections.Concurrent;
using NetworkRobustness.Lib;
using NetSimulation.Lib;
using BasicNet;


namespace NetConsole
{
    class Program
    {
        static public ConsoleUser user = null;
        static public Program App = null;
        static bool IsQuit = false;
        public static TextCommand[] sysCommands = new TextCommand[]
        {
            new TextCommand(true,"test","Test program", OnTestCommand,null),
            new TextCommand(true,"help"," Show commands available in the software", OnHelpCommand,new KeyValuePair<string,object>[]
                {
                    new KeyValuePair<string,object>("\tThe command order to look up (-1 = all)",1),
                }),
             
                 new TextCommand(true,"quit"," Quit this application", OnQuitCommand,null),
                 new TextCommand(true,"clear"," Clear the screen", OnClearCommand,null),
                 new TextCommand(true,"task"," Show ID of running tasks", OnTaskCommand,null),
                 new TextCommand(true,"kill"," Kill running task(s)", OnKillCommand,new KeyValuePair<string,object>[]
                {
                    new KeyValuePair<string,object>("The taskID to kill (-1 = kill all)",-1)
                }),
                 new TextCommand(true,"lock"," Lock/unlock displaying messages", OnLockMessageCommand,null)
                 
        };
        public static IEnumerable<TextCommand> commands
        {
            get
            {
                foreach (var c in sysCommands)
                    yield return c;

                foreach (var c in CommandFunction.appCommands)
                    yield return c;
            }
        }
        static string LogFile = Directory.GetCurrentDirectory() + "\\ErrorLog1.txt";
        /// <summary>
        /// Run program with some parameters
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {


            /*
            string logFile = "errorlog.txt";
            try
            {
                
                
                //---> Begin GPS study
                if (args == null)
                {
                    TextDB.WriteTextFile("args is null", logFile);
                }
                else
                    if (args.Count() != 3)//GSP test
                    {
                        
                        TextDB.WriteTextFile("The program has no enough argument to run. It needs 3 arguments: gpsfilename, resultfilename, and size of cell:"+string.Join("--",args), logFile);

                        return;

                    }
                //App = new Program();
                //Console.CursorVisible = true;
                //user = new ConsoleUser();

                   
                    string sourceFile = args[0];
                    string outFile = args[1];
                    float R = Convert.ToSingle(args[2]);
                    NetworkRobustness.Geonetwork.GeoNet gpsNet = new NetworkRobustness.Geonetwork.GeoNet(R, sourceFile);
                    gpsNet.NetStudy(outFile);


                    
            }
            catch (Exception ex)
            {
                User.One.SendErrorToUser(ex);

            }
           */
            
            //---> End GPS study
            
            ConsoleTool.EnableCloseButton(false);
            ConsoleTool.MaximizeConsoleWindow();
                
                App = new Program();
                Console.CursorVisible = true;
                
                user = new ConsoleUser();
                
            
    #if DEBUG
               // App.ProcessCommand("40");
                //App.ProcessCommand("1");//for testing call
    #endif
            #if TRIAL
                if (DateTime.Today > new DateTime(2019, 5, 6))
                    return;
            #endif

                while (!IsQuit)
                {

                    string cmd = Console.ReadLine();
                    App.ProcessCommand(cmd.Trim());
                    Program.user.GoToPrompt();
                
                }
             
                 
           
        }
        
        /// <summary>
        /// Process user command
        /// </summary>
        /// <param name="cmd"></param>
        void ProcessCommand(string command)
        {

            if (command.Length <= 0)
            {
                User.One.MessageToUser("Program requires parameters to run!");
                return;
            }

            List<object> Para = new List<object>();
            object temp = 0;
            int cmdOrder = -1;
            bool isValid = false;
            try
            {
                cmdOrder = int.Parse(command)-1;
            }
            catch
            { }
            if (cmdOrder > -1 && cmdOrder < commands.Count())
            {
                isValid = true;
                var e = commands.ElementAt(cmdOrder);
                e.DoCommand();
                if (e.Command == "quit")
                    IsQuit = true;
                return;
            }
            
            foreach (var cmd in commands)
            {
                if (cmd.Command == command)
                {
                    isValid = true;
                    cmd.DoCommand();
                    if (command == "quit")
                        IsQuit = true;
                }
            }
            if (!isValid)
                User.One.MessageToUser("Command is incorrect!");
        }
        #region System functions
        private static void OnTestCommand(Dictionary<int, KeyValuePair<string, object>> Parameter)
        {
            //BasicNet.Examination.SignalingStudy.CalculateCompetitiveNetwork("Competition2set.txt", "7,8,9", "10,11", "none.txt");

            //BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile("Competition2set.txt");

            //BasicNet.Examination.SignalingStudy.CalculateLoyalMatrixPoint("karate.txt", "karate 1.txt");
            BasicNet.Examination.SignalingStudy.CalculateCompetitiveNetwork("test.txt", "1", "10", "1_10.txt");

            //Node pNode = null;
            //BasicNetwork newNet = net.CreateNetworkByMergedNode(new Node[] { net["7"], net["8"], net["9"] }, ref pNode);
            //Netutil.DumpNet(newNet);

            ////string fileName = "Markov.txt";
            //string fileName = "BinhAnh.txt";

            ////BooleanNetwork net = BooleanNetwork.ReadSignalingNetworkFile(fileName);
            //net.WriteToFile(null);
            //Dictionary<Node, float> carRanking = net.TaxiPassengerRank();
            //Dictionary<Node, float> pageRank = net.PageRankCentralityInLink();

            //int x = 0;
            //Dictionary<Node, int> reachingShell = net.R_ShellCentrality();
            //Dictionary<string, double> HCcentrality = net.HierarchicalClosenessCentrality();
            //Dictionary<string, Mathutil.Triple<double>> HCcentralitys = net.HierarchicalClosenessCentralityAnalysis();
            //var coreNode = from p in reachingShell where p.Value > reachingShell.Values.Min() select p.Key;
            
            //BasicNetwork.ReadNetworkFromKeggXML(net, "1926_Bladder cancer.xml");
            //net.WriteToFile("abc1234.txt");

        }
        private static void ConvertTxtToExcel(string Folder)
        {
            IEnumerable<string> files = Directory.EnumerateFiles(Netutil.InPutDirector + (Folder != "" ? "\\" + Folder : ""));

            string savingFolder = Netutil.OutPutDirector + "\\";
            foreach (string fileName in files)
            {
                string name = Netutil.ExtractMainFileName(Netutil.ExtractFileNameFromPath(fileName)) + ".xlsx";
                BooleanNetwork Net = BooleanNetwork.ReadSignalingNetworkFile(fileName);
                ExcelDB exFile = new ExcelDB(new string[] { "Node", "In-degree", "Out-degree" },"Degree");

                try
                {
                    
                    foreach(Node n in Net.Nodes)
                    {
                         exFile.WriteRow(new object[] {n.name,n.InDegree,n.OutDegree });
                    }
                    exFile.NewSheet("Network", new string[]{"Start","InteractionType","End"});
                    for (int i = 0; i < Net.Arcs.Count(); i++)
                    {
                        Interaction inter = Net.Arcs.ElementAt(i);
                        exFile.WriteRow(new object[] { inter.startNode.name, inter.Type, inter.endNode.name });
                    }
                    IEnumerable<Node> isolateNodes = Net.IsolateNodes;
                    if (isolateNodes.Count() > 0)
                    {

                        foreach (Node n in isolateNodes)
                        {
                            exFile.WriteRow(new object[] { string.Format("{0}", n.name) });
                        }
                    }
                    exFile.SaveToFile(name);
                }
                finally
                {
                    exFile.Dispose();
                }
                
            }
        }
        private static void OnComputingModularityFolderCommand(string Prefix, string Folder)
        {
            IEnumerable<string> files = Directory.EnumerateFiles(Netutil.InPutDirector + (Folder != "" ? "\\" + Folder : ""));
            int i = 1;
            
            string savingFolder=Netutil.OutPutDirector+"\\";
            foreach (string fileName in files)
            {
                string name=Netutil.ExtractFileNameFromPath(fileName);

                int endDot=name.IndexOf('.');
                endDot = name.IndexOf('.',endDot+1);
                name=Prefix+name.Remove(0,endDot+1);
                File.Move(fileName, savingFolder + name);
                User.One.ShowWaitIndicator(i++, files.Count());
            }
        }

        private static void OnTaskCommand(Dictionary<int, KeyValuePair<string, object>> Parameter)
        {
            ConcurrentDictionary<int, Thread> tasks = TextCommand.threadManager.ManagedWorks;
            int currentTaskCommand = tasks.Keys.Max();
            int couter = 0;
            User.One.MessageToUser("Running tasks:");

            foreach (var t in tasks)
            {
                if (t.Key == currentTaskCommand) continue;
                if (TextCommand.DoneTasks.ContainsKey(t.Key) && tasks[t.Key].IsAlive)
                {

                    User.One.MessageToUser(string.Format("\t{0}.\t {1,-25}\t [TaskID = {2}]", ++couter, TextCommand.DoneTasks[t.Key], t.Value.ManagedThreadId));
                }
            }

            if (couter == 0)
                User.One.MessageToUser("\tNo any running task!");
        }
        public static void OnHelpCommand(Dictionary<int, KeyValuePair<string, object>> Parameter)
        {
            
            int i = 0;
            int cmdOrder = -1;
            bool details = (Parameter == null ? false : true);

            if (Parameter != null)
            {
                cmdOrder=Convert.ToInt32(Parameter[0].Value);
                if (cmdOrder > commands.Count() || cmdOrder<-1)
                {
                    User.One.MessageToUser(string.Format("Command order is invalid!\nIt should be in the range of [{0}..{1}]\n",1,commands.Count()));
                    return;
                }
            }
            User.One.MessageToUser(string.Format("{0,6}\t{1,-25}\t{2,-140}", "No.", "Command name", "Comment"));
            User.One.MessageToUser(string.Format("{0,6}\t{1,-25}\t{2,-140}", "".PadRight(6, '.'), "".PadRight(25, '.'), "".PadRight(140, '.')));

            foreach (var e in commands)
            {
                i++;
                if (!details)
                {

                    User.One.MessageToUser(string.Format("{0,6}.\t{1,-25}\t{2,-140}", i, e.Command, e.Comment));
                }
                else
                {
                    if (i == cmdOrder || cmdOrder == -1)
                    {
                        User.One.MessageToUser(string.Format("{0,6}.\t{1,-25}\t{2,-140}", i, e.Command, e.Comment));
                        int j = 0;
                        if (e.defaultParameter != null)
                        {
                            User.One.MessageToUser("\n\tInput requirement:");
                            if (e.defaultParameter.Count() == 1)
                            {
                                User.One.MessageToUser(string.Format("\t\t- {0}:", e.defaultParameter.ElementAt(0).Key));

                            }
                            else
                                foreach (var p in e.defaultParameter)
                                {
                                    User.One.MessageToUser(string.Format("\t\t{0}- {1}", ++j, p.Key));
                                }
                        }
                        User.One.MessageToUser("\n");
                    }
                }
            }
            if (!details)
            {
                User.One.MessageToUser("\nExecute the command by entering its name or number!");
            }
            //Console.ResetColor();
        }
        private static void OnClearCommand(Dictionary<int, KeyValuePair<string, object>> Parameter)
        {
            User.One.Clear();

        }
        private static void OnQuitCommand(Dictionary<int, KeyValuePair<string, object>> Parameter)
        {
            //TextCommand.threadManager.AbortAll();
        }
        private static void OnLockMessageCommand(Dictionary<int, KeyValuePair<string, object>> Parameter)
        {
           User.One.LockWritingMessage();
            
        }
        private static void OnKillCommand(Dictionary<int, KeyValuePair<string, object>> Parameter)
        {
            int ThreadID = Convert.ToInt32(Parameter[0].Value);
            if (ThreadID == -1)
            {
                
                User.One.MessageToUser("All running tasks were killed!");
                TextCommand.threadManager.AborAllManagedThreadExcept(Thread.CurrentThread.ManagedThreadId);
                
            }
            else
            {
                if (!TextCommand.threadManager.isManagedThreadRunning(ThreadID))
                {
                    User.One.MessageToUser(string.Format("TaskID {0} isn't valid!", ThreadID));
                    return;
                }

                TextCommand.threadManager.AborManagedThread(ThreadID);
                User.One.MessageToUser("Task " + ThreadID.ToString() + " was killed!");
            }

        }
        #endregion
        
    }
}
