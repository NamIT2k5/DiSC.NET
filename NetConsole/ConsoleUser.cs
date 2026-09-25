using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetSimulation.Lib;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using BasicNet;
using NetworkRobustness.Lib;
namespace NetConsole
{
    public class ConsoleUser : User
    {
        public void ShowHelloText()
        {
            int widthBar = 80;
            try { widthBar = Console.BufferWidth; } catch { }
            string line = string.Format("").PadRight(widthBar, ' ');
                string Program = "  NetCmd is a network analysis program.".PadRight(widthBar);
                string Author = "  Authors: Tien-Dzung Tran (1,2) and Yung-Keun Kwon* (1)".PadRight(widthBar);
                string Address1 = "  1. Complex System Computing Lab, School of Electrical Engineering, University of Ulsan, South Korea".PadRight(widthBar);
                string Address2 = "  2. Faculty of Information Technology, Hanoi University of Industry, Vietnam".PadRight(widthBar);
                string Help = "  Input files are loaded from folder 'Input', and Output files are exported to folder 'Output' by default.".PadRight(widthBar);

                //MessageToUserColor(line, ConsoleColor.Gray, ConsoleColor.Gray);
                MessageToUserColor(Program, ConsoleColor.Black, ConsoleColor.Gray);
                MessageToUserColor(Author, ConsoleColor.Black, ConsoleColor.Gray);
                MessageToUserColor(Address1, ConsoleColor.Black, ConsoleColor.Gray);
                MessageToUserColor(Address2, ConsoleColor.Black, ConsoleColor.Gray);

                
                MessageToUserColor(Help, ConsoleColor.DarkGray, ConsoleColor.Black);
                MessageToUser("\n");
                //return "*****************************************************************************************************************************************\n* The simulation software to investigate the relationship between modularity and robustness against mutation on signaling networks. \t*\n* Authors: Tien-Dzung Tran (1,2) and Yung-Keun Kwon* (1)\t\t\t\t\t\t\t\t\t\t\t*\n* 1. School of Electrical Engineering, University of Ulsan, South Korea\t\t\t\t\t\t\t\t\t\t*\n* 2. School of Information Technology, Hanoi University of Industry, Hanoi, Vietnam\t\t\t\t\t\t\t\t*\n*****************************************************************************************************************************************\n\n\t[Using data folders: 'Input' and 'Output']. Type 'help' for our guide";
            
        }
       

        public ConsoleUser()
        {
            User.One = this;
            ConsoleTool.EnableVTProcessing();
            try { Console.BufferHeight = Int16.MaxValue - 1; } catch { }
            try { Console.BufferWidth = Math.Max(Console.WindowWidth, 300); } catch { }
            Clear();
            InitalizeProgressor();
        }
        ~ConsoleUser()
        {
            taskManager.AbortAllTask();
        }
        public override void BeginWait(string reason, int maximumStep, int minimumStep = 0)
        {
        }
        public override void EndWait(string reason)
        {
        }
        int _Left = 0, _Top = 0;
        /// <summary>
        /// Save cursor to smoothly write text
        /// </summary>
        private void saveCursor()
        {
            _Left = Console.CursorLeft;
            _Top = Console.CursorTop;
        }
        private void restoreCursor()
        {
            Console.CursorLeft = _Left;
            Console.CursorTop = _Top;
        }
        const int IndicatorTop = 80;
        #region Progressor
        private ConcurrentDictionary<Thread, KeyValuePair<int, int>> ProgressManager = new ConcurrentDictionary<Thread, KeyValuePair<int, int>>();
        private ConcurrentDictionary<Thread, DateTime> TimerProgressManager = new ConcurrentDictionary<Thread, DateTime>();
        WorkManager<int, int> taskManager = new WorkManager<int, int>();
        public void InitalizeProgressor()
        {
            taskManager.AddWork(-1, ShowProgess, 0);
            taskManager.Start();
        }
        void ShowProgess(WorkManager<int, int> Context, int WorkID)
        {
            string buffer = "";
            while (true)
            {
                if (ProgressManager.Count > 0)
                {
                    // Clean up dead threads
                    foreach (var t in ProgressManager.Keys.ToList())
                    {
                        if (!t.IsAlive)
                        {
                            KeyValuePair<int, int> status;
                            ProgressManager.TryRemove(t, out status);
                            DateTime removetime;
                            TimerProgressManager.TryRemove(t, out removetime);
                        }
                    }

                    if (ProgressManager.Count > 0)
                    {
                        // Find the total target step count (e.g. 1000)
                        int total = ProgressManager.Values.Where(v => v.Value > 0).Select(v => v.Value).FirstOrDefault();
                        int completed = ProgressManager.Values.Where(v => v.Value > 0).Select(v => v.Key).DefaultIfEmpty(0).Max();

                        if (total > 0)
                        {
                            double percent = (double)completed / total * 100.0;
                            
                            // Calculate remaining time based on earliest thread start time
                            string timer = "";
                            DateTime earliestTime = DateTime.MaxValue;
                            foreach (var t in ProgressManager.Keys)
                            {
                                if (TimerProgressManager.TryGetValue(t, out DateTime startTime))
                                {
                                    if (startTime < earliestTime) earliestTime = startTime;
                                }
                                else if (ProgressManager[t].Key > 0)
                                {
                                    DateTime now = DateTime.Now;
                                    TimerProgressManager.TryAdd(t, now);
                                    if (now < earliestTime) earliestTime = now;
                                }
                            }

                            if (earliestTime != DateTime.MaxValue && completed > 0)
                            {
                                TimeSpan ts = TimeSpan.FromTicks(DateTime.Now.Subtract(earliestTime).Ticks * (total - completed) / completed);
                                timer = string.Format(" remaining time: {0:D2}h:{1:D2}m:{2:D2}s",
                                                ts.Hours + ts.Days * 24,
                                                ts.Minutes,
                                                ts.Seconds);
                            }

                            buffer = string.Format("[Progress: {0}/{1} ({2:F1}%)] [Active Threads: {3}]{4}",
                                completed, total, percent, ProgressManager.Count, timer);

                            if (completed >= total)
                            {
                                foreach (var t in ProgressManager.Keys.ToList())
                                {
                                    ProgressManager[t] = new KeyValuePair<int, int>(0, -1);
                                    DateTime removetime;
                                    TimerProgressManager.TryRemove(t, out removetime);
                                }
                            }
                        }
                        else
                        {
                            buffer = string.Format("[Running: {0} active thread(s)]", ProgressManager.Count);
                        }

                        lock (LockedObj)
                        {
                            int consoleWidth = 79;
                            try { consoleWidth = Math.Max(79, Console.WindowWidth - 1); } catch { }
                            string printBuffer = buffer.Length > consoleWidth ? buffer.Substring(0, consoleWidth) : buffer.PadRight(consoleWidth);
                            Console.Write("\r" + printBuffer);
                        }
                    }
                }
                Thread.Sleep(1000); 
            }
        }
        public override void ShowWaitIndicator(int atStep, int totalStep)
        {
            Thread t = Thread.CurrentThread;
            KeyValuePair<int, int> ci =new KeyValuePair<int, int>(atStep, totalStep);
            ProgressManager.AddOrUpdate(t, ci, (key, existingVal) =>
            {
                return ci;
            });


        }
       
        
        #endregion
        private delegate void MessageToUserDelegate(String message);
        int PromptLeft = 0;
        int Messageline = -1;
        
