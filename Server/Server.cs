﻿using IOCP.Handle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IOCP
{

    public class Server
    {
        private int m_numConnections;   // the maximum number of connections the sample is designed to handle simultaneously 
        private int m_receiveBufferSize;// buffer size to use for each socket I/O operation 
        BufferManager m_bufferManager;  // represents a large reusable set of buffers for all socket operations
        const int opsToPreAlloc = 2;    // read, write (don't alloc buffer space for accepts)
        Socket listenSocket;            // the socket used to listen for incoming connection requests
        // pool of reusable SocketAsyncEventArgs objects for write, read and accept socket operations
        SocketAsyncEventArgsPool m_readWritePool;
        int m_totalBytesRead;           // counter of the total # bytes received by the server
        int m_numConnectedSockets;      // the total number of clients connected to the server 
        Semaphore m_maxNumberAcceptedClients;

        /// <summary>
        /// 获取业务处理类
        /// </summary>
        /// <returns></returns>
        public delegate IHandle GetIHandle();

        private GetIHandle getHandleFn;

        // Create an uninitialized server instance.  
        // To start the server listening for connection requests
        // call the Init method followed by Start method 
        //
        // <param name="numConnections">the maximum number of connections the sample is designed to handle simultaneously</param>
        // <param name="receiveBufferSize">buffer size to use for each socket I/O operation</param>
        public Server(int numConnections, int receiveBufferSize)
        {
            m_totalBytesRead = 0;
            m_numConnectedSockets = 0;
            m_numConnections = numConnections;
            m_receiveBufferSize = receiveBufferSize;
            // allocate buffers such that the maximum number of sockets can have one outstanding read and 
            //write posted to the socket simultaneously  
            m_bufferManager = new BufferManager(receiveBufferSize * numConnections * opsToPreAlloc,
                receiveBufferSize);

            m_readWritePool = new SocketAsyncEventArgsPool(numConnections);
            m_maxNumberAcceptedClients = new Semaphore(numConnections, numConnections);
        }

        // Initializes the server by preallocating reusable buffers and 
        // context objects.  These objects do not need to be preallocated 
        // or reused, but it is done this way to illustrate how the API can 
        // easily be used to create reusable objects to increase server performance.
        //
        public Server Init(GetIHandle fn)
        {
            this.getHandleFn = fn;

            // Allocates one large byte buffer which all I/O operations use a piece of.  This gaurds 
            // against memory fragmentation
            m_bufferManager.InitBuffer();

            // preallocate pool of SocketAsyncEventArgs objects
            SocketAsyncEventArgs readWriteEventArg;

            for (int i = 0; i < m_numConnections; i++)
            {
                //Pre-allocate a set of reusable SocketAsyncEventArgs
                readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                // 初始化业务处理类
                IHandle handle = getHandleFn?.Invoke();
                readWriteEventArg.UserToken = new UserToken(this, handle ?? new DefaultHandle());
                
                // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object
                m_bufferManager.SetBuffer(readWriteEventArg);

                // add SocketAsyncEventArg to the pool
                m_readWritePool.Push(readWriteEventArg);
            }
            return this;
        }

        // Starts the server such that it is listening for 
        // incoming connection requests.    
        //
        // <param name="localEndPoint">The endpoint which the server will listening 
        // for connection requests on</param>
        public void Start(IPEndPoint localEndPoint)
        {
            // create the socket which listens for incoming connections
            listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(localEndPoint);
            // start the server with a listen backlog of 100 connections
            listenSocket.Listen(int.MaxValue);

            // post accepts on the listening socket
            StartAccept(null);

            //Console.WriteLine("{0} connected sockets with one outstanding receive posted to each....press any key", m_outstandingReadCount);
            Console.WriteLine("Press CTRL + C to terminate the server process....");
            
            // Prevent example from ending if CTL+C is pressed.
            Console.TreatControlCAsInput = true;
            ConsoleKeyInfo cki;
            bool isCtrl = false;
            do
            {
                cki = Console.ReadKey();
                isCtrl = (cki.Modifiers & ConsoleModifiers.Control) != 0;
            } while (isCtrl && cki.Key != ConsoleKey.C);

            // 结束工作
        }


        // Begins an operation to accept a connection request from the client 
        //
        // <param name="acceptEventArg">The context object to use when issuing 
        // the accept operation on the server's listening socket</param>
        public void StartAccept(SocketAsyncEventArgs acceptEventArg)
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
        void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            // Get the socket for the accepted cli00ent connection and put it into the 
            //ReadEventArg object user token
            m_maxNumberAcceptedClients.WaitOne();

            SocketAsyncEventArgs readEventArgs = m_readWritePool.Pop();
            UserToken token = readEventArgs.UserToken as UserToken;
            token.Socket = e.AcceptSocket;

            Interlocked.Increment(ref m_numConnectedSockets);
            Console.WriteLine("Client connection accepted. There are {0} clients connected to the server",
                m_numConnectedSockets);

            // As soon as the client is connected, post a receive to the connection
            bool willRaiseEvent = e.AcceptSocket.ReceiveAsync(readEventArgs);
            if (!willRaiseEvent)
            {
                ProcessReceive(readEventArgs);
            }

            // Accept the next connection request
            StartAccept(e);
        }

        // This method is called whenever a receive or send operation is completed on a socket 
        //
        // <param name="e">SocketAsyncEventArg associated with the completed receive operation</param>
        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            // determine which type of operation just completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive");
            }
        }

        // This method is invoked when an asynchronous receive operation completes. 
        // If the remote host closed the connection, then the socket is closed.  
        // If data was received then the data is echoed back to the client.
        //
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            // check if the remote host closed the connection
            UserToken token = e.UserToken as UserToken;
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                //increment the count of the total bytes receive by the server
                Interlocked.Add(ref m_totalBytesRead, e.BytesTransferred);
                Console.WriteLine("The server has read a total of {0} bytes", m_totalBytesRead);

                token.Receive(e.Buffer, e.Offset, e.BytesTransferred);
            } else
            {
                // 执行关闭操作
                CloseClientSocket(e);
                return;
            }

            // 继续接收
            bool willRaiseEvent = token.Socket.ReceiveAsync(e);
            if (!willRaiseEvent)
            {
                ProcessReceive(e);
            }
        }

        // This method is invoked when an asynchronous send operation completes.  
        // The method issues another receive on the socket to read any additional 
        // data sent from the client
        //
        // <param name="e"></param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            // 还原缓存
            BufferItem item = e.UserToken as BufferItem;
            e.SetBuffer(item.Buffer, item.Offset, item.Count);
            e.UserToken = item.UserToken;

            if (e.SocketError == SocketError.Success)
            {
                m_readWritePool.Push(e);
                m_maxNumberAcceptedClients.Release();
                return;
            }

            Console.WriteLine("发送失败 {0}", e.SocketError);
        }

        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            UserToken token = e.UserToken as UserToken;

            // close the socket associated with the client
            try
            {
                token.Socket.Shutdown(SocketShutdown.Send);
                token.Socket.Close();
            }
            // throws if client process has already closed
            catch (Exception ex)
            {
                Console.WriteLine("{0}:{1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                token.Close();
            }

            // decrement the counter keeping track of the total number of clients connected to the server
            Interlocked.Decrement(ref m_numConnectedSockets);
            Console.WriteLine("A client has been disconnected from the server. There are {0} clients connected to the server", m_numConnectedSockets);

            // Free the SocketAsyncEventArg so they can be reused by another client
            m_readWritePool.Push(e);
            m_maxNumberAcceptedClients.Release();
        }

        /// <summary>
        /// 添加到发送队列
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="buffer"></param>
        public void Send(Socket socket, byte[] buffer)
        {
            m_maxNumberAcceptedClients.WaitOne();
            SocketAsyncEventArgs writerEventArgs = m_readWritePool.Pop();
            // 暂存缓存信息
            BufferItem item = new BufferItem()
            {
                Buffer = writerEventArgs.Buffer,
                Offset = writerEventArgs.Offset,
                Count = writerEventArgs.Count,
                UserToken = writerEventArgs.UserToken
            };
            writerEventArgs.UserToken = item;
            // 放置待发送的信息
            writerEventArgs.SetBuffer(buffer, 0, buffer.Length);

            bool willRaiseEvent = socket.SendAsync(writerEventArgs);
            if (!willRaiseEvent)
            {
                ProcessSend(writerEventArgs);
            }
        }
    }
}