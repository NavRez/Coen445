using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace UDPSocketProject
{
    class UdpClients : UdpClient, IDisposable
    {
        #region properties
        public string clientName { get; set; }
        public string hostName { get; set; }
        public IPEndPoint ip { get; set; }
        public bool registered { get; set; }
        public string subject { get; set; }
        public int socketNum { get; set; }
        #endregion


        public UdpClients(string _clientName, string _hostName, IPEndPoint _ip, int _socketNum, string _subject, bool _registered) 
        {
            clientName = _clientName;
            hostName = _hostName;
            ip = _ip;
            socketNum = _socketNum;
            subject = _subject;
            registered = _registered;
        }

        public UdpClients(string _clientName, IPEndPoint _ip, int _socketNum)
        {
            clientName = _clientName;
            ip = _ip;
            socketNum = _socketNum;
            registered = true;
        }


        //These are already in the properties with .get .set methods, so idk if a function is needed?
        //public void ModifyIP(IPEndPoint newIP)
        //{
        //    ip = newIP;
        //}
        //public void ModifySocketNum(int newSocketNum)
        //{
        //    socketNum = newSocketNum;
        //}


        //May be needed if registered gets set to false to remove all the information of the object
        protected virtual void Dispose(bool disposing)
        {
            
        }
    }
}
