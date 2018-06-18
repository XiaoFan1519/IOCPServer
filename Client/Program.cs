using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    class Program
    {
        static ConcurrentBag<long> list = new ConcurrentBag<long>();

        static void Main(string[] args)
        {
            string countStr;
            do
            {
                Console.Write("请输入启动线程数:");
                countStr = Console.ReadLine();
                int count = Convert.ToInt32(countStr);
                int times = 0;
                do
                {
                    Semaphore task = new Semaphore(0, count);
                    Semaphore wait = new Semaphore(0, count);

                    for (int i = 0; i < count; i++)
                    {
                        ManualResetEvent @event = new ManualResetEvent(false);
                        new Thread(Send).Start(new Tuple<Semaphore, Semaphore>(task, wait));
                    }
                    task.Release(count);
                    Console.WriteLine("请等待线程执行结束");
                    WaitAll(wait, count);
                    Console.WriteLine("平均耗时:{0}", list.Average());
                } while (++times < 100);
                Console.WriteLine("总平均耗时:{0}", list.Average());
            } while (true);
        }

        static void Send(object param)
        {
            var p = param as Tuple<Semaphore, Semaphore>;
            Semaphore task = p.Item1;
            Semaphore wait = p.Item2;
            // 等待
            task.WaitOne();

            //设定服务器IP地址
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(new IPEndPoint(ip, 9000));
            Stopwatch watch = new Stopwatch();
            watch.Start();
            byte[] buff = Encoding.Default.GetBytes("0123456789");
            clientSocket.Send(buff);
            buff = new byte[10];

            int receiveCount = 0;
            byte[] tmp = new byte[10];
            do
            {
                receiveCount += clientSocket.Receive(tmp);
            } while (receiveCount < 10);
            watch.Stop();
            //Console.WriteLine("{0}:{1}", watch.ElapsedMilliseconds,
            //    Encoding.Default.GetString(tmp));
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
            // 记录运行耗时
            list.Add(watch.ElapsedMilliseconds);
            // 不考虑异常情况
            wait.Release();
        }

        static void WaitAll(Semaphore wait, int count)
        {
            int time = 0;
            do
            {
                wait.WaitOne();
                time++;
            } while (time < count);
        }
    }
}
