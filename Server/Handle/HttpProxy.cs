using System;
using System.Text;

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
            
            for (; offset < msg.ReadableBytes(); offset++ )
            {
                byte b = msg.Peek(offset);
                if (b == terminator[match])
                {
                    match++;
                    if (match == terminator.Length)
                    {
                        Console.WriteLine("HTTP消息头接收完毕");
                        byte[] bytes = msg.ReadBytes(offset + 1);
                        string aaa = Encoding.Default.GetString(bytes);
                        Console.WriteLine("HTTP消息头接收完毕: {0}", aaa);
                    }
                }
            }
        }

        public void Close()
        {

        }
    }
}
