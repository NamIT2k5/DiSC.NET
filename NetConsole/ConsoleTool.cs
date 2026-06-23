using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace NetConsole
{
    public class ConsoleTool
    {
        #region Handle Window Console

        [DllImport("user32.dll")]
        static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);
        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        // Dùng để enable ANSI/VT escape sequences trong Windows console (cần cho VS Code 2026)
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        /// <summary>
        /// Bật ANSI/VT escape sequence cho Windows console.
        /// Cần thiết để SetCursorPosition-style ANSI codes hoạt động trong VS Code 2026 và Windows Terminal.
        /// </summary>
        public static void EnableVTProcessing()
        {
            try
            {
                IntPtr stdout = GetStdHandle(STD_OUTPUT_HANDLE);
                uint mode = 0;
                if (GetConsoleMode(stdout, out mode))
                    SetConsoleMode(stdout, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
            }
            catch { }
        }

        internal const UInt32 SC_CLOSE = 0xF060;
        internal const UInt32 MF_ENABLED = 0x00000000;
        internal const UInt32 MF_GRAYED = 0x00000001;
        internal const UInt32 MF_DISABLED = 0x00000002;
        internal const uint MF_BYCOMMAND = 0x00000000;

        public static void MaximizeConsoleWindow()
        {
            Console.WindowTop = Console.WindowLeft = 0;
            Console.WindowHeight= Console.LargestWindowHeight;
            Console.WindowWidth = Console.LargestWindowWidth;
        }
        public static void EnableCloseButton(bool bEnabled)
        {
            IntPtr window = FindWindow(null, Console.Title);

            if (window != IntPtr.Zero)
            {
                IntPtr hSystemMenu = GetSystemMenu(window, false);
                EnableMenuItem(hSystemMenu, SC_CLOSE, (uint)(MF_ENABLED | (bEnabled ? MF_ENABLED : MF_GRAYED)));
            }
        }
        #endregion
    }
}
