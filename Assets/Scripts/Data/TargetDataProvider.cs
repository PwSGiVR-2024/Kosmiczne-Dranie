using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile(FloatPrecision.Low, FloatMode.Fast)]
public struct TargetDataProvider : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<JobUnitData> enemies;

    [ReadOnly]
    public NativeArray<JobUnitData> allies;

    public NativeArray<TargetDataLite> outcomeTargets;

    public void Execute(int allyIndex)
    {
        float distance = float.PositiveInfinity;
        float newDistance;
        int closestEnemy = 0;

        for (int enemyIndex = 0; enemyIndex < enemies.Length; enemyIndex++)
        {
            //newDistance = Vector3.Distance(allies[allyIndex].position, enemies[enemyIndex].position);
            newDistance = math.distance(allies[allyIndex].position, enemies[enemyIndex].position);

            if (newDistance < distance)
            {
                distance = newDistance;
                closestEnemy = enemyIndex;
            }
        }

        float angleRadians = math.acos(math.dot(allies[allyIndex].forward, math.normalize(enemies[closestEnemy].position - allies[allyIndex].position)));
        float angleDegrees = math.degrees(angleRadians);

        outcomeTargets[allyIndex] = new TargetDataLite(
            position: enemies[closestEnemy].position,
            rotation: enemies[closestEnemy].rotation,
            forward: enemies[closestEnemy].forward,
            distance: distance,
            //angle: Vector3.Angle(allies[allyIndex].forward, enemies[closestEnemy].position - allies[allyIndex].position)
            angle: angleDegrees
            );
    }
}
