using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserProjectile : Projectile
{
    public SpriteRenderer sprite;
    public bool hitObject = false;
    private Vector3 stashedScale;

    protected override void Init(WeaponController weapon)
    {
        base.Init(weapon);

        stashedScale = sprite.transform.localScale;
    }


    protected override bool TryProcessHit()
    {
        if (hitObject) return false;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, ShotBy.Values.attackDistance, targetMask))
        {
            // Enemies or Allies layer
            // assuming every script from the layer has the interface
            if ((hit.collider.gameObject.layer == 6) || (hit.collider.gameObject.layer == 7))
            {
                IInteractable interactable = hit.collider.GetComponent<MonoBehaviour>() as IInteractable;
                interactable.Damage(this);
            }

            SetProjectilePosition(transform.position, hit.point);
            hitObject = true;
            return true;
        }

        return false;
    }

    protected void SetProjectilePosition(Vector3 origin, Vector3 end)
    {
        float distance = Vector3.Distance(origin, end);
        Vector3 midpoint = (origin + end) / 2;
        transform.position = midpoint;

        Vector3 newScale = sprite.transform.localScale;
        newScale.y = distance / sprite.bounds.size.y;
        sprite.transform.localScale = newScale;

        Debug.DrawRay(midpoint, Vector3.up, Color.red, 1f);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        sprite.transform.localScale = stashedScale;
        hitObject = false;
    }
}
