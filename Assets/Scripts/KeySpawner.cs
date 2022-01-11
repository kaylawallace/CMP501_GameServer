using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeySpawner : MonoBehaviour
{
    public KeySpawner instance;
    public static Dictionary<int, KeySpawner> keySpawners = new Dictionary<int, KeySpawner>();
    private static int nextSpawnerId = 1; //in dictionary 
    public int spawnerId;
    public bool hasKey = false;
    public int keysCollected = 0;

    void Start()
    {
        // Intitialise key spawner variables
        hasKey = false;
        spawnerId = nextSpawnerId;
        nextSpawnerId++;
        // Add the key spawner to the dictionary
        keySpawners.Add(spawnerId, this);
        // Start coroutine to handle key spawning
        StartCoroutine(SpawnKey());
    }

   /*
    * Runs when something enters a key spawner collider 
    */
    private void OnTriggerEnter(Collider other)
    {
        // Determines whether it was a player that entered 
        if (other.CompareTag("Player"))
        {
            Player _plr = other.GetComponent<Player>();
            // If the player successfully picks up the key, increment their number of keys & run the KeyPickedUp() method 
            if (_plr.KeyPickup())
            {
                keysCollected++;
                KeyPickedUp(_plr.id, keysCollected);
            }
        }
    }

    /*
     * Coroutine to handle key spawning 
     */
    private IEnumerator SpawnKey()
    {
        yield return new WaitForSeconds(1f);
        hasKey = true;
        // Send KeySpawned packet to the client 
        ServerSend.KeySpawned(spawnerId);
    }

    /*
     * Method to handle when a key has been picked up 
     * Param: _plr - player picking up the item,_keyCollected - number of keys collected 
     */
    private void KeyPickedUp(int _plr, int _keysCollected)
    {
        hasKey = false;
        ServerSend.KeyPickedUp(spawnerId, _plr, _keysCollected);
    }
}
