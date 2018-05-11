using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Channel
    {
        /// <summary>
        /// 本次连接的socket
        /// </summary>
        private Socket socket;

        /// <summary>
        /// 用来控制退出
        /// </summary>
        private CountdownEvent closeEvent = new CountdownEvent(1);

        /// <summary>
        /// 用来存储接收的数据
        /// </summary>
        private ByteBuffer buffer;

        /// <summary>
        /// 业务处理
        /// </summary>
        private IHandle handle;

        public Channel(Socket socket)
        {
            this.socket = socket;
            buffer = new ByteBuffer();
        }

        public IHandle Handle { get => handle; set => handle = value; }

        /// <summary>
        /// 关闭Channel
        /// </summary>
        public void Close()
        {
            socket?.Shutdown(SocketShutdown.Both);
            socket?.Close();
            closeEvent.Signal();
        }

        /// <summary>
        /// 等待Channel结束或发生异常
        /// </summary>
        public void Wait()
        {
            SocketAsyncEventArgs receiveEventArg = new SocketAsyncEventArgs();
            receiveEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            // 分配buffer
            byte[] buffer = new byte[4096];
            receiveEventArg.SetBuffer(buffer, 0, 4096);
            socket.ReceiveAsync(receiveEventArg);
            closeEvent.Wait();
        }

        // This method is called whenever a receive or send operation is completed on a socket 
        //
        // <param name="e">SocketAsyncEventArg associated with the completed receive operation</param>
        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            // determine which type of operation just completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    // ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        // This method is invoked when an asynchronous receive operation completes. 
        // If the remote host closed the connection, then the socket is closed.  
        // If data was received then the data is echoed back to the client.
        //
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            // 判断是否需要继续处理
            if (closeEvent.IsSet)
            {
                return;
            }

            NotifyHandle(e);
        }

        /// <summary>
        /// 通知处理程序
        /// </summary>
        /// <param name="e"></param>
        private void NotifyHandle(SocketAsyncEventArgs e)
        {
            if (null != handle)
            {
                buffer.WriteBytes(e.Buffer, e.BytesTransferred);
                handle.ChannelRead(this, buffer);
            }
        }
    }
}
