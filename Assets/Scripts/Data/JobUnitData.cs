using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct JobUnitData
{
    public float3 position { get; private set; }
    public Quaternion rotation { get; private set; }
    public float3 forward { get; private set; }

    public JobUnitData(Vector3 position, Quaternion rotation, Vector3 forward)
    {
        this.position = position;
        this.rotation = rotation;
        this.forward = forward;
    }
}
