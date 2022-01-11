using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Class to handle the end of the level 
 */
public class EndOfLevel : MonoBehaviour
{

    int playersAtEOL = 0;

    /*
     * Runs when something enters the collider of the door at the end of the level (enlarged to allow an end of level (EOL) 'zone')
     */
    private void OnTriggerEnter(Collider other)
    {
        // Determines whether it was a player that entered the EOL zone 
        if (other.CompareTag("Player"))
        {
            // Increments the number of players in the zone 
            playersAtEOL++;

            // Triggers the end of level if there are 2 or more players in the one and each of them have a key 
            if (playersAtEOL >= 2)
            {
                if (other.gameObject.GetComponent<Player>().keys >= 1)
                {
                    // Send the 'true' in the end of level packet to the client to trigger the end of level events 
                    ServerSend.EndOfLevel(true);
                    // Disable the players controller 
                    other.gameObject.GetComponent<CharacterController>().enabled = false;
                }
            }
        }
    }

    /*
     * Runs when something exits the collider of the door at the end of the level 
     */
    private void OnTriggerExit(Collider other)
    {
        // Decrements the number of players in the end of level zone when a player leaves the zone 
        if (other.CompareTag("Player"))
        {
            playersAtEOL--;
        }
    }
}
