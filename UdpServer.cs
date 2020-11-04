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
        public UdpServer(string newHost,int newPort)
        {
            personalID = ++initServer;
            ports[personalID - 1] = newPort;
            hosts[personalID - 1] = newHost;
            data = new byte[1024];
            ip = new IPEndPoint(IPAddress.Parse(newHost), newPort);
            BindSocket(ip);

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

            while (true)
            {
                allowReceive = false;
                semaphore.WaitOne();
                internalSemaphore.WaitOne();
                running = true;
                if (!stopwatch.IsRunning)
                {
                    Console.WriteLine("Starting Server {0}", personalID);
                    Console.WriteLine("Waiting for Client...");

                    if (serverSocket.Client == null)
                    {
                        BindSocket(ip);
                    }
                    else
                    {
                        serverSocket.Client.ReceiveTimeout = 5000;
                        serverSocket.Client.SendTimeout = 5000;
                    }
                    stopwatch.Restart();
                }

                while (true)
                {
                    try
                    {
                        data = new byte[1024];
                        var sender = new IPEndPoint(IPAddress.Any, 0);
                        Console.WriteLine("Trying to Receive...");
                        data = serverSocket.Receive(ref sender);

                        data = data.Where(x => x != 0x00).ToArray(); // functions inspired from https://stackoverflow.com/questions/13318561/adding-new-line-of-data-to-textbox 
                        string myString = Encoding.ASCII.GetString(data).Trim();//see link on the aboce line

                        Console.WriteLine("Server {0} : {1}", personalID, myString);
                        string newString = "String " + myString + " has been received from " + personalID.ToString();

                        byte[] feed = Encoding.ASCII.GetBytes(myString);

                        if(myString.Equals("connect user request"))
                        {
                            feed = Encoding.ASCII.GetBytes("user connected");
                        }

                        if (myString.Equals("remove user request"))
                        {
                            feed = Encoding.ASCII.GetBytes("user removed");
                        }

                        if(myString.Equals("send submit"))
                        {
                            feed = Encoding.ASCII.GetBytes("submit received");
                        }

                        if(myString.Equals("send updated list"))
                        {
                            feed = Encoding.ASCII.GetBytes("updated list received");
                        }
                        
                        serverSocket.Send(feed, feed.Length, sender);
                        Thread.Sleep(200);

                        IPEndPoint doupdateServIp = new IPEndPoint(IPAddress.Parse(hosts[personalID % 2]), ports[personalID % 2]);
                        serverSocket.Connect(doupdateServIp);
                        allowReceive = true;
                        //Console.WriteLine("Trying to Send...");
                        serverSocket.Send(feed, feed.Length);
                        Thread.Sleep(500);

                        if (stopwatch.ElapsedMilliseconds > 31000)
                        {
                            Console.WriteLine("Exiting Server {0} ...", personalID);
                            stopwatch.Stop();
                            //serverSocket.Client.Disconnect(true);
                            serverSocket.Close();
                            semaphore.Release();
                            internalSemaphore.Release();
                            running = false;
                            break;
                        }

                    }
                    catch (Exception e)
                    {
                        if (stopwatch.ElapsedMilliseconds > 11000)
                        {
                            Console.WriteLine("Exiting Server {0} ...", personalID);
                            stopwatch.Stop();
                            //serverSocket.Client.Disconnect(true);
                            serverSocket.Close();
                            semaphore.Release();
                            internalSemaphore.Release();
                            running = false;
                            break;
                        }
                        else
                        {
                            //Console.WriteLine("Operation Failed \n");
                        }
                    }
                    //serverSocket.Client.Disconnect(true);

                    serverSocket.Close();
                    BindSocket(ip);
                }

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

        /// <summary>
        /// Allows a Server to change its port and hostname and notifies Twin Server 
        /// </summary>
        /// <param name="changePort">new port of to change towards </param>
        /// <param name="changeHost">new hostname of to change towards </param>
        public void NotifyChange(int changePort, string changeHost)
        {
            if (!running)
            {
                internalSemaphore.WaitOne();
                if (!notifyStopwatch.IsRunning)
                {
                    try
                    {
                        IPEndPoint otherServIp = new IPEndPoint(IPAddress.Parse(hosts[personalID % 2]), ports[personalID % 2]);
                        UdpClient tempSocket = new UdpClient();
                        tempSocket.Connect(otherServIp);

                        //ip = new IPEndPoint(IPAddress.Parse(changeHost), changePort);
                        //serverSocket.Client.Disconnect(true);
                        //serverSocket.Close();
                        //serverSocket = null;
                        //serverSocket = new UdpClient(ip);
                        //ports[personalID - 1] = changePort;
                        //hosts[personalID - 1] = changeHost;

                        string change = "Server " + personalID + " Update, Hostname : " + changeHost + ", Socket : " + changePort;
                        byte[] update = Encoding.ASCII.GetBytes(change);
                        tempSocket.Send(update, update.Length);

                        notifyStopwatch.Start();

                    }
                    catch (SocketException sameSoc)
                    {
                        notifyStopwatch.Start();
                    }
                    
                }
                else if (notifyStopwatch.ElapsedMilliseconds > 300000)
                {
                    notifyStopwatch.Reset();
                }

                try
                {
                    IPEndPoint updateServIp = new IPEndPoint(IPAddress.Parse(hosts[personalID % 2]), ports[personalID % 2]);
                    if (serverSocket.Client == null)
                    {
                        //serverSocket.Client.Disconnect(true);
                        serverSocket.Close();
                        serverSocket = null;
                        serverSocket = new UdpClient(ip);
                    }
                    bool timeTracker = TrackFunction(TimeSpan.FromSeconds(15), () =>
                    {
                        try
                        {
                            if(allowReceive)
                            {
                                data = serverSocket.Receive(ref updateServIp);
                            
                            
                                if (updateServIp.Address.ToString().Equals(hosts[personalID % 2]))
                                {
                                    if (updateServIp.Port == ports[personalID % 2])
                                    {
                                    

                                        data = data.Where(x => x != 0x00).ToArray(); // functions inspired from https://stackoverflow.com/questions/13318561/adding-new-line-of-data-to-textbox 
                                        string myString = Encoding.ASCII.GetString(data).Trim();//see link on the aboce line
                                        if (!myString.Equals(""))
                                        {
                                            Console.WriteLine("External Server {0} out", personalID);
                                            Console.WriteLine("External Server {0} : {1}", personalID, myString);

                                        }
                                        //serverSocket.Client.Disconnect(true);
                                        serverSocket.Close();
                                    }
                                    else
                                    {
                                        throw new InvalidCastException("Address is not other Server");
                                    }

                                }

                            }
                        }
                        catch (Exception except)
                        {

                        }

                    });


                    internalSemaphore.Release();
                    Thread.Sleep(1000);

                }
                catch (Exception exception)
                {
                    //serverSocket.Client.Disconnect(true);
                    serverSocket.Close();
                    internalSemaphore.Release();
                    Thread.Sleep(1000);
                }


            }



        }


        public class ClientElements
        {
            public ClientElements(string name, string host, int port)
            {
                clientName = name;
                clientHost = host;
                clientPort = port;
                ipAdress = clientHost + "." + clientHost.ToString();
            }

            public ClientElements(string name, string host, int port, List<string> subs)
            {
                clientName = name;
                clientHost = host;
                clientPort = port;
                clientSubjects = subs;
                ipAdress = clientHost + "." + clientHost.ToString();
            }

            public void resetSubjects(List<string> newSubs)
            {
                clientSubjects = newSubs;
            }

            public void changeIP(string newHost, int newPort)
            {
                clientHost = newHost;
                clientPort = newPort;
                ipAdress = newHost + "." + newPort.ToString();
            }

            private string clientName;
            private string clientHost;
            private string ipAdress;
            private int clientPort;
            private List<string> clientSubjects;

        }

        //these attributes are unique to each server
        protected UdpClient serverSocket;//protected socket element
        protected IPEndPoint ip;//the ip of the server
        protected byte[] data;//the databus of the server
        protected static int initServer = 0;//a static element that is incremented the moment a server is initalized
        protected int personalID = 0;//
        protected Stopwatch stopwatch = new Stopwatch();
        protected Stopwatch notifyStopwatch = new Stopwatch();
        public string[] subjects = { "computer engineering", "Disney Marvel", "Pokemon", "Final Fantasy", "Zack Fair", "Mario", "Mexican Studies", "Calculus", "Protocols", "US politics" };
        public static bool allowReceive = false;

        //shared information between servers
        protected static int[] ports = new int[2];//retains the ports of the servers
        protected static string[] hosts = new string[2];//retains the hosts of the servers
        public static Semaphore semaphore = new Semaphore(1, 1);
        public Semaphore internalSemaphore = new Semaphore(1, 1);
        public List<ClientElements> clients = new List<ClientElements>();
        public bool running = false;
    }
}