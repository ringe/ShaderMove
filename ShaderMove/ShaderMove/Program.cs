using System;

namespace FishPond
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Pond game = new Pond())
            {
                game.Run();
            }
        }
    }
#endif
}

