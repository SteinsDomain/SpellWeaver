using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Squad", menuName = "Enemy Squad")]
public class EnemySquad : ScriptableObject {
    public GameObject[] enemies;
}