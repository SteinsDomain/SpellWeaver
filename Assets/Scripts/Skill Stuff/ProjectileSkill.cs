using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ProjectileSkill : Skill {

    private GameObject currentDetonatableProjectile;

    private void Awake() {
    }

    public override void CastPressed() {
        var projectileSkill = skillData as ProjectileSkillData;
        if (projectileSkill == null) return;

        if (projectileSkill.isDetonatable && currentDetonatableProjectile != null) {
            Debug.Log("Detonating existing projectile.");
            DetonateProjectile();
        }
        else if (TryToCast()) {
            Debug.Log("Casting new projectile.");
            FireProjectiles(projectileSkill);
            AudioSource.PlayClipAtPoint(projectileSkill.castSound, Vector3.zero);
            Instantiate(skillData.castEffect, castPoint.position, Quaternion.identity, castPoint);
        }
    }

    public override void CastHeld() {
        var projectileSkill = skillData as ProjectileSkillData;
        if (projectileSkill == null) return;

        if (projectileSkill.shotType == ProjectileSkillData.ShotType.Auto && TryToCast()) {
            Debug.Log("Continuously casting projectile.");
            FireProjectiles(projectileSkill);
            AudioSource.PlayClipAtPoint(projectileSkill.castSound, Vector3.zero);
            Instantiate(skillData.castEffect, castPoint.position, Quaternion.identity, castPoint);
        }
    }

    public override void CastReleased() {
        if (skillData.requiresConcentration) {
            manaManager.GetComponent<SkillManager>().IsConcentrating = false;
        }
    }

    private void FireProjectiles(ProjectileSkillData projectileSkill) {
        if (projectileSkill.shotsPerCast == 1)
        {
            // If there's only one projectile, no need to calculate spread.
            Vector3 direction = castPoint.up;
            float randomAngle = Random.Range(-projectileSkill.projectileAccuracy, projectileSkill.projectileAccuracy);
            direction = Quaternion.Euler(0, 0, randomAngle) * direction;
            FireProjectile(projectileSkill, castPoint.position, direction);
            
        }
        else
        {
            float totalSpread = projectileSkill.maxSpread;
            float spreadIncrement = projectileSkill.shotsPerCast > 1 ? totalSpread / (projectileSkill.shotsPerCast - 1) : 0;
            float halfSpread = totalSpread / 2;

            for (int i = 0; i < projectileSkill.shotsPerCast; i++)
            {
                float baseOffset = -halfSpread + spreadIncrement * i;


                if (projectileSkill.shotDirection == ProjectileSkillData.ShotDirection.Arc) {
                    float randomAngle = Random.Range(-projectileSkill.projectileAccuracy, projectileSkill.projectileAccuracy);
                    float finalAngle = baseOffset + randomAngle;
                    Vector3 direction = Quaternion.Euler(0, 0, finalAngle) * castPoint.up;
                    FireProjectile(projectileSkill, castPoint.position, direction);
                }
                else { // Straight path
                    Vector3 offset = castPoint.up * baseOffset;
                    Vector3 startPosition = castPoint.position + offset;
                    Vector3 direction = castPoint.up;
                    float randomAngle = Random.Range(-projectileSkill.projectileAccuracy, projectileSkill.projectileAccuracy);
                    direction = Quaternion.Euler(0, 0, randomAngle) * direction;

                    FireProjectile(projectileSkill, startPosition, direction);
                }
            }
        }
    }

    private void FireProjectile(ProjectileSkillData projectileSkill, Vector3 startPosition, Vector3 direction) {
        GameObject projectile = Instantiate(projectileSkill.projectilePrefab, startPosition, Quaternion.identity);
        projectile.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
        float scale = castPoint.parent.transform.localRotation.eulerAngles.y == 180 ? -1 : 1;
        projectile.transform.localScale = new Vector3(scale * projectileSkill.projectileSizeMod, projectileSkill.projectileSizeMod, 1);

        //var caster = castPoint.parent;

        if (castPoint.parent != null) {
            if (castPoint.parent.CompareTag("Player")) {
                projectile.layer = LayerMask.NameToLayer("Player Projectiles");

                //Screen Shake for player only 
                CinemachineShake.Instance.ShakeCamera(projectileSkill.screenShakeAmount, .1f);

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

        var movementManager = castPoint.GetComponentInParent<MovementManager>();
        if (movementManager != null) {
            Debug.Log("Applying recoil to " + movementManager.name);

            movementManager.ApplyRecoil(projectileSkill.projectileRecoil);
        }

        var behaviour = projectile.AddComponent<ProjectileBehaviour>();
        behaviour.skillData = projectileSkill;
        behaviour.castPoint = castPoint;
        behaviour.SetOriginLayer(castPoint.parent.gameObject.layer);

        if (projectileSkill.isDetonatable) {
            currentDetonatableProjectile = projectile;
            Debug.Log("Set new detonatable projectile.");
        }
        if (projectileSkill.isExplosive) {
            Debug.Log("Initiating explosion sequence.");
            behaviour.InitiateExplosionCountdown(projectileSkill.explosionDelay);
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
