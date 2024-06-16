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

    public TaskForcePreset(
        GameObject frigatePrefab,
        GameObject destroyerPrefab,
        GameObject cruiserPrefab,
        GameObject battleshipPrefab,
        int frigatesCount,
        int destroyersCount,
        int cruisersCount,
        int battleshipsCount)
    {
        this.frigatesCount = frigatesCount;
        this.destroyersCount = destroyersCount;
        this.cruisersCount = cruisersCount;
        this.battleshipsCount = battleshipsCount;

        this.frigatePrefab = frigatePrefab;
        this.destroyerPrefab = destroyerPrefab;
        this.cruiserPrefab = cruiserPrefab;
        this.battleshipPrefab = battleshipPrefab;

        power = 0;
        metalsPrice = 0;
        crysalsPrice = 0;
        maintenancePrice = 0;

        AiController member;

        if (battleshipsCount > 0 && battleshipPrefab)
        {
            member = battleshipPrefab.GetComponent<AiController>();
            power += member.Values.power * battleshipsCount;
            metalsPrice += member.Values.metalPrice * battleshipsCount;
            crysalsPrice += member.Values.crystalPrice * battleshipsCount;
            maintenancePrice += member.Values.maintenancePrice * battleshipsCount;
        }

        if (cruisersCount > 0 && cruiserPrefab)
        {
            member = cruiserPrefab.GetComponent<AiController>();
            power += member.Values.power * cruisersCount;
            metalsPrice += member.Values.metalPrice * cruisersCount;
            crysalsPrice += member.Values.crystalPrice * cruisersCount;
            maintenancePrice += member.Values.maintenancePrice * cruisersCount;
        }

        if (destroyersCount > 0 && destroyerPrefab)
        {
            member = destroyerPrefab.GetComponent<AiController>();
            power += member.Values.power * destroyersCount;
            metalsPrice += member.Values.metalPrice * destroyersCount;
            crysalsPrice += member.Values.crystalPrice * destroyersCount;
            maintenancePrice += member.Values.maintenancePrice * destroyersCount;
        }

        if (frigatesCount > 0 && frigatePrefab)
        {
            member = frigatePrefab.GetComponent<AiController>();
            power += member.Values.power * frigatesCount;
            metalsPrice += member.Values.metalPrice * frigatesCount;
            crysalsPrice += member.Values.crystalPrice * frigatesCount;
            maintenancePrice += member.Values.maintenancePrice * frigatesCount;
        }
    }
    //public TaskForceController Recruit(EnemyFleetManager manager, Vector3 pos, string name)
    //{
    //    TaskForceController frigates = manager.ProcureTaskForce(manager.frigatePrefab, pos, frigatesCount, name);
    //    TaskForceController destroyers = manager.ProcureTaskForce(manager.destroyerPrefab, pos, destroyersCount, name);
    //    TaskForceController cruisers = manager.ProcureTaskForce(manager.cruiserPrefab, pos, cruisersCount, name);
    //    TaskForceController battleships = manager.ProcureTaskForce(manager.battleshipPrefab, pos, battleshipsCount, name);

    //    List<TaskForceController> forces = new();

    //    if (frigates) forces.Add(frigates);
    //    if (destroyers) forces.Add(destroyers);
    //    if (cruisers) forces.Add(cruisers);
    //    if (battleships) forces.Add(battleships);

    //    return MergeRecursive(forces);
    //}

    //public TaskForceController Recruit(FleetManager manager, Vector3 pos, string name)
    //{
    //    TaskForceController frigates = manager.ProcureTaskForce(frigatePrefab, pos, frigatesCount, name);
    //    TaskForceController destroyers = manager.ProcureTaskForce(destroyerPrefab, pos, destroyersCount, name);
    //    TaskForceController cruisers = manager.ProcureTaskForce(cruiserPrefab, pos, cruisersCount, name);
    //    TaskForceController battleships = manager.ProcureTaskForce(battleshipPrefab, pos, battleshipsCount, name);

    //    List<TaskForceController> forces = new();

    //    if (frigates) forces.Add(frigates);
    //    if (destroyers) forces.Add(destroyers);
    //    if (cruisers) forces.Add(cruisers);
    //    if (battleships) forces.Add(battleships);

    //    return MergeRecursive(forces);
    //}

    //private TaskForceController MergeRecursive(List<TaskForceController> forces)
    //{
    //    if (forces.Count == 1)
    //        return forces[0];

    //    else
    //    {
    //        forces[0].Merge(forces[1]);
    //        forces.RemoveAt(1);
    //        return MergeRecursive(forces);
    //    }
    //}
}
