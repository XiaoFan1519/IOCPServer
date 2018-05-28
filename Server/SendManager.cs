using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    /// <summary>
    /// 发送队列管理
    /// </summary>
    class SendManager
    {
        /// <summary>
        /// 待发送队列
        /// </summary>
        private Queue<Item> items = new Queue<Item>();

        /// <summary>
        /// 判断当前是否需要结束
        /// </summary>
        private bool Close = false;

        SocketAsyncEventArgsPool m_WritePool;

        Semaphore m_maxNumberClients;

        public SendManager(int numConnections)
        {
            m_WritePool = new SocketAsyncEventArgsPool(numConnections);
            m_maxNumberClients = new Semaphore(numConnections, numConnections);

            // 初始化Pool
            for (int i = 0; i < numConnections; i++)
            {
                //Pre-allocate a set of reusable SocketAsyncEventArgs
                SocketAsyncEventArgs writeEventArg = new SocketAsyncEventArgs();
                writeEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);

                // add SocketAsyncEventArg to the pool
                m_WritePool.Push(writeEventArg);
            }

            new Thread(SendItem).Start();
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            // determine which type of operation just completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a send");
            }
        }

        public void Send(Socket socket, byte[] buffer)
        {
            Item item = new Item
            {
                Socket = socket,
                buffer = buffer
            };

            lock(items)
            {
                items.Enqueue(item);
                // 接收到发送请求，唤醒等待中的线程
                Monitor.PulseAll(items);
            }
        }

        /// <summary>
        /// 将队列中的数据发送出去
        /// </summary>
        private void SendItem()
        {
            do
            {
                lock (items)
                {
                    // 当队列为空时，等待发送请求
                    if (items.Count == 0)
                    {
                        Monitor.Wait(items);
                    }

                    Item result = items.Dequeue();

                    Socket socket = result.Socket;
                    m_maxNumberClients.WaitOne();
                    SocketAsyncEventArgs writeEventArg = m_WritePool.Pop();
                    writeEventArg.SetBuffer(result.buffer, 0, result.buffer.Length);
                    bool willRaiseEvent = socket.SendAsync(writeEventArg);
                    if (!willRaiseEvent)
                    {
                        ProcessSend(writeEventArg);
                    }
                }
            } while (!Close);
        }

        /// <summary>
        /// 发送后的处理
        /// </summary>
        /// <param name="e"></param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            m_WritePool.Push(e);
            m_maxNumberClients.Release();
        }
    }
}
