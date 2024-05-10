using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualWeaponUnitController : AiController
{
    [SerializeField] private WeaponController weapon_1;
    [SerializeField] private WeaponController weapon_2;

    //debug
    [SerializeField] private Projectile[] wep1;
    [SerializeField] private Projectile[] wep2;

    [SerializeField] private bool enablePoolLogging = false;

    private void RotateToTarget()
    {
        Vector3 direction = Target.position - transform.position;
        direction.y = 0f;
        direction.Normalize();
        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 10f * Time.deltaTime);
    }

    protected override void AdditionalInit()
    {
        weapon_1.Init(this);
        weapon_2.Init(this);

        wep1 = weapon_1.Pool.Projectiles;
        wep2 = weapon_2.Pool.Projectiles;
    }

    protected override void CombatState()
    {
        if (Target.distance <= Values.attackDistance)
        {
            RotateToTarget();

            if (GameUtils.CalculateForwardAngle(transform, Target.position) <= weapon_1.Values.angleError)
            {
                weapon_1.FireProjectile();
                weapon_2.FireProjectile();
            }
        }
    }

    protected override void IdleState()
    {

    }

    protected override void MovingState()
    {
        if (Agent.hasPath && Agent.remainingDistance <= 1f)
            SetIdleState();
    }

    protected override void OnTargetPositionChanged()
    {
        SetCombatState();
        Agent.SetDestination(Target.position);
    }

    public override void SetCombatState()
    {
        base.SetCombatState();
    }
    public override void SetIdleState()
    {
        base.SetIdleState();
    }

    public override void SetMovingState(Vector3 pos)
    {
        base.SetMovingState(pos);
    }

    protected override void BeforeDeactivation()
    {

    }

    protected override void UpdateOperations()
    {
        base.UpdateOperations();

        if (enablePoolLogging)
            weapon_1.Pool.EnableLogging();
        else
            weapon_1.Pool.DisableLogging();
        
    }

    protected override void RetreatState()
    {
        throw new System.NotImplementedException();
    }
}
