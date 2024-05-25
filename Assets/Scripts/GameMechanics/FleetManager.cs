using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static TaskForceController;


// klasa odpowiada za zarz�dzanie flot� gracza (wszystkie task forcy)
// mo�na powiedzie� �e to interfejs mi�dzy graczem a spawnerem i nieistniej�cym jeszcze systemem ekonomii
// posiada na razie cz�� zada� spawnera (np. prefaby jednostek), ale to si� zmieni
public class FleetManager : MonoBehaviour
{
    public TaskForceNames taskForceNames;
    public OutpostNames outpostNames;

    public bool debug = false;
    public int targetFrameRate = 0;

    public Spawner spawner; // skrypt odpowiedzialny za instancjonowanie jednostek
    public InputManager inputManager; // skrpyt odpowiedzialny za input gracza
    public Canvas worldSpaceCanvas; // canvas, na kt�rym b�d� renderowane ikony ka�dego taskForca
    public GameObject iconPrefab; // prefab ikony
    public Vector3 iconOffset = new(0, 20, 0); // offset ikony, �eby by�a renderowana troch� wy�ej

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

        // input manager rejestruje r�zne wej�cia, na podstawie kt�rych fleetManager mo�e podejmowa� r�ne akcje
        // ale potrzebne s� wrappery na metody, bo eventy przenosz� RaycastHit, a metody nie potrzebuj� takiego parametru (zazwyczaj)
        // albo potrzebuj� dodatkowych parametr�w
        inputManager.onPlaneLeftClickCtrl.AddListener(ActionSpawnEntity);
        inputManager.onPlaneRightClick.AddListener(ActionSetTaskForceDestinationMultiple);
    }

    // wrappery na metody
    private void ActionSpawnEntity(RaycastHit hit)
    {
        SpawnEntity(hit.point);
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


    // spawner instancjonuje taskForca i jednostki. Listy fleetManagera s� updateowane
    public void SpawnEntity(Vector3 position)
    {
        if (spawner.spawnOutpost)
        {
            spawner.SpawnOutpost(position, outpostNames.GetRandomName(), iconPrefab, worldSpaceCanvas, iconOffset);
            return;
        }

        TaskForceController taskForce = spawner.SpawnTaskForce(position, taskForceNames.GetRandomName(), iconPrefab, worldSpaceCanvas, iconOffset);
        taskForce.onTaskForceDestroyed.AddListener(RemoveTaskForce);

        if (taskForce.Side == Affiliation.Blue)
        {
            allyTaskForceList.Add(taskForce);
            taskForce.onTaskForceDestroyed.AddListener(RemoveTaskForce);
            allyTaskForceCount++;
        }
        else if (taskForce.Side == Affiliation.Red)
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

        if (taskForce.Side == Affiliation.Blue)
            allyTaskForceList.Remove(taskForce);

        else if (taskForce.Side == Affiliation.Red)
            enemyTaskForceList.Remove(taskForce);

        //Destroy(taskForce);
    }

    
}
