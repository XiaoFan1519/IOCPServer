using IOCP;
using IOCP.Handle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpProxyTest
{
    class Program
    {
        static void Main(string[] args)
        {
            IPAddress ipAddress = IPAddress.Parse("0.0.0.0");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 9000);
            new Server(400, 1024).Init(() => new HttpProxyHandle())
                .Start(localEndPoint);
        }
    }
}
