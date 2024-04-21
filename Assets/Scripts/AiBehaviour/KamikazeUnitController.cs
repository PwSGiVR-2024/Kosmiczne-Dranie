using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KamikazeUnitController : AiController
{


    protected override void AdditionalInit()
    {

    }

    protected override void CombatState()
    {
        float targetDistance = Vector3.Distance(transform.position, closestTargetPosition);

        if (targetDistance <= Values.attackDistance)
            FireProjectile();
    }

    protected override void IdleState()
    {
    }

    protected override void MovingState()
    {
        if (Agent.hasPath && Agent.remainingDistance <= 10f)
            SetIdleState();
    }

    protected override void OnTargetPositionChanged()
    {
        SetCombatState();
        Agent.SetDestination(closestTargetPosition);
    }
}
