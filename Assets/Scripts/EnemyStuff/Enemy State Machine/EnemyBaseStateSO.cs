using UnityEngine;

public abstract class EnemyBaseStateSO : ScriptableObject
{
    public abstract void EnterState(Enemy enemy);
    public abstract void UpdateState(Enemy enemy);
    public abstract void ExitState(Enemy enemy);
}