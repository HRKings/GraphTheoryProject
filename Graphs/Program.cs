using System;

namespace Graphs
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (var game = new Graphs(args))
                game.Run();
        }
    }
}
