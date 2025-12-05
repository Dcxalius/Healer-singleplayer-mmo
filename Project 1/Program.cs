namespace Project_1;

using System;
using System.Windows.Forms;

internal static class Program
{
    private const string LaunchFlag = "--run-game";

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (ShouldLaunchGame(args))
            {
                using var game = new Game1();
                game.Run();
                return 0;
            }

#if NET6_0_OR_GREATER
            ApplicationConfiguration.Initialize();
#else
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
#endif

            Application.Run(new StartGameForm());
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unhandled exception:");
            Console.WriteLine(ex);
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
            return 1;
        }
        return 0;
    }

    private static bool ShouldLaunchGame(string[] args)
    {
        if (args == null)
        {
            return false;
        }

        foreach (var arg in args)
        {
            if (string.Equals(arg, LaunchFlag, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
