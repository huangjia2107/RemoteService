using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ClientCore.Config
{
    [Serializable]
    public class ServerConfig
    { 
        public string Domain { get; set; }
        public int Port { get; set; }
        public int P2P_Port { get; set; }
        public int Test_Port { get; set; }

        [XmlIgnore]
        public string IP { get; set; }
    }
}
