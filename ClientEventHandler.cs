using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

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
            public List<string> clientSubjects;

        }

        public List<ClientElements> clients = new List<ClientElements>();

        public string SwitchCase(string incomingInfo)
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
                default:
                    return null;
                    break;
            }

        }
    }
}
