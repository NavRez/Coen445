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
        [ObsoleteAttribute("Constructor is obsolete. Use the second constructor instead.", false)]
        public UdpServer(int newPort)
        {
            personalID = ++initServer;
            ports[personalID - 1] = newPort;
            data = new byte[1024];
            ip = new IPEndPoint(IPAddress.Any, newPort);
            serverSocket = new UdpClient(ip);
        }

        /// <summary>
        /// Initializes a new Udp Server, it takes in a port and host argument and then initalizes things like the id, ipAddress and the socket 
        /// </summary>
        /// <param name="newPort">the port assigned to a new UdpServer </param>
        /// <param name="newHost">host assinged to a new Udp Server</param>
        public UdpServer(int newPort, string newHost)
        {
            personalID = ++initServer;
            ports[personalID - 1] = newPort;
            hosts[personalID - 1] = newHost;
            data = new byte[1024];
            ip = new IPEndPoint(IPAddress.Parse(newHost), newPort);
            Construct(ip);

        }

        public void Construct(IPEndPoint endPoint)
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
            
            while (true)
            {
                semaphore.WaitOne();
                if (!stopwatch.IsRunning)
                {
                    Console.WriteLine("Starting Server {0}", personalID);
                    Console.WriteLine("Waiting for Client...");
                    if(serverSocket.Client == null)
                    {
                        Construct(ip);
                    }
                    stopwatch.Restart();
                }

                while (true)
                {
                    try
                    {
                        data = new byte[1024];
                        var sender = new IPEndPoint(IPAddress.Any, 0);
                        data = serverSocket.Receive(ref sender);

                        data = data.Where(x => x != 0x00).ToArray(); // functions inspired from https://stackoverflow.com/questions/13318561/adding-new-line-of-data-to-textbox 
                        string myString = Encoding.ASCII.GetString(data).Trim();//see link on the aboce line

                        Console.WriteLine("Server {0} : {1}", personalID, myString);
                        string newString = "String " + myString + " has been received from " + personalID.ToString();

                        byte[] feed = Encoding.ASCII.GetBytes(newString);
                        serverSocket.Send(feed, feed.Length, sender);

                        if (stopwatch.ElapsedMilliseconds > 10000)
                        {
                            Console.WriteLine("Exiting Server {0} ...", personalID);
                            stopwatch.Stop();
                            serverSocket.Close();
                            semaphore.Release();
                            break;
                        }

                    }
                    catch (Exception e)
                    {
                        if (stopwatch.ElapsedMilliseconds > 15000)
                        {
                            Console.WriteLine("Exiting Server {0} ...", personalID);
                            stopwatch.Stop();
                            serverSocket.Close();
                            semaphore.Release();
                            break;
                        }
                    }
                }
                
            }

        }

        [ObsoleteAttribute("Busywait function is obsolete. Functionality is implemented in Start function.", false)]
        public void BusyWait()
        {
            Console.WriteLine("Pausing Server {0} ...", personalID);
            Thread.Sleep(300000);
        }

        /// <summary>
        /// Allows a Server to change its port and hostname and notifies Twin Server 
        /// </summary>
        /// <param name="changePort">new port of to change towards </param>
        /// <param name="changeHost">new hostname of to change towards </param>
        public void NotifyChange(int changePort, string changeHost)
        {
            IPEndPoint otherServIp = new IPEndPoint(IPAddress.Parse(hosts[personalID % 2]), ports[personalID % 2]);
            UdpClient tempSocket = new UdpClient();
            tempSocket.Connect(otherServIp);

            string change = "Server " + personalID + " Update, Hostname : " + changeHost + ", Socket : " + changePort;
            byte[] update = Encoding.ASCII.GetBytes(change);
            tempSocket.Send(update, update.Length);

            ip = new IPEndPoint(IPAddress.Parse(changeHost), changePort);
            serverSocket = null;
            serverSocket = new UdpClient(ip);

        }

        private static bool TrackFunction(TimeSpan timeSpan, Action action)
        {
            try
            {
                Task task = Task.Factory.StartNew(() => action());
                task.Wait(timeSpan);
                return task.IsCompleted;
            }
            catch (AggregateException ae)
            {
                throw ae.InnerException;
            }
        }

        
        protected UdpClient serverSocket;//protected socket element
        protected IPEndPoint ip;//the ip of the server
        protected byte[] data;//the databus of the server
        protected static int initServer = 0;//a static element that is incremented the moment a server is initalized
        protected int personalID = 0;//
        protected Stopwatch stopwatch = new Stopwatch();

        protected static int[] ports = new int[2];//retains the ports of the servers
        protected static string[] hosts = new string[2];//retains the hosts of the servers
        public static Semaphore semaphore = new Semaphore(1, 1);
    }
}
