using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Project_1.Managers.Saves;
using Project_1.Messaging;
using Project_1.UI;

namespace Project_1.Managers
{
    internal static partial class DebugManager
    {
        static readonly object diagnosticsFileLock = new object();
        static StreamWriter diagnosticsWriter;


        public static void Print(object aObject,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
        {
            Print(aObject?.ToString() ?? "null", filePath, lineNumber, memberName);
        }

        public static void Print(string aMsg,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
        {
            if (!Mode(DebugMode.Print)) return;

            string fileName = string.IsNullOrWhiteSpace(filePath) ? "unknown" : Path.GetFileName(filePath);
            WriteDiagnosticsLine($"{fileName}:{lineNumber} {memberName}: {aMsg}", true);
        }

        static void WriteDiagnosticsLine(string line, bool echoConsole) //Q: Why is this bool here if it's always true? Shouldn't the check be if debug mode console is on or not?
        {
            if (string.IsNullOrWhiteSpace(line)) return;

            if (echoConsole)
            {
                try
                {
                    Console.WriteLine(line);
                }
                catch (Exception)
                {
                    // ignore

                    //Q: What are the fail conditions and why aren't we handling them?
                }
            }

            lock (diagnosticsFileLock)
            {
                try
                {
                    diagnosticsWriter?.WriteLine($"{DateTime.UtcNow:O} {line}");
                }
                catch (Exception)
                {
                    // ignore

                    //Q: What are the fail conditions and why aren't we handling them?
                }
            }
        }
    }
}
