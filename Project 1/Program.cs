namespace Project_1;

using System;
using global::System;
using Project_1.Managers;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        ThreadAffinity.InitMainThread();

        using var game = new Game1();
        game.Run();
        return 0;
    }
}
