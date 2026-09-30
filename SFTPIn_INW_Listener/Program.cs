using System;
using System.ServiceProcess;

namespace SFTPIn_INW_Listener
{
    internal static class Program
    {
        static void Main()
        {
            var svc = new SFTPInINWListener();

            if (Environment.UserInteractive)
            {
                svc.DebugStart();
                Console.WriteLine("Running. Press Enter to stop.");
                Console.ReadLine();
                svc.DebugStop();
            }
            else
            {
                ServiceBase.Run(new ServiceBase[] { svc });
            }
        }
    }
}