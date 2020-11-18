using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDPSocketProject
{
    public class MessageEventHandler
    {

        public MessageEventHandler() 
        {
            ; 
        }
        public class ClientElements
        {
            public ClientElements(string name, string ip)
            {
                clientName = name;
                ipAddress = ip;
            }

            public ClientElements(string name, string ip, List<string> subs)
            {
                clientName = name;
                ipAddress = ip;       
                clientSubjects = subs;
            }

            public void resetSubjects(List<string> newSubs)
            {
                clientSubjects = newSubs;
            }

            public void changeIP(string ip)
            {
                ipAddress = ip;
            }

            public string clientName;
            public string ipAddress;
            public int clientPort;
            public List<string> clientSubjects = new List<string>();

        }

        public List<ClientElements> clients = new List<ClientElements>();

        public string SwitchCase(string incomingInfo, Socket socket)
        {
            string[] array = incomingInfo.Split(",");

            string val = array[0];
            string RQ = null;
            string Name = null;
            string message = null;
            string ipAddress = null;
            switch (val)
            {
                case "REGISTER":
                    RQ = array[1];
                    Name = array[2];
                    ipAddress = array[3];
                    var User1 = new ClientElements(Name, ipAddress);
                
                    if (clients.Any(i => i.clientName.Equals(Name)))
                    {
                        //Register-Denied
                        message = "REGISTER-DENIED,";
                        message += RQ + ",Name is already in use";                  
                    }
                    else
                    {
                        clients.Add(User1);
                        message = "REGISTERED,";
                        message += RQ + "," + Name + "," + ipAddress;    
                    }
                    return message;
                case "DE-REGISTER":
                    RQ = array[1];
                    Name = array[2];
                    if (clients.Any(i => i.clientName.Equals(Name)))
                    {
                        //de-register
                        clients.RemoveAll(n => n.clientName.Equals(Name));
                        message = "DE-REGISTERED,";
                        message += Name;    
                    } 
                    else
                    {
                        message = "User not registered";
                    }
                    return message;
                case "UPDATE":
                    RQ = array[1];
                    Name = array[2];
                    ipAddress = array[3];

                    if (clients.Any(i=>i.clientName.Equals(Name)))
                    {
                        var element = clients.Find(obj => obj.clientName.Equals(Name));
                        element.changeIP(ipAddress);
                        clients[clients.FindIndex(obj=>obj.clientName.Equals(Name))] = element;
                        message = "UPDATE-CONFIRMED,";
                        message += RQ + "," + Name + "," + ipAddress;
                    } 
                    else
                    {
                        message = "UPDATE-DENIED,";
                        message += RQ + "," + Name + " does not exist";
                    }
                    return message;
                case "PUBLISH":
                    RQ = array[1];
                    Name = array[2];
                    string subj = array[3];
                    string userMessage = array[4];
                    ipAddress = array[5];
                    bool subjectInterest = false;
                    message = String.Format("MESSAGE,{0},{1},{2}", Name, subj, userMessage);
                    foreach (ClientElements element in clients)
                    {
                        if (element.clientSubjects.Contains(subj))
                        {

                            List<string> ipandPort = element.ipAddress.Split(":").ToList();

                            IPEndPoint clientIP = new IPEndPoint(IPAddress.Parse(ipandPort[0]),
                                Int32.Parse(ipandPort[1]));
                            byte[] userFeed = Encoding.ASCII.GetBytes(message);
                            socket.SendTo(userFeed, 0, userFeed.Length,SocketFlags.None, clientIP);
                            subjectInterest = true;
                        }
                    }

                    if (!subjectInterest) 
                    {
                        message = String.Format("PUBLISH-DENIED,{0},{1}, Error, no clients contain such a subject", Name, subj);
                        return message;
                    }
                    return "";

                case "SUBJECTS":
                    RQ = array[1];
                    Name = array[2];
                    List<string> newSubs = array[3].Split("@").ToList();
                    if (clients.Any(i => i.clientName.Equals(Name)))
                    {
                        var element = clients.Find(obj => obj.clientName.Equals(Name));
                        element.clientSubjects = new List<string>();
                        
                        element.clientSubjects = newSubs;
                        message = String.Format("SUBJECTS-UPDATED,{0},{1},{2}", RQ, Name, array[3]);
                    }
                    else
                    {
                        message = String.Format("SUBJECTS-REJECTED,{0},{1},{2}", RQ, Name, array[3]);
                    }                        
                    return message;
                case "WAKE-UP":
                    string thisServerIP = array[1];
                    foreach (ClientElements element in clients)
                    {
                        List<string> ipandPort = element.ipAddress.Split(":").ToList();
                        IPEndPoint clientIP = new IPEndPoint(IPAddress.Parse(ipandPort[0]),
                            Int32.Parse(ipandPort[1]));

                        byte[] userFeed = Encoding.ASCII.GetBytes("CHANGE-SERVER,"+thisServerIP);
                        socket.SendTo(userFeed, 0, userFeed.Length, SocketFlags.None, clientIP);
                    }
                    return "";

                default:
                    return null;
            }
        }
    }
}
