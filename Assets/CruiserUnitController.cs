using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CruiserUnitController : AiController
{
    [SerializeField] private WeaponController weapon_1;
    [SerializeField] private WeaponController weapon_2;
    [SerializeField] private WeaponController weapon_3;
    [SerializeField] private WeaponController weapon_4;
    [SerializeField] private int health;

    protected override void AdditionalInit()
    {
        weapon_1.Init(this);
        weapon_2.Init(this);
        weapon_3.Init(this);
        weapon_4.Init(this);
    }

    protected override void BeforeDeactivation()
    {

    }

    protected override void CombatState()
    {
        if (Target.distance <= Values.attackDistance)
        {
            if (weapon_2.CheckIfFacingTarget(Target.position))
                weapon_2.FireProjectile();

            if (weapon_3.CheckIfFacingTarget(Target.position))
                weapon_3.FireProjectile();

            if (weapon_4.CheckIfFacingTarget(Target.position))
                weapon_4.FireProjectile();
        }

        if (Target.distance <= Values.attackDistance * 0.5f)
        {
            if (weapon_1.CheckIfFacingTarget(Target.position))
                weapon_1.FireProjectile();
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

        weapon_1.RotateWeaponAtPosition(Target.position);
        weapon_2.RotateWeaponAtPosition(Target.position);
        weapon_3.RotateWeaponAtPosition(Target.position);
        weapon_4.RotateWeaponAtPosition(Target.position);

        if (Target.distance > Values.attackDistance)
        {
            Agent.isStopped = false;
            Agent.SetDestination(Target.position);
        }

        else
            Agent.isStopped = true;
    }

    protected override void RetreatState()
    {

    }

    protected override void UpdateOperations()
    {
        base.UpdateOperations();
        health = Health;

    }

    public override void SetCombatState()
    {
        base.SetCombatState();
    }
    public override void SetIdleState()
    {
        base.SetIdleState();

        Agent.isStopped = false;
    }

    public override void SetMovingState(Vector3 pos)
    {
        base.SetMovingState(pos);

        Agent.isStopped = false;
    }
}
