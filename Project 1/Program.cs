namespace Project_1;

using System;
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
            Console.WriteLine("Unhandled exception:");
            Console.WriteLine(ex);
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.Read();
            return 1;
        }
    }
}
