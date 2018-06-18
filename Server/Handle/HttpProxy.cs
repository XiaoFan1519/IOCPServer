using System;

namespace IOCP.Handle
{
    public class HttpProxyHandle : IHandle
    {
        /// <summary>
        /// 请求终止符
        /// </summary>
        private const string terminator = "\r\n\r\n";

        private int match = 0;

        /// <summary>
        /// 当前处理到的索引位置
        /// </summary>
        private int offset = 0;

        public void Receive(UserToken token, ByteBuffer msg)
        {
            if (msg.ReadableBytes() < terminator.Length)
            {
                return;
            }
            
        }

        public void Close()
        {

        }
    }
}
