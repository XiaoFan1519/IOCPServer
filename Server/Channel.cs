using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Channel
    {
        private Socket socket;

        public Channel(Socket socket)
        {
            this.socket = socket;
        }
    }
}
