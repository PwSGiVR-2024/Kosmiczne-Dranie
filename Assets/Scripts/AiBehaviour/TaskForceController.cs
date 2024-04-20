using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

// Klasa odpowiada za sterowanie ka�d� jednostk� wchodz�c� w jej sk�ad
// Ka�dy Task Force to oddzielny byt, z niezale�nym zachowaniem zdefiniowanym przez jego atrybuty i atrybuty jednostek
// Jednostki mog� wp�ywa� na stan tej klasy
public class TaskForceController : MonoBehaviour
{
    public bool debug = false;

    public GameObject commander; // jednostka dowodz�ca. Potrzebne tak naprawd� tylko dlatego, �eby ikona taskForca mog�a co� followowa�
    public GameObject icon; // atrybuty przypisywane podczas instancjonowania
    public Vector3 iconOffset;

    public float spotDistance = 0;
    public int maxSize;
    public bool friendly; // sojusznik/przeciwnik
    public string taskForceName;
    public float strength = 1.0f; // (0.0 - 1.0) si�a taskForca w %
    public List<GameObject> units = new();
    public List<AiController> controllers = new();// lista jednostek

    // eventy sygnalizuj�ce jakie� zmiany. G��wnie przydatne do UI
    public UnityEvent<int> onSizeChanged = new();
    public UnityEvent<float> onStrengthChanged = new();
    public UnityEvent<int> onPowerChanged = new();
    public UnityEvent<int> onStatusChanged = new();

    // work in progress
    public enum Status { Idle, Moving, Combat, Retreat }
    public Status currentStatus = Status.Idle;
    public enum Behaviour { Passive, Aggresive, Evasive }
    public Behaviour behaviour = Behaviour.Passive;
    public TaskForceController currentTarget;

    //public UnityEvent<int, bool> OnTaskForceDestroyed; // invokowany kiedy lista jednostek b�dzie mia�a d�ugo�� 0. FleetManager musi zaj�� si� wyczyszczeniem wszystkich referencji zwi�zanych z taskForcem je�li zostanie zniszczony 
    public UnityEvent<TaskForceController> onTaskForceDestroyed = new();

    public UnityEvent<TaskForceController, TaskForceController> onEnemyTaskForceSpotted = new();


    //public TaskForceController(string name, int maxSize, bool friendly, GameObject icon, Vector3 iconOffset)
    //{
    //    this.friendly = friendly;
    //    this.name = name;
    //    this.maxSize = maxSize;
    //    this.icon = icon;
    //    this.iconOffset = iconOffset;
    //}

    //public void Init(string name, int maxSize, bool friendly, GameObject icon, Vector3 iconOffset)
    //{
    //    this.friendly = friendly;
    //    this.taskForceName = name;
    //    this.maxSize = maxSize;
    //    this.icon = icon;
    //    this.iconOffset = iconOffset;
    //}

    public LayerMask targetMask;

    public Collider[] colliders = new Collider[1];

    public IEnumerator SpotForTargets()
    {
        if (debug)
            Debug.Log("starting spotting coroutine");

        WaitForSeconds interval = new(0.1f);

        if (commander.CompareTag("Ally"))
            targetMask = LayerMask.GetMask("Enemies");

        else if (commander.CompareTag("Enemy"))
            targetMask = LayerMask.GetMask("Allies");

        while (true)
        {
            if (currentStatus != Status.Combat)
            {
                if (debug)
                    Debug.Log("spotting...");

                int count = Physics.OverlapSphereNonAlloc(commander.transform.position, spotDistance, colliders, targetMask);
                if (count > 0)
                {
                    if (debug)
                        Debug.Log("enemy spotted");

                    TaskForceController enemyTaskForce = colliders[0].gameObject.GetComponent<AiController>().UnitTaskForce;
                    //onTaskForceSpotted?.Invoke(enemyTaskForce);
                    SetTarget(enemyTaskForce);
                }
                else if (count == 0)
                {
                    if (debug)
                        Debug.Log("no enemies spotted");
                }
            }

            else if (currentStatus == Status.Combat)
            {
                if (debug)
                    Debug.Log("combat state, waiting to exit state");
            }

            yield return interval;
        }
    }

    struct DestinationProvider : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Vector3> unitsLocations;

        [ReadOnly]
        public NativeArray<Vector3> potentialTargets;

        public NativeArray<Vector3> outcomeTargets;

