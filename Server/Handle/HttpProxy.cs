using System;

namespace IOCP.Handle
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
