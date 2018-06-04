using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class UserToken
    {
        Socket Socket;

        private Server server;

        private ByteBuffer m_buffer = new ByteBuffer();

        private IHandle handle;

        public UserToken(Server server, IHandle handle)
        {
            this.server = server;
            this.handle = handle;
        }

        void Receive(byte[] buffer, int offset, int count)
        {
            m_buffer.WriteBytes(buffer, offset, count);
            handle?.Receive(this, m_buffer);
        }

        public void Send(byte[] buffer)
        {
            server.Send(Socket, buffer);
        }

        /// <summary>
        /// 停止线程
        /// </summary>
        void Close()
        {
            handle?.Close();
        }
    }
}
