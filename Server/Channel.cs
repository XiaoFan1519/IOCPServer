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
        private Socket socket;

        private CountdownEvent closeEvent = new CountdownEvent(1);

        private List<IHandle> pipeline = new List<IHandle>();

        public Channel(Socket socket)
        {
            this.socket = socket;
        }

        public List<IHandle> Pipeline => pipeline;

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
            // 判断是否停止
            if (closeEvent.IsSet)
            {
                return;
            }
        }
    }
}
