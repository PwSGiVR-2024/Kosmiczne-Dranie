using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BasicUnitController : AiController
{
    /// <summary>
    /// Kalibruje stoppingDistance Agenta na podstawie relatywnej prêdkoœci do namierzonego celu. Zapobiega zbytniemu zbli¿eniu siê do celu, jeœli obie jednostki zmierzaj¹ naprzeciwko siebie.
    /// </summary>
    private void CalibrateStoppingDistance()
    {
        if (Target == null)
            return;

        Vector3 velocityVector = Agent.velocity - Target.Agent.velocity;
        targetRelativeVelocity = velocityVector.magnitude;
        Agent.stoppingDistance = targetRelativeVelocity * 2;
    }

    protected override void AdditionalInit()
    {

    }

    protected override void CombatState()
    {
        CalibrateStoppingDistance();

        targetDistance = Vector3.Distance(transform.position, Agent.destination);

        if (targetDistance <= Values.attackDistance)
        {
            Agent.speed = 0;

            Vector3 direction = closestTargetPosition - transform.position;
            direction.y = 0f;
            direction.Normalize();
            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 10f * Time.deltaTime);
            
            FireProjectile();
        }
        else
        {
            Agent.speed = Values.unitSpeed;
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
        Agent.SetDestination(closestTargetPosition);
    }

    protected override void FireProjectile()
    {
        base.FireProjectile();
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

}
