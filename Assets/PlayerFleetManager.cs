using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerFleetManager : FleetManager
{
    public FleetPanelController fleetPanelController;
    public InputManager inputManager;
    public TaskForceNames taskForceNames;
    public OutpostNames outpostNames;

    [Header("Enables spawning enemy units")]
    public bool debugMode = true;
    public EnemyFleetManager enemyFleetManager;
    public GameObject prefabToSpawn;
    public int spawnCount;

    protected override void Start()
    {
        base.Start();
        inputManager.onPlaneLeftClickCtrl.AddListener((hit) => ClickAction(hit.point));
        inputManager.onPlaneRightClick.AddListener((hit) => SetTaskForceDestinationMultiple(hit.point, fleetPanelController.selectedTaskForces));
    }

    private void ClickAction(Vector3 position)
    {
        if (debugMode && prefabToSpawn)
        {
            DebugSpawn(position);
            return;
        }

        if (fleetPanelController.spawnOutpost && CheckIfPositionViable(position, fleetPanelController.buttonSelectOutpost.unitPrefab.GetComponent<Outpost>()))
            ProcureOutpost(fleetPanelController.buttonSelectOutpost.unitPrefab, position, outpostNames.GetRandomName());

        else if (CheckIfPositionViable(position, fleetPanelController.currentPreset))
            ProcureTaskForce(fleetPanelController.currentPreset, position, taskForceNames.GetRandomName());



        //if (fleetPanelController.currentSelection.unitPrefab == null)
        //    return;

            //if (CheckIfPositionViable(position, fleetPanelController.currentSelection.unitPrefab))
            //{
            //    if (fleetPanelController.currentSelection.isOutpost)
            //        ProcureOutpost(fleetPanelController.currentSelection.unitPrefab, position, outpostNames.GetRandomName());

            //    else ProcureTaskForce(fleetPanelController.currentSelection.unitPrefab, position, fleetPanelController.currentSelection.selectedCount, taskForceNames.GetRandomName());
            //}
    }

    private bool CheckIfPositionViable(Vector3 pos, Outpost outpostToSpawn)
    {
        if (Vector3.Distance(pos, headquarters.transform.position) <= headquarters.range + outpostToSpawn.values.range)
            return true;

        foreach (Outpost outpost in outposts)
        {
            if (Vector3.Distance(pos, outpost.transform.position) <= outpost.range + outpostToSpawn.values.range)
                return true;
        }
        return false;
    }

    private bool CheckIfPositionViable(Vector3 pos, TaskForcePreset preset)
    {
        if (Vector3.Distance(pos, headquarters.transform.position) <= headquarters.range)
            return true;

        foreach (Outpost outpost in outposts)
        {
            if (Vector3.Distance(pos, outpost.transform.position) <= outpost.range)
                return true;
        }
        return false;
    }

    private void DebugSpawn(Vector3 pos)
    {
        if (prefabToSpawn.TryGetComponent(out Outpost _))
            enemyFleetManager.ProcureOutpost(prefabToSpawn, pos, "debug_outpost");

        else if (prefabToSpawn.TryGetComponent(out AiController _))
            enemyFleetManager.ProcureTaskForce(prefabToSpawn, pos, spawnCount, "debug_task_force");
    }
}
