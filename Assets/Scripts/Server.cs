using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class Server 
{
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }
    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public delegate void PacketHandler(int _clientId, Packet _packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    private static TcpListener tcpListener;
    private static UdpClient udpListener;

    /*
     * Method handling starting the server 
     */
    public static void Start(int _maxPlayers, int _port)
    {
        // Set the maximum players and port number and call method to initialise server data 
        MaxPlayers = _maxPlayers;
        Port = _port;
        Debug.Log($"Starting server...");
        InitServerData();

        // Create listener for connections from TCP network clients 
        tcpListener = new TcpListener(IPAddress.Any, Port);
       // Start listening for incoming connections 
        tcpListener.Start();
        // Begin asynchronous acceptance of connections 
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

        // Create UDP client and bind to the port 
        udpListener = new UdpClient(Port);
        // Begin receiving data asynchronously 
        udpListener.BeginReceive(UDPRcvCallback, null);

        Debug.Log($"Server started on {Port}.");
    }

    private static void TCPConnectCallback(IAsyncResult _result)
    {
        // Asynchronously accepts incoming connection attempt and creates new socket to handle communication 
        TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
        // Starts listening for new clietns again 
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
        Debug.Log($"Incoming connection from {_client.Client.RemoteEndPoint}...");

        // Connects clients if server is not yet full
        for (int i = 1; i <= MaxPlayers; i++)
        {
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(_client);
                return;
            }
        }

        Debug.Log($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
    }

    private static void UDPRcvCallback(IAsyncResult _result)
    {
        try
        {
            // creates a new IP end point at any IP
            IPEndPoint _clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
            // Ends current receive
            byte[] _data = udpListener.EndReceive(_result, ref _clientEndpoint);
            // Opens to receiving data again
            udpListener.BeginReceive(UDPRcvCallback, null);

            // Incomplete byte 
            if (_data.Length < 4)
            {
                return;
            }

            // Using packet disposes of packet automatically when finished 
            using (Packet _packet = new Packet(_data))
            {
                int _clientId = _packet.ReadInt();

                // Should never be 0 - would cause problems if it is
                if (_clientId == 0)
                {
                    return;
                }

                if (clients[_clientId].udp.endpoint == null)
                {
                    clients[_clientId].udp.Connect(_clientEndpoint);
                    return;
                }

                // Include this check to ensure hackers cant impersonate other clients 
                if (clients[_clientId].udp.endpoint.ToString() == _clientEndpoint.ToString())
                {
                    clients[_clientId].udp.HandleData(_packet);
                }
            }
        }
        catch (Exception _e)
        {
            Debug.Log($"Error receiving UDP data: {_e}");
        }
    }

    public static void SendUDPData(IPEndPoint _clientEndpoint, Packet _packet)
    {
        try
        {
            // Send packet through if the client end point is not null 
            if (_clientEndpoint != null)
            {
                udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndpoint, null, null);
            }
        }
        catch (Exception _e)
        {
            Console.Write($"Error sending data to {_clientEndpoint} via UDP: {_e}");
        }
    }

    private static void InitServerData()
    {
        // Add clients for up to maximum number of players 
        for (int i = 1; i <= MaxPlayers; i++)
        {
            clients.Add(i, new Client(i));
        }
        
        // Create new dictionary of packet handlers 
        packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ClientPackets.welcomeReceived, ServerHandle.WelcomeRcvd},
                {(int)ClientPackets.plrMovement, ServerHandle.PlayerMovement}
            };
        Debug.Log("Initialised packets.");
    }

    public static void Stop()
    {
        // Stop each listener upon server stop 
        tcpListener.Stop();
        udpListener.Close();
    }
}
