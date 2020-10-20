using System;
using System.Threading;

namespace UDPSocketProject
{
    class Program
    {
        static void Main(string[] args)
        {
            UdpServer Server = new UdpServer(8080, "127.0.0.2");
            UdpServer dualServer = new UdpServer(5080, "127.0.0.2");

            Thread server1 = new Thread(Server.Start);
            Thread server2 = new Thread(dualServer.Start);

            server1.Start();
            server2.Start();
            Thread.Sleep(100);
            Thread looping = new Thread(() =>
            {
                while (true)
                {
                    Random random = new Random();
                    int val = random.Next(30000);
                    Server.NotifyChange(val, "127.0.0.5");
                }
            });
            Thread seclooping = new Thread(() =>
            {
                while (true)
                {
                    Random random = new Random();
                    int val = random.Next(30000);
                    val += 30000;
                    dualServer.NotifyChange(val, "127.0.0.5");
                }
            });
            looping.Start();
            seclooping.Start();


        }
    }
}