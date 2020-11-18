using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDPSocketProject
{
    public class Response
    {
        public string message;
        public bool valid;
        public Response()
        {
            message = "";
            valid = false;
        }
        public Response(string _message, bool _valid)
        {
            message = _message;
            valid = _valid;
        }
            
            
    }


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

        public Response SwitchCase(string incomingInfo, Socket socket)
        {
            Response response = new Response();
            string[] array = incomingInfo.Split(",");

            string val = array[0];
            string RQ = null;
            string Name = null;
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
                        response.message = "REGISTER-DENIED,";
                        response.message += RQ + ",Name is already in use";                  
                    }
                    else
                    {
                        clients.Add(User1);
                        response.message = "REGISTERED,";
                        response.message += RQ + "," + Name + "," + ipAddress;
                        response.valid = true;
                    }
                    return response;
                case "DE-REGISTER":
                    RQ = array[1];
                    Name = array[2];
                    if (clients.Any(i => i.clientName.Equals(Name)))
                    {
                        //de-register
                        clients.RemoveAll(n => n.clientName.Equals(Name));
                        response.message = "DE-REGISTERED,";
                        response.message += Name;
                        response.valid = true;
                    } 
                    else
                    {
                        response.message = "User not registered";
                    }
                    return response;
                case "UPDATE":
                    RQ = array[1];
                    Name = array[2];
                    ipAddress = array[3];

                    if (clients.Any(i=>i.clientName.Equals(Name)))
                    {
                        var element = clients.Find(obj => obj.clientName.Equals(Name));
                        element.changeIP(ipAddress);
                        clients[clients.FindIndex(obj=>obj.clientName.Equals(Name))] = element;
                        response.message = "UPDATE-CONFIRMED,";
                        response.message += RQ + "," + Name + "," + ipAddress;
                        response.valid = true;
                    } 
                    else
                    {
                        response.message = "UPDATE-DENIED,";
                        response.message += RQ + "," + Name + " does not exist";
                    }
                    return response;
                case "PUBLISH":
                    RQ = array[1];
                    Name = array[2];
                    string subj = array[3];
                    string userMessage = array[4];
                    ipAddress = array[5];
                    bool subjectInterest = false;
                    response.message = String.Format("MESSAGE,{0},{1},{2}", Name, subj, userMessage);
                    foreach (ClientElements element in clients)
                    {
                        if (element.clientSubjects.Contains(subj))
                        {

                            List<string> ipandPort = element.ipAddress.Split(":").ToList();

                            IPEndPoint clientIP = new IPEndPoint(IPAddress.Parse(ipandPort[0]),
                                Int32.Parse(ipandPort[1]));
                            byte[] userFeed = Encoding.ASCII.GetBytes(response.message);
                            socket.SendTo(userFeed, 0, userFeed.Length,SocketFlags.None, clientIP);
                            subjectInterest = true;
                        }
                    }

                    if (!subjectInterest) 
                    {
                        response.message = String.Format("PUBLISH-DENIED,{0},{1}, Error, no clients contain such a subject", Name, subj);
                        return response;
                    }
                    response.message = "";
                    return response;

                case "SUBJECTS":
                    RQ = array[1];
                    Name = array[2];
                    List<string> newSubs = array[3].Split("@").ToList();
                    if (clients.Any(i => i.clientName.Equals(Name)))
                    {
                        var element = clients.Find(obj => obj.clientName.Equals(Name));
                        element.clientSubjects = new List<string>();
                        
                        element.clientSubjects = newSubs;
                        response.message = String.Format("SUBJECTS-UPDATED,{0},{1},{2}", RQ, Name, array[3]);
                        response.valid = true;
                    }
                    else
                    {
                        response.message = String.Format("SUBJECTS-REJECTED,{0},{1},{2}", RQ, Name, array[3]);
                    }                        
                    return response;
                case "WAKE-UP":
                    string thisServerIP = array[2];
                    foreach (ClientElements element in clients)
                    {
                        List<string> ipandPort = element.ipAddress.Split(":").ToList();
                        IPEndPoint clientIP = new IPEndPoint(IPAddress.Parse(ipandPort[0]),
                            Int32.Parse(ipandPort[1]));

                        byte[] userFeed = Encoding.ASCII.GetBytes("CHANGE-SERVER,"+thisServerIP);
                        socket.SendTo(userFeed, 0, userFeed.Length, SocketFlags.None, clientIP);
                    }
                    response.message = "Told Clients come to my server";
                    return response;

                default:
                    return response;
            }
        }
    }
}
