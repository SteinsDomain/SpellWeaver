using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "BarrierSpell", menuName = "Scriptable Objects/Spells/BarrierSpell")]

public class BarrierSpellData : SpellData {

    [Header("Barrier Spell Properties")]

    public GameObject barrierPrefab;

    public bool requiresHold;
    public bool drainsMana;

    public bool isBreakable;
    public int barrierHealth;

    public float barrierDuration;

    public bool centersOnCaster;
    public bool isStationary;
}
