using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/EnemyStates/AttackState")]
public class EnemyAttackStateSO : EnemyBaseStateSO
{
    //[SerializeField] private float attackCooldown = 1.5f;
    //private float lastAttackTime;

    public override void EnterState(Enemy enemy)
    {
        enemy.lastAttackTime = Time.time;
        enemy.shouldMove = false;
    }

    public override void UpdateState(Enemy enemy)
    {
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);
        if (distanceToPlayer > enemy.attackRange)
        {
            enemy.TransitionToState(enemy.chaseState);
            return;
        }

        if (Time.time - enemy.lastAttackTime >= enemy.attackCooldown)
        {
            enemy.skillManager.CastPressed();
            enemy.lastAttackTime = Time.time;
        }
    }

    public override void ExitState(Enemy enemy) { }
}