using UnityEngine;

public class PlayerStateController : MonoBehaviour
{
    [Header("Respawn")]
    [SerializeField] private Transform[] players; // Assign your two player GameObjects/Transforms here
    private Vector3 currentCheckpointPosition;

    // NOTE: This Awake() logic is likely in your Checkpoint.cs, 
    // but the GameController itself needs its own Awake() or Start() too.
    private void Awake()
    {
        // Set the initial spawn point (e.g., the starting position of Player 1)
        if (players.Length > 0)
        {
            currentCheckpointPosition = players[0].position;
        }
    }

    // Called by Checkpoint.cs
    public void UpdateCheckpoint(Vector3 newPosition)
    {
        currentCheckpointPosition = newPosition;
        Debug.Log("Checkpoint Updated to: " + newPosition);
    }

    // Called by Bullet.cs when a player is hit
    public void PlayerDied()
    {
        Debug.Log("Player Hit! Initiating team respawn.");
        // A short delay before respawning provides visual feedback
        Invoke("RespawnTeam", 1.0f);

        // OPTIONAL: Temporarily disable player input/movement here if needed
    }

    private void RespawnTeam()
    {
        if (players.Length != 2)
        {
            Debug.LogError("GameController needs exactly two players assigned in the Inspector.");
            return;
        }

        // Respawn both players at the last activated checkpoint
        players[0].position = currentCheckpointPosition;

        // For the second player, offset them slightly from the respawn point
        // (Assuming P2 should spawn right next to P1)
        players[1].position = currentCheckpointPosition + new Vector3(2f, 0, 0);

        // OPTIONAL: Re-enable player input/movement here if needed
        Debug.Log("Team Respawned at Checkpoint.");
    }
}