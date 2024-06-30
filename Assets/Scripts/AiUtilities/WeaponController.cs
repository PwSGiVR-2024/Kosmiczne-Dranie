using System.Collections;
using UnityEngine;

public class WeaponController: MonoBehaviour
{
    public AudioSource shotSoundPrefab;
    private SoundManager soundManager;

    [SerializeField] private WeaponValues values;
    public bool debug = false;

    private ProjectilePool pool;
    private AiController ownerUnit;
    private bool onCooldown = false;
    private float cooldownRemaining = 0f;
    public WeaponValues Values { get => values; }
    public ProjectilePool Pool { get => pool; }
    public AiController OwnerUnit { get => ownerUnit; }

    public Outpost ownerOutpost;
    private Vector3 targetPos;
    private Collider target;

    // debug
    [SerializeField] private Projectile[] projectilePool;

    public void Init(AiController unit)
    {
        if (!isActiveAndEnabled)
            return;

        ownerUnit = unit;
        unit.onUnitNeutralized.AddListener((_) => pool.Clean());

        int poolSize = (int)((values.projectileLifeSpan / values.attackCooldown) + 2.0f);

        pool = new(poolSize, this);

        //debug
        projectilePool = pool.Projectiles;

        soundManager = unit.GameManager.soundManager;

        StartCoroutine(AttackCooldown());
    }

    public void Init(Outpost outpost)
    {
        if (!isActiveAndEnabled)
            return;

        ownerOutpost = outpost;
        outpost.onOutpostDestroy.AddListener(() => pool.Clean());

        int poolSize = (int)((values.projectileLifeSpan / values.attackCooldown) + 2.0f);

        pool = new(poolSize, this);

        //debug
        projectilePool = pool.Projectiles;

        soundManager = outpost.gameManager.soundManager;

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
        projectile.targetPos = targetPos;
        
        if (projectile is LaserProjectile lp)
            lp.target = target;

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

    public bool FireProjectile(out Projectile projectile)
    {
        if (onCooldown)
        {
            projectile = null;
            return false;
        }

        Projectile proj = PutProjectile();

        if (proj)
        {
            cooldownRemaining = values.attackCooldown;
            onCooldown = true;

            if (proj is GuidedProjectile guidedProj)
            {
                projectile = guidedProj;
                return true;
            }

            else if (proj is KineticProjectile kineticProj)
            {
                projectile = kineticProj;
                return true;
            }

            else if (proj is LaserProjectile laserProj)
            {
                projectile = laserProj;
                return true;
            }
        }

        projectile = null;
        return false;
    }

    public bool FireProjectile()
    {
        if (onCooldown)
            return false;

        Projectile proj = PutProjectile();

        if (proj)
        {
            cooldownRemaining = values.attackCooldown;
            onCooldown = true;
            soundManager.PlaySoundEffect(shotSoundPrefab);
            return true;
        }

        return false;
    }

    public bool FireProjectile(Collider target)
    {
        this.target = target;

        return FireProjectile();
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
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 50f * Time.deltaTime);
    }

    //private void PlayShotSound()
    //{
    //    if (shotSoundPrefab)
    //    {
    //        Debug.Log("sound");
    //        shotSoundPrefab.Play();
    //    }
            
    //}
}
