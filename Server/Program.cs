using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Server.Config;
using Server.Utils;
using Server.Core;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                var serverConfig = ConfigHelper.Instance().GetServerConfig();
                try
                {
                    (new ServerCore(serverConfig)).Start();
                }
                catch (Exception ex)
                {
                    ConsoleHelper.Error(string.Format("Failed to start server, Exception: {0}", ex.Message));
                }
                finally
                {

                }
            });

            Console.ReadKey();
        }
    }
}
