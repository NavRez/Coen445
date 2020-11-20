using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Reflection.Emit;

namespace UDPSocketProject
{
    class UdpServer
    {
        public MessageEventHandler messageEventHandler;
        protected IPEndPoint thisServerIP;//the ip of the server
        protected IPEndPoint otherServerIP;
        protected Socket thisServerSocket;

        Thread serverListenThread;
        Thread serverSwapThread;

        public static bool sleeping = true;
        public static bool twoServerComm = false;
        Response response = new Response();


        public string[] subjects = { "computer engineering", "Disney Marvel", "Pokemon", "Final Fantasy",
            "Zack Fair", "Mario", "Mexican Studies", "Calculus", "Protocols", "US politics" };



        /// <summary>
        /// Initializes a new Udp Server, it takes in a port and host argument and then initalizes things like the id, ipAddress and the socket 
        /// </summary>
        /// <param name="newPort">the port assigned to a new UdpServer </param>
        /// <param name="newHost">host assinged to a new Udp Server</param>
        public UdpServer(string _bindingIP, string _otherServerIP, int _thisServerPort, int _otherServerPort)
        {
            thisServerIP = new IPEndPoint(IPAddress.Parse(_bindingIP), _thisServerPort);
            otherServerIP = new IPEndPoint(IPAddress.Parse(_otherServerIP), _otherServerPort);

            thisServerSocket = new Socket(thisServerIP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            thisServerSocket.Bind(thisServerIP);
        }

        /// <summary>
        /// The primary execution of the server occurs in this function. The outer loop is blocked by a semaphore to prevent concurrent thread executions and the inner loop is where the server attempts to receive and send data
        /// </summary>
        public void Start(string currentServer)
        {


            NewServerIP();
            messageEventHandler = new MessageEventHandler();
            Console.WriteLine("Server Started at IP: " + thisServerIP.ToString());
            
            //if (currentServer.Equals("A"))
            //{
            //    sleeping = false;
            //    Console.WriteLine("I am Serving");
            //}
            //else
            //{
            //    NewServerIP();
            //    Console.WriteLine("I am Sleeping");
            //}
            serverListenThread = new Thread(ServerListen)
            {
                IsBackground = true
            };

            serverSwapThread = new Thread(ServerSwap)
            {
                IsBackground = true
            };

            serverListenThread.Start();
            serverSwapThread.Start();

            

            serverListenThread.Join();
            serverSwapThread.Join();
        }

        private void ServerListen()
        {
            byte[] receiveBytes = new byte[1024];
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = (EndPoint)sender;
            messageEventHandler.ReadFromFile();

            while (true)
            {
                int bytesLengthReceived = 0;
                string receivedMessage = "";

                try
                {
                    bytesLengthReceived = thisServerSocket.ReceiveFrom(receiveBytes, ref senderRemote);
                    receivedMessage = Encoding.ASCII.GetString(receiveBytes, 0,
                            bytesLengthReceived) + "," + senderRemote.ToString();
                }
                catch
                {
                    Console.WriteLine("Other server is not responding, I am Active");
                    sleeping = false;
                    continue;
                }
                
                if (!sleeping)
                {
                    if (receivedMessage.Equals("UPDATE-SERVER"+ "," + senderRemote.ToString()))
                    {
                        twoServerComm = true;
                        Console.Write("Updating other Server IP From " + otherServerIP);
                        string[] ipAndPort = senderRemote.ToString().Split(":").ToArray();
                        otherServerIP = new IPEndPoint(IPAddress.Parse(ipAndPort[0]), Int32.Parse(ipAndPort[1]));
                        Console.WriteLine(" To " + otherServerIP +"... letting clients know");

                    }
                    Console.WriteLine("Server {0} : {1}", thisServerIP, receivedMessage);

                    response = messageEventHandler.SwitchCase(receivedMessage, thisServerSocket);
                    byte[] feed = Encoding.ASCII.GetBytes(response.message);
                    if(response.message.Length > 0)
                    {
                        Console.WriteLine(response.message);
                    }
                    thisServerSocket.SendTo(feed, 0, feed.Length, SocketFlags.None, (IPEndPoint)senderRemote);
                    if (response.valid && twoServerComm)
                    {
                        feed = Encoding.ASCII.GetBytes(receivedMessage + ",");
                        thisServerSocket.SendTo(feed, 0, feed.Length, SocketFlags.None, (IPEndPoint)otherServerIP);
                    }             
                }
                else
                {
                    if (receivedMessage.Equals("WAKE-UP" + "," + senderRemote.ToString()))
                    {
                        Console.WriteLine("\n*****SENDING TO OTHER SERVER******");
                        Console.WriteLine("What I think the ip is:" + otherServerIP.ToString());
                        Console.WriteLine("what it actually is   : " + senderRemote.ToString() + "\n");
                        if (!otherServerIP.ToString().Equals(senderRemote.ToString()))
                        {
                            otherServerIP.ToString().Equals(senderRemote.ToString());
                        }
                        byte[] feed = Encoding.ASCII.GetBytes("GO-SLEEP");
                        thisServerSocket.SendTo(feed, 0, feed.Length, SocketFlags.None, (IPEndPoint)otherServerIP);
                    }

                    if (otherServerIP.Equals((IPEndPoint)senderRemote) && receivedMessage.Length > 0)
                    {
                        Console.WriteLine("Server {0} : From that server: {1}", thisServerIP, receivedMessage);
                        receivedMessage += "," + thisServerIP.ToString();
                        response = messageEventHandler.SwitchCase(receivedMessage, thisServerSocket);
                        if (response.message.Length > 0)
                        {
                            Console.WriteLine("From other server: " + response.message);
                        }

                    }
                }

            }
        }

        private void ServerSwap()
        {
            while (true)
            {
                if (!sleeping)
                {
                    Thread.Sleep(15000);
                    Console.WriteLine("Sending wake up");
                    string serverSwapMessage = "WAKE-UP";
                    byte[] feed = Encoding.ASCII.GetBytes(serverSwapMessage);
                    try
                    {
                        thisServerSocket.SendTo(feed, 0, feed.Length, SocketFlags.None, otherServerIP);
                    }
                    catch
                    {
                        Console.WriteLine("Cannot send wakeup, other server not initialized");
                    }
                    
                }
                while (sleeping) ;
            }
        }

        private void NewServerIP()
        {
            string serverSwapMessage = "UPDATE-SERVER";
            byte[] feed = Encoding.ASCII.GetBytes(serverSwapMessage);
            try
            {
                Console.WriteLine("Sending other server an update");
                thisServerSocket.SendTo(feed, 0, feed.Length, SocketFlags.None, otherServerIP);
                twoServerComm = true;
            }
            catch
            {
                Console.WriteLine("Other server does not exist, cannot update it with my IP");
                sleeping = false;
                twoServerComm = false;
            }
        }

    }
}