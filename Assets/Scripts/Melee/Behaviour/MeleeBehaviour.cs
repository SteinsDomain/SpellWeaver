using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeBehaviour : MonoBehaviour
{

    public MeleeAttackData attackData;
    private int originLayer;


    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"MeleeBehaviour: Collision detected with {collision.gameObject.name}.");

        if (ShouldAffectTarget(collision.gameObject))
        {
            Debug.Log($"MeleeBehaviour: Processing hit with {collision.gameObject.name}.");
            HealthManager healthManager = collision.gameObject.GetComponent<HealthManager>();
            if (healthManager != null)
            {
                healthManager.TakeDamage(attackData.damage);
                Debug.Log($"MeleeBehaviour: Applied {attackData.damage} damage to {collision.gameObject.name}.");
            }
        }

    }

    public void SetOriginLayer(int layer)
    {
        originLayer = layer;
    }

    private bool ShouldAffectTarget(GameObject target)
    {
        return target.layer != originLayer;
    }
}
