using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KineticProjectile : Projectile
{
    // Kinetic projectile only moves forward. No additional features
    protected override void Update()
    {
        base.Update();

        Vector3 newPosition = transform.position + transform.forward * Values.projectileSpeed * Time.deltaTime;
        transform.position = newPosition;
    }
}
