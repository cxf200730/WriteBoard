using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Server
{
    public class ServerIpEndPoint
    {
        public int ServerId { get; set; }
        public string ServerIp { get; set; }
        public string ServerPort { get; set; }
        public int ServerIsWork { get; set; }
        public string ServerOnlineTime { get; set; }
        public string ServerPublicIp { get; set; }
    }
}