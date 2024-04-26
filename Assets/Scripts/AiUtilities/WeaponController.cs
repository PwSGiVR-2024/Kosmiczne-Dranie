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

    public void Init(AiController unit)
    {
        this.unit = unit;
        unit.onUnitNeutralized.AddListener((_) => pool.Clean());
        pool = new(values.projectileLifeSpan, values.attackCooldown);
    }

    private Projectile SpawnProjectile()
    {
        GameObject instance = Instantiate(values.projectile, transform.position, SetProjectileRotation());
        Projectile projectile = instance.GetComponent<Projectile>();
        projectile.Init(values, unit);
        return projectile;
    }

    private Quaternion SetProjectileRotation()
    {
        Quaternion projectileRotation = transform.rotation;

        float randomAngle = Random.Range(-values.angleError, values.angleError);
        projectileRotation *= Quaternion.AngleAxis(randomAngle, Vector3.up);

        return projectileRotation;
    }

    private void PutProjectile()
    {
        Projectile projectile;
        if (pool.TryGetProjectile(out projectile))
        {
            projectile.transform.position = transform.position;
            projectile.transform.rotation = SetProjectileRotation();
            projectile.gameObject.SetActive(true);
        }

        else
        {
            projectile = SpawnProjectile();
            pool.TryPutProjectile(projectile);
        }
    }

    private IEnumerator AttackCooldown()
    {
        onCooldown = true;

        while (cooldownRemaining > 0)
        {
            cooldownRemaining -= Time.deltaTime;

            if (cooldownRemaining < 0)
                cooldownRemaining = 0;

            yield return null;
        }

        onCooldown = false;
    }

    public void FireProjectile()
    {
        if (onCooldown)
            return;

        PutProjectile();
        cooldownRemaining = values.attackCooldown;

        StartCoroutine(AttackCooldown());
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
