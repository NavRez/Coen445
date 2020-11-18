﻿using System;
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

        public MessageEventHandler messageEventHandler = new MessageEventHandler();

        protected IPEndPoint thisServerIP;//the ip of the server
        protected IPEndPoint otherServerIP;
        protected Socket thisServerSocket;

        Thread serverListenThread;
        Thread serverSwapThread;

        bool sleeping = true;


        public string[] subjects = { "computer engineering", "Disney Marvel", "Pokemon", "Final Fantasy",
            "Zack Fair", "Mario", "Mexican Studies", "Calculus", "Protocols", "US politics" };



        /// <summary>
        /// Initializes a new Udp Server, it takes in a port and host argument and then initalizes things like the id, ipAddress and the socket 
        /// </summary>
        /// <param name="newPort">the port assigned to a new UdpServer </param>
        /// <param name="newHost">host assinged to a new Udp Server</param>
        public UdpServer(string _bindingIP,string _otherServerIP, int _thisServerPort, int _otherServerPort)
        {
            thisServerIP = new IPEndPoint(IPAddress.Parse(_bindingIP),_thisServerPort);
            otherServerIP = new IPEndPoint(IPAddress.Parse(_otherServerIP), _otherServerPort);

            thisServerSocket = new Socket(thisServerIP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            thisServerSocket.Bind(thisServerIP);
        }

        /// <summary>
        /// The primary execution of the server occurs in this function. The outer loop is blocked by a semaphore to prevent concurrent thread executions and the inner loop is where the server attempts to receive and send data
        /// </summary>
        public void Start(string currentServer)
        {


            Console.WriteLine("Server Started at IP: " + thisServerIP.ToString());

            if (currentServer.Equals("A"))
            {
                sleeping = false;
                Console.WriteLine("I am Serving");
            }
            else
            {
                Console.WriteLine("I am Sleeping");
            }
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

            while (true)
            {

                int bytesLengthReceived = thisServerSocket.ReceiveFrom(receiveBytes, ref senderRemote);
                string receivedMessage = Encoding.ASCII.GetString(receiveBytes, 0,
                        bytesLengthReceived) + "," + senderRemote.ToString();
                if (!sleeping)
                {
                    string[] arr = receivedMessage.Split(",");

                    Console.WriteLine("Server {0} : {1}", thisServerIP, receivedMessage);
                    string LogMessage = "String " + receivedMessage + " has been received from " + thisServerIP.ToString();

                    string serverResponse = messageEventHandler.SwitchCase(receivedMessage, thisServerSocket);
                    byte[] feed = Encoding.ASCII.GetBytes(serverResponse);
                    Console.WriteLine(serverResponse);

                    thisServerSocket.SendTo(feed, 0, feed.Length, SocketFlags.None, (IPEndPoint)senderRemote);

                }                    
                else
                {
                    if(receivedMessage.Equals("WAKE-UP" + "," + senderRemote.ToString()))
                    {
                        string serverResponse = messageEventHandler.SwitchCase(receivedMessage, thisServerSocket);
                        Console.WriteLine("I AM AWAKE");
                        sleeping = false;
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
                    Thread.Sleep(10000);
                    Console.WriteLine("Sendign wake up");
                    string serverSwapMessage = "WAKE-UP";
                    byte[] feed = Encoding.ASCII.GetBytes(serverSwapMessage);
                    thisServerSocket.SendTo(feed, 0, feed.Length, SocketFlags.None, otherServerIP);
                    sleeping = true;
                    Console.WriteLine("And now I rest...zzz");
                }
                while (sleeping) ;
            }
        }

    }
}