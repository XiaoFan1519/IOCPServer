using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Handle
{
    public class DefaultHandle : IHandle
    {
        public void Receive(UserToken token, ByteBuffer msg)
        {
            if (msg.ReadableBytes() < 10)
            {
                return;
            }

            token.Send(msg.ReadBytes(10));
        }

        public void Close()
        {

        }
    }
}
