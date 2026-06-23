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

                int widthBar = Console.BufferWidth;
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
            string buffer="";
            while (true)
            {
                if (ProgressManager.Count > 0)
                {
                    buffer = "";
                    for (int i = 0; i < ProgressManager.Count; i++)
                    {
                        Thread t = ProgressManager.Keys.ElementAt(i);
                        var e = ProgressManager[t];

                        if (t.IsAlive)
                        {
                            if (e.Value == -1)
                            {
                                buffer += string.Format("[TaskID = {0}: running{1,-3}]\t", t.ManagedThreadId, "".PadRight(e.Key, '.'));
                                ProgressManager[t] = new KeyValuePair<int, int>((e.Key + 1) % 4, e.Value);
                            }
                            else
                            {
                                string timer = "";
                                if (e.Key<=1)
                                {
                                    if (!TimerProgressManager.ContainsKey(t))
                                    {
                                        DateTime ab = DateTime.Now;
                                        TimerProgressManager.AddOrUpdate(t, ab, (key, existingVal) =>
                                        {
                                            return ab;
                                        });
                                    }
                                }
                                else if(e.Key >= e.Value-1)
                                {
                                    DateTime removetime;
                                    TimerProgressManager.TryRemove(t, out removetime);
                                }
                                if (TimerProgressManager.ContainsKey(t))
                                {
                                    TimeSpan ts = TimeSpan.FromTicks(DateTime.Now.Subtract(TimerProgressManager[t]).Ticks * (e.Value - (e.Key + 1)) / (e.Key + 1));


                                    //TimeSpan timespent = DateTime.Now - TimerProgressManager[t];
                                    //int secondsremaining = (int)(timespent.TotalSeconds / (e.Key+1)* (e.Value - e.Key));

                                    //TimeSpan ts = TimeSpan.FromSeconds(secondsremaining);

                                    timer = string.Format("\t remaining time:{0:D2}d {1:D2}h:{2:D2}m:{3:D2}s",
                                                    ts.Days,
                                                    ts.Hours,
                                                    ts.Minutes,
                                                    ts.Seconds);
                                }else
                                    timer = "";

                                buffer += string.Format("[TaskID = {0}: {1}/{2}{3}]\t", t.ManagedThreadId, e.Key, e.Value, timer);
                                if(e.Key>=e.Value-1)
                                {
                                     ProgressManager[t] = new KeyValuePair<int,int>(0,-1);
                                     // Clear the progress line and print newline when task completes
                                     lock (LockedObj)
                                     {
                                         Console.Write("\r\x1B[K\n");  // Clear current line + move to next line
                                     }
                                }
                            }
                        }
                        else
                        {
                            KeyValuePair<int, int> status;
                            ProgressManager.TryRemove(t, out status);
                        }
                    }
                    lock (LockedObj)
                    {
                        // \r = về col 0 dòng hiện tại (ghi đè progress cũ tại chỗ)
                        // \x1B[K = xóa từ cursor đến cuối dòng (xóa ký tự thừa của lần trước)
                        // KHÔNG PadRight → không wrap trong terminal hẹp (VS Code 2026)
                        Console.Write("\r" + buffer + "\x1B[K");
                    }
                }
                Thread.Sleep(2000);  // Giảm từ 2000ms xuống 100ms để progress indicator hiển thị liên tục
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
            Console.Clear();
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
