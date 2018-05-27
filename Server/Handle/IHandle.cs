using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public interface IHandle
    {
        void Receive(UserToken token, ByteBuffer msg);

        void Close();
    }
}
