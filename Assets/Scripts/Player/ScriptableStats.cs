using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player/Stats")]
public class ScriptableStats : ScriptableObject
{
    [Header("Movement")]
    public float MaxSpeed = 8f;
    public float Acceleration = 80f;
    public float GroundDeceleration = 60f;
    public float AirDeceleration = 30f;

    [Header("Jumping")]
    public float JumpPower = 15f;
    public float CoyoteTime = 0.15f;
    public float JumpBuffer = 0.15f;
    public float JumpEndEarlyGravityModifier = 2f;

    [Header("Gravity")]
    public float FallAcceleration = 40f;
    public float MaxFallSpeed = 20f;
    public float GroundingForce = -1f;

    [Header("Input")]
    public bool SnapInput = true;
    public float HorizontalDeadZoneThreshold = 0.1f;
    public float VerticalDeadZoneThreshold = 0.1f;

    [Header("Collisions")]
    public float GrounderDistance = 0.1f;
    public LayerMask PlayerLayer;
}
