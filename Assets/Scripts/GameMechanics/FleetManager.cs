using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


// klasa odpowiada za zarz¹dzanie flot¹ gracza (wszystkie task forcy)
// mo¿na powiedzieæ ¿e to interfejs miêdzy graczem a spawnerem i nieistniej¹cym jeszcze systemem ekonomii
// posiada na razie czêœæ zadañ spawnera (np. prefaby jednostek), ale to siê zmieni
public class FleetManager : MonoBehaviour
{
    public bool debug = false;
    public int targetFrameRate = 0;

    public Spawner spawner; // skrypt odpowiedzialny za instancjonowanie jednostek
    public InputManager inputManager; // skrpyt odpowiedzialny za input gracza
    public Canvas worldSpaceCanvas; // canvas, na którym bêd¹ renderowane ikony ka¿dego taskForca
    public GameObject iconPrefab; // prefab ikony
    public Vector3 iconOffset = new(0, 20, 0); // offset ikony, ¿eby by³a renderowana trochê wy¿ej

    public enum UnitType { Allrounder,Sniper, Tank, Potato, Kamikaze, Enemy };
    public UnitType unitToSpawn = UnitType.Allrounder;
    public int unitsToSpawn = 0;
    public string taskForceName;

    private int allyTaskForceCount = 0;
    private int enemyTaskForceCount = 0;
    public List<TaskForceController> allyTaskForceList = new();
    public List<TaskForceController> enemyTaskForceList = new();

    public FleetPanelController fleetPanelController;



    private void Start()
    {
        if (targetFrameRate > 0)
        {
            Application.targetFrameRate = targetFrameRate;
        }

        // input manager rejestruje rózne wejœcia, na podstawie których fleetManager mo¿e podejmowaæ ró¿ne akcje
        // ale potrzebne s¹ wrappery na metody, bo eventy przenosz¹ RaycastHit, a metody nie potrzebuj¹ takiego parametru (zazwyczaj)
        // albo potrzebuj¹ dodatkowych parametrów
        inputManager.onPlaneLeftClickCtrl.AddListener(ActionSpawnTaskForce);
        inputManager.onPlaneRightClick.AddListener(ActionSetTaskForceDestinationMultiple);
    }

    // wrappery na metody
    private void ActionSpawnTaskForce(RaycastHit hit)
    {
        SpawnTaskForce(hit.point);
    }

    private void ActionSetTaskForceDestinationMultiple(RaycastHit hit)
    {
        SetTaskForceDestinationMultiple(hit.point, fleetPanelController.selectedTaskForces);
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


    // spawner instancjonuje taskForca i jednostki. Listy fleetManagera s¹ updateowane
    public void SpawnTaskForce(Vector3 position)
    {
        TaskForceController taskForce = spawner.SpawnTaskForce(position, taskForceName, iconPrefab, worldSpaceCanvas, iconOffset);
        taskForce.onTaskForceDestroyed.AddListener(RemoveTaskForce);

        if (taskForce.friendly)
        {
            allyTaskForceList.Add(taskForce);
            taskForce.onTaskForceDestroyed.AddListener(RemoveTaskForce);
            allyTaskForceCount++;
        }
        else if (!taskForce.friendly)
        {
            enemyTaskForceList.Add(taskForce);
            taskForce.onTaskForceDestroyed.AddListener(RemoveTaskForce);
            enemyTaskForceCount++;
        }
    }


    private void RemoveTaskForce(TaskForceController taskForce)
    {
        if (taskForce == null)
            return;

        //Destroy(taskForce.icon);

        if (taskForce.friendly)
            allyTaskForceList.Remove(taskForce);

        else if (!taskForce.friendly)
            enemyTaskForceList.Remove(taskForce);

        //Destroy(taskForce);
    }

    
}
