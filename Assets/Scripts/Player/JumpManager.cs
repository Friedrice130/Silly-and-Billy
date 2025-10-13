using UnityEngine;

public static class JumpManager
{
    public static float lastPlayer1JumpTime;
    public static float lastPlayer2JumpTime;

    // Checks if the jumps happened close together in time
    public static bool IsSynchronized()
    {
        return Mathf.Abs(lastPlayer1JumpTime - lastPlayer2JumpTime) <= 0.15f; // within 150ms
    }
}