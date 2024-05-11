using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using static ProjectileTracker;

public class ProjectileTransformer : MonoBehaviour
{
    // displaying statistics from ProjectileTracker
    public int currentTrackedActive = 0;
    public int currentTrackedAll = 0;
    public int maxTrackedActive = 0;
    public int maxTrackedAll = 0;
    public int registeredEver = 0;

    // must keep track of all created data in each frame, and dispose it at the end of the frame
    private Stack<NativeArray<Vector3>> arraysToDispose = new();

    // Calculator modifies transform.position of each projectile from provided TransformAccessArray, in a single frame
    // Index of given projectile from TransformAccessArray must correspond to its data from projPositions and projForwardVectors
    // projSpeedTimeDelta = projectileSpeed * Time.deltaTime (same value for every projectile from array in a single frame)
    // projPositions = projectiles forward vectors in the frame
    // projForwardVectors = projectiles positions in the frame
    struct TransformCalculator : IJobParallelForTransform
    {
        public float projSpeedTimeDelta;

        [ReadOnly]
        public NativeArray<Vector3> projPositions;

        [ReadOnly]
        public NativeArray<Vector3> projForwardVectors;

        public void Execute(int index, TransformAccess transform)
        {
            Vector3 newPosition = projPositions[index] + projForwardVectors[index] * projSpeedTimeDelta;
            transform.position = newPosition;
        }
    }

    void Update()
    {
        currentTrackedActive = ProjectileTracker.currentTrackedActive;
        currentTrackedAll = ProjectileTracker.currentTrackedAll;
        maxTrackedActive = ProjectileTracker.maxTrackedActive;
        maxTrackedAll = ProjectileTracker.maxTrackedAll;
        registeredEver = ProjectileTracker.registeredEver;

        // each record from ProjectileTracker needs its own handle
        // it is because each record represents different weapon type, that can have different projectile parameters
        // number of records may vary for each frame
        JobHandle[] workload = new JobHandle[weaponsData.Count];

        // creating TransformerCalculator job for every record using its data
        for (int i = 0; i < workload.Length; i++)
        {
            TransformCalculator transformer = new();
            transformer.projSpeedTimeDelta = weaponsData[i].weaponValues.projectileSpeed * Time.deltaTime;

            NativeArray<Vector3> projPositions = new NativeArray<Vector3>(weaponsData[i].activeProjectiles.Count, Allocator.Persistent);
            NativeArray<Vector3> projForwardVectors = new NativeArray<Vector3>(weaponsData[i].activeProjectiles.Count, Allocator.Persistent);

            arraysToDispose.Push(projPositions);
            arraysToDispose.Push(projForwardVectors);

            int j = 0;
            foreach (var proj in weaponsData[i].activeProjectiles)
            {
                projPositions[j] = proj.transform.position;
                projForwardVectors[j] = proj.transform.forward;
                j++;
            }

            transformer.projPositions = projPositions;
            transformer.projForwardVectors = projForwardVectors;

            workload[i] = transformer.Schedule(weaponsData[i].GetTransformAccessArray());
        }

        // has to wait for every handle to complete
        for (int i = 0; i < workload.Length; i++)
        {
            workload[i].Complete();
        }

        for (int i = 0; i < arraysToDispose.Count; i++)
        {
            arraysToDispose.Pop().Dispose();
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < arraysToDispose.Count; i++)
        {
            arraysToDispose.Pop().Dispose();
        }
    }
}
