using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace UDPSocketProject
{
    class Program
    {
        static int v = 0;
        static void Main(string[] args)
        {
            try
            {
                int val = 8080;
                UdpServer Server = new UdpServer("127.0.0.2", val);
                //UdpServer dualServer = new UdpServer("127.0.0.2", 5080);
                Thread server1 = new Thread(Server.Start);
                //Thread server2 = new Thread(dualServer.Start);

                server1.Start();

                Thread looping1 = new Thread(() =>
                {
                    while (true)
                    {
                        Random random = new Random();
                        int val = random.Next(30000);
                        Server.NotifyChange(val, "127.0.0.5");
                    }
                });

                looping1.Start();

            }
            catch
            {
                int val = 5080;
                UdpServer Server2 = new UdpServer("127.0.0.2", val);
                //UdpServer dualServer = new UdpServer("127.0.0.2", 5080);

                Thread server2 = new Thread(Server2.Start);
                //Thread server2 = new Thread(dualServer.Start);

                server2.Start();

                Thread seclooping1 = new Thread(() =>
                {
                    while (true)
                    {
                        Random random = new Random();
                        int val = random.Next(30000);
                        val += 30000;
                        Server2.NotifyChange(val, "127.0.0.5");
                    }
                });

                seclooping1.Start();
            }
            //server2.Start();
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            /*
            Thread thread3 = new Thread(() => {
                while(true)
                    try
                    {

                        byte[] bus = new byte[1024];
                        var ip = new IPEndPoint(IPAddress.Parse("127.0.0.2"), 5080);
                        var clienting = new UdpClient(ip);
                        bus = clienting.Receive(ref ip);
                        Console.WriteLine("printing : {0}, from {1}", bus.ToString(),1);
                        clienting.Close();
                        clienting = null;


                    }
                    catch(Exception exc)
                    {
                        Console.WriteLine(exc.Message);
                    }


            });
            Thread thread4 = new Thread(() => {
                while(true)
                    try
                    {
                        byte[] bus = new byte[1024];
                        var ip = new IPEndPoint(IPAddress.Parse("127.0.0.2"), 5081);
                        var clienting = new UdpClient(ip);
                        bus = clienting.Receive(ref ip);
                        Console.WriteLine("printing : {0}, from {1}", bus.ToString(), 2);
                        clienting.Close();
                        clienting = null;
                    }
                    catch (Exception exc)
                    {

                        Console.WriteLine(exc.Message);
                    }


            });
            Thread thread5 = new Thread(() => {
                while(true)
                    try
                    {

                        byte[] bus = new byte[1024];
                        var ip = new IPEndPoint(IPAddress.Parse("127.0.0.2"), 5082);
                        var clienting = new UdpClient(ip);
                        bus = clienting.Receive(ref ip);
                        Console.WriteLine("printing : {0}, from {1}", bus.ToString(), 3);
                        clienting.Close();
                        clienting = null;
                    }
                    catch (Exception exc)
                    {

                        Console.WriteLine(exc.Message);
                    }


            });
            thread3.Start();
            thread4.Start();
            thread5.Start();
            */
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            Thread.Sleep(100);
            Thread looping = new Thread(() =>
            {
                while (true)
                {
                    Random random = new Random();
                    int val = random.Next(30000);
                    //Server.NotifyChange(val, "127.0.0.5");
                }
            });
            Thread seclooping = new Thread(() =>
            {
                while (true)
                {
                    Random random = new Random();
                    int val = random.Next(30000);
                    val += 30000;
                    //dualServer.NotifyChange(val, "127.0.0.5");
                }
            });
            //looping.Start();
            //seclooping.Start();


        }
    }
}