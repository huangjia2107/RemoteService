using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

namespace Server.Config
{
    class ConfigHelper
    {
        private static readonly ConfigHelper _instance = new ConfigHelper();

        private ConfigHelper() { }

        public static ConfigHelper Instance()
        {
            return _instance;
        }

        public ServerConfig GetServerConfig()
        {
            return new ServerConfig
            {
                IP = "127.0.0.1",
                Port = 6666,
            };
        }
    }
}
