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

            string currentPort = "";
            string otherServerIP = "";
            string otherPort = "";
            Console.WriteLine("Is this server A or server B");
            currentServer = Console.ReadLine();
            while(!(currentServer.Equals("A") || currentServer.Equals("B")))
            {
                Console.WriteLine("Invalid argument, Is this server A or server B");
                currentServer = Console.ReadLine();
            }

            //if (currentServer.Equals("A"))
            //{
            //    currentPort = "4444";
            //    otherPort = "3333";
            //    otherServerIP = GetLocalIPAddress();
            //}
            //else
            //{
            //    currentPort = "8888";
            //    currentPort = Console.ReadLine();
            //    otherPort = "4444";
            //    otherServerIP = GetLocalIPAddress();
            //}



            Console.WriteLine("Enter Port for this server: ");
            currentPort = Console.ReadLine();


            Console.WriteLine("Server " + currentServer + " IP address: "
                + GetLocalIPAddress() + ":" + currentPort);

            Console.WriteLine("Enter IP (no port) for the other server: ");
            otherServerIP = Console.ReadLine();
            while (otherServerIP.Length < 1)
            {
                Console.WriteLine("Invalid IP, try again");
                otherServerIP = Console.ReadLine();
            }
            

            Console.WriteLine("Enter Port for the other server: ");
            otherPort = Console.ReadLine();


            int currentPortInt;
            int otherPortInt;
            try
            {
                currentPortInt = int.Parse(currentPort);
                otherPortInt = int.Parse(otherPort);
            }
            catch(FormatException)
            {
                Console.WriteLine("Invalid Port");
                return;
            }

            UdpServer Server = new UdpServer(GetLocalIPAddress(), otherServerIP, currentPortInt, otherPortInt);
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