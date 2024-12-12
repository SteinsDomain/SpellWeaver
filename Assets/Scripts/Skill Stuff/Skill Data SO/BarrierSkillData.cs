using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "BarrierSkill", menuName = "Scriptable Objects/Skills/BarrierSkill")]

public class BarrierSkillData : SkillData {

    [Header("Barrier Skill Properties")]

    public GameObject barrierPrefab;

    public bool requiresHold;
    public bool drainsMana;

    public bool isBreakable;
    public int barrierHealth;

    public float barrierDuration;

    public bool centersOnCaster;
    public bool isStationary;
}
