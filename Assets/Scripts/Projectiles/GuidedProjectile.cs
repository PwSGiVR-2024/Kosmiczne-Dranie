using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.DebugUI;

public class GuidedProjectile : Projectile
{
    public Transform targetTransform;
    public float rotationSpeed = 1f;
    public float guidingError = 3f;
    public float rotationMultiplier = 1f;

    protected override void Update()
    {
        base.Update();

        // get transform of the terget if feasible (target of the unit, that shot this projectile)
        if (targetTransform == null)
        {
            if (ShotByUnit.Target.TryLockTarget(out AiController controller))
                targetTransform = controller.transform;
        }

        // if projectile is active, gradually increase rotation speed. May prevent being stuck going in a loop around the target
        if (isActiveAndEnabled)
        {
            if (rotationMultiplier < 5)
            {
                float frameRotationIncrease = 4 * Time.deltaTime / Values.projectileLifeSpan;
                rotationMultiplier += frameRotationIncrease;
            }
        }

        // rotate projectile in the direction of target if assigned. If not then projectile just moves forward
        if (targetTransform)
        {
            // target direction
            Vector3 direction = (ShotByUnit.Target.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // guiding error
            float error = Random.Range(-guidingError, guidingError);
            transform.rotation = transform.rotation * Quaternion.Euler(0, error, 0);

            // rotation towards target
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // move forward
        Vector3 newPosition = transform.position + transform.forward * Values.projectileSpeed * Time.deltaTime;
        transform.position = newPosition;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // must reset value
        rotationMultiplier = 1f;
    }
}
