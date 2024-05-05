using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

// ka¿dy pocisk posiada ten komponent
// klasa definiuje zachowanie pocisku i wszytskie jego atrybuty
public class Projectile : MonoBehaviour
{
    private AiController.UnitSide side;
    private AiController shotBy;
    private WaitForSeconds waitForSeconds;
    private bool destroyAfterDeactivated = false;
    private WeaponValues values;

    public WeaponValues Values { get => values; }
    public AiController ShotBy { get => shotBy; }

    public void Init(WeaponValues values, AiController shooter)
    {
        this.values = values;
        shotBy = shooter;
        waitForSeconds = new WaitForSeconds(values.projectileLifeSpan);
        transform.SetParent(shotBy.ProjectileContainer.transform);

        side = shotBy.Side;

        StartCoroutine(DeactivateProjectile());
    }

    private void OnEnable()
    {
        if (waitForSeconds != null)
            StartCoroutine(DeactivateProjectile());
    }

    private IEnumerator DeactivateProjectile()
    {
        yield return waitForSeconds;
        gameObject.SetActive(false);
    }

    void Update()
    {
        Vector3 newPosition = transform.position + transform.forward * values.projectileSpeed * Time.deltaTime;
        transform.position = newPosition;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Projectile"))
            return;

        if (collider.CompareTag("Ally"))
        {
            if (side == AiController.UnitSide.Ally)
                return;

            else if (side == AiController.UnitSide.Enemy)
                collider.GetComponent<AiController>().Damage(this);
        }

        else if (collider.CompareTag("Enemy"))
        {
            if (side == AiController.UnitSide.Enemy)
                return;

            else if (side == AiController.UnitSide.Ally)
                collider.GetComponent<AiController>().Damage(this);
        }

        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (shotBy == null || destroyAfterDeactivated)
            Destroy(gameObject);

        else
            StopCoroutine(DeactivateProjectile());
    }

    public void DestroyAfterDeactivated()
    {
        destroyAfterDeactivated = true;
    }
}
