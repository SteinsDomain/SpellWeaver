using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileSpell", menuName = "Scriptable Objects/Spells/ProjectileSpell")]
public class ProjectileSpellData : SpellData {


    public enum ShotType {
        Single,  // Represents a single shot per trigger pull
        Auto,    // Represents fully automatic fire
        Charge,  // Represents charge-up shots (e.g., charging before firing)
    }

    public enum ShotDirection {
        Arc,
        Straight
    }

    [Header("Projectile Spell Properties")]

    public ShotType shotType;
    public ShotDirection shotDirection;

    public GameObject projectilePrefab;
    public GameObject explosionPrefab;
    public ParticleSystem hitEffect;

    //possible uses for all shot types I think
    public float projectileSpeed;
    public float projectileKnockback;
    public float projectileRecoil;
    public int projectileDamage;
    public float maxProjectileRange;
    public bool isExplosive;
    public float explosionDelay;
    public float explosionSize;
    public float explosionKnockbackForce;
    public int explosionDamage;

    //only used for Single and Charge shots
    public bool isDetonatable;

    //Charge specific
    public float chargeTime;
    public float minimumChargeNeeded;
    public float chargeBoostAmount;

    // New variables
    public int shotsPerCast = 1;
    public float projectileAccuracy = 1.0f;
    public float maxSpread = 30.0f;  // Maximum spread angle in degrees


    public float screenShakeAmount = 0.5f;
    public AudioClip castSound;
}
