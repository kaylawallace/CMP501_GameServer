using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class Client
{
    public int id;
    public Player player;
    public TCP tcp;
    public UDP udp;
    public static int dataBufferSize = 4096;

    public Client(int _clientId)
    {
        // Initialise client ID and create TCP and UDP references s
        id = _clientId;
        tcp = new TCP(id);
        udp = new UDP(id);
    }

    public class TCP
    {
        public TcpClient socket;

        private readonly int id;
        private NetworkStream stream;
        private byte[] rcvBuffer;
        private Packet rcvdData;

        // Intialise ID 
        public TCP(int _id)
        {
            id = _id;
        }

        public void Connect(TcpClient _socket)
        {
            // Initialise socket info 
            socket = _socket;
            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;

            // Returns network stream to send and receive data 
            stream = socket.GetStream();
            
            // Create new packet
            rcvdData = new Packet();

            // Create new buffer 
            rcvBuffer = new byte[dataBufferSize];

            // Begin asynchronous read from the network stream 
            stream.BeginRead(rcvBuffer, 0, dataBufferSize, RcvCallback, null);

            // Send welcome packet 
            ServerSend.Welcome(id, "welcome to the server!");
        }

        public void SendData(Packet _packet)
        {
            try
            {
                // Send packet if socket is not null
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }

            }
            catch (Exception _e)
            {
                Debug.Log($"Error sending data to player: {id} via TCP: {_e}");
            }
        }

        private void RcvCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLen = stream.EndRead(_result);
                // Disconnect client if strange _byteLen 
                if (_byteLen <= 0)
                {
                    Server.clients[id].Disconnect();
                    return;
                }

                // Create a byte array of the length returned from .EndRead() above
                byte[] _data = new byte[_byteLen];
                // Copy contents of receive buffer into byte array 
                Array.Copy(rcvBuffer, _data, _byteLen);

                // Reset data received 
                rcvdData.Reset(HandleData(_data));

                // Open to read again 
                stream.BeginRead(rcvBuffer, 0, dataBufferSize, RcvCallback, null);
            }
            catch (Exception _e)
            {
                Debug.Log($"Error recieving TCP data: {_e}");
                Server.clients[id].Disconnect();
            }
        }

        private bool HandleData(byte[] _data)
        {
            int _packetLen = 0;

            rcvdData.SetBytes(_data);

            // Still a byte left if greater than 4 (typically integer) 
            if (rcvdData.UnreadLength() >= 4)
            {
                _packetLen = rcvdData.ReadInt();
                if (_packetLen <= 0)
                {
                    return true;
                }
            }

            // While there is still data to read 
            while (_packetLen > 0 && _packetLen <= rcvdData.UnreadLength())
            {
                // Read bytes from the received data packet into new byte array 
                byte[] _packetBytes = rcvdData.ReadBytes(_packetLen);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    // Add the packet to the packet handlers dictionary 
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        Server.packetHandlers[_packetId](id, _packet);
                    }
                });

                _packetLen = 0;
                if (rcvdData.UnreadLength() >= 4)
                {
                    _packetLen = rcvdData.ReadInt();
                    if (_packetLen <= 0)
                    {
                        return true;
                    }
                }
            }
            
            // Strange length, return out 
            if (_packetLen <= 1)
            {
                return true;
            }

            return false;
        }

        public void Disconnect()
        {
            // Close socket and reset vars on disconnect
            socket.Close();
            stream = null;
            rcvdData = null;
            rcvBuffer = null;
            socket = null;
        }
    }

    public class UDP
    {
        public IPEndPoint endpoint;
        private int id;

        public UDP(int _id)
        {
            // Initialise ID 
            id = _id;
        }

        public void Connect(IPEndPoint _endpoint)
        {
            // Set endpoint on connect 
            endpoint = _endpoint;
        }

        public void SendData(Packet _packet)
        {
            Server.SendUDPData(endpoint, _packet);
        }

        public void HandleData(Packet _packetData)
        {
            int _packetLen = _packetData.ReadInt();
            byte[] _packetBytes = _packetData.ReadBytes(_packetLen);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                // Add packet to packet handlers dictionary 
                using (Packet _packet = new Packet(_packetBytes))
                {
                    int _packetId = _packet.ReadInt();
                    Server.packetHandlers[_packetId](id, _packet);
                }
            });
        }

        public void Disconnect()
        {
            // Reset endpoint on disconnect 
            endpoint = null;
        }
    }

    /*
     * Handles sending a player into the game 
     */
    public void SendIntoGame(string _plrName)
    {
        // Intantiate the player and initialise their info
        player = NetworkManager.instance.InstantiatePlayer();
        player.Initialize(id, _plrName);

        // Iterate through all clients 
        foreach (Client _client in Server.clients.Values)
        {
            // Client has a player component 
            if (_client.player != null)
            {
                // Client ID doesn't match ID set for intialize 
                if (_client.id != id)
                {
                    // Spawn with new ID 
                    ServerSend.SpawnPlayer(id, _client.player);
                }
            }
        }

        // Iterate through all clients 
        foreach (Client _client in Server.clients.Values)
        {
            // Client has a player component 
            if (_client.player != null)
            {
                // Spawn into game with client id and player component 
                ServerSend.SpawnPlayer(_client.id, player);
            }
        }

        // Iterate through all key spawners 
        foreach (KeySpawner _keySpawner in KeySpawner.keySpawners.Values)
        {
            // Send create key spawner packet for each one 
            ServerSend.CreateKeySpawner(id, _keySpawner.spawnerId, _keySpawner.transform.position, _keySpawner.hasKey);
        }
    }

    private void Disconnect()
    {
        Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");

        ThreadManager.ExecuteOnMainThread(() =>
        {
            // Destroy player object upon disconnect 
            UnityEngine.Object.Destroy(player.gameObject);
            player = null;
        });

        tcp.Disconnect();
        udp.Disconnect();

        // Send player disconnect packet 
        ServerSend.PlayerDisconnect(id);
    }
}
