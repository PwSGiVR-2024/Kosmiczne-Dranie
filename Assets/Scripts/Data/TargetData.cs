using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public struct TargetData
{
    public float dataTimeStamp { get; private set; }
    public AiController targetController { get; private set; }
    public Vector3 pastPosition { get; private set; }
    public Vector3 position { get; private set; }
    public float distance { get; private set; }
    public float angle { get; private set; }
    public float relativeVelocity { get; private set; }
    public Vector3 forward { get; private set; }

    public void UpdateData(Vector3 position, float distance, float angle, Vector3 forward)
    {
        this.position = position;
        this.distance = distance;
        this.angle = angle;
        this.forward = forward;
    }

    public bool TryLockTarget(out AiController controller)
    {
        Ray ray = new Ray(new Vector3(position.x, -1, position.z), Vector3.up);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            targetController = hit.collider.gameObject.GetComponent<AiController>();
            controller = targetController;
            return true;
        }

        controller = targetController;
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
