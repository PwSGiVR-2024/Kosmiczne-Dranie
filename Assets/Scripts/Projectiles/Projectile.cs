using System;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public GameManager gameManager;

    public event Action OnProjectileEnable;
    public event Action OnProjectileDisable;
    public event Action OnProjectileDestroy;

    private Affiliation affiliation;
    private AiController shotByUnit;
    private WeaponValues values;
    private float timeTillDeactivation;
    public LayerMask damageMask;
    public LayerMask hitMask;

    public WeaponValues Values { get => values; }
    public AiController ShotByUnit { get => shotByUnit; }
    public Affiliation Affiliation { get => affiliation; }

    public Outpost shotByOutpost;
    public Vector3 targetPos;

    public static Projectile Create(WeaponController weapon)
    {
        Projectile proj = Instantiate(weapon.Values.projectile).GetComponent<Projectile>();
        proj.Init(weapon);
        return proj;
    }

    protected virtual void Init(WeaponController weapon)
    {
        values = weapon.Values;
        shotByUnit = weapon.OwnerUnit;

        shotByOutpost = weapon.ownerOutpost;
        shotByUnit = weapon.OwnerUnit;

        gameManager = shotByOutpost?.gameManager;
        gameManager = shotByUnit?.GameManager;

        if (weapon.ownerOutpost)
            affiliation = weapon.ownerOutpost.Affiliation;

        else if (weapon.OwnerUnit)
            affiliation = weapon.OwnerUnit.Affiliation;

        timeTillDeactivation = values.projectileLifeSpan;
        

        if (affiliation == Affiliation.Blue)
        {
            gameObject.layer = LayerMask.NameToLayer("AllyProjectiles");
            damageMask = LayerMask.GetMask("Enemies", "EnemyOutposts", "EnemyHeadquarters");
            hitMask = LayerMask.GetMask("Scene", "Enemies", "EnemyOutposts", "EnemyHeadquarters");
        }

        else if (affiliation == Affiliation.Red)
        {
            gameObject.layer = LayerMask.NameToLayer("EnemyProjectiles");
            damageMask = LayerMask.GetMask("Allies", "AllyOutposts", "PlayerHeadquarters");
            hitMask = LayerMask.GetMask("Scene", "Allies", "AllyOutposts", "PlayerHeadquarters");

        }
    }

    // base update only controlls hit calculations and deactivation. Not movement
    protected virtual void Update()
    {
        if (TryProcessHit())
            return;

        timeTillDeactivation -= Time.deltaTime;

        if (timeTillDeactivation <= 0)
            gameObject.SetActive(false);
    }

    protected virtual bool TryProcessHit()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 1, hitMask))
        {
            gameObject.SetActive(false);

            // every layer from target mask except scene layer (static objects)
            // assuming every script from the layer has the interface
            if (CheckIfHitDamageMask(hit))
            {
                IInteractable interactable = hit.collider.GetComponent<MonoBehaviour>() as IInteractable;
                interactable.Damage(this);
            }

            // if other layer, then there is nothing to damage, but still counts as hit
            return true;
        }

        return false;
    }

    protected virtual void OnEnable()
    {
        OnProjectileEnable?.Invoke();
    }

    protected virtual void OnDisable()
    {
        timeTillDeactivation = Values.projectileLifeSpan;
        OnProjectileDisable?.Invoke();
    }
    protected virtual void OnDestroy()
    {
        OnProjectileDestroy?.Invoke();
    }

    protected bool CheckIfHitDamageMask(RaycastHit hit)
    {
        return (damageMask.value & (1 << hit.collider.gameObject.layer)) != 0;
    }
}
