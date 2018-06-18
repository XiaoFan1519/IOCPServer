using System;
using System.Net;

namespace IocpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            IPAddress ipAddress = IPAddress.Parse("0.0.0.0");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 9000);
            IoServer iocp = new IoServer(1000, 10);
            iocp.Start(localEndPoint);
            Console.WriteLine("Press any key to terminate the server process....");
            Console.ReadKey();
        }
    }
}
