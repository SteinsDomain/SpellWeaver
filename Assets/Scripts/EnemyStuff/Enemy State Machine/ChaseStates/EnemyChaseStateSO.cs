using UnityEngine;

[CreateAssetMenu(menuName = "EnemyStates/ChaseState")]
public class EnemyChaseStateSO : EnemyBaseStateSO
{
    public override void EnterState(Enemy enemy)
    {
        enemy.shouldMove = true;
    }

    public override void UpdateState(Enemy enemy)
    {
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);
        if (distanceToPlayer <= enemy.attackRange)
        {
            enemy.TransitionToState(enemy.attackState);
            return;
        }

        if (distanceToPlayer > enemy.chaseRange)
        {
            enemy.TransitionToState(enemy.idleState);
            return;
        }

        float xDirection = enemy.player.position.x > enemy.transform.position.x ? 1 : -1;
        if (Mathf.Abs(enemy.transform.position.x - enemy.player.position.x) < enemy.safeDistance)
        {
            xDirection = -xDirection;
        }
        enemy.SetMoveDirection(new Vector2(xDirection, 0));
    }

    public override void ExitState(Enemy enemy)
    {
        enemy.shouldMove = false;
    }
}