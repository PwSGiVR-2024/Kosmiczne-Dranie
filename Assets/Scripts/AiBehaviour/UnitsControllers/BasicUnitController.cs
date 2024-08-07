using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BasicUnitController : AiController
{
    [SerializeField] private WeaponController weapon_1;
    [SerializeField] private Vector3 targetPos;
    [SerializeField] private float targetDistance;

    private void RotateToTarget()
    {
        if (Target.angle == 0)
            return;

        Vector3 direction = Target.position - transform.position;
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
        if (Target.distance <= Values.attackDistance)
        {
            RotateToTarget();

            if (Target.angle <= weapon_1.Values.angleError)
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

        if (Target.distance > Values.attackDistance)
        {
            Agent.isStopped = false;
            Agent.SetDestination(Target.position);
        }
           
        else
            Agent.isStopped = true;
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

    protected override void BeforeDeactivation()
    {
        
    }

    protected override void RetreatState()
    {
        throw new System.NotImplementedException();
    }

    protected override void UpdateOperations()
    {
        base.UpdateOperations();

        targetPos = Target.position;
        targetDistance = Target.distance;

        if (debug)
        {
            Debug.DrawRay(targetPos, Vector3.up + new Vector3(0, 5, 0), Color.green, Time.deltaTime);
        }
    }
}
