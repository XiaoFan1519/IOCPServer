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

        private CountdownEvent @event = new CountdownEvent(1);

        public Channel(Socket socket)
        {
            this.socket = socket;
        }

        /// <summary>
        /// 关闭Channel
        /// </summary>
        public void Close()
        {

            @event.Signal();
        }

        /// <summary>
        /// 等待Channel结束或发生异常
        /// </summary>
        public void Wait()
        {
            @event.Wait();
        }
    }
}
