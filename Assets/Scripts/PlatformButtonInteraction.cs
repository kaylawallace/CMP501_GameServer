using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Class to handle the button interaction at the moving platform section of the level 
 */
public class PlatformButtonInteraction : MonoBehaviour
{
    public GameObject platform;
    public Vector3 targetPos;
    public Vector3 initPos;
    public string platformTag;

    
    void Start()
    {
        // Initialise the starting position of the platform, the target position and the platform tag (indicating the colour) 
        initPos = platform.transform.position;
        targetPos = new Vector3(platform.transform.position.x, platform.transform.position.y + 3f, platform.transform.position.z);
        platformTag = platform.tag;
    }

    /*
     * Method runs when something enters the button collider 
     */
    private void OnTriggerEnter(Collider other)
    {
        // Determines if it is a player that entered 
        if (other.CompareTag("Player"))
        {
            // Snaps the platform to the target position 
            platform.transform.position = new Vector3(targetPos.x, targetPos.y, targetPos.z);
            // Sends the platform colour (tag) and new position to the client
            ServerSend.PlatformPosition(platformTag, targetPos);
        }
    }

   /*
    * Method runs when something exits the button collider 
    */
    private void OnTriggerExit(Collider other)
    {
        // Determines if it is a player that exited 
        if (other.CompareTag("Player"))
        {
            // Moves the platform back to its initial position 
            platform.transform.position = initPos;
            // Sends the platform colour (tag) and new position to the client 
            ServerSend.PlatformPosition(platformTag, initPos);
        }
    }
}
