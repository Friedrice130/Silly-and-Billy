using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Add this for easily finding components

public class GameController : MonoBehaviour
{
    Vector2 checkpointPos;

    // References to all player controllers
    public MovementController[] players;

    private void Awake()
    {
        // Find all MovementControllers in the scene and store them
        players = Object.FindObjectsByType<MovementController>(FindObjectsSortMode.None);

        if (players.Length == 0)
        {
            Debug.LogError("No MovementController found in the scene. Check player setup.");
        }
    }

    private void Start()
    {
        // Initial checkpoint is the position of the first player at Start
        if (players.Length > 0)
        {
            checkpointPos = players[0].transform.position;
        }
    }

    // This is still needed for the Checkpoint script to call
    public void UpdateCheckpoint(Vector2 pos)
    {
        checkpointPos = pos;
        Debug.Log("Checkpoint Updated to: " + pos);
    }

    // OnTriggerEnter2D will remain on the *player* objects now. 
    // This is the public method to call when a player dies.
    public void Die(MovementController deadPlayer)
    {
        Debug.Log(deadPlayer.gameObject.name + " died! Triggering Co-op Respawn.");
        StartCoroutine(Respawn(0.5f));
    }

    IEnumerator Respawn(float duration)
    {
        // 1. Disable player simulation and visibility for all players
        foreach (var player in players)
        {
            if (player.TryGetComponent(out Rigidbody2D playerRb))
            {
                playerRb.simulated = false;
                player.transform.localScale = new Vector3(0, 0, 0);
            }
        }

        yield return new WaitForSeconds(duration);

        // 2. Teleport both players to the checkpoint
        foreach (var player in players)
        {
            player.transform.position = checkpointPos;
        }

        // 3. Re-enable player simulation and visibility for all players
        foreach (var player in players)
        {
            if (player.TryGetComponent(out Rigidbody2D playerRb))
            {
                player.transform.localScale = new Vector3(1, 1, 1);
                playerRb.simulated = true;
                // Optional: Reset velocity upon respawn
                playerRb.linearVelocity = Vector2.zero;
            }
        }
    }
}