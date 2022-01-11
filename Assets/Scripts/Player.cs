using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string uname;
    public Vector3 pos;
    public Quaternion rot;
    public CharacterController controller;
    public float gravity = -5f;
    public float speed = 5f;
    public float jumpForce = 7f;
    public int keys = 0;
    int maxKeys = 1;
    public int health;
    public int maxHealth;

    private bool[] inputs;
    private float yVel = 0;

    public void Start()
    {
        // Initialise gravity, speed, and jump force variables 
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        speed *= Time.fixedDeltaTime;
        jumpForce *= Time.fixedDeltaTime;
    }

    /*
     * Method to initialise a players information (including their health) and the inputs array 
     * Params: _id - players ID, _uname - players username
     */
    public void Initialize(int _id, string _uname)
    {
        id = _id;
        uname = _uname;
        health = maxHealth;

        inputs = new bool[5];
    }

    public void FixedUpdate()
    {
        // Return out if player is dead 
        if (health <= 0)
        {
            return;
        }

        // Update the input direction based on the input 
        Vector2 _inputDir = Vector2.zero;
        if (inputs[0])
        {
            _inputDir.y += 1;
        }
        if (inputs[1])
        {
            _inputDir.x -= 1;
        }
        if (inputs[2])
        {
            _inputDir.y -= 1;
        }
        if (inputs[3])
        {
            _inputDir.x += 1;
        }

        // Handle player movement 
        Move(_inputDir);
    }

   /*
    * Method to handle player movement 
    * Params: _inputDir - input direction as calculated above 
    */
    private void Move(Vector2 _inputDir)
    {
        // Calculate the move direction of the player and multiply this with the speed value 
        Vector3 _moveDir = (transform.right * _inputDir.x + transform.forward * _inputDir.y).normalized;
        _moveDir *= speed;

        // Allow player jumping if they are currently grounded 
        if (controller.isGrounded)
        {
            yVel = 0f;
            if (inputs[4])
            {
                yVel = jumpForce;
            }
        }
        // Add gravity to the player after jumping 
        yVel += gravity;

        _moveDir.y = yVel;
        controller.Move(_moveDir);

        // Send the players new position and rotation to the client 
        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    /*
     * Method to handle setting input 
     * Params: _inputs - inputs boolean array, _rot - player rotation 
     */
    public void SetInput(bool[] _inputs, Quaternion _rot)
    {
        inputs = _inputs;
        transform.rotation = _rot;
    }

    /*
     * Method to handle player attempting to pick up a key 
     */
    public bool KeyPickup()
    {
        // Only allow the player to pick up a key if they currently have less than the max keys value set 
        if (keys < maxKeys)
        {
            keys++;
            return true;
        }
        else
        {
            return false;
        }
    }

    /*
     * Method handles collision detection of water (hazard)
     */
    private void OnTriggerEnter(Collider other)
    {
        // Determines if the player touched the water in the scene and damages them if so 
        if (other.CompareTag("Water"))
        {
            TakeDamage(1);
        }
    }

    /*
     * Method handles player taking damage 
     * Param: amount of damage to player 
     */
    public void TakeDamage(int _damage)
    {
        // Remove the damage from the players health
        health -= _damage;
        // Handle player death if new health value is less than or is 0 
        if (health <= 0)
        {
            health = 0;
            controller.enabled = false;
            // Set their respawn position to much higher than the spawn point for a falling spawn effect
            transform.position = new Vector3(NetworkManager.instance.spawnPoint.x, NetworkManager.instance.spawnPoint.y+20, NetworkManager.instance.spawnPoint.z);
            // Send players new position to the client 
            ServerSend.PlayerPosition(this);
            // Start coroutine which handles respawning 
            StartCoroutine(Respawn());
        }
        // Send the players health to the client 
        ServerSend.PlayerHealth(this);
    }

    /*
     * Coroutine to handle player respawning
     */
    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(0.3f);
        // Reset player health and re-enable their control
        health = maxHealth;
        controller.enabled = true;
        // Send player respawn packet to the client 
        ServerSend.PlayerRespawn(this);
    }
}
