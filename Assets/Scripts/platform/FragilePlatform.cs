using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class FragilePlatform : MonoBehaviour
{
    [Header("Settings")]
    public float delayBeforeBreak = 2f;   // Time before the platform breaks after being stepped on
    public float respawnTime = 5f;        // Time before the platform respawns after breaking

    private bool isBroken = false;
    private Collider2D col;
    private SpriteRenderer sprite;
    private TilemapRenderer tilemapRenderer; // Supports both Sprite and Tilemap platforms

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        sprite = GetComponent<SpriteRenderer>();
        tilemapRenderer = GetComponent<TilemapRenderer>();

        if (col == null)
            Debug.LogError(" Missing Collider2D component!");
        if (sprite == null && tilemapRenderer == null)
            Debug.LogError(" Missing SpriteRenderer or TilemapRenderer component!");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isBroken && collision.collider.CompareTag("Player"))
        {
            Debug.Log(" Player touched the platform, starting break countdown...");
            StartCoroutine(BreakAndRespawn());
        }
    }

    private IEnumerator BreakAndRespawn()
    {
        isBroken = true;

        // Wait before breaking
        yield return new WaitForSeconds(delayBeforeBreak);

        // Break effect: disable collider + hide visuals
        col.enabled = false;
        if (sprite != null) sprite.enabled = false;
        if (tilemapRenderer != null) tilemapRenderer.enabled = false;
        Debug.Log(" Platform broken");

        // Wait before respawning
        yield return new WaitForSeconds(respawnTime);

        // Restore platform
        col.enabled = true;
        if (sprite != null) sprite.enabled = true;
        if (tilemapRenderer != null) tilemapRenderer.enabled = true;
        isBroken = false;

        Debug.Log(" Platform respawne");
    }
}
