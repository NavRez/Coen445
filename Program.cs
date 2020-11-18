using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace UDPSocketProject
{
    class Program
    {
        public static string currentServer;
        static void Main(string[] args)
        {
            Console.WriteLine("Is this server A or server B");
            currentServer = Console.ReadLine();
            while(!(currentServer.Equals("A") || currentServer.Equals("B")))
            {
                Console.WriteLine("Invalid argument, Is this server A or server B");
                currentServer = Console.ReadLine();
            }
            Console.WriteLine("Enter Port for this server: ");
            //string currentPort = Console.ReadLine();

            Console.WriteLine("Enter Port for the other server: ");
            //string otherPort = Console.ReadLine();


            Console.WriteLine("Enter IP for the other server: ");
            //string otherServerIP = Console.ReadLine();
            string currentPort = "4444";
            string otherPort = "3333";
            string otherServerIP = GetLocalIPAddress();

            if (currentServer.Equals("A"))
            {
                currentPort = "4444";
                otherPort = "3333";
                otherServerIP = GetLocalIPAddress();
            }
            else
            {
                currentPort = "3333";
                otherPort = "4444";
                otherServerIP = GetLocalIPAddress();
            }

            

            string currentServerIP = "127.0.0.1";
            int currentPortInt = 0;
            int OtherPortInt = 0;
            
            try
            {
                currentPortInt  =  Int32.Parse(currentPort);
                OtherPortInt = Int32.Parse(otherPort);
            }
            catch(FormatException)
            {
                Console.WriteLine("Invalid Port");
                return;
            }

            UdpServer Server = new UdpServer(GetLocalIPAddress(), otherServerIP, currentPortInt, OtherPortInt);
            //UdpServer dualServer = new UdpServer("127.0.0.2", 5080);
            Server.Start(currentServer);

            
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }


    }
}