        public void GoToPrompt()
        {
            lock (LockedObj)
            {
                const string Cmd = "Type command:";
                // Xóa dòng hiện tại (có thể chứa progress) rồi in prompt
                Console.Write("\r\x1B[2K" + Cmd + " ");
            }
        }
        private readonly object ConsoleLock = new object();

        private void EraseLine(int line)
        {
            lock (ConsoleLock)
            {
                int left = Console.CursorLeft;
                int top = Console.CursorTop;

                int maxTop = Math.Max(Console.BufferHeight - 1, 0);
                int safeLine = Math.Min(Math.Max(line, 0), maxTop);

                Console.SetCursorPosition(0, safeLine);
                Console.Write(new string(' ', Console.BufferWidth));
                // restore cursor: ensure it's in range too
                int safeTop = Math.Min(Math.Max(top, 0), maxTop);
                int safeLeft = Math.Min(Math.Max(left, 0), Console.BufferWidth - 1);
                Console.SetCursorPosition(safeLeft, safeTop);
            }
        }
        public bool lockWritingMessage = false;
        public override bool LockWritingMessage()
        {
            if(lockWritingMessage==false)
                this.MessageToUserColor("Lock messages written on the screen!", ConsoleColor.White, ConsoleColor.Black);
            lockWritingMessage = !lockWritingMessage;
            if (lockWritingMessage == false)
                this.MessageToUserColor("Unlock messages written on the screen!", ConsoleColor.White, ConsoleColor.Black);
            GoToPrompt();
            return lockWritingMessage;
        }
        object LockedObj = new object();
        public override void MessageToUser(string strMessage)
        {
            if (lockWritingMessage)
                return;
            string[] lineStrings = strMessage.Split(new char[] { '\n' });
            lock (LockedObj)
            {
                // Xóa dòng hiện tại (có thể đang chứa progress) trước khi in message
                Console.Write("\r\x1B[2K");
                foreach (string s in lineStrings)
                {
                    Interlocked.Increment(ref Messageline);
                    Console.WriteLine(s);
                }
            }
        }
        int BufferWith
        {
            get
            {
                return Console.BufferWidth;
            }
        }
        public void MessageToUserColor(string strMessage, ConsoleColor ForegroundColor, ConsoleColor BackgroundColor)
        {
            if (lockWritingMessage)
                return;
            Console.ForegroundColor = ForegroundColor;
            Console.BackgroundColor = BackgroundColor;
            string[] lineStrings = strMessage.Split(new char[] { '\n' });
            lock (LockedObj)
            {
                // Xóa dòng hiện tại (có thể đang chứa progress) trước khi in message
                Console.Write("\r\x1B[2K");
                foreach (string s in lineStrings)
                {
                    Interlocked.Increment(ref Messageline);
                    Console.WriteLine(s);
                }
            }
            Console.ResetColor();
        }
        public override void SendCalculationResult(string Result)
        {
           
           MessageToUser((string.Format(":-) Computing result ({0}): {1}", DateTime.Now.ToString(), Result)));
           
        }
        
