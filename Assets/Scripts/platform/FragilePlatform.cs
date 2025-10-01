using UnityEngine;
using System.Collections;

public class FragilePlatform : MonoBehaviour
{
    [Header("Settings")]
    public float delayBeforeBreak = 5f; // ??????????
    private bool isBroken = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isBroken && collision.collider.CompareTag("Player"))
        {
            StartCoroutine(BreakPlatform());
        }
    }

    private IEnumerator BreakPlatform()
    {
        yield return new WaitForSeconds(delayBeforeBreak);

        // ?????????
        Destroy(gameObject);
        isBroken = true;
    }
}