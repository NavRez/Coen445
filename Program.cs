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


            Console.WriteLine("Enter Port for this server: ");
            //string currentPort = Console.ReadLine();

            Console.WriteLine("Enter Port for the other server: ");
            //string otherPort = Console.ReadLine();


            Console.WriteLine("Enter IP for the other server: ");
            //string otherServerIP = Console.ReadLine();



            string currentPort = "4444";
            string otherPort = "3333";
            string otherServerIP = "127.0.0.2";

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

            UdpServer Server = new UdpServer(currentServerIP, otherServerIP, currentPortInt, OtherPortInt);
            //UdpServer dualServer = new UdpServer("127.0.0.2", 5080);
            Server.Start();

            
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