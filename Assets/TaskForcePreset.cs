using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TaskForcePreset
{
    public int power;
    public int metalsPrice;
    public int crysalsPrice;
    public int maintenancePrice;

    public int frigatesCount;
    public int destroyersCount;
    public int cruisersCount;
    public int battleshipsCount;

    public GameObject frigatePrefab;
    public GameObject destroyerPrefab;
    public GameObject cruiserPrefab;
    public GameObject battleshipPrefab;

    public TaskForceController Recruit(EnemyFleetManager manager, Vector3 pos, string name)
    {
        TaskForceController frigates = manager.ProcureTaskForce(manager.frigatePrefab, pos, frigatesCount, name);
        TaskForceController destroyers = manager.ProcureTaskForce(manager.destroyerPrefab, pos, destroyersCount, name);
        TaskForceController cruisers = manager.ProcureTaskForce(manager.cruiserPrefab, pos, cruisersCount, name);
        TaskForceController battleships = manager.ProcureTaskForce(manager.battleshipPrefab, pos, battleshipsCount, name);

        List<TaskForceController> forces = new();

        if (frigates) forces.Add(frigates);
        if (destroyers) forces.Add(destroyers);
        if (cruisers) forces.Add(cruisers);
        if (battleships) forces.Add(battleships);

        return MergeRecursive(forces);
    }

    public TaskForceController Recruit(FleetManager manager, Vector3 pos, string name)
    {
        TaskForceController frigates = manager.ProcureTaskForce(frigatePrefab, pos, frigatesCount, name);
        TaskForceController destroyers = manager.ProcureTaskForce(destroyerPrefab, pos, destroyersCount, name);
        TaskForceController cruisers = manager.ProcureTaskForce(cruiserPrefab, pos, cruisersCount, name);
        TaskForceController battleships = manager.ProcureTaskForce(battleshipPrefab, pos, battleshipsCount, name);

        List<TaskForceController> forces = new();

        if (frigates) forces.Add(frigates);
        if (destroyers) forces.Add(destroyers);
        if (cruisers) forces.Add(cruisers);
        if (battleships) forces.Add(battleships);

        return MergeRecursive(forces);
    }

    private TaskForceController MergeRecursive(List<TaskForceController> forces)
    {
        if (forces.Count == 1)
            return forces[0];

        else
        {
            forces[0].Merge(forces[1]);
            forces.RemoveAt(1);
            return MergeRecursive(forces);
        }
    }
}
