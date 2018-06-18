using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOCP
{
    public interface IHandle
    {
        void Receive(UserToken token, ByteBuffer msg);

        void Close();
    }
}
