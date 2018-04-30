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

        private SocketAsyncEventArgsPool socketAsyncEventArgsPool;

        private BufferManager bufferManager;

        private Semaphore maxNumberAcceptedClients;

        public delegate void InitChannelDelegate(Channel channel);

        // Create an uninitialized server instance.  
        // To start the server listening for connection requests
        // call the Init method followed by Start method 
        //
        // <param name="numConnections">the maximum number of connections the sample is designed to handle simultaneously</param>
        // <param name="receiveBufferSize">buffer size to use for each socket I/O operation</param>
        public Server(int numConnections, int receiveBufferSize)
        {
            // allocate buffers such that the maximum number of sockets can have one outstanding read and 
            //write posted to the socket simultaneously  
            bufferManager = new BufferManager(receiveBufferSize * numConnections, receiveBufferSize);

            socketAsyncEventArgsPool = new SocketAsyncEventArgsPool(numConnections);
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
                acceptEventArg = socketAsyncEventArgsPool.Pop();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
                acceptEventArg.UserToken = null;
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

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            // determine which type of operation just completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }

        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            SocketAsyncEventArgs readEventArgs = new SocketAsyncEventArgs
            {
                UserToken = e.AcceptSocket
            };
            readEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            readEventArgs.SetBuffer(new byte[255], 0, 255);
            // As soon as the client is connected, post a receive to the connection
            bool willRaiseEvent = e.AcceptSocket.ReceiveAsync(readEventArgs);
            if (!willRaiseEvent)
            {
                ProcessReceive(readEventArgs);
            }

            // Accept the next connection request
            StartAccept(e);
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            Console.WriteLine(Encoding.ASCII.GetString(e.Buffer, e.Offset, e.BytesTransferred));
            Socket socket = e.UserToken as Socket;
            bool willRaiseEvent = socket.ReceiveAsync(e);
            if (!willRaiseEvent)
            {
                ProcessAccept(e);
            }
        }
    }
}