using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Unity.Mathematics;

// ka¿dy pocisk posiada ten komponent
// klasa definiuje zachowanie pocisku i wszytskie jego atrybuty
public class Projectile : MonoBehaviour
{
    public event Action<Projectile> OnProjectileEnable;
    public event Action<Projectile> OnProjectileDisable;
    public event Action<Projectile> OnProjectileDestroy;

    private Affiliation side;
    private AiController shotBy;
    private WeaponValues values;
    private float timeTillDeactivation;

    public WeaponValues Values { get => values; }
    public AiController ShotBy { get => shotBy; }
    public Affiliation Side { get => side; }

    //private LayerMask projectileMask;
    //private LayerMask alliesMask;
    //private LayerMask enemiesMask;
    //private LayerMask sceneMask;
    private LayerMask targetMask;

    public static Projectile Create(WeaponController weapon)
    {
        Projectile proj = Instantiate(weapon.Values.projectile).GetComponent<Projectile>();
        proj.Init(weapon);
        return proj;
    }

    public void Init(WeaponController weapon)
    {
        values = weapon.Values;
        shotBy = weapon.Unit;

        side = shotBy.Affiliation;
        timeTillDeactivation = values.projectileLifeSpan;
        transform.SetParent(shotBy.ProjectileContainer.transform);

        //projectileMask = LayerMask.GetMask("Projectiles");
        //alliesMask = LayerMask.GetMask("Allies");
        //enemiesMask = LayerMask.GetMask("Enemies");
        //sceneMask = LayerMask.GetMask("Scene");

        if (side == Affiliation.Blue)
        {
            gameObject.layer = LayerMask.NameToLayer("AllyProjectiles");
            targetMask = LayerMask.GetMask("Enemies", "Scene");
        }

        else if (side == Affiliation.Red)
        {
            gameObject.layer = LayerMask.NameToLayer("EnemyProjectiles");
            targetMask = LayerMask.GetMask("Allies", "Scene");
        }
    }

    void Update()
    {
        if (TryProcessHit())
            return;

        if (timeTillDeactivation <= 0)
        {
            timeTillDeactivation = values.projectileLifeSpan;
            gameObject.SetActive(false);
            return;
        }

        Vector3 newPosition = transform.position + transform.forward * values.projectileSpeed * Time.deltaTime;
        transform.position = newPosition;

        timeTillDeactivation -= Time.deltaTime;
    }

    //private void OnTriggerEnter(Collider collider)
    //{
    //    ////int colliderLayer = collider.gameObject.layer;

    //    ////if ((projectileMask & (1 << colliderLayer)) != 0)
    //    ////    return;

    //    ////if ((alliesMask & (1 << colliderLayer)) != 0)
    //    ////{
    //    ////    if (side == AiController.UnitSide.Ally)
    //    ////        return;

    //    ////    else if (side == AiController.UnitSide.Enemy)
    //    ////        collider.GetComponent<AiController>().Damage(this);
    //    ////}

    //    ////else if ((enemiesMask & (1 << colliderLayer)) != 0)
    //    ////{
    //    ////    if (side == AiController.UnitSide.Enemy)
    //    ////        return;

    //    ////    else if (side == AiController.UnitSide.Ally)
    //    ////        collider.GetComponent<AiController>().Damage(this);
    //    ////}

    //    ////gameObject.SetActive(false);

    //    if (!((sceneMask & (1 << collider.gameObject.layer)) != 0))
    //        collider.gameObject.GetComponent<AiController>().Damage(this);

    //    gameObject.SetActive(false);
    //}

    private bool TryProcessHit()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 1, targetMask))
        {
            gameObject.SetActive(false);

            if (hit.collider.gameObject.layer == 10) // Scene layer
                return true;

            // Enemies or Allies layer
            // assuming every script from the layer has the interface
            IInteractable interactable = hit.collider.GetComponent<MonoBehaviour>() as IInteractable;
            interactable.Damage(this);

            return true;
        }

        return false;
    }

    private void OnEnable()
    {
        OnProjectileEnable?.Invoke(this);
    }

    private void OnDisable()
    {
        OnProjectileDisable?.Invoke(this);
    }

    private void OnDestroy()
    {
        OnProjectileDestroy?.Invoke(this);
    }
}
