using System.Collections.Generic;
using UnityEngine;


public abstract class FleetManager : MonoBehaviour
{
    public Spawner spawner;
    
    public List<TaskForceController> taskForces = new();
    public List<Outpost> outposts = new();

    public int cumulativePower = 0;
    public int frigateCount = 0;
    public int destroyerCount = 0;
    public int cruiserCount = 0;
    public int battleshipCount = 0;

    public ResourceManager resources;
    public Headquarters headquarters;

    protected virtual void Start()
    {
        spawner.onUnitSpawned.AddListener(RegisterUnit);
    }

    private void RegisterUnit(AiController unit)
    {
        switch (unit.Values.shipClass)
        {
            case ShipClass.Frigate:
                frigateCount++;
                break;

            case ShipClass.Destroyer:
                destroyerCount++;
                break;

            case ShipClass.Cruiser:
                cruiserCount++;
                break;

            case ShipClass.Battleship:
                battleshipCount++;
                break;
        }

        unit.onUnitNeutralized.AddListener(UnregisterUnit);
        cumulativePower += unit.Values.power;
    }

    private void UnregisterUnit(AiController unit)
    {
        switch (unit.Values.shipClass)
        {
            case ShipClass.Frigate:
                frigateCount--;
                break;

            case ShipClass.Destroyer:
                destroyerCount--;
                break;

            case ShipClass.Cruiser:
                cruiserCount--;
                break;

            case ShipClass.Battleship:
                battleshipCount--;
                break;
        }

        cumulativePower -= unit.Values.power;
    }

    public TaskForceController ProcureTaskForce(GameObject unitPrefab, Vector3 position, int count, string name)
    {
        if (resources.CheckIfHavingResources(unitPrefab, count))
        {
            TaskForceController taskForce = spawner.SpawnTaskForce(unitPrefab, position, count, name);
            taskForces.Add(taskForce);
            resources.RemoveResources(unitPrefab, count);
            return taskForce;
        }

        return null;
    }

    public TaskForceController ProcureTaskForce(TaskForcePreset preset, Vector3 position, string name)
    {
        if (resources.CheckIfHavingResources(preset))
        {
            TaskForceController taskForce = spawner.SpawnTaskForce(preset, position, name);
            taskForces.Add(taskForce);
            resources.RemoveResources(preset);
            taskForce.onTaskForceDestroyed.AddListener((tf) => resources.RemoveMaintenance(preset.maintenancePrice));
            return taskForce;
        }

        return null;
    }

    public Outpost ProcureOutpost(GameObject outpostPrefab, Vector3 position, string name)
    {
        if (resources.CheckIfHavingResources(outpostPrefab))
        {
            Outpost outpost = spawner.SpawnOutpost(outpostPrefab, position, name);
            outposts.Add(outpost);
            resources.RemoveResources(outpostPrefab);
            return outpost;
        }

        return null;
    }

    public void SetTaskForceDestinationMultiple(Vector3 destination, List<TaskForceController> forces)
    {
        if (forces.Count == 0)
            return;

        foreach (var taskForce in forces)
        {
            if (taskForce != null)
                taskForce.SetDestination(GameUtils.RandomPlanePositionCircle(destination, forces.Count * 5));
        }
    }

    public void SplitTaskForce(TaskForceController mother, out TaskForceController newTaskForce)
    {
        newTaskForce = TaskForceController.Create(mother.gameObject.name + "_split", mother.gameManager, mother.Affiliation);
        newTaskForce.transform.SetParent(mother.transform.parent);

        int addCountMax = mother.battleships.Count / 2;
        int addCountCurrent = 0;
        foreach (var battleship in mother.battleships)
        {
            newTaskForce.AddUnit(battleship);
            addCountCurrent++;

            if (addCountCurrent == addCountMax)
                break;
        }
        newTaskForce.battleshipsRDY = true;

        addCountMax = mother.cruisers.Count / 2;
        addCountCurrent = 0;
        foreach (var cruiser in mother.cruisers)
        {
            newTaskForce.AddUnit(cruiser);
            addCountCurrent++;

            if (addCountCurrent == addCountMax)
                break;
        }
        newTaskForce.cruisersRDY = true;

        addCountMax = mother.destroyers.Count / 2;
        addCountCurrent = 0;
        foreach (var destroyer in mother.destroyers)
        {
            newTaskForce.AddUnit(destroyer);
            addCountCurrent++;

            if (addCountCurrent == addCountMax)
                break;
        }
        newTaskForce.destroyersRDY = true;

        addCountMax = mother.frigates.Count / 2;
        addCountCurrent = 0;
        foreach (var frigate in mother.frigates)
        {
            newTaskForce.AddUnit(frigate);
            addCountCurrent++;

            if (addCountCurrent == addCountMax)
                break;
        }
        newTaskForce.frigatesRDY = true;

        foreach (var unit in newTaskForce.Units)
        {
            mother.RemoveUnit(unit);
        }

        spawner.AddLineRenderer(newTaskForce.gameObject, spawner.taskForceLineRendererMaterial);
        HUDController.Create(newTaskForce, spawner.taskForceHUD, mother.gameManager);

        spawner.onTaskForceSpawned.Invoke(newTaskForce);
    }
}
