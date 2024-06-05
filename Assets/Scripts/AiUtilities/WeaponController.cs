using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static AiController;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.DebugUI;

public class WeaponController: MonoBehaviour
{
    [SerializeField] private WeaponValues values;

    private ProjectilePool pool;
    private AiController unit;
    private bool onCooldown = false;
    private float cooldownRemaining = 0f;
    public WeaponValues Values { get => values; }
    public ProjectilePool Pool { get => pool; }
    public AiController Unit { get => unit; }

    // debug
    [SerializeField] private Projectile[] projectilePool;

    public void Init(AiController unit)
    {
        if (!isActiveAndEnabled)
            return;

        this.unit = unit;
        unit.onUnitNeutralized.AddListener((_) => pool.Clean());

        int poolSize = (int)((values.projectileLifeSpan / values.attackCooldown) + 2.0f);

        pool = new(poolSize, this);

        //debug
        projectilePool = pool.Projectiles;

        StartCoroutine(AttackCooldown());
    }

    private void SetProjectilePosition(Projectile projectile)
    {
        Quaternion newRotation = transform.rotation;
        float randomAngle = UnityEngine.Random.Range(-values.angleError, values.angleError);
        newRotation *= Quaternion.AngleAxis(randomAngle, Vector3.up);

        projectile.transform.rotation = newRotation;
        projectile.transform.position = transform.position;
    }

    private Projectile PutProjectile()
    {
        Projectile projectile = pool.GetProjectile();
        SetProjectilePosition(projectile);
        return projectile;
    }

    private IEnumerator AttackCooldown()
    {
        while (true)
        {
            if (onCooldown)
            {
                cooldownRemaining -= Time.deltaTime;

                if (cooldownRemaining <= 0)
                    onCooldown = false;
            }
            
            yield return null;
        }
    }

    public Projectile FireProjectile()
    {
        if (onCooldown)
            return null;

        cooldownRemaining = values.attackCooldown;
        onCooldown = true;

        Projectile proj = PutProjectile();

        if (proj is GuidedProjectile guidedProj)
        {
            return guidedProj;
        }

        else
        {
            return proj;
        }
    }

    public bool CheckIfFacingTarget(Vector3 targetPos)
    {
        float angleToTarget = GameUtils.CalculateForwardAngle(transform, targetPos);
        if (angleToTarget <= values.angleError)
            return true;

        return false;
    }

    public bool CheckIfFacingTarget(Vector3 targetPos, ref float angle)
    {
        float angleToTarget = GameUtils.CalculateForwardAngle(transform, targetPos);
        if (angleToTarget <= values.angleError)
            return true;

        angle = angleToTarget;
        return false;
    }

    public void RotateWeaponAtPosition(Vector3 position)
    {
        Vector3 direction = position - transform.position;
        direction.Normalize();
        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 5f * Time.deltaTime);
    }
}
