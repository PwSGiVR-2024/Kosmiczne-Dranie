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

// Klasa odpowiada za sterowanie ka¿d¹ jednostk¹ wchodz¹c¹ w jej sk³ad
// Ka¿dy Task Force to oddzielny byt, z niezale¿nym zachowaniem zdefiniowanym przez jego atrybuty i atrybuty jednostek
// Jednostki mog¹ wp³ywaæ na stan tej klasy
public class TaskForceController : MonoBehaviour
{
    public GameManager gameManager;
    public bool debug = false;

    public AiController commander;
    //public GameObject commander; // jednostka dowodz¹ca. Potrzebne tak naprawdê tylko dlatego, ¿eby ikona taskForca mog³a coœ followowaæ
    public GameObject icon; // atrybuty przypisywane podczas instancjonowania
    public Vector3 iconOffset;

    public float spotDistance = 0;
    public int maxSize;
    public bool friendly; // sojusznik/przeciwnik
    public string taskForceName;
    public float strength = 1.0f; // (0.0 - 1.0) si³a taskForca w %
    //public List<GameObject> units = new();
    public List<AiController> controllers = new();// lista jednostek

    // eventy sygnalizuj¹ce jakieœ zmiany. G³ównie przydatne do UI
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

    //public UnityEvent<int, bool> OnTaskForceDestroyed; // invokowany kiedy lista jednostek bêdzie mia³a d³ugoœæ 0. FleetManager musi zaj¹æ siê wyczyszczeniem wszystkich referencji zwi¹zanych z taskForcem jeœli zostanie zniszczony 
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
            if (commander == null)
            {
                yield return interval;
            }

            if (currentStatus != Status.Combat)
            {
                if (debug)
                    Debug.Log("spotting...");

                int count = Physics.OverlapSphereNonAlloc(commander.transform.position, spotDistance + (float)Math.Sqrt(controllers.Count) * commander.Volume, colliders, targetMask);
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
                unitsLocations[i] = controllers[i].transform.position;
            }

