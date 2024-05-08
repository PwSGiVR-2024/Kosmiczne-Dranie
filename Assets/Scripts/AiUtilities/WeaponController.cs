using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static AiController;
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

    public void Init(AiController unit)
    {
        this.unit = unit;
        unit.onUnitNeutralized.AddListener((_) => pool.Clean());

        int poolSize = (int)((values.projectileLifeSpan / values.attackCooldown) + 2.0f);

        pool = new(poolSize, this);

        StartCoroutine(AttackCooldown());
    }

    //private Projectile SpawnProjectile()
    //{
    //    GameObject instance = Instantiate(values.projectile, transform.position, SetProjectileRotation());
    //    Projectile projectile = instance.GetComponent<Projectile>();
    //    projectile.Init(this);
    //    return projectile;
    //}

    private void SteProjectilePosition(Projectile projectile)
    {
        Quaternion newRotation = transform.rotation;
        float randomAngle = UnityEngine.Random.Range(-values.angleError, values.angleError);
        newRotation *= Quaternion.AngleAxis(randomAngle, Vector3.up);

        projectile.transform.rotation = newRotation;
        projectile.transform.position = transform.position;
    }

    private void PutProjectile()
    {
        Projectile projectile = pool.GetProjectile();
        SteProjectilePosition(projectile);
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

    public void FireProjectile()
    {
        if (onCooldown)
            return;

        PutProjectile();
        cooldownRemaining = values.attackCooldown;
        onCooldown = true;
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
}
