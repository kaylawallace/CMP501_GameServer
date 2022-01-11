using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    #region TCP
    /*
     * Method to send data to a client through TCP
     * Params: _toClient - client to send packet to, _packet - data packet 
     */
    private static void SendTCPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].tcp.SendData(_packet);
    }

    /*
     * Method to send data to all clients through TCP 
     * Params: _packet - data packet
     */
    private static void SendTCPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i < Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(_packet);
        }
    }

    /*
     * Overload method for the previous method - excluding a client 
     * Params: _exceptClient - client not to send the data to, _packet - data packet
     */
    private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i < Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
    }
    #endregion

    #region UDP
    /*
    * Method to send data to a client through UDP
    * Params: _toClient - client to send packet to, _packet - data packet 
    */
    private static void SendUDPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].udp.SendData(_packet);
    }

    /*
    * Method to send data to all clients through UDP 
    * Params: _packet - data packet
    */
    private static void SendUDPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i < Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(_packet);
        }
    }

    /*
    * Overload method for the previous method - excluding a client 
    * Params: _exceptClient - client not to send the data to, _packet - data packet
    */
    private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i < Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }
    }

    #endregion

    #region Packets
    /*
     * Welcome packet 
     * Sent through TCP
     * Params: _toClient - client to send to, _msg - welcome message 
     */
    public static void Welcome(int _toClient, string _msg)
    {
        // 'Using' disposes of the packet instance automatically
        using (Packet _packet = new Packet((int)ServerPackets.welcome))
        {
            _packet.Write(_msg);
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }
    }

    /*
     * Spawn player packet 
     * Sends the players ID, username, position and rotation 
     * Params: _toClient - client to send to, _plr - player to spawn 
     */
    public static void SpawnPlayer(int _toClient, Player _plr)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnPlr))
        {
            _packet.Write(_plr.id);
            _packet.Write(_plr.uname);
            _packet.Write(_plr.transform.position);
            _packet.Write(_plr.transform.rotation);

            SendTCPData(_toClient, _packet);
        }
    }

    /*
     * Player position packet 
     * Sends the players ID and position to all clients through UDP 
     * Params: _plr - player to send position of 
     */
    public static void PlayerPosition(Player _plr)
    {
        using (Packet _packet = new Packet((int)ServerPackets.plrPos))
        {
            _packet.Write(_plr.id);
            _packet.Write(_plr.transform.position);

            SendUDPDataToAll(_packet);
        }
    }

    /*
     * Player rotation packet 
     * Sends the players ID and rotation to all clients through UDP 
     * Params: _plr - player to send rotation of 
     */
    public static void PlayerRotation(Player _plr)
    {
        using (Packet _packet = new Packet((int)ServerPackets.plrRot))
        {
            _packet.Write(_plr.id);
            _packet.Write(_plr.transform.rotation);

            SendUDPDataToAll(_plr.id, _packet);
        }
    }

    /*
     * Player disconnect packet 
     * Sends the ID of the player that has disconnected through TCP 
     * Params: _plr - ID of disconnected player 
     */
    public static void PlayerDisconnect(int _plrId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.plrDisconnect))
        {
            _packet.Write(_plrId);

            SendTCPDataToAll(_packet);
        }
    }


    /*
     * Create key spawner packet 
     * Sends the spawners info through TCP 
     * Params: _toClient - client ID to send packet to, _spawnerId - ID of spawner, _spawnerPos - spawner position, hasKey - whether it has a key 
     */
    public static void CreateKeySpawner(int _toClient, int _spawnerId, Vector3 _spawnerPos, bool _hasKey)
    {
        using (Packet _packet = new Packet((int)ServerPackets.createKeySpawner))
        {
            _packet.Write(_spawnerId);
            _packet.Write(_spawnerPos);
            _packet.Write(_hasKey);

            SendTCPData(_toClient, _packet);
        }
    }

    /*
     * Key spawned packet 
     * Sends the spawner ID that the key was spawned in through TCP to all clients 
     * Params: _spawnerId - ID of spawner 
     */
    public static void KeySpawned(int _spawnerId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.keySpawned))
        {
            _packet.Write(_spawnerId);

            SendTCPDataToAll(_packet);
        }        
    }

    /*
     * Key picked up packet 
     * Sends the spawner info to all clients when a key has been picked up - sends through TCP
     * Params: _spawnerId - ID of spawner, _plr - player that picked up key, _keys - number of keys 
     */
    public static void KeyPickedUp(int _spawnerId, int _plr, int _keys)
    {
        using (Packet _packet = new Packet((int)ServerPackets.keyPickedUp))
        {
            _packet.Write(_spawnerId);
            _packet.Write(_plr);
            _packet.Write(_keys);

            SendTCPDataToAll(_packet);
        }
    }

    /*
     * Bridge position packet 
     * Sends the bridge position to all clients through TCP 
     * Params: _pos - bridge position 
     */
    public static void BridgePosition(Vector3 _pos)
    {
        using (Packet _packet = new Packet((int)ServerPackets.bridgePos))
        {
            _packet.Write(_pos);

            SendTCPDataToAll(_packet);
        }
    }

    /*
     * Platform position packet 
     * Sends the platform position to all clients through TCP 
     * Params: _tag - platform tag (indicating colour), _pos - bridge position 
     */
    public static void PlatformPosition(string _tag, Vector3 _pos)
    {
        using (Packet _packet = new Packet((int)ServerPackets.platformPos))
        {
            _packet.Write(_tag);
            _packet.Write(_pos);

            SendTCPDataToAll(_packet);
        }
    }

    /*
     * Player health packet 
     * Sends player health to all clients through TCP
     * Params: _plr - player who's health to send 
     */
    public static void PlayerHealth (Player _plr)
    {
        using (Packet _packet = new Packet((int)ServerPackets.plrHealth))
        {
            _packet.Write(_plr.id);
            _packet.Write(_plr.health);

            SendTCPDataToAll(_packet);
        }
    }

    /*
     * Player respawn packet 
     * Sends the id of respawning player to all clients through TCP 
     * Params: _plr - player respawning
     */
    public static void PlayerRespawn(Player _plr)
    {
        using (Packet _packet = new Packet((int)ServerPackets.plrRespawned))
        {
            _packet.Write(_plr.id);

            SendTCPDataToAll(_packet);
        }
    }

    /*
     * End of level packet 
     * Sends the end of level trigger to all clients 
     * Params: _isEOL - boolean stating whether it is the end of level 
     */
    public static void EndOfLevel(bool _isEOL)
    {
        using (Packet _packet = new Packet((int)ServerPackets.endOfLevel))
        {
            _packet.Write(_isEOL);

            SendTCPDataToAll(_packet);
        }
    }
    #endregion
}
