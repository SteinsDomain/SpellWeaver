using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMeleeAttack", menuName = "Scriptable Objects/MeleeAttack")]
public class MeleeAttackData : ScriptableObject {
    public GameObject meleePrefab;
    public float hitDuration;
    public int damage;
    public Vector3 swingStartPoint;
    public Vector3 swingEndPoint;
    public Vector3 swingStartRotation;
    public Vector3 swingEndRotation;
    public float arcHeight;
    public float startUpTime;
    public float coolDown;
}
