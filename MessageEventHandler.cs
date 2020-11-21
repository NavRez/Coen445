using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDPSocketProject
{
    public class Response
    {
        public string message;
        public bool serverValid;
        public bool clientValid;
        public Response()
        {
            message = "";
            serverValid = false;
            clientValid = false;
        }
        public Response(string _message, bool _valid, bool _clientValid)
        {
            message = _message;
            serverValid = _valid;
            clientValid = _clientValid;
        }        
    }

    public class ClientElements
    {

        public string clientName;
        public string ipAddress;
        public int clientPort;
        public List<string> clientSubjects = new List<string>();

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

        public string printSubjects()
        {
            string allSubjects = "";
            foreach(string subject in clientSubjects)
            {
                allSubjects += "@" + subject;
            }


            return allSubjects;
        }

    }

    public class MessageEventHandler
    {

        public List<ClientElements> clients = new List<ClientElements>();
        public string filePathA = Path.Combine("../../../ServerA.txt");
        public string filePathB = Path.Combine("../../../ServerB.txt");
        public string currentFile = Path.Combine("../../../ServerA.txt");
        public string otherFile = Path.Combine("../../../ServerB.txt");
        public MessageEventHandler() 
        {
            if (!File.Exists(filePathA))
            {
                File.Create(filePathA);
            }
            if (!File.Exists(filePathB))
            {
                File.Create(filePathB);
            }

            if (Program.currentServer.Equals("A"))
            {
                currentFile = filePathA;
                otherFile = filePathB;
            }
            else
            {
                currentFile = filePathB;
                otherFile = filePathA;
            }
            //CheckEqualFiles();

            
        }

        public string SendWholeFile()
        {
            string wholeFile = "";

            if (UdpServer.twoServerComm)
            {
                
                string[] currentLines = File.ReadAllLines(currentFile);

                for (int i = 0; i < currentLines.Length; i++)
                {
                    wholeFile += currentLines[i] + "\n";
                }                               
            }
            return wholeFile;
        }

        public void CheckEqualFiles()
        {
            
            if (UdpServer.twoServerComm)
            {
                string[] currentLines = File.ReadAllLines(currentFile);
                string[] otherLines = File.ReadAllLines(otherFile);

                if (currentLines.Length != otherLines.Length)
                {
                    File.WriteAllLines(currentFile, otherLines);
                }
                else
                {
                    if (!currentLines.SequenceEqual(otherLines))
                    {
                        File.WriteAllLines(currentFile, otherLines);
                    }
                }
            }
        }

        public void ReadFromFile()
        {       
            if (File.Exists(currentFile))
            {
                clients.Clear();
                string[] lines = File.ReadAllLines(currentFile);
                foreach (var line in lines)
                {
                    if(line.Length > 0)
                    {
                        List<string> clientElements = line.Split(",").ToList();
                        List<string> clientSubjects = clientElements[2].Split("@").ToList();
                        ClientElements client = new ClientElements(clientElements[0], clientElements[1], clientSubjects);
                        clients.Add(client);

                        Console.WriteLine("Added client: " + clientElements[0] + " " + clientElements[1] + " " + client.printSubjects());
                    }
                    
                }
            }
        }

        private void UpdateSubjectsInFile(string clientName,string newSubs)
        {
            if (File.Exists(currentFile))
            {
                string[] lines = File.ReadAllLines(currentFile);
                for (int i = 0; i < lines.Length; i++)
                {
                    List<string> clientElements = lines[i].Split(",").ToList();
                    if (clientElements[0].Equals(clientName))
                    {
                        lines[i] = clientElements[0] + "," + clientElements[1] + "," + newSubs;
                    }
                }
                File.WriteAllLines(currentFile, lines);
            }
        }

        private void RemoveClientFromFile (string clientName)
        {
            if (File.Exists(currentFile))
            {
                string[] lines = File.ReadAllLines(currentFile);
                for (int i = 0; i < lines.Length; i++)
                {
                    List<string> clientElements = lines[i].Split(",").ToList();
                    if (clientElements[0].Equals(clientName))
                    {
                        lines[i] = "";
                    }
                }
                File.WriteAllLines(currentFile, lines);
            }
        }

        private void UpdateIPInFile(string clientName, string newIP)
        {
            if (File.Exists(currentFile))
            {
                string[] lines = File.ReadAllLines(currentFile);
                for (int i = 0; i < lines.Length; i++)
                {
                    List<string> clientElements = lines[i].Split(",").ToList();
                    if (clientElements[0].Equals(clientName))
                    {
                        lines[i] = clientElements[0] + "," + newIP + "," + clientElements[2];
                    }
                }
                File.WriteAllLines(currentFile, lines);
            }
        }

        private void WriteToFile(List<ClientElements> clientsInfo)
        {           
            if (!File.Exists(currentFile))
            {
                using StreamWriter sw = File.CreateText(currentFile);
                foreach (ClientElements client in clients)
                {
                    sw.WriteLine(client.clientName + "," + client.ipAddress + "," + client.printSubjects());
                }
                        
            }
            else
            {
                using StreamWriter sw = File.CreateText(currentFile);
                foreach (ClientElements client in clients)
                {
                    sw.WriteLine(client.clientName + "," + client.ipAddress + "," + client.printSubjects());
                }
                        
            }
        }       

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
                    response.clientValid = true;
                
                    if (clients.Any(i => i.clientName.Equals(Name)))
                    {
                        //Register-Denied
                        response.message = "REGISTER-DENIED,";
                        response.message += RQ + ",Name is already in use";
                        response.serverValid = false;

                    }
                    else
                    {
                        clients.Add(User1);
                        response.message = "REGISTERED,";
                        response.message += RQ + "," + Name + "," + ipAddress;
                        response.serverValid = true;
                        WriteToFile(clients);

                    }
                    return response;
                case "DE-REGISTER":
                    response.clientValid = true;
                    RQ = array[1];
                    Name = array[2];
                    if (clients.Any(i => i.clientName.Equals(Name)))
                    {
                        //de-register
                        clients.RemoveAll(n => n.clientName.Equals(Name));
                        response.message = "DE-REGISTERED,";
                        response.message += Name;
                        response.serverValid = true;
                        RemoveClientFromFile(array[2]);
                    } 
                    else
                    {
                        response.serverValid = false;
                        response.message = "User not registered";
                    }
                    return response;
                case "UPDATE":
                    RQ = array[1];
                    Name = array[2];
                    ipAddress = array[3];
                    response.clientValid = true;

                    if (clients.Any(i=>i.clientName.Equals(Name)))
                    {
                        var element = clients.Find(obj => obj.clientName.Equals(Name));
                        element.changeIP(ipAddress);
                        clients[clients.FindIndex(obj=>obj.clientName.Equals(Name))] = element;
                        response.message = "UPDATE-CONFIRMED,";
                        response.message += RQ + "," + Name + "," + ipAddress;
                        UpdateIPInFile(element.clientName, array[3]);
                        response.serverValid = true;
                    } 
                    else
                    {
                        response.message = "UPDATE-DENIED,";
                        response.message += RQ + "," + Name + " does not exist";
                        response.serverValid = false;
                    }
                    return response;
                case "PUBLISH":
                    RQ = array[1];
                    Name = array[2];
                    string subj = array[3];
                    string userMessage = array[4];
                    ipAddress = array[5];
                    bool subjectInterest = false;
                    response.clientValid = true;
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
                    response.serverValid = false;
                    return response;

                case "SUBJECTS":
                    RQ = array[1];
                    Name = array[2];
                    response.clientValid = true;
                    List<string> newSubs = array[3].Split("@").ToList();
                    if (clients.Any(i => i.clientName.Equals(Name)))
                    {
                        var element = clients.Find(obj => obj.clientName.Equals(Name));
                        element.clientSubjects = new List<string>();
                        
                        element.clientSubjects = newSubs;
                        response.message = String.Format("SUBJECTS-UPDATED,{0},{1},{2}", RQ, Name, array[3]);
                        UpdateSubjectsInFile(element.clientName, array[3]);
                        response.serverValid = true;
                    }
                    else
                    {
                        response.message = String.Format("SUBJECTS-REJECTED,{0},{1},{2}", RQ, Name, array[3]);
                        response.serverValid = false;
                    }                        
                    return response;
                case "WAKE-UP":
                    if (UdpServer.sleeping)
                    {
                        string thisServerIP = array[2];
                        UdpServer.sleeping = false;
                        foreach (ClientElements element in clients)
                        {
                            List<string> ipandPort = element.ipAddress.Split(":").ToList();
                            IPEndPoint clientIP = new IPEndPoint(IPAddress.Parse(ipandPort[0]),
                                Int32.Parse(ipandPort[1]));

                            byte[] userFeed = Encoding.ASCII.GetBytes("CHANGE-SERVER," + thisServerIP);
                            socket.SendTo(userFeed, 0, userFeed.Length, SocketFlags.None, clientIP);
                        }
                        Console.WriteLine("\nI'M AWAKE\nTelling other server to sleep.");

                        response.serverValid = true;
                        response.message = "GO-SLEEP";
                    }                   
                    return response;
                case "GO-SLEEP":
                    Console.WriteLine("\nI'M ASLEEP\nI was told to go sleep, And now I sleep...zzz");
                    UdpServer.sleeping = true;
                    response.serverValid = false;
                    return response;
                case "UPDATE-SERVER":
                    string otherServerIP = array[1];
                    foreach (ClientElements element in clients)
                    {
                        List<string> ipandPort = element.ipAddress.Split(":").ToList();
                        IPEndPoint clientIP = new IPEndPoint(IPAddress.Parse(ipandPort[0]),
                            Int32.Parse(ipandPort[1]));

                        byte[] userFeed = Encoding.ASCII.GetBytes("UPDATE-OTHER-SERVER," + otherServerIP);
                        socket.SendTo(userFeed, 0, userFeed.Length, SocketFlags.None, clientIP);
                    }
                    //response.serverValid = true;
                    //response.message = "SENT-FILE," + SendWholeFile();
                    return response;
                case "SENT-FILE":
                    var a = incomingInfo.Split(Environment.NewLine).ToArray();
                    List<string> fullFile = new List<string>();
                    Console.WriteLine("*INCOMING INFO**");
                    Console.WriteLine(incomingInfo);
                    List<string> newClientFile = incomingInfo.Split("\n").ToList();
                    newClientFile[0] = newClientFile[0].Replace("SENT-FILE,", "");
                    newClientFile.RemoveAt(newClientFile.Count - 1);

                    File.WriteAllLines(currentFile, newClientFile);
                    ReadFromFile();

                    return response;

                default:
                    return response;
            }
        }
    }
}
