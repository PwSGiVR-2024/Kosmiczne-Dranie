using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHeadquarters : Headquarters
{
    GameManager manager;
    FleetManager enemyFleetManager;

    private enum TargetPriority { Low, Medium, High }
    private enum Objective { Defend, Attack }
    private enum Behaviour { Defensive, Offensive }

    public List<TaskForceController> taskForces = new();

    private void Start()
    {
        spawner.onEnemyOutpostSpawned.AddListener((outpost) => outpostNetwork.Add(outpost));
    }

    private void EvaluateStatus()
    {



    }
}
