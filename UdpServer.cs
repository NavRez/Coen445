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

        public ClientEventHandler clientTest = new ClientEventHandler();

        //these attributes are unique to each server
        protected UdpClient serverSocket;//protected socket element
        protected IPEndPoint ip;//the ip of the server
        protected byte[] data;//the databus of the server
        protected static int initServer = 0;//a static element that is incremented the moment a server is initalized
        protected int personalID = 0;//
        protected Stopwatch stopwatch = new Stopwatch();
        protected Stopwatch notifyStopwatch = new Stopwatch();
        
        public static bool allowReceive = true;

        //shared information between servers
        protected static int[] ports = { 8080, 5080 };//retains the ports of the servers
        protected static string[] hosts = { "127.0.0.2", "127.0.0.2" };//retains the hosts of the servers
        public static Semaphore semaphore = new Semaphore(1, 1, "Originate");
        public Semaphore internalSemaphore = new Semaphore(1, 1);
        public bool running = false;



        protected IPEndPoint thisServerIP;//the ip of the server
        protected IPEndPoint otherServerIP;
        protected Socket thisServerSocket;
        Thread ServerListenThread;


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


            //BindSocket(ip);
        }

        /// <summary>
        /// Binds the ip to the socket and sets the timeout values for Send and Receive
        /// </summary>
        /// <param name="endPoint"></param>
        public void BindSocket(IPEndPoint endPoint)
        {
            serverSocket = new UdpClient(endPoint);
            serverSocket.Client.ReceiveTimeout = 5000;
            serverSocket.Client.SendTimeout = 5000;
        }


        /// <summary>
        /// The primary execution of the server occurs in this function. The outer loop is blocked by a semaphore to prevent concurrent thread executions and the inner loop is where the server attempts to receive and send data
        /// </summary>
        public void Start()
        {
            ServerListenThread = new Thread(ServerListen)
            {
                IsBackground = true
            };
            ServerListenThread.Start();

            ServerListenThread.Join();
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
                                        bytesLengthReceived)+","+ senderRemote.ToString();

                string[] arr = receivedMessage.Split(",");

                Console.WriteLine("Server {0} : {1}", personalID, receivedMessage);
                string LogMessage = "String " + receivedMessage + " has been received from " + personalID.ToString();

                string serverResponse = clientTest.SwitchCase(receivedMessage, thisServerSocket);
                byte[] feed = Encoding.ASCII.GetBytes(serverResponse);
                Console.WriteLine(serverResponse);

                //thisServerSocket.Send(feed, feed.Length, senderRemote);
                thisServerSocket.SendTo(feed, 0, feed.Length,SocketFlags.None,(IPEndPoint)senderRemote);
            }
        }

        [ObsoleteAttribute("Busywait function is obsolete. Functionality is implemented in Start function.", false)]
        public void BusyWait()
        {
            Thread.Sleep(300000);
        }
        /// <summary>
        /// Computes an individual action block on a time-based limit. Failure to complete the action in time forces the block to exit
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <param name="codeBlock"></param>
        /// <returns></returns>
        private static bool TrackFunction(TimeSpan timeSpan, Action codeBlock)
        {
            try
            {
                Task task = Task.Factory.StartNew(() => codeBlock());
                task.Wait(timeSpan);
                return task.IsCompleted;
            }
            catch (AggregateException ae)
            {
                throw ae.InnerExceptions[0];
            }
        }

    }
}