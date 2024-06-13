using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static TaskForceController;


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

    public void ProcureTaskForce(GameObject unitPrefab, Vector3 position, int count, string name)
    {
        if (resources.CheckIfHavingResources(unitPrefab, count))
        {
            TaskForceController taskForce = spawner.SpawnTaskForce(unitPrefab, position, count, name);
            taskForces.Add(taskForce);
            resources.RemoveResources(unitPrefab, count);
        }
    }

    public void ProcureOutpost(GameObject outpostPrefab, Vector3 position, string name)
    {
        if (resources.CheckIfHavingResources(outpostPrefab))
        {
            Outpost outpost = spawner.SpawnOutpost(outpostPrefab, position, name);
            outposts.Add(outpost);
            resources.RemoveResources(outpostPrefab);
        }
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
}
