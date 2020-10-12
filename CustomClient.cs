using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace UDPSocketProject
{
    class CustomClient : UdpClient, IDisposable
    {
        #region properties
        public string clientName { get; set; }
        public string clientHost { get; set; }
        public string clientIP { get; set; }
        public bool registered { get; set; }
        public List<string> clientSubjects { get; set; }
        public int clientPort { get; set; }

        public UdpClient udpClient { get; set; }



        #endregion


        public CustomClient(string _clientName, string _clientHost, string _ip, int _port, List<string> _subject, bool _registered) 
        {
            clientName = _clientName;
            clientHost = _clientHost;
            clientIP = _ip;
            clientPort = _port;
            clientSubjects = _subject;
            registered = _registered;
        }

        public CustomClient(string _clientName, string _ip, int _port)
        {
            clientName = _clientName;
            clientPort = _port;
            clientIP = _ip;
            registered = true;
        }


        //public CustomClient(string name, string host, int port)
        //{
        //    clientName = name;
        //    clientHost = host;
        //    clientPort = port;
        //    clientIp = clientHost + "." + clientHost.ToString();
        //    IPEndPoint ip = new IPEndPoint(IPAddress.Parse(host), port);
        //    udpClient = new UdpClient(ip);
        //    udpClient.Client.ReceiveTimeout = 3000;
        //    udpClient.Client.SendTimeout = 3000;

        //}

        //This can just be obj.clientSubjects.Clear()
        //public void resetSubjects(List<string> newSubs)
        //{
        //    clientSubjects = newSubs;
        //}

        //These are already in the properties with .get .set methods, so idk if a function is needed?
        //public void ModifyIP(IPEndPoint newIP)
        //{
        //    ip = newIP;
        //}
        //public void Modifyport(int newport)
        //{
        //    port = newport;
        //}

        public void RestartClient()
        {
            udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(clientHost), clientPort));
            udpClient.Client.ReceiveTimeout = 3000;
            udpClient.Client.SendTimeout = 3000;
        }

        public void ChangeIP(string newHost, int newPort)
        {
            clientHost = newHost;
            clientPort = newPort;
            clientIP = newHost + "." + newPort.ToString();
        }


        public void AddSubject(string sbject)
        {
            if (!clientSubjects.Contains(sbject))
            {
                clientSubjects.Add(sbject);
            }
        }

        public void AddSubjects(List<string> subjects)
        {
            foreach (var subject in subjects)
            {
                if (!clientSubjects.Contains(subject))
                {
                    clientSubjects.Add(subject);
                }
            }
        }

        //May be needed if registered gets set to false to remove all the information of the object
        protected virtual void Dispose(bool disposing)
        {
            
        }
    }
}
