using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpellData : ScriptableObject{

    [Header("Basic Spell Properties")]
    public string spellName;
    public int mpCost = 10;
    public float manaDrainRate;
    public float mpRegenCooldown = 3.0f;
    [Min(.01f)]public float castsPerSecond = 1f;
    public bool requiresConcentration = false;
    public bool canAim;

    public float hitStunScale;
    public float hitStunDuration;

    public AudioClip castSound;
    public ParticleSystem castEffect;
}
