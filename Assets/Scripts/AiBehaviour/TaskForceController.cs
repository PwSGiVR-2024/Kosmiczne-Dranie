using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Jobs;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;
using Debug = UnityEngine.Debug;


public class TaskForceController : MonoBehaviour
{
    //public int frigateCount = 0;
    //public int destroyerCount = 0;
    //public int cruiserCount = 0;
    //public int battleshipCount = 0;

    HashSet<AiController> frigates = new();
    HashSet<AiController> destroyers = new();
    HashSet<AiController> cruisers = new();
    HashSet<AiController> battleships = new();

    // helper attributes
    private bool initialized = false;

    private float secondsAfterStop = 0;
    private bool counterRunning = false;
    private readonly float waitingForSeconds = 1.0f;

    private JobHandle targetProviderJobHandle;
    [ReadOnly]
    private NativeArray<JobUnitData> jobAllies;
    [ReadOnly]
    private NativeArray<JobUnitData> jobEnemies;
    [ReadOnly]
    private NativeArray<TargetDataLite> targetData;
    //
    
    private enum TargetCalculations { Update, Coroutine }
    [Header("Internal logic:")]
    [SerializeField] private TargetCalculations targetCalculations = TargetCalculations.Update;
    [SerializeField] public GameManager gameManager;
    [SerializeField] private bool debug = false;
    [SerializeField] [Range(0.01f, 5.0f)] private float coroutineRefreshRate = 0.1f;

    [Header("Main attributes:")]
    [SerializeField] private MonoBehaviour currentTarget;
    [SerializeField] private AiController commander;
    [SerializeField] private float spotDistance;
    [SerializeField] private int initialHealth;
    [SerializeField] private int initialSize;
    [SerializeField] private int initialPower;
    [SerializeField] private float currentStrength; // percentage value of currentPower / initialPower
    [SerializeField] private int currentPower; // cumulative power of every unit
    [SerializeField] private int currentHealth; // cumulative health of every unit
    [SerializeField] private List<AiController> unitControllers = new();

    [Header("Misc attributes:")]
    [SerializeField] private Vector3 escapePoint;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private Collider[] targetCollider = new Collider[1];

    [Header("Dynamic attributes:")]
    [SerializeField] private float travelSpeed;
    [SerializeField] private float travelAcceleration;

    [Header("States:")]
    [SerializeField] private Affiliation affiliation = Affiliation.Green;
    [SerializeField] private State currentState = State.Idle;
    [SerializeField] private TaskForceBehaviour currentBehaviour = TaskForceBehaviour.Aggresive;
    [SerializeField] private TaskForceOrder currentOrder = TaskForceOrder.None;

    [Header("Events:")]
    public UnityEvent<int> onSizeChanged = new();
    public UnityEvent<float> onStrengthChanged = new();
    public UnityEvent<int> onPowerChanged = new();
    public UnityEvent<int> onHealthChanged = new();
    public UnityEvent<State> onStateChanged = new();
    public UnityEvent<TaskForceBehaviour> onBehaviourChanged = new();
    public UnityEvent<TaskForceOrder> onOrderChanged = new();
    public UnityEvent<TaskForceController> onTaskForceDestroyed = new();

    public Affiliation Affiliation { get => affiliation; }
    public State CurrentState {
        get => currentState;
        private set {
            currentState = value;
            onStateChanged.Invoke(value);
        }
    }
    public TaskForceBehaviour CurrentBehaviour {
        get => currentBehaviour;
        private set
        {
            currentBehaviour = value;
            onBehaviourChanged.Invoke(value);
        }
    }
    public TaskForceOrder CurrentOrder
    {
        get => currentOrder;
        private set
        {
            currentOrder = value;
            onOrderChanged.Invoke(value);
        }
    }
    public int CurrentPower
    {
        get => currentPower;
        private set
        {
            currentPower = value;
            onPowerChanged.Invoke(value);
        }
    }

    public float CurrentStrength
    {
        get => currentStrength;
        private set
        {
            currentStrength = value;
            onStrengthChanged.Invoke(value);
        }
    }

