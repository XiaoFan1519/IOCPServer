using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    
    class Server
    {
        private Socket listenSocket;            // the socket used to listen for incoming connection requests

        private Semaphore maxNumberAcceptedClients;

        public delegate void InitChannelDelegate(Channel channel);

        // Create an uninitialized server instance.  
        // To start the server listening for connection requests
        // call the Init method followed by Start method 
        //
        // <param name="numConnections">the maximum number of connections the sample is designed to handle simultaneously</param>
        // <param name="receiveBufferSize">buffer size to use for each socket I/O operation</param>
        public Server(int numConnections)
        {
            maxNumberAcceptedClients = new Semaphore(numConnections, numConnections);
        }

        /// <summary>
        /// 用于让使用者对Channel进行初始化
        /// </summary>
        private InitChannelDelegate initChannelDelegate;

        private Channel GetChannel(Socket socket)
        {
            Channel channel = new Channel(socket);
            initChannelDelegate.Invoke(channel);
            return channel;
        }

        /// <summary>
        /// 当接收到一个scoket时，会创建一个Channel来与它进行关联，
        /// 并由使用者来对Channel进行初始化
        /// </summary>
        /// <param name="_Init"></param>
        /// <returns></returns>
        public Server InitChannel(InitChannelDelegate initChannelDelegate)
        {
            this.initChannelDelegate = initChannelDelegate;
            return this;
        }

        public Server Bind(int port)
        {
            return Bind(IPAddress.Parse("0.0.0.0"), port);
        }

        public Server Bind(IPAddress ipAddress, int port)
        {
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
            // Create a TCP/IP socket.
            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Bind the socket to the local endpoint and listen for incoming connections.

            listenSocket.Bind(localEndPoint);
            listenSocket.Listen(128);
            return this;
        }

        // Begins an operation to accept a connection request from the client 
        //
        // <param name="acceptEventArg">The context object to use when issuing 
        // the accept operation on the server's listening socket</param>
        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {
                // socket must be cleared since the context object is being reused
                acceptEventArg.AcceptSocket = null;
            }

            bool willRaiseEvent = listenSocket.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArg);
            }
        }

        // This method is the callback method associated with Socket.AcceptAsync 
        // operations and is invoked when an accept operation is complete
        //
        private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            new Thread(CreateChannel).Start(e.AcceptSocket);
            StartAccept(e);
        }

        private void CreateChannel(object param)
        {
            Socket socket = param as Socket;
            Channel channel = new Channel(socket);
            channel.Wait();
        }
    }
}