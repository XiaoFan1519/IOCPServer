using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {

        static ConcurrentBag<long> cost = new ConcurrentBag<long>();

        static void Main(string[] args)
        {
            string countStr;
            do
            {
                Console.Write("请输入启动线程数:");
                countStr = Console.ReadLine();
                int count = Convert.ToInt32(countStr);
                Semaphore semaphore = new Semaphore(0, count);

                for (int i = 0; i < count; i++)
                {
                    new Thread(Send).Start(semaphore);
                }
                semaphore.Release(count);
                Console.WriteLine("请等待线程执行结束");
                Thread.Sleep((int)(count * 30));
            } while (true);
        }

        static void Send(object semaphore)
        {
            //设定服务器IP地址  
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // 等待
            (semaphore as Semaphore).WaitOne();
            clientSocket.Connect(new IPEndPoint(ip, 9000));
            Stopwatch watch = new Stopwatch();
            watch.Start();
            byte[] buff = Encoding.Default.GetBytes("0123456789");
            clientSocket.Send(buff);
            buff = new byte[10];

            int receiveCount = 0;
            do
            {
                receiveCount += clientSocket.Receive(buff);
            } while (receiveCount < 10);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            cost.Add(watch.ElapsedMilliseconds);
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
    }
}
