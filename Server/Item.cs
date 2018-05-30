using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    /// <summary>
    /// 缓存信息
    /// </summary>
    class BufferItem
    {
        public byte[] Buffer;

        public int Offset;

        public int Count;
    }
}
