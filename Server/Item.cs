using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    /// <summary>
    /// 待发送的数据
    /// </summary>
    class Item
    {
        /// <summary>
        /// 待发送的Socket
        /// </summary>
        public Socket Socket;

        /// <summary>
        /// 待发送的数据
        /// </summary>
        public byte[] buffer;
    }
}
