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
        public Socket Socket;

        private Server server;

        private ByteBuffer m_buffer = new ByteBuffer();

        private bool close = false;

        private IHandle handle;

        /// <summary>
        /// 业务处理
        /// </summary>
        public IHandle Handle {
            set
            {
                handle = value;
                new Thread(NotifyHandle).Start();
            }
        }

        public UserToken(Server server)
        {
            this.server = server;
        }

        private void NotifyHandle()
        {
            do
            {
                lock (this)
                {
                    Monitor.Wait(this);
                    handle?.Receive(this, m_buffer);
                }
            } while (!close);
        }

        public void Receive(byte[] buffer, int offset, int count)
        {
            lock (this)
            {
                m_buffer.WriteBytes(buffer, offset, count);
                Monitor.PulseAll(this);
            }
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
            handle?.Close();
            close = true;
        }
    }
}
