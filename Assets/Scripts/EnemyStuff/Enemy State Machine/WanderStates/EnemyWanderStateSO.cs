using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/EnemyStates/WanderState")]
public class EnemyWanderStateSO : EnemyBaseStateSO
{
    public override void EnterState(Enemy enemy)
    {
        enemy.isMovingToTarget = false;
    }

    public override void UpdateState(Enemy enemy)
    {
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);
        if (distanceToPlayer <= enemy.chaseRange)
        {
            enemy.TransitionToState(enemy.chaseState);
            return;
        }

        if (enemy.isMovingToTarget)
        {
            if (Mathf.Abs(enemy.transform.position.x - enemy.wanderTarget.x) < 0.1f)
            {
                enemy.TransitionToState(enemy.idleState);
                return;
            }
        }
        else
        {
            float targetOffset = Random.Range(-enemy.wanderRange, enemy.wanderRange);
            enemy.wanderTarget = new Vector3(enemy.initialPosition.x + targetOffset, enemy.transform.position.y, enemy.transform.position.z);
            enemy.wanderTarget.x = Mathf.Clamp(enemy.wanderTarget.x, enemy.initialPosition.x - enemy.wanderRange, enemy.initialPosition.x + enemy.wanderRange);

            enemy.SetMoveDirection(enemy.wanderTarget.x > enemy.transform.position.x ? Vector2.right : Vector2.left);
            enemy.isMovingToTarget = true;
            enemy.shouldMove = true;
        }
    }

    public override void ExitState(Enemy enemy)
    {
        enemy.shouldMove = false;
        enemy.SetMoveDirection(Vector2.zero);
    }
}