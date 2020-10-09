using System;
using System.Threading;

namespace UDPSocketProject
{
    class Program
    {
        static void Main(string[] args)
        {
            UdpServer Server = new UdpServer(8080,"127.0.0.2");
            UdpServer dualServer = new UdpServer(5080, "127.0.0.2");

            Thread server1 = new Thread(Server.Start);
            Thread server2 = new Thread(dualServer.Start);

            server1.Start();
            server2.Start();
           /* Server.NotifyChange(6050,"127.0.0.1");
            Thread.Sleep(1000);
            Server.NotifyChange(6051, "127.0.0.3");
            Thread.Sleep(1000);
            Server.NotifyChange(6052, "127.0.0.2");
           */
        }
    }
}
