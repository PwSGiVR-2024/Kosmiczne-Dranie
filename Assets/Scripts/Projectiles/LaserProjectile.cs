using UnityEngine;

public class LaserProjectile : Projectile
{
    public bool hitObject = false;
    public LineRenderer lineRenderer;
    public Collider target;
    private Vector3 cachedLastTargetPos;

    protected override void Init(WeaponController weapon)
    {
        base.Init(weapon);
    }

    protected override bool TryProcessHit()
    {
        if (hitObject) return false;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, float.PositiveInfinity, damageMask))
        {
            // every layer from target mask except scene layer (static objects)
            // assuming every script from the layer has the interface
            if (CheckIfHitDamageMask(hit))
            {
                IInteractable interactable = hit.collider.GetComponent<MonoBehaviour>() as IInteractable;
                interactable.Damage(this);
            }

            hitObject = true;
            return true;
        }

        return false;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        hitObject = false;
    }

    protected void UpdatePosition()
    {
        lineRenderer.SetPosition(0, transform.position);

        if (ShotByUnit)
            lineRenderer.SetPosition(1, ShotByUnit.Target.position);

        else if (shotByOutpost && target)
        {
            lineRenderer.SetPosition(1, target.transform.position);
            cachedLastTargetPos = target.transform.position;
        }

        else if (shotByOutpost && target == null)
            lineRenderer.SetPosition(1, cachedLastTargetPos);
    }

    protected override void Update()
    {
        base.Update();
        UpdatePosition();
    }
}
