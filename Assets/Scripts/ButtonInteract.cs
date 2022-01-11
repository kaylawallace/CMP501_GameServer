using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Class to handle button presses for the bridge 
 */
public class ButtonInteract : MonoBehaviour
{
    public Transform bridge;
    public Vector3 targetPos;
    public Vector3 initPos;

    
    void Start()
    {
        // Initialise the starting position and target position of the bridge 
        initPos = bridge.position;
        targetPos = new Vector3(bridge.position.x, bridge.position.y + 2.15f, bridge.position.z)
;    }


    /*
     * Runs when something enters the collider of a button 
     */
    private void OnTriggerEnter(Collider other)
    {
        // Determines if it was a player that triggered the enter 
        if (other.CompareTag("Player"))
        {
            // Button has been pressed - Snap bridge to target position 
            bridge.position = new Vector3(targetPos.x, targetPos.y, targetPos.z);
            // Send the new bridge position to the client  
            ServerSend.BridgePosition(targetPos);
        }
    }

    /*
     * Runs when something exits the collider of a button 
     */
    private void OnTriggerExit(Collider other)
    {
        // Determines if it was a player that trigger the exit 
        if (other.CompareTag("Player"))
        {
            // Snap the bridge position back to its initial position 
            bridge.position = initPos;
            // Send the new bridge position to the client 
            ServerSend.BridgePosition(initPos);
        }
    }
}
