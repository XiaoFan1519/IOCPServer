using System;

namespace Server.Handle
{
    public class HttpProxyHandle : IHandle
    {
        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Receive(UserToken token, ByteBuffer msg)
        {
        }
    }
}
