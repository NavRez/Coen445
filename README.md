# Coen445
This was a project for a Network Protocols class where students were asked to create a specific UDP communication system where two servers would be active (either on the same or different devices) and multiple clients located on various machines attempt to communicate with said servers. This branch was used to define the Server and ensure that no more two of them are running at all times.
UdpServer/client project

# How to use
Run the executable file, the server will begin by asking the user if it is server A or B, it should be able to fetch the ip Address of the system and request for a port number. It will then request the user to enter the port number of its sibling server. Once the info for both servers have been given, they will go on cycles of wither being active orsleeping while awaiting for users to initiate any form of communcation with them. 
