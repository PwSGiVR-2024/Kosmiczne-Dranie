using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static TaskForceController;


// klasa odpowiada za zarz¹dzanie flot¹ gracza (wszystkie task forcy)
// mo¿na powiedzieæ ¿e to interfejs miêdzy graczem a spawnerem i nieistniej¹cym jeszcze systemem ekonomii
// posiada na razie czêœæ zadañ spawnera (np. prefaby jednostek), ale to siê zmieni
public class FleetManager : MonoBehaviour
{
    public bool devMode = true;

    public Spawner spawner; // skrypt odpowiedzialny za instancjonowanie jednostek
    public InputManager inputManager; // skrpyt odpowiedzialny za input gracza\

    public List<TaskForceController> allyTaskForces = new();
    public List<Outpost> allyOutposts = new();

    public FleetPanelController fleetPanelController;

    private void Start()
    {
        inputManager.onPlaneLeftClickCtrl.AddListener(ActionSpawn);
        inputManager.onPlaneRightClick.AddListener((hit) => SetTaskForceDestinationMultiple(hit.point, fleetPanelController.selectedTaskForces));
    }

    private void ActionSpawn(RaycastHit hit)
    {
        Object obj = spawner.SpawnEntity(hit.point);

        if (obj is TaskForceController tf)
        {
            if (tf.Affiliation == Affiliation.Blue)
            {
                allyTaskForces.Add(tf);
                tf.onTaskForceDestroyed.AddListener((force) => allyTaskForces.Remove(force));
            }
        }

        else if (obj is Outpost op)
        {
            if (op.Affiliation == Affiliation.Blue)
            {
                allyOutposts.Add(op);
                op.onOutpostDestroy.AddListener(() => allyOutposts.Remove(op));
            }
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
