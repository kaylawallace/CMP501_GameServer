using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.NetworkInformation;
using System.Net;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    public GameObject plrPrefab;
    public Vector3 spawnPoint = new Vector3(-12f, 3.5f, 10f);

    private void Awake()
    {
        // Set instance to this if null
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }

        // Gets local IP of the computer the server is running on and prints it to the console 
        var host = Dns.GetHostEntry(Dns.GetHostName());
        string readIn;
        foreach(var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                readIn = ip.ToString();
                print($"Local IP: " + readIn);
            }      
            else
            {
                print("no IP found");
            }
        }
    }

    private void Start()
    {
        // Limited to match ticks per second of game - otherwise runs at very high FPS and uses unnecessary CPU %
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;

        // Start the server 
        Server.Start(3, 5555);
    }

    private void OnApplicationQuit()
    {
        // Stops the server when the application has been exited 
        Server.Stop();
    }

    /*
     * Method to handle instantiating a player 
     */
    public Player InstantiatePlayer()
    {
        Quaternion rot = Quaternion.Euler(new Vector3(0, 0, 0));
        return Instantiate(plrPrefab, spawnPoint, rot).GetComponent<Player>();
    }
}

