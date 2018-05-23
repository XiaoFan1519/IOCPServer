using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class UserToken
    {
        public Socket Socket;

        private Server server;

        private ByteBuffer m_buffer = new ByteBuffer();

        public UserToken(Server server)
        {
            this.server = server;
        }

        public void Receive(byte[] buffer, int offset, int count)
        {
            m_buffer.WriteBytes(buffer, offset, count);
        }

        public void Send(byte[] buffer, int count)
        {

        }
    }
}
