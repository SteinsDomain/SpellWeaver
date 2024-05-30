using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ProjectileSpell : Spell {

    private GameObject currentDetonatableProjectile;

    private void Awake() {
    }

    public override void CastPressed() {
        var projectileSpell = spellData as ProjectileSpellData;
        if (projectileSpell == null) return;

        if (projectileSpell.isDetonatable && currentDetonatableProjectile != null) {
            Debug.Log("Detonating existing projectile.");
            DetonateProjectile();
        }
        else if (TryToCast()) {
            Debug.Log("Casting new projectile.");
            FireProjectiles(projectileSpell);
            AudioSource.PlayClipAtPoint(projectileSpell.castSound, Vector3.zero);
        }
    }

    public override void CastHeld() {
        var projectileSpell = spellData as ProjectileSpellData;
        if (projectileSpell == null) return;

        if (projectileSpell.shotType == ProjectileSpellData.ShotType.Auto && TryToCast()) {
            Debug.Log("Continuously casting projectile.");
            FireProjectiles(projectileSpell);
            AudioSource.PlayClipAtPoint(projectileSpell.castSound, Vector3.zero);

        }
    }

    public override void CastReleased() {
        if (spellData.requiresConcentration) {
            manaManager.GetComponent<SpellManager>().IsConcentrating = false;
        }
    }

    private void FireProjectiles(ProjectileSpellData projectileSpell) {
        if (projectileSpell.shotsPerCast == 1)
        {
            // If there's only one projectile, no need to calculate spread.
            Vector3 direction = castPoint.up;
            float randomAngle = Random.Range(-projectileSpell.projectileAccuracy, projectileSpell.projectileAccuracy);
            direction = Quaternion.Euler(0, 0, randomAngle) * direction;
            FireProjectile(projectileSpell, castPoint.position, direction);
            
        }
        else
        {
            float totalSpread = projectileSpell.maxSpread;
            float spreadIncrement = projectileSpell.shotsPerCast > 1 ? totalSpread / (projectileSpell.shotsPerCast - 1) : 0;
            float halfSpread = totalSpread / 2;

            for (int i = 0; i < projectileSpell.shotsPerCast; i++)
            {
                float baseOffset = -halfSpread + spreadIncrement * i;


                if (projectileSpell.shotDirection == ProjectileSpellData.ShotDirection.Arc) {
                    float randomAngle = Random.Range(-projectileSpell.projectileAccuracy, projectileSpell.projectileAccuracy);
                    float finalAngle = baseOffset + randomAngle;
                    Vector3 direction = Quaternion.Euler(0, 0, finalAngle) * castPoint.up;
                    FireProjectile(projectileSpell, castPoint.position, direction);
                }
                else { // Straight path
                    Vector3 offset = castPoint.up * baseOffset;
                    Vector3 startPosition = castPoint.position + offset;
                    Vector3 direction = castPoint.up;
                    float randomAngle = Random.Range(-projectileSpell.projectileAccuracy, projectileSpell.projectileAccuracy);
                    direction = Quaternion.Euler(0, 0, randomAngle) * direction;

                    FireProjectile(projectileSpell, startPosition, direction);
                }
            }
        }
    }

    private void FireProjectile(ProjectileSpellData projectileSpell, Vector3 startPosition, Vector3 direction) {
        GameObject projectile = Instantiate(projectileSpell.projectilePrefab, startPosition, Quaternion.identity);
        projectile.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
        float scale = castPoint.parent.localScale.x > 0 ? 1 : -1;
        projectile.transform.localScale = new Vector3(scale, 1, 1);

        if (castPoint.parent != null) {
            if (castPoint.parent.CompareTag("Player")) {
                projectile.layer = LayerMask.NameToLayer("Player Projectiles");

                //Screen Shake for player only 
                CinemachineShake.Instance.ShakeCamera(projectileSpell.screenShakeAmount, .1f);

                Debug.Log("Assigned to Player Projectiles layer.");
            }
            else if (castPoint.parent.CompareTag("Enemy")) {
                projectile.layer = LayerMask.NameToLayer("Enemy Projectiles");
                Debug.Log("Assigned to Enemy Projectiles layer.");
            }
            else {
                Debug.LogError("Origin parent is neither Player nor Enemy. Using default layer.");
                projectile.layer = LayerMask.NameToLayer("Default");
            }
        }

        var behaviour = projectile.AddComponent<ProjectileBehaviour>();
        behaviour.spellData = projectileSpell;
        behaviour.castPoint = castPoint;
        behaviour.SetOriginLayer(castPoint.parent.gameObject.layer);

        if (projectileSpell.isDetonatable) {
            currentDetonatableProjectile = projectile;
            Debug.Log("Set new detonatable projectile.");
        }
        if (projectileSpell.isExplosive) {
            Debug.Log("Initiating explosion sequence.");
            behaviour.InitiateExplosionCountdown(projectileSpell.explosionDelay);
        }
    }

    private void DetonateProjectile() {
        if (currentDetonatableProjectile != null) {
            var behaviour = currentDetonatableProjectile.GetComponent<ProjectileBehaviour>();
            if (behaviour != null) {
                behaviour.Explode();
                Debug.Log("Detonated the projectile.");
            }
            currentDetonatableProjectile = null;
        }
        else {
            Debug.Log("No detonatable projectile available for detonation.");
        }
    }
}
