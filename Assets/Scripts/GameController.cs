using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] playerDeathSound;

    Vector2 checkpointPos;

    // References to all player controllers
    public MovementController[] players;

    private void Awake()
    {
        players = Object.FindObjectsByType<MovementController>(FindObjectsSortMode.None);

        if (players.Length == 0)
        {
            Debug.LogError("No MovementController found in the scene. Check player setup.");
        }
    }

    private void Start()
    {
        if (players.Length > 0)
        {
            checkpointPos = players[0].transform.position;
        }
    }

    public void UpdateCheckpoint(Vector2 pos)
    {
        checkpointPos = pos;
        Debug.Log("Checkpoint Updated to: " + pos);
    }

    public void Die(MovementController deadPlayer)
    {
        Debug.Log(deadPlayer.gameObject.name + " died! Triggering Fade Transition.");

        if (audioSource != null && playerDeathSound != null)
        {
            AudioClip clip = playerDeathSound[Random.Range(0, playerDeathSound.Length)];
            audioSource.PlayOneShot(clip);
        }

        if (SceneFader.Instance != null)
        {
            StartCoroutine(SceneFader.Instance.StartTransition(
                SceneFader.Instance.CurrentFadeType,
                () =>
                {
                    PrepareForRespawn();
                    Respawn();
                    FinalizeRespawn();
                }
            ));
        }
        else
        {
            Debug.LogWarning("SceneFader not found. Respawning instantly.");
            StartCoroutine(RespawnWithoutFade(0.5f));
        }
    }

    private void PrepareForRespawn()
    {
        foreach (var player in players)
        {
            if (player.TryGetComponent(out Rigidbody2D playerRb))
            {
                playerRb.simulated = false;
                player.transform.localScale = new Vector3(0, 0, 0);
            }
        }
    }

    // New non-coroutine Respawn function (executed at the peak of the fade)
    private void Respawn()
    {
        Debug.Log("Executing Respawn at Checkpoint: " + checkpointPos);

        foreach (var player in players)
        {
            player.transform.position = checkpointPos;
        }

        FinalBoss finalBoss = FindFirstObjectByType<FinalBoss>();
        if (finalBoss != null)
        {
            finalBoss.ResetBossState();
        }

        StationaryBoss stationaryBoss = FindFirstObjectByType<StationaryBoss>();
        if (stationaryBoss != null)
        {
            stationaryBoss.ResetBossState();
        }
    }
    
    private void FinalizeRespawn()
    {
        foreach (var player in players)
        {
            if (player.TryGetComponent(out Rigidbody2D playerRb))
            {
                player.transform.localScale = new Vector3(1, 1, 1); 
                playerRb.simulated = true;
                playerRb.linearVelocity = Vector2.zero;
            }
        }
    }
    
    IEnumerator RespawnWithoutFade(float duration)
    {
        PrepareForRespawn();
        yield return new WaitForSeconds(duration);
        Respawn(); 
        FinalizeRespawn();
    }
}