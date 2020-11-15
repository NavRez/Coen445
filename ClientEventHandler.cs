using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDPSocketProject
{
    public class ClientEventHandler
    {

        public ClientEventHandler() 
        {
            ; 
        }
        public class ClientElements
        {
            public ClientElements(string name, string host, int port)
            {
                clientName = name;
                clientHost = host;
                clientPort = port;
                ipAdress = clientHost + "." + clientHost.ToString();
            }

            public ClientElements(string name, string host, int port, List<string> subs)
            {
                clientName = name;
                clientHost = host;
                clientPort = port;
                clientSubjects = subs;
                ipAdress = clientHost + "." + clientHost.ToString();
            }

            public void resetSubjects(List<string> newSubs)
            {
                clientSubjects = newSubs;
            }

            public void changeIP(string newHost, int newPort)
            {
                clientHost = newHost;
                clientPort = newPort;
                ipAdress = newHost + "." + newPort.ToString();
            }

            public string clientName;
            public string clientHost;
            public string ipAdress;
            public int clientPort;
            public List<string> clientSubjects = new List<string>();

        }

        public List<ClientElements> clients = new List<ClientElements>();

        public string SwitchCase(string incomingInfo,UdpClient socket)
        {
            string[] array = incomingInfo.Split(",");

            string val = array[0];
            string RQ = array[1];
            string Name = array[2];
            string message;

            switch (val)
            {
                case "REGISTER":
                    string IPaddress1 = array[3];
                    int Socket1 = Int32.Parse(array[4]);
                    var User1 = new ClientElements(Name, IPaddress1, Socket1);
                
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
                        message += RQ + "," + Name + "," + IPaddress1 + "," + Socket1;    
                    }
                    return message;
                    break;
                case "DE-REGISTER":
                    
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
                    break;
                case "UPDATE":
                    string IPaddress3 = array[3];
                    int Socket3 = Int32.Parse(array[4]);
                    
                    if (clients.Any(i=>i.clientName.Equals(Name)))
                    {
                        var element = clients.Find(obj => obj.clientName.Equals(Name));
                        element.changeIP(IPaddress3, Socket3);
                        clients[clients.FindIndex(obj=>obj.clientName.Equals(Name))] = element;
                         message = "UPDATE-CONFIRMED,";
                        message += RQ + "," + Name + "," + IPaddress3 + "," + Socket3;
                    } 
                    else
                    {
                        message = "UPDATE-DENIED,";
                        message += RQ + "," + Name + "does not exist";
                    }
                    return message;
                    break;
                case "PUBLISH":
                    string subj = array[3];
                    string userMessage = array[4];
                    int counter = 0;
                    message = String.Format("MESSAGE,{0},{1},{2}", Name, subj, userMessage);
                    foreach (ClientElements element in clients)
                    {
                        if (element.clientSubjects.Contains(subj))
                        {
                            IPEndPoint clientIP = new IPEndPoint(IPAddress.Parse(element.clientHost), element.clientPort);
                            byte[] userFeed = Encoding.ASCII.GetBytes(message);
                            socket.Send(userFeed, userFeed.Length, clientIP);
                            counter++;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (counter == 0) {

                        message = String.Format("PUBLISH-DENIED,{0},{1}, Error, no clients contain such a subject", Name, subj);
                    }

                    return message;
                    break;
                case "SUBJECTS":
                    if (clients.Any(i => i.clientName.Equals(Name)))
                    {
                        var element = clients.Find(obj => obj.clientName.Equals(Name));
                        element.clientSubjects = new List<string>();
                        List<string> newSubs = array[3].Split("@").ToList();
                        element.clientSubjects = newSubs;
                        message = String.Format("SUBJECTS-UPDATED,{0},{1},{2}", RQ, Name, array[3]);
                    }
                    else
                    {
                        message = String.Format("SUBJECTS-REJECTED,{0},{1},{2}", RQ, Name, array[3]);
                    }
                        
                    return message;
                    break;
                default:
                    return null;
                    break;
            }

        }

        public void DromantServerReceive(string incoming)
        {
            string[] array = incoming.Split(",");

            string val = array[0];
            string RQ = array[1];
            string Name = array[2];
            string message;

            switch (val)
            {
                case "REGISTERED":
                    string IPaddress1 = array[3];
                    int Socket1 = Int32.Parse(array[4]);
                    var User1 = new ClientElements(Name, IPaddress1, Socket1);

                    clients.Add(User1);
                    break;
                case "DE-REGISTERED":

                    //de-register
                    clients.RemoveAll(n => n.clientName.Equals(Name));
                    message = "DE-REGISTERED,";
                    message += Name;
                    break;
                case "UPDATE-CONFIRMED":
                    string IPaddress3 = array[3];
                    int Socket3 = Int32.Parse(array[4]);

                    var element = clients.Find(obj => obj.clientName.Equals(Name));
                    element.changeIP(IPaddress3, Socket3);
                    clients[clients.FindIndex(obj => obj.clientName.Equals(Name))] = element;
                    message = "UPDATE-CONFIRMED,";
                    message += RQ + "," + Name + "," + IPaddress3 + "," + Socket3;
                    
                    break;
                case "SUBJECTS-UPDATED":
                    var element1 = clients.Find(obj => obj.clientName.Equals(Name));
                    element1.clientSubjects = new List<string>();
                    List<string> newSubs = array[3].Split("@").ToList();
                    element1.clientSubjects = newSubs;
                    message = String.Format("SUBJECTS-UPDATED,{0},{1},{2}", RQ, Name, array[3]);
                    break;
                default:
                    break;
            }
        }

        public void ChangeServer(UdpClient socket, string changeHost, int changePort)
        {
            string message = String.Format("CHANGE-SERVER,{0},{1}", changeHost, changePort);
            foreach (ClientElements element in clients)
            {
                IPEndPoint clientIP = new IPEndPoint(IPAddress.Parse(element.clientHost), element.clientPort);
                byte[] userFeed = Encoding.ASCII.GetBytes(message);
                try
                {
                    socket.Send(userFeed, userFeed.Length);
                }
                catch(InvalidOperationException inv)
                {
                    socket.Send(userFeed, userFeed.Length, clientIP);

                }
            }
        }
    }
}
