using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    /*
     * Method to handle WelcomeRcvd packet from the client 
     * Params: _clientId - clients ID, _packet - contains client ID and username 
     */
    public static void WelcomeRcvd(int _clientId, Packet _packet)
    {
        // Read out the client ID and username 
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();

        Debug.Log($"{Server.clients[_clientId].tcp.socket.Client.RemoteEndPoint} connected successfully & is now player {_clientId}.");

        // If the client ID in the packet doesn't match the client ID param input to this method then client has assumed wrong ID 
        if (_clientId != _clientIdCheck)
        {
            Debug.Log($"Player \"{_username}\" (ID: {_clientId} has assumed the wrong client ID ({_clientIdCheck})");
        }

        // Send this client into the game by calling the SendIntoGame() method with the username input to this method 
        Server.clients[_clientId].SendIntoGame(_username);
    }

    /*
     * Method to handle PlayerMovement packet from the client 
     * Params: _clientID - clients ID, _packet - contains length of the boolean input array, the array and player rotation 
     */
    public static void PlayerMovement(int _clientId, Packet _packet)
    {
        // Read out the boolean inputs in the array 
        bool[] _inputs = new bool[_packet.ReadInt()];
        for (int i = 0; i < _inputs.Length; i++)
        {
            _inputs[i] = _packet.ReadBool();
        }
        // Read out the player rotation
        Quaternion _rot = _packet.ReadQuaternion();

        // Call the players SetInput() method with the input values and rotation read out of the packet 
        Server.clients[_clientId].player.SetInput(_inputs, _rot);
    }
}
