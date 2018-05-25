using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class UserToken
    {
        public Socket Socket;

        private Server server;

        private ByteBuffer m_buffer = new ByteBuffer();

        private Mutex mutex = new Mutex();

        private bool close = false;

        /// <summary>
        /// 业务处理
        /// </summary>
        private IHandle handle;

        public UserToken(Server server)
        {
            this.server = server;
        }

        private void NotifyHandle()
        {
            do
            {
                mutex.WaitOne();
                handle.Receive(this, m_buffer);
            } while (!close);
        }

        public void Receive(byte[] buffer, int offset, int count)
        {
            m_buffer.WriteBytes(buffer, offset, count);
            mutex.ReleaseMutex();
        }

        public void Send(byte[] buffer)
        {
            server.Send(Socket, buffer);
        }

        /// <summary>
        /// 停止线程
        /// </summary>
        public void Close()
        {
            close = true;
        }
    }
}
