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

// ka¿dy pocisk posiada ten komponent
// klasa definiuje zachowanie pocisku i wszytskie jego atrybuty
public class Projectile : MonoBehaviour
{
    private AiController.UnitSide side;
    private AiController shotBy;
    private WeaponValues values;
    private float timeTillDeactivation;

    public WeaponValues Values { get => values; }
    public AiController ShotBy { get => shotBy; }
    public AiController.UnitSide Side { get => side; }

    private LayerMask projectileMask;
    private LayerMask alliesMask;
    private LayerMask enemiesMask;

    //private TransformAccessArray thisTransform;

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

        side = shotBy.Side;
        timeTillDeactivation = values.projectileLifeSpan;
        transform.SetParent(shotBy.ProjectileContainer.transform);

        projectileMask = LayerMask.GetMask("Projectiles");
        alliesMask = LayerMask.GetMask("Allies");
        enemiesMask = LayerMask.GetMask("Enemies");

        //thisTransform = new(1);
        //thisTransform.Add(transform);
    }

    void Update()
    {
        if (timeTillDeactivation <= 0)
        {
            timeTillDeactivation = values.projectileLifeSpan;
            gameObject.SetActive(false);
            return;
        }

        //var job = new TransformCalculator
        //{
        //    deltaTime = Time.deltaTime,
        //    speed = values.projectileSpeed,
        //    position = transform.position,
        //    forwardVector = transform.forward,
        //};

        //JobHandle handle = job.Schedule(thisTransform);
        //handle.Complete();

        Vector3 newPosition = transform.position + transform.forward * values.projectileSpeed * Time.deltaTime;
        transform.position = newPosition;

        timeTillDeactivation -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider collider)
    {
        int colliderLayer = collider.gameObject.layer;

        if ((projectileMask & (1 << colliderLayer)) != 0)
            return;

        if ((alliesMask & (1 << colliderLayer)) != 0)
        {
            if (side == AiController.UnitSide.Ally)
                return;

            else if (side == AiController.UnitSide.Enemy)
                collider.GetComponent<AiController>().Damage(this);
        }

        else if ((enemiesMask & (1 << colliderLayer)) != 0)
        {
            if (side == AiController.UnitSide.Enemy)
                return;

            else if (side == AiController.UnitSide.Ally)
                collider.GetComponent<AiController>().Damage(this);
        }

        gameObject.SetActive(false);
    }

    //struct TransformCalculator : IJobParallelForTransform
    //{
    //    public float deltaTime;
    //    public int speed;
    //    public Vector3 position;
    //    public Vector3 forwardVector;

    //    public void Execute(int index, TransformAccess transform)
    //    {
    //        Vector3 newPosition = position + forwardVector * speed * deltaTime;
    //        transform.position = newPosition;
    //    }
    //}
}
