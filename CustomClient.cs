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
        public string ClientName { get; set; }
        public string ClientHost { get; set; }
        public string ClientIP { get; set; }
        public bool Registered { get; set; }
        public List<string> ClientSubjects { get; set; }
        public int ClientPort { get; set; }

        public UdpClient UdpClient { get; set; }



        #endregion


        public CustomClient(string _clientName, string _clientHost, string _ip, int _port, List<string> _subject, bool _registered) 
        {
            ClientName = _clientName;
            ClientHost = _clientHost;
            ClientIP = _ip;
            ClientPort = _port;
            ClientSubjects = _subject;
            Registered = _registered;
        }

        public CustomClient(string _clientName, string _ip, int _port)
        {
            ClientName = _clientName;
            ClientPort = _port;
            ClientIP = _ip;
            Registered = true;
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
            UdpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(ClientHost), ClientPort));
            UdpClient.Client.ReceiveTimeout = 3000;
            UdpClient.Client.SendTimeout = 3000;
        }

        public void ChangeIP(string newHost, int newPort)
        {
            ClientHost = newHost;
            ClientPort = newPort;
            ClientIP = newHost + "." + newPort.ToString();
        }


        public void AddSubject(string sbject)
        {
            if (!ClientSubjects.Contains(sbject))
            {
                ClientSubjects.Add(sbject);
            }
        }

        public void AddSubjects(List<string> subjects)
        {
            foreach (var subject in subjects)
            {
                if (!ClientSubjects.Contains(subject))
                {
                    ClientSubjects.Add(subject);
                }
            }
        }

        //May be needed if registered gets set to false to remove all the information of the object
        protected virtual void Dispose(bool disposing)
        {
            
        }
    }
}
