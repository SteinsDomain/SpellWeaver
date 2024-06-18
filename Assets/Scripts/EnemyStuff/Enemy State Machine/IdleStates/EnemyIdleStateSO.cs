using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/EnemyStates/IdleState")]
public class EnemyIdleStateSO : EnemyBaseStateSO
{
    //[SerializeField] private float idlePauseDuration = 2.0f;
    //private float idleEndTime;

    public override void EnterState(Enemy enemy)
    {
        Debug.Log("Entering Idle State");

        enemy.idleEndTime = Time.time + enemy.idlePauseDuration;
        enemy.SetMoveDirection(Vector2.zero);
        enemy.shouldMove = false;
    }

    public override void UpdateState(Enemy enemy)
    {
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);
        if (distanceToPlayer <= enemy.chaseRange)
        {
            Debug.Log("Transitioning to Chase State");

            enemy.TransitionToState(enemy.chaseState);
            return;
        }

        if (Time.time >= enemy.idleEndTime)
        {
            Debug.Log("Transitioning to Wander State");

            enemy.TransitionToState(enemy.wanderState);
        }
    }

    public override void ExitState(Enemy enemy) { }
}