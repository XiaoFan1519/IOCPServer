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

        private IHandle handle;

        private Thread thread;

        /// <summary>
        /// 业务处理
        /// </summary>
        public IHandle Handle {
            set
            {
                handle = value;
                if (null == thread)
                {
                    thread = new Thread(NotifyHandle);
                    thread.Start();
                }
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
                lock (m_buffer)
                {
                    while (m_buffer.ReadableBytes() == 0)
                    {
                        Monitor.Wait(m_buffer);
                    }
                    
                    handle?.Receive(this, m_buffer);
                }
            } while (true);
        }

        public void Receive(byte[] buffer, int offset, int count)
        {
            lock (m_buffer)
            {
                m_buffer.WriteBytes(buffer, offset, count);
                Monitor.PulseAll(m_buffer);
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
            thread?.Abort();
        }
    }
}
