using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.AI;

public struct TargetData
{
    // public float dataTimeStamp { get; private set; }
    private AiController targetController;
    public Vector3 pastPosition { get; private set; }
    public Vector3 position { get; private set; }
    public float distance { get; private set; }
    public float angle { get; private set; }
    public float relativeVelocity { get; private set; }
    public Vector3 forward { get; private set; }

    public int targetLayer;
    private Collider[] targetCollider;

    public TargetData(int targetLayer)
    {
        // dataTimeStamp = 0;
        targetController = null;
        pastPosition = Vector3.zero;
        position = Vector3.zero;
        distance = 0;
        angle = 0;
        relativeVelocity = 0;
        forward = Vector3.zero;
        this.targetLayer = 1 << targetLayer;
        targetCollider = new Collider[1];
    }

    public void UpdateData(Vector3 position, float distance, float angle, Vector3 forward)
    {
        this.position = position;
        this.distance = distance;
        this.angle = angle;
        this.forward = forward;
    }

    public bool TryLockTarget(out AiController controller)
    {
        if (targetController)
        {
            controller = targetController;
            return true;
        }

        int colliderCount = Physics.OverlapSphereNonAlloc(position, 0.01f, targetCollider, targetLayer);

        if (colliderCount > 0)
        {
            targetController = targetCollider[0].GetComponent<AiController>();
            controller = targetController;
            return true;
        }

        controller = null;
        return false;
    }

    public bool TryLockTarget()
    {
        if (targetController)
            return true;

        int colliderCount = Physics.OverlapSphereNonAlloc(position, 0.01f, targetCollider, targetLayer);

        if (colliderCount > 0)
        {
            targetController = targetCollider[0].GetComponent<AiController>();
            return true;
        }

        return false;
    }

    public float CalculateTargetRelativeVelocity(NavMeshAgent agent)
    {
        if (targetController)
        {
            Vector3 velocityVector = agent.velocity - targetController.Agent.velocity;
            return velocityVector.magnitude;
        }

        return -1;
    }
}

public struct TargetDataLite
{
    public Vector3 position { get; private set; }
    public Quaternion rotation { get; private set; }
    public Vector3 forward { get; private set; }
    public float distance { get; private set; }
    public float angle { get; private set; }

    public TargetDataLite(Vector3 position, Quaternion rotation, Vector3 forward, float distance, float angle)
    {
        this.position = position;
        this.rotation = rotation;
        this.forward = forward;
        this.distance = distance;
        this.angle = angle;
    }
}
