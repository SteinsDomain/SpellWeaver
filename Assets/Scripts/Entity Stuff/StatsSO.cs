using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stats", menuName = "Scriptable Objects/Stats")]
public class StatsSO : ScriptableObject {


    [Header("Health Parameters")]
    public int maxHP;
    public int hpRegenRate;

    [Header("Mana Parameters")]
    public int maxMP;
    public int mpRegenAmount;
    public float mpRegenRate;

    [Header("Ground Speed Parameters")]
    public float groundSpeed;
    public float groundAccelerationTime;
    public float groundDecelerationTime;

    [Header("Air Speed Parameters")]
    public float airSpeed;
    public float airAccelerationTime;
    public float airDecelerationTime;

    [Header("Jump Parameters")]
    public float groundJumpHeight;
    public float jumpDecay;
    public float gravity;

    [Header("Air Jump Parameters")]
    public float airJumpHeight;
    public int maxAirJumps;

    [Header("Wall Jump Parameters")]
    public Vector2 wallJumpDirection = new Vector2(1, 1);
    public float wallJumpForce;
    public float wallJumpDuration;
    public float wallSlideGravityMod;
    public float maxWallSlideSpeed;
    public float inputOverrideDuration;

    [Header("Dash Parameters")]
    public float dashSpeed = 2f;
    public float dashDuration = 0.5f;
    public float dashCooldown = 1f;
    public float dashGravityMod;
}
