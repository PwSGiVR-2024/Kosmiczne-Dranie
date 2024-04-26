using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BasicUnitController : AiController
{
    [SerializeField] private WeaponController weapon_1;

    private void RotateToTarget()
    {
        Vector3 direction = ClosestTargetPosition - transform.position;
        direction.y = 0f;
        direction.Normalize();
        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 10f * Time.deltaTime);
    }

    protected override void AdditionalInit()
    {
        weapon_1.Init(this);
    }

    protected override void CombatState()
    {
        if (TargetDistance <= Values.attackDistance)
        {
            RotateToTarget();

            if (weapon_1.CheckIfFacingTarget(ClosestTargetPastPosition))
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
        Agent.SetDestination(ClosestTargetPosition);
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
}
