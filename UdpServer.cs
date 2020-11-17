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
            //while (true)
            //{
            //    semaphore.WaitOne();
            //    allowReceive = false;
            //    Console.WriteLine("moving past first Semaphore in Server {0} ", personalID);
            //    internalSemaphore.WaitOne();
            //    Console.WriteLine("moving past internal Semaphore in Server {0} ", personalID);
            //    running = true;
            //    if (!stopwatch.IsRunning)
            //    {
            //        Console.WriteLine("Starting Server {0}", personalID);
            //        Console.WriteLine("Waiting for Client...");

            //        if (serverSocket.Client == null)
            //        {
            //            BindSocket(ip);
            //        }
            //        else
            //        {
            //            serverSocket.Client.ReceiveTimeout = 5000;
            //            serverSocket.Client.SendTimeout = 5000;
            //        }
            //        stopwatch.Restart();
            //    }

            //    while (true)
            //    {
            //        try
            //        {
            //            data = new byte[1024];
            //            var sender = new IPEndPoint(IPAddress.Any, 0);
            //            Console.WriteLine("Trying to Receive...");
            //            data = serverSocket.Receive(ref sender);

            //            data = data.Where(x => x != 0x00).ToArray(); // functions inspired from https://stackoverflow.com/questions/13318561/adding-new-line-of-data-to-textbox 
            //            string myString = Encoding.ASCII.GetString(data).Trim();//see link on the aboce line

            //            string[] arr = myString.Split(",");

            //            Console.WriteLine("Server {0} : {1}", personalID, myString);
            //            string newString = "String " + myString + " has been received from " + personalID.ToString();

            //            string serverResponse = clientTest.SwitchCase(myString, serverSocket);
            //            byte[] feed = Encoding.ASCII.GetBytes(serverResponse);

            //            //byte[] feed = Encoding.ASCII.GetBytes(newString);

            //            //if(myString.Equals("connect user request"))
            //            //{
            //            //    feed = Encoding.ASCII.GetBytes("user connected");
            //            //}

            //            //if (myString.Equals("remove user request"))
            //            //{
            //            //    feed = Encoding.ASCII.GetBytes("user removed");
            //            //}

            //            //if(myString.Equals("send submit"))
            //            //{
            //            //    feed = Encoding.ASCII.GetBytes("submit received");
            //            //}

            //            //if(myString.Equals("send updated list"))
            //            //{
            //            //    feed = Encoding.ASCII.GetBytes("updated list received");
            //            //}
            //            if (serverResponse.Contains("MESSAGE"))
            //            {

            //                continue;
            //            }
            //            else
            //            {
            //                serverSocket.Send(feed, feed.Length, sender);

            //            }
            //            Thread.Sleep(100);

            //            IPEndPoint doupdateServIp = new IPEndPoint(IPAddress.Parse(hosts[personalID % 2]), ports[personalID % 2]);
            //            serverSocket.Connect(doupdateServIp);
            //            allowReceive = true;

            //            serverSocket.Send(feed, feed.Length);
            //            Thread.Sleep(500);

            //            if (stopwatch.ElapsedMilliseconds > 11000)
            //            {
            //                clientTest.ChangeServer(serverSocket, hosts[personalID % 2], ports[personalID % 2]);
            //                serverSocket.Connect(doupdateServIp);
            //                feed = Encoding.ASCII.GetBytes("CHANGE");
            //                serverSocket.Send(feed, feed.Length);

            //                Console.WriteLine("Exiting Server {0} ...", personalID);
            //                stopwatch.Stop();
            //                serverSocket.Close();
            //                semaphore.Release();
            //                internalSemaphore.Release();
            //                running = false;
            //                break;
            //            }

            //        }
            //        catch (Exception e)
            //        {
            //            if (stopwatch.ElapsedMilliseconds > 11000)
            //            {
            //                IPEndPoint doupdateServIp = new IPEndPoint(IPAddress.Parse(hosts[personalID % 2]), ports[personalID % 2]);
            //                clientTest.ChangeServer(serverSocket, hosts[personalID % 2], ports[personalID % 2]);
            //                try
            //                {
            //                    serverSocket.Connect(doupdateServIp);
            //                }
            //                catch(Exception error)
            //                {

            //                }
            //                byte[] feed = Encoding.ASCII.GetBytes("CHANGE");
            //                serverSocket.Send(feed, feed.Length);

            //                Console.WriteLine("Exiting Server {0} ...", personalID);
            //                stopwatch.Stop();
            //                serverSocket.Close();
            //                semaphore.Release();
            //                internalSemaphore.Release();
            //                running = false;
            //                break;
            //            }
            //            else
            //            {
            //                //Console.WriteLine("Operation Failed \n");
            //            }
            //        }


            //        serverSocket.Close();
            //        BindSocket(ip);
            //    }

            //}
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

                string serverResponse = clientTest.SwitchCase(receivedMessage, serverSocket);
                byte[] feed = Encoding.ASCII.GetBytes(serverResponse);
                Console.WriteLine(serverResponse);

                serverSocket.Send(feed, feed.Length, senderRemote);
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
        //public void NotifyChange(int changePort, string changeHost)
        //{
        //    Thread.Sleep(1000);
        //    internalSemaphore.WaitOne();
        //    while (true)
        //    {

        //        if (!notifyStopwatch.IsRunning)
        //        {
        //            Console.WriteLine("Going into update in NotifyChange");
        //            try
        //            {
        //                IPEndPoint otherServIp = new IPEndPoint(IPAddress.Parse(hosts[personalID % 2]), ports[personalID % 2]);
        //                UdpClient tempSocket = new UdpClient();
        //                tempSocket.Connect(otherServIp);

        //                ip = new IPEndPoint(IPAddress.Parse(changeHost), changePort);
        //                serverSocket.Close();
        //                serverSocket = null;
        //                serverSocket = new UdpClient(ip);
        //                ports[personalID - 1] = changePort;
        //                hosts[personalID - 1] = changeHost;

        //                string change = String.Format("SERVER-UPDATE,{0},{1}", changeHost, changePort);
        //                byte[] update = Encoding.ASCII.GetBytes(change);
        //                tempSocket.Send(update, update.Length);

        //                notifyStopwatch.Restart();
        //                Console.WriteLine("Restarted stopwatch in update");

        //            }
        //            catch (SocketException sameSoc)
        //            {
        //                notifyStopwatch.Restart();
        //            }
        //            catch (System.FormatException forsec)
        //            {
        //                notifyStopwatch.Restart();
        //            }

        //        }

        //        try
        //        {
        //            IPEndPoint updateServIp = new IPEndPoint(IPAddress.Parse(hosts[personalID % 2]), ports[personalID % 2]);
        //            if (serverSocket.Client == null)
        //            {
        //                serverSocket.Close();
        //                serverSocket = null;
        //                serverSocket = new UdpClient(ip);
        //                serverSocket.Client.ReceiveTimeout = 5000;
        //            }
        //            bool timeTracker = TrackFunction(TimeSpan.FromSeconds(7), () =>
        //            {
        //            try
        //            {
        //                if(allowReceive)
        //                {
        //                Console.WriteLine("entering the trackfunc");
        //                serverSocket.Connect(hosts[personalID % 2], ports[personalID % 2]);
        //                data = serverSocket.Receive(ref updateServIp);


        //                if (updateServIp.Address.ToString().Equals(hosts[personalID % 2]))
        //                {
        //                    if (updateServIp.Port == ports[personalID % 2])
        //                    {


        //                        data = data.Where(x => x != 0x00).ToArray(); // functions inspired from https://stackoverflow.com/questions/13318561/adding-new-line-of-data-to-textbox 
        //                        string myString = Encoding.ASCII.GetString(data).Trim();//see link on the aboce line
        //                        if (myString.Equals("CHANGE"))
        //                        {
        //                            int a = 0;
        //                            a = a / 0;
        //                        }
        //                        if (!myString.Equals(""))
        //                        {
        //                            Console.WriteLine("External Server {0} out", personalID);
        //                            Console.WriteLine("External Server {0} : {1}", personalID, myString);
        //                            clientTest.DromantServerReceive(myString);
                                    

        //                        }
        //                        serverSocket.Close();
        //                    }
        //                    else
        //                    {
        //                        throw new InvalidCastException("Address is not other Server");
        //                    }

        //                }

        //                }
        //            }
        //            catch (SocketException timeOut)
        //            {
        //                Console.WriteLine("Could not receive packets from sibling server");
        //            }
        //            catch(DivideByZeroException zero)
        //            {
        //                Console.WriteLine("Server Replacement Call : Exiting dormant {0} ...", personalID);
        //                serverSocket.Close();
        //                internalSemaphore.Release();
        //                notifyStopwatch.Stop();
        //                Thread.Sleep(1000);
        //                break;
        //            }
        //            catch (Exception except)
        //            {
        //                except.ToString();
        //            }

        //            });



        //        }
        //        catch (Exception exception)
        //        {
        //            if (notifyStopwatch.ElapsedMilliseconds > 19000)
        //            {
        //                Console.WriteLine("Exception : Exiting dormant {0} ...", personalID);
        //                serverSocket.Close();
        //                internalSemaphore.Release();
        //                notifyStopwatch.Stop();
        //                Thread.Sleep(1000);
        //                break;
        //            }
        //        }

        //        if (notifyStopwatch.ElapsedMilliseconds > 19000)
        //        {
        //            Console.WriteLine("Exiting dormant {0} ...", personalID);
        //            serverSocket.Close();
        //            internalSemaphore.Release();
        //            notifyStopwatch.Stop();
        //            Thread.Sleep(1000);
        //            break;
        //        }


        //    }



        //}


    }
}