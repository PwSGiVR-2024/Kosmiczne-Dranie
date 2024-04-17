using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

// ka¿dy pocisk posiada ten komponent
// klasa definiuje zachowanie pocisku i wszytskie jego atrybuty
public class Projectile : MonoBehaviour
{
    public AiController shotBy;
    public int speed;
    public int dmg;
    public float lifeSpan;
    public bool friendly;
    public float angleError = 0;
    private WaitForSeconds waitForSeconds;
    public bool destroyAfterDeactivated = false;

    public void Init(Unit values, bool friendly, GameObject container, AiController shooter)
    {
        transform.SetParent(container.transform);

        shotBy = shooter;
        dmg = values.projectileDamage;
        speed = values.projectileSpeed;
        lifeSpan = values.projectileLifeSpan;
        waitForSeconds = new WaitForSeconds(lifeSpan);
        this.friendly = friendly;

        //Destroy(gameObject, lifeSpan);
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
    // Update is called once per frame
    void Update()
    {
        Vector3 newPosition = transform.position + transform.forward * speed * Time.deltaTime;
        transform.position = newPosition;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Projectile"))
            return;

        if (friendly)
        {
            if (collider.CompareTag("Ally"))
                return;

            else if (collider.CompareTag("Enemy"))
                collider.gameObject.GetComponent<AiController>().Damage(this);
        }

        else
        {
            if (collider.CompareTag("Enemy"))
                return;

            else if (collider.CompareTag("Ally"))
                collider.gameObject.GetComponent<AiController>().Damage(this);
        }

        
        gameObject.SetActive(false);
        //ProjectilePooling.PutProjectileOnStack(gameObject, friendly);
        //Destroy(gameObject);
    }

    private void OnDisable()
    {
        if (destroyAfterDeactivated)
            Destroy(gameObject);

        else
            StopCoroutine(DeactivateProjectile());
    }

    public void MarkToDestroy()
    {
        if (gameObject.activeSelf)
            destroyAfterDeactivated = true;

        else
            Destroy(gameObject);
    }
}
