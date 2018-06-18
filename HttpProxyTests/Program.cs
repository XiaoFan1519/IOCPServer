using IOCP;
using IOCP.Handle;
using System;
using System.Net;

namespace HttpProxyTests
{
    class Program
    {
        static void Main(string[] args)
        {
            IPAddress ipAddress = IPAddress.Parse("0.0.0.0");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 9000);
            new Server(100, 1024 * 1024 * 1024).Init(() => new HttpProxyHandle())
                .Start(localEndPoint);
        }
    }
}