        public void Execute(int index)
        {
            float distance = -1;

            for (int j = 0; j < potentialTargets.Length; j++)
            {
                float newDistance = Vector3.Distance(unitsLocations[index], potentialTargets[j]);

                if (distance == -1)
                {
                    distance = newDistance;
                    outcomeTargets[index] = potentialTargets[j];
                }

                else if (newDistance < distance)
                {
                    distance = newDistance;
                    outcomeTargets[index] = potentialTargets[j];
                }

            }

        }
    }

    private IEnumerator RefreshDestination(TaskForceController target)
    {
        if (debug)
            Debug.Log("refresh destination started");

        WaitForSeconds interval = new(0.1f);
        List<AiController> enemies = target.controllers;

        while (currentStatus == Status.Combat)
        {
            if (debug)
                Debug.Log("combat");

            this.ClearDeactivatedUnits();
            target.ClearDeactivatedUnits();

            NativeArray<Vector3> unitsLocations = new(controllers.Count, Allocator.Persistent);
            NativeArray<Vector3> potentialTargets = new(enemies.Count, Allocator.Persistent);
            NativeArray<Vector3> outcomeTargets = new(controllers.Count, Allocator.Persistent);

            for (int i = 0; i < controllers.Count; i++)
            {
                unitsLocations[i] = controllers[i].gameObject.transform.position;
            }

            for (int i = 0; i < enemies.Count; i++)
            {
                potentialTargets[i] = enemies[i].gameObject.transform.position;
            }

            var job = new DestinationProvider
            {
                unitsLocations = unitsLocations,
                potentialTargets = potentialTargets,
                outcomeTargets = outcomeTargets
            };

            JobHandle handle = job.Schedule(controllers.Count, 1);
            handle.Complete();

            for (int i = 0; i < controllers.Count; i++)
            {
                if (controllers[i].gameObject.activeSelf && controllers[i].Agent)
                    controllers[i].SetCombatState(outcomeTargets[i]);
            }

            if (currentTarget == null)
                SetIdleState();

            unitsLocations.Dispose();
            potentialTargets.Dispose();
            outcomeTargets.Dispose();

            yield return interval;
        }

        if (debug)
            Debug.Log("refresh destination stopped");
    }

    public static TaskForceController Create(string name, int maxSize, bool friendly, GameObject icon, Vector3 iconOffset, GameObject container)
    {
        TaskForceController instance = new GameObject(name).AddComponent<TaskForceController>();
        instance.gameObject.transform.SetParent(container.transform);
        instance.taskForceName = name;
        instance.friendly = friendly;
        instance.maxSize = maxSize;
        instance.icon = icon;
        instance.iconOffset = iconOffset;

        //instance.StartCoroutine(instance.SpotForTargets());

        return instance;
    }

    public void SetTarget(TaskForceController taskForce)
    {
        if (currentStatus == Status.Combat)
            return;

        currentStatus = Status.Combat;
        currentTarget = taskForce;

        StartCoroutine(RefreshDestination(taskForce));
        //onEnemyTaskForceSpotted.Invoke(this, taskForce);

        if (debug)
            Debug.Log("task force detected");
    }


    // Wywo�ywane w update() dla ka�dego taskForce (w HUDManager). Ustawia ikon� w odpowiedniej pozycji
    private void UpdateHUD()
    {

        if (commander == null)
            return;

        icon.transform.LookAt(Camera.main.transform, Vector3.up);
        icon.transform.position = commander.transform.position + iconOffset;
    }

    public void Update()
    {
        GameUtils.DrawCircle(gameObject, spotDistance, commander.transform);

        UpdateHUD();
        //ClearDeactivatedUnitsRecursive();

        // Je�li task force jest w walce to zniszczenie dezaktywowanych jednostek musi zosta� wykonane w algorytmie odpowiedzialnym za wyznaczanie cel�w (TaskForceManager.RefreshDestination())
        // Jest tak poniewa� jednostki nie mog� by� null, w czasie wykonywania algorytmu
        // Algorytm sam kontroluje wtedy moment, w kt�rym jednostki mog� zosta� zniszczone
        //if (currentStatus != Status.Combat)
        //    ClearDeactivatedUnitsRecursive();
    }

    public void SetIdleState()
    {
        currentStatus = Status.Idle;



        foreach (var unit in controllers)
        {
            unit.SetIdleState();
        }
    }

    private void FindNextCommanderRecursive(int index)
    {
        if (units.Count == 0)
            return;

        if (commander == units[index] || !units[index].activeSelf || units[index] == null)
        {
            FindNextCommanderRecursive(index + 1);
            return;
        }

        commander = units[index];
    }


    public void AddUnit(GameObject unit)
    {
        units.Add(unit);
        AiController unitController = unit.GetComponent<AiController>();
        unitController.UnitTaskForce = this;
        controllers.Add(unitController);

        //OnSizeChanged?.Invoke(units.Count);

        SetNewSpotDistance();


        if (units.Count == 1)
        {
            commander = unit;
            StartCoroutine(SpotForTargets());
        }
            

        // je�li jednostka zostanie zniszczona, trzeba to odzwierciedli� w tym taskForce
        unitController.onUnitDestroyed.AddListener(RemoveUnitFromTaskForce);
        unitController.onUnitEngaged.AddListener(SetTarget);
    }

    private void SetNewSpotDistance()
    {
        if (units.Count == 0)
            return;

        spotDistance = 0;

        if (units.Count == 1)
        {
            spotDistance = units[0].GetComponent<AiController>().Values.spotDistance;
            return;
        }

        else
        {
            float newSpotDistance;
            foreach (var unit in units)
            {
                newSpotDistance = unit.GetComponent<AiController>().Values.spotDistance;
                if (newSpotDistance > spotDistance)
                    spotDistance = newSpotDistance + (float)Math.Sqrt(units.Count) * 2;
            }
        }
    }

    private void RemoveUnitFromTaskForce(GameObject unit)
    {
        //unit.SetActive(false);

        units.Remove(unit);
        //controllers.Remove(unit.GetComponent<AiController>());

        if (units.Count == 0)
            DestroyTaskForce();

        float unitSpotDistance = unit.GetComponent<AiController>().Values.spotDistance;
        if (unitSpotDistance == spotDistance)
            SetNewSpotDistance();

        if (commander == unit)
            FindNextCommanderRecursive(0);

        UpdateStrength();

        onSizeChanged?.Invoke(units.Count);

        //Destroy(unit);


    }

    private void UpdateStrength()
    {
        strength = (float)units.Count / maxSize;
        onStrengthChanged?.Invoke(strength);
    }

    // jest invokowany event poniewa� FleetManager musi si� zaj�� wyczyszczeniem wszystkich referencji przed zniszczeniem
    // fleetManger subskrybuje ten event kiedy taskForce zostanie stworzony
    public void DestroyTaskForce()
    {
        if (debug)
            Debug.Log("destroying task force " + name);

        onTaskForceDestroyed?.Invoke(this);
        ClearDeactivatedUnits();
        Destroy(icon);
        Destroy(gameObject);
    }

    public void AddDestination(Vector3 destination)
    {

    }

    // dla ka�dej jednostki ustawia stan "Moving" i randomowy destination w zakresie zale�nym od wielko�ci taskForce
    public void SetDestination(Vector3 destination)
    {
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] != null)
                units[i].GetComponent<AiController>().SetMovingState(GameUtils.RandomPlanePositionCircle(destination, Mathf.Sqrt(units.Count) * 2));
        }

        if (commander != null)
            commander.GetComponent<AiController>().Agent.destination = destination;
    }

    public void ResetOrders(Vector3 destination)
    {

    }

    // merguje dwa taskForcy. Wszystkie jednostki z reinforcements s� dodawane do listy, a nast�pnie reinforcements jest niszczone
    public void Merge(TaskForceController reinforcements)
    {
        if (reinforcements == this || reinforcements == null)
            return;

        // potencjalny bug, je�li ilo�� jednostek w do��czanym TaskForce zmniejszy si� w trakcie iteracji
        for (int i = 0; i < reinforcements.units.Count; i++)
        {
            AddUnit(reinforcements.units[i]);
        }

        maxSize = units.Count;
        strength = 1.0f;
        onSizeChanged?.Invoke(units.Count);
        onStrengthChanged?.Invoke(strength);
        reinforcements.DestroyTaskForce();
    }

    //public void DeactivateUnit(AiController unit)
    //{
    //    unit.gameObject.SetActive(false);
    //}

    public void ClearDeactivatedUnits()
    {
        if (debug)
            Debug.Log("clearing units for: " + taskForceName);

        ClearDeactivatedUnitsRecursive(0);
    }

    private void ClearDeactivatedUnitsRecursive(int index)
    {

        if (controllers.Count == 0)
            return;

        if (controllers[index] == null)
        {
            controllers.RemoveAt(index);
            return;
        }

        if (index == controllers.Count - 1)
        {
            if (!controllers[index].gameObject.activeSelf && controllers[index] != null)
            {
                if (debug)
                    Debug.Log("Destroying: " + controllers[index].name);

                Destroy(controllers[index].gameObject);
                //controllers.RemoveAt(index);
                controllers.RemoveAt(index);
                return;
            }
        }

        else
        {
            if (!controllers[index].gameObject.activeSelf && controllers[index] != null)
            {


                Destroy(controllers[index].gameObject);
                //controllers.RemoveAt(index);
                controllers.RemoveAt(index);
                ClearDeactivatedUnitsRecursive(index);
                return;
            }

            ClearDeactivatedUnitsRecursive(index + 1);
        }
    }
}
