using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers
{
    enum DebugMode
    {
        Off,
        On
    }

    internal static class DebugManager
    {
        public static readonly DebugMode mode = DebugMode.On;


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();


        public static void Init()
        {

            AllocConsole();
        }


        public static void Print(Type test, string aMsg)
        {
            Print(test, aMsg, DebugMode.On);
        }

        public static void Print(Type test, string aMsg, DebugMode aMode)
        {
            if (mode == aMode || mode == DebugMode.On)
            {
                Console.WriteLine(test.ToString() + ": " + aMsg);
            }
        }
    }
}