            for (int i = 0; i < enemies.Count; i++)
            {
                potentialTargets[i] = enemies[i].transform.position;
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
                if (controllers[i].gameObject.activeSelf)
                    controllers[i].SetTargetPosition(outcomeTargets[i]);
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

    public static TaskForceController Create(string name, int maxSize, bool friendly, GameObject icon, Vector3 iconOffset, GameObject container, GameManager gameManager)
    {
        TaskForceController instance = new GameObject(name).AddComponent<TaskForceController>();
        instance.gameObject.transform.SetParent(container.transform);
        instance.taskForceName = name;
        instance.friendly = friendly;
        instance.maxSize = maxSize;
        instance.icon = icon;
        instance.iconOffset = iconOffset;
        instance.gameManager = gameManager;

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


    // Wywo³ywane w update() dla ka¿dego taskForce (w HUDManager). Ustawia ikonê w odpowiedniej pozycji
    private void UpdateHUD()
    {

        if (commander == null)
            return;

        icon.transform.LookAt(Camera.main.transform, Vector3.up);
        icon.transform.position = commander.transform.position + iconOffset;
    }

    public void Update()
    {
        if (commander)
            GameUtils.DrawCircle(gameObject, spotDistance + (float)Math.Sqrt(controllers.Count) * commander.Volume, commander.transform);

        UpdateHUD();
        //ClearDeactivatedUnitsRecursive();

        // Jeœli task force jest w walce to zniszczenie dezaktywowanych jednostek musi zostaæ wykonane w algorytmie odpowiedzialnym za wyznaczanie celów (TaskForceManager.RefreshDestination())
        // Jest tak poniewa¿ jednostki nie mog¹ byæ null, w czasie wykonywania algorytmu
        // Algorytm sam kontroluje wtedy moment, w którym jednostki mog¹ zostaæ zniszczone
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
        if (controllers.Count == 0)
            return;

        if (commander == controllers[index] || !controllers[index].gameObject.activeSelf || controllers[index].gameObject == null)
        {
            FindNextCommanderRecursive(index + 1);
            return;
        }

        commander = controllers[index];
    }


    public void AddUnit(AiController unit)
    {
        AiController unitController = unit.GetComponent<AiController>();
        unitController.UnitTaskForce = this;
        controllers.Add(unitController);

        //OnSizeChanged?.Invoke(units.Count);

        SetNewSpotDistance();


        if (controllers.Count == 1)
        {
            commander = unitController;
            StartCoroutine(SpotForTargets());
        }
            

        // jeœli jednostka zostanie zniszczona, trzeba to odzwierciedliæ w tym taskForce
        unitController.onUnitNeutralized.AddListener(RemoveUnitFromTaskForce);
        unitController.onUnitEngaged.AddListener(SetTarget);
    }

    private void SetNewSpotDistance()
    {
        if (controllers.Count == 0)
            return;

        spotDistance = 0;

        if (controllers.Count == 1)
        {
            spotDistance = controllers[0].Values.spotDistance;
            return;
        }

        else
        {
            float newSpotDistance;
            foreach (var unit in controllers)
            {
                newSpotDistance = unit.Values.spotDistance;
                if (newSpotDistance > spotDistance)
                    spotDistance = newSpotDistance;
            }
        }
    }

    private void RemoveUnitFromTaskForce(AiController unit)
    {
        gameManager.AddToExterminationCamp(unit.gameObject);
        //unit.SetActive(false);

        controllers.Remove(unit);
        //controllers.Remove(unit.GetComponent<AiController>());

        if (controllers.Count == 0)
            DestroyTaskForce();

        float unitSpotDistance = unit.Values.spotDistance;
        if (unitSpotDistance == spotDistance)
            SetNewSpotDistance();

        if (commander == unit)
            FindNextCommanderRecursive(0);

        UpdateStrength();

        onSizeChanged?.Invoke(controllers.Count);

        //Destroy(unit);


    }

    private void UpdateStrength()
    {
        strength = (float)controllers.Count / maxSize;
        onStrengthChanged?.Invoke(strength);
    }

    // jest invokowany event poniewa¿ FleetManager musi siê zaj¹æ wyczyszczeniem wszystkich referencji przed zniszczeniem
    // fleetManger subskrybuje ten event kiedy taskForce zostanie stworzony
    public void DestroyTaskForce()
    {
        if (debug)
            Debug.Log("destroying task force " + name);

        StopAllCoroutines();
        onTaskForceDestroyed?.Invoke(this);
        ClearDeactivatedUnits();
        Destroy(icon);
        Destroy(gameObject);
    }

    public void AddDestination(Vector3 destination)
    {

    }

    // dla ka¿dej jednostki ustawia stan "Moving" i randomowy destination w zakresie zale¿nym od wielkoœci taskForce
    public void SetDestination(Vector3 destination)
    {
        for (int i = 0; i < controllers.Count; i++)
        {
            if (controllers[i] != null)
                controllers[i].SetMovingState(GameUtils.RandomPlanePositionCircle(destination, Mathf.Sqrt(controllers.Count) * commander.Volume));
        }

        if (commander != null)
            commander.GetComponent<AiController>().SetMovingState(destination);
    }

    public void ResetOrders(Vector3 destination)
    {

    }

    // merguje dwa taskForcy. Wszystkie jednostki z reinforcements s¹ dodawane do listy, a nastêpnie reinforcements jest niszczone
    public void Merge(TaskForceController reinforcements)
    {
        if (reinforcements == this || reinforcements == null)
            return;

        // potencjalny bug, jeœli iloœæ jednostek w do³¹czanym TaskForce zmniejszy siê w trakcie iteracji
        for (int i = 0; i < reinforcements.controllers.Count; i++)
        {
            AddUnit(reinforcements.controllers[i]);
        }

        maxSize = controllers.Count;
        strength = 1.0f;
        onSizeChanged?.Invoke(controllers.Count);
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

                //Destroy(controllers[index].gameObject);
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