    public int CurrentHealth
    {
        get => currentHealth;
        private set
        {
            currentHealth = value;
            onHealthChanged.Invoke(value);
        }
    }

    public int InitialHealth
    {
        get => initialHealth;
        private set => initialHealth = value;
    }

    public int InitialPower
    {
        get => initialPower;
        private set => initialPower = value;
    }

    public int InitialSize
    {
        get => initialSize;
        private set => initialSize = value;
    }

    public float CurrentSize
    {
        get => unitControllers.Count;
    }

    public AiController Commander { get =>  commander; }
    public List<AiController> Units { get => unitControllers; }
    public float Strength { get => currentStrength; }
    public MonoBehaviour Target { get => currentTarget; }

    public NativeArray<JobUnitData> CreateUnitsDataSnapshot()
    {
        NativeArray<JobUnitData> data = new(Units.Count, Allocator.Persistent);

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = new JobUnitData(unitControllers[i].transform.position, unitControllers[i].transform.rotation, unitControllers[i].transform.forward);
        }

        return data;
    }

    public static TaskForceController Create(string name, GameManager gameManager, Affiliation affiliation)
    {
        TaskForceController instance = new GameObject(name).AddComponent<TaskForceController>();

        instance.gameObject.name = name;
        instance.gameManager = gameManager;
        instance.affiliation = affiliation;

        return instance;
    }

    private void Start()
    {
        onStateChanged.AddListener(OnStateChanged);

        if (affiliation == Affiliation.Blue)
            targetMask = LayerMask.GetMask("Enemies", "EnemyOutposts", "EnemyHeadquarters");

        else if (affiliation == Affiliation.Red)
            targetMask = LayerMask.GetMask("Allies", "AllyOutposts", "PlayerHeadquarters");
    }

    public void AddUnit(AiController unit, bool reinforce=false)
    {
        unit.UnitTaskForce = this;
        unitControllers.Add(unit);

        SetNewSpotDistance();
        SetNewTravelSpeed();

        if (reinforce == false)
        {
            InitialSize++;
            CurrentPower += unit.Values.power;
            InitialPower += unit.Values.power;
            CurrentHealth += unit.Values.health;
            InitialHealth += unit.Values.health;
        }

        else
        {
            if (CurrentPower + unit.Values.power <= InitialPower)
                CurrentPower += unit.Values.power;
            else
            {
                InitialPower = CurrentPower + unit.Values.power;
                CurrentPower += unit.Values.power;
            }

            if (CurrentHealth + unit.Values.health <= CurrentHealth)
                CurrentHealth += unit.Values.health;
            else
            {
                InitialHealth = CurrentHealth + unit.Values.health;
                CurrentHealth += unit.Values.health;
            }

            if (CurrentSize > InitialSize)
                InitialSize++;
        }

        switch (unit.Values.shipClass)
        {
            case ShipClass.Frigate:
                frigates.Add(unit);
                break;

            case ShipClass.Destroyer:
                destroyers.Add(unit);
                break;

            case ShipClass.Cruiser:
                cruisers.Add(unit);
                break;

            case ShipClass.Battleship:
                battleships.Add(unit);
                break;
        }

        UpdateStrength();

        if (unitControllers.Count == 1)
            commander = unit;

        unit.onUnitNeutralized.AddListener(RemoveUnit);
        unit.onUnitEngaged.AddListener((attacker) => Engage(attacker));

        onSizeChanged.Invoke(unitControllers.Count);
    }

    private void RemoveUnit(AiController unit)
    {
        unitControllers.Remove(unit);

        CurrentPower -= unit.Values.power;
        CurrentHealth -= unit.Values.health;

        switch (unit.Values.shipClass)
        {
            case ShipClass.Frigate:
                frigates.Remove(unit);
                break;

            case ShipClass.Destroyer:
                destroyers.Remove(unit);
                break;

            case ShipClass.Cruiser:
                cruisers.Remove(unit);
                break;

            case ShipClass.Battleship:
                battleships.Remove(unit);
                break;
        }

        if (unitControllers.Count == 0)
            DestroyTaskForce();

        float unitSpotDistance = unit.Values.spotDistance;
        if (unitSpotDistance == spotDistance)
            SetNewSpotDistance();

        if (commander == unit)
            FindNextCommanderRecursive(0);

        UpdateStrength();

        onSizeChanged.Invoke(unitControllers.Count);
    }

    private void SetNewTravelSpeed()
    {
        if (unitControllers.Count == 0)
            return;

        if (unitControllers.Count == 1)
        {
            travelSpeed = unitControllers[0].Values.unitSpeed;
            travelAcceleration = unitControllers[0].Values.acceleration;
            return;
        }

        else
        {
            float newSpeed;
            float newAcceleration;
            foreach (var unit in unitControllers)
            {
                newSpeed = unit.Values.unitSpeed;
                newAcceleration = unit.Values.acceleration;

                if (newSpeed < travelSpeed)
                    travelSpeed = newSpeed;

                if (newAcceleration < travelAcceleration)
                    travelAcceleration = newAcceleration;
            }
        }
    }

    private void SetNewSpotDistance()
    {
        if (unitControllers.Count == 0)
            return;

        if (unitControllers.Count == 1)
        {
            spotDistance = unitControllers[0].Values.spotDistance;
            return;
        }

        else
        {
            float cumulativeSpotDistance = 0;
            foreach (var unit in unitControllers)
            {
                cumulativeSpotDistance += unit.Values.spotDistance;
            }

            // assuming every given unit has 4, 8, 16, 32 size, for simplification. Actual unit size is controlled by UnitValues.size (AiController.Values)
            float avarageSize = (float)(frigates.Count * 4 + destroyers.Count * 8 + cruisers.Count * 16 + battleships.Count * 32) / unitControllers.Count;

            float newSpotDistance = (cumulativeSpotDistance / unitControllers.Count) + (float)Math.Sqrt(unitControllers.Count) * avarageSize;
            spotDistance = newSpotDistance;
        }
    }

    private void UpdateStrength()
    {
        if (currentPower == initialPower)
            currentStrength = 1.0f;

        else
            currentStrength = (float)currentPower / initialPower;

        onStrengthChanged?.Invoke(currentStrength);
    }

    public void DestroyTaskForce()
    {
        StopAllCoroutines();
        onTaskForceDestroyed?.Invoke(this);
        //Destroy(icon);
        Destroy(gameObject);
    }

    private void FindNextCommanderRecursive(int index)
    {
        if (unitControllers.Count == 0)
            return;

        if (commander == unitControllers[index] || !unitControllers[index].gameObject.activeSelf || unitControllers[index].gameObject == null)
        {
            FindNextCommanderRecursive(index + 1);
            return;
        }

        commander = unitControllers[index];
    }

    private bool TryGetTargetProviderJob(ref JobHandle handle)
    {
        if (!targetProviderJobHandle.IsCompleted)
            return false;

        if (currentTarget == null || Units.Count == 0)
        {
            SetIdleState();
            return false;
        }

        jobAllies = CreateUnitsDataSnapshot();
        targetData = new(jobAllies.Length, Allocator.Persistent);

        if (currentTarget is TaskForceController controller)
        {
            if (controller.Units.Count == 0)
                return false;

            jobEnemies = controller.CreateUnitsDataSnapshot();
        }

        else if (currentTarget is Outpost)
        {
            jobEnemies = new NativeArray<JobUnitData>(1, Allocator.Persistent);
            jobEnemies[0] = new JobUnitData(currentTarget.transform.position, currentTarget.transform.rotation, currentTarget.transform.forward);
        }

        else if (currentTarget is Headquarters)
        {
            jobEnemies = new NativeArray<JobUnitData>(1, Allocator.Persistent);
            jobEnemies[0] = new JobUnitData(currentTarget.transform.position, currentTarget.transform.rotation, currentTarget.transform.forward);
        }

        var job = new TargetDataProvider
        {
            enemies = jobEnemies,
            allies = jobAllies,
            outcomeTargets = targetData,
        };

        handle = job.Schedule(jobAllies.Length, 64);
        return true;
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case State.Combat:
                if (targetCalculations == TargetCalculations.Update)
                    TryGetTargetProviderJob(ref targetProviderJobHandle);
                break;

            case State.Idle:
                break;

            case State.Moving:
                MovingState();
                break;

            case State.Retreat:
                break;
        }

        switch (currentBehaviour)
        {
            case TaskForceBehaviour.Passive:
                PassiveBehaviour();
                break;

            case TaskForceBehaviour.Aggresive:
                AggresiveBehaviour();
                break;

            case TaskForceBehaviour.Evasive:
                EvasiveBehaviour();
                break;
        }
    }

    private void AggresiveBehaviour()
    {
        if (TrySpotTarget(out MonoBehaviour target))
            Engage(target);
    }

    private void EvasiveBehaviour()
    {

    }

    private void PassiveBehaviour()
    {

    }

    private void MovingState()
    {
        if (commander.Agent.pathPending)
            return;

        IEnumerator Counter()
        {
            counterRunning = true;
            secondsAfterStop = 0;
            WaitForSeconds interval = new WaitForSeconds(0.1f);

            while (commander.Agent.velocity.magnitude == 0)
            {
                yield return interval;
                secondsAfterStop += 0.1f;

                if (waitingForSeconds <= secondsAfterStop)
                {
                    CurrentState = State.Idle;
                    counterRunning = false;
                    yield break;
                }
            }
            counterRunning = false;
            yield break;
        }

        if (commander.Agent.velocity.magnitude == 0)
        {
            if (!counterRunning)
                StartCoroutine(Counter());
        }
    }

    private void OnStateChanged(State state)
    {
        switch (state)
        {
            case State.Idle:
                OnIdleChange();
                break;

            case State.Moving:
                OnMovingChange();
                break;

            case State.Combat:
                OnCombatChange();
                break;

            case State.Retreat:
                OnRetreatChange();
                break;
        }
    }

    private void OnIdleChange()
    {
        foreach (var unit in unitControllers)
        {
            unit.Agent.speed = unit.Values.unitSpeed;
            unit.Agent.acceleration = unit.Values.acceleration;
        }
    }

    private void OnMovingChange()
    {
        foreach (var unit in unitControllers)
        {
            unit.Agent.speed = travelSpeed;
            unit.Agent.acceleration = travelAcceleration;
        }
    }

    private void OnCombatChange()
    {
        foreach (var unit in unitControllers)
        {
            unit.Agent.speed = unit.Values.unitSpeed;
            unit.Agent.acceleration = unit.Values.acceleration;
        }
    }

    private void OnRetreatChange()
    {
        foreach (var unit in unitControllers)
        {
            unit.Agent.speed = unit.Values.unitSpeed;
            unit.Agent.acceleration = unit.Values.acceleration;
        }
    }

    // important assumption:
    // every gameObject from targetMask must have mono script with IInteractable implemented
    private bool TrySpotTarget(out MonoBehaviour target)
    {
        if (commander == null)
        {
            target = null;
            return false;
        }

        //int count = Physics.OverlapSphereNonAlloc(commander.transform.position, spotDistance + (float)Math.Sqrt(unitControllers.Count) * commander.Volume, targetCollider, targetMask);

        //if (count > 0)
        //{
        //    TaskForceController enemyTaskForce = targetCollider[0].gameObject.GetComponent<AiController>().UnitTaskForce;
        //    target = enemyTaskForce;
        //    return true;
        //}

        //else
        //{
        //    target = null;
        //    return false;
        //}

        Collider[] targets = Physics.OverlapSphere(commander.transform.position, spotDistance, targetMask);

        if (targets.Length == 0)
        {
            target = null;
            return false;
        }

        int closestIndex = 0;
        float distance = float.PositiveInfinity;
        for (int i = 0; i < targets.Length; i++)
        {
            float newDistance = Vector3.Distance(targets[i].transform.position, commander.transform.position);

            if (newDistance < distance)
                closestIndex = i;
        }

        if (targets[closestIndex].TryGetComponent(out MonoBehaviour mono))
        {
            if (mono is AiController controller)
                target = controller.UnitTaskForce;

            else
                target = mono;

            return true;
        }
        
        else
        {
            target = null;
            return false;
        }
    }

    public void Engage(MonoBehaviour target)
    {
        if (target == null)
            return;

        currentTarget = target;

        CurrentState = State.Combat;

        if (targetCalculations == TargetCalculations.Coroutine)
            StartCoroutine(RefreshTargetDataInInterval());
    }

    public void Disengage(Vector3 escapePoint)
    {
        CurrentState = State.Retreat;

        foreach (var controller in unitControllers)
        {
            if (controller != null)
                controller.SetRetreatState(GameUtils.RandomPlanePositionCircle(escapePoint, 0));
        }

        if (commander != null)
            commander.SetRetreatState(escapePoint);
    }

    public void SetDestination(Vector3 destination)
    {
        CurrentState = State.Moving;

        Vector3 frigatesDestination = GameUtils.RandomPlanePositionCircle(destination, frigates.Count, frigates.Count);
        Vector3 destroyersDestination = GameUtils.RandomPlanePositionCircle(destination, destroyers.Count, destroyers.Count);
        Vector3 cruisersDestination = GameUtils.RandomPlanePositionCircle(destination, cruisers.Count, cruisers.Count);
        Vector3 battleshipsDestination = GameUtils.RandomPlanePositionCircle(destination, battleships.Count, battleships.Count);

        foreach (var controller in frigates)
        {
            controller.SetMovingState(GameUtils.RandomPlanePositionCircle(frigatesDestination, Mathf.Sqrt(frigates.Count) * controller.Values.size));
        }

        foreach (var controller in destroyers)
        {
            controller.SetMovingState(GameUtils.RandomPlanePositionCircle(destroyersDestination, Mathf.Sqrt(frigates.Count) * controller.Values.size));
        }

        foreach (var controller in cruisers)
        {
            controller.SetMovingState(GameUtils.RandomPlanePositionCircle(cruisersDestination, Mathf.Sqrt(frigates.Count) * controller.Values.size));
        }

        foreach (var controller in battleships)
        {
            controller.SetMovingState(GameUtils.RandomPlanePositionCircle(battleshipsDestination, Mathf.Sqrt(frigates.Count) * controller.Values.size));
        }
    }

    public void SetIdleState()
    {
        CurrentState = State.Idle;

        foreach (var unit in unitControllers)
        {
            unit.SetIdleState();
        }
    }

    public void Merge(TaskForceController reinforcements)
    {
        if (reinforcements == this || reinforcements == null)
            return;

        reinforcements.SetDestination(commander.transform.position);

        foreach (var controller in reinforcements.unitControllers)
        {
            AddUnit(controller, reinforce: true);
        }

        onSizeChanged?.Invoke(unitControllers.Count);
        onStrengthChanged?.Invoke(currentStrength);
        reinforcements.DestroyTaskForce();
    }

    private void OnDestroy()
    {
        targetProviderJobHandle.Complete();

        if (jobAllies.IsCreated)
            jobAllies.Dispose();

        if (jobEnemies.IsCreated)
            jobEnemies.Dispose();

        if (targetData.IsCreated)
            targetData.Dispose();
    }

    private IEnumerator RefreshTargetDataInInterval()
    {
        WaitForSeconds interval = new(coroutineRefreshRate);

        while (currentTarget)
        {
            if (targetProviderJobHandle.IsCompleted)
                TryGetTargetProviderJob(ref targetProviderJobHandle);

            yield return interval;
        }
    }

    private void LateUpdate()
    {
        if (commander)
        {
            GameUtils.DrawCircle(gameObject, spotDistance, commander.transform);

            //icon.transform.LookAt(Camera.main.transform, Vector3.up);
            //icon.transform.position = commander.transform.position + iconOffset;
        }

        if (targetData.IsCreated)
        {
            targetProviderJobHandle.Complete();

            if (unitControllers.Count == targetData.Length)
                for (int i = 0; i < unitControllers.Count; i++)
                {
                    unitControllers[i].SetTargetData(targetData[i]);
                }

            jobEnemies.Dispose();
            jobAllies.Dispose();
            targetData.Dispose();
        }
    }
}