        public override void SendErrorToUser(Exception ex)
        {
            TextWriter errStream = null;
            try
            {
                DateTime appStart = DateTime.Now;

                string folder = Directory.GetCurrentDirectory() + "\\OutPut";
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = "error.log";
                fileName = folder + "\\" + fileName;

                errStream = new StreamWriter(fileName,true);
                string appName = typeof(ConsoleUser).Assembly.Location;
                appName = appName.Substring(appName.LastIndexOf('\\') + 1);
                
                // Redirect standard error stream to file.
                Console.SetError(errStream);
                // Write file header.
                Console.Error.WriteLine("----Error Log for \"{0}\"\t at {1}-----", appName, appStart);
                Console.Error.WriteLine("Message: "+ ex.Message);
                Console.Error.WriteLine("StackTrace: " + ex.StackTrace);
                Console.Error.WriteLine("HelpLink: "+ ex.HelpLink);
                Console.Error.WriteLine("Source: " + ex.Source);
                
                Console.Error.Close();
                MessageToUser("ERROR!\t"+ex.Message);
            }
            finally
            {
                if (errStream != null)
                    errStream.Close();
                GoToPrompt();
            }
        }
        public override YesNoQuestion AskUserYesNoQuestion(string strMessage)
        {
            try
            {
                MessageToUser(strMessage + "\t(Y/N) = ?");
                ConsoleKeyInfo key = new ConsoleKeyInfo();
                do
                {
                    key = Console.ReadKey();

                    if (key.Key == ConsoleKey.Y)
                        return YesNoQuestion.Yes;
                    else if (key.Key == ConsoleKey.N)
                        return YesNoQuestion.No;
                    MessageToUser("Wrong data format! Please retype...");
                } while (true);

            }
            finally
            {
                GoToPrompt();
            }
        }
        public override YesNoCancelQuestion AskUserYesNoCancelQuestion(string strMessage)
        {
            try
            {
                MessageToUser(strMessage + "\t(Y/N) = ?");
                ConsoleKeyInfo key = new ConsoleKeyInfo();
                do
                {
                    key = Console.ReadKey();

                    if (key.Key == ConsoleKey.Y)
                        return YesNoCancelQuestion.Yes;
                    else if (key.Key == ConsoleKey.N)
                        return YesNoCancelQuestion.No;
                    else if (key.Key == ConsoleKey.Escape)
                        return YesNoCancelQuestion.Cancel;

                    MessageToUser("Wrong data format! Please retype...");
                } while (true);
            }
            finally
            {
                GoToPrompt();
            }
        }
        public override void PressAnyKey()
        {
            Console.ReadKey(false);
        }
       
        public override void Clear()
        {
            try { Console.Clear(); } catch { }
            lock (this)
            {
                this.Messageline = -1;
            }
            //User.One.MessageToUser(HelloText);
            ShowHelloText();
            Program.OnHelpCommand(null);
            GoToPrompt();
        }
        private void RemoveDisplayCharacter()
        {
            Console.Write('\b');
            Console.Write(" ");
            Console.Write('\b');
        }
        public override YesNoQuestion AskUserAnValue(string Comment, string Prompt, System.Type type, object DefaultValue, ref object Return)
        {
            try
            {
                MessageToUser(Comment + ". " + Prompt + string.Format("{0,30}", "\t\t(Press enter = '" + DefaultValue.ToString() + "' : " + type.Name.ToString() + ")"));
                string buffer = "";
                do
                {
                    var info = Console.ReadKey(true);
                    char key = info.KeyChar;

                    if (info.Key == ConsoleKey.Escape)
                    {
                        throw new KeyNotFoundException("Canceled by user!");
                    }
                    else if (info.Key == ConsoleKey.Backspace)
                    {
                        if (buffer.Length > 0)
                        {
                            buffer = buffer.Remove(buffer.Length - 1);
                            RemoveDisplayCharacter();
                        }
                        continue;
                    }
                    else if (info.Key == ConsoleKey.Enter)
                    {
                        try
                        {
                            if (buffer == "")
                            {
                                Return = DefaultValue;
                                return YesNoQuestion.Yes;
                            }
                            Return = Convert.ChangeType(buffer, type);
                            return YesNoQuestion.Yes;
                        }
                        catch
                        {
                        }
                        MessageToUser("Wrong data format! Please retype data in type of " + type.Name.ToString() + "...");
                        buffer = "";
                        continue;
                    }

                    try
                    {
                        if (!("-+*/:,.;").Contains(key) && !('0' <= key && key <= 'z'))
                            Convert.ChangeType(buffer + key, type);
                        buffer += key;
                        Console.Write(key);
                    }
                    catch { }

                } while (true);
            }
            finally
            {
                GoToPrompt();
            }
        }
        

    }
}
