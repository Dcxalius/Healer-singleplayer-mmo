namespace Project_1;

using System;
using global::System;
using Project_1.Managers;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            ThreadAffinity.InitMainThread();

            using var game = new Game1();
            game.Run();
            return 0;
        }
        catch (Exception ex)
        {
            DebugManager.ReportFatalException(ex, "Program.Main");
            throw;
        }
    }
}
