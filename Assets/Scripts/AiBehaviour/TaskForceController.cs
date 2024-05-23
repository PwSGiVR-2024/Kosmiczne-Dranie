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

    [Header("Display info:")]
    [SerializeField] private GameObject icon;
    [SerializeField] private Vector3 iconOffset;

    [Header("Main attributes:")]
    [SerializeField] private TaskForceController target;
    [SerializeField] private AiController commander;
    [SerializeField] private string taskForceName;
    [SerializeField] private float spotDistance;
    [SerializeField] private int maxSize;
    [SerializeField] private float strength;
    [SerializeField] private List<AiController> unitControllers = new();

    [Header("Misc attributes:")]
    [SerializeField] private Vector3 escapePoint;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private Collider[] targetCollider = new Collider[1];

    [Header("Dynamic attributes:")]
    [SerializeField] private float travelSpeed;
    [SerializeField] private float travelAcceleration;

    [Header("States:")]
    [SerializeField] private Affiliation side = Affiliation.Green;
    [SerializeField] private State currentState = State.Idle;
    [SerializeField] private TaskForceBehaviour currentBehaviour = TaskForceBehaviour.Aggresive;
    [SerializeField] private TaskForceOrder currentOrder = TaskForceOrder.None;

    [Header("Events:")]
    public UnityEvent<int> onSizeChanged = new();
    public UnityEvent<float> onStrengthChanged = new();
    public UnityEvent<State> onStateChanged = new();
    public UnityEvent<TaskForceBehaviour> onBehaviourChanged = new();
    public UnityEvent<TaskForceOrder> onOrderChanged = new();
    public UnityEvent<TaskForceController> onTaskForceDestroyed = new();

    public Affiliation Side { get => side; }
    public State CurrentState {
        get => currentState;
        set {
            currentState = value;
            onStateChanged.Invoke(value);
        }
    }
    public TaskForceBehaviour CurrentBehaviour {
        get => currentBehaviour;
        set
        {
            currentBehaviour = value;
            onBehaviourChanged.Invoke(value);
        }
    }
    public TaskForceOrder CurrentOrder
    {
        get => currentOrder;
        set
        {
            currentOrder = value;
            onOrderChanged.Invoke(value);
        }
    }

    public string TaskForceName { get => taskForceName; }
    public AiController Commander { get =>  commander; }
    public List<AiController> Units { get => unitControllers; }
    public float Strength { get => strength; }
    public TaskForceController Target { get => target; }

    public NativeArray<JobUnitData> CreateUnitsDataSnapshot()
    {
        NativeArray<JobUnitData> data = new(Units.Count, Allocator.Persistent);

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = new JobUnitData(unitControllers[i].transform.position, unitControllers[i].transform.rotation, unitControllers[i].transform.forward);
        }

        return data;
    }

    public void Init(string name, int maxSize, GameObject icon, Vector3 iconOffset, GameManager gameManager, Affiliation side)
    {
        if (initialized)
            return;

        taskForceName = name;
        this.maxSize = maxSize;
        this.icon = icon;
        this.iconOffset = iconOffset;
        this.gameManager = gameManager;
        this.side = side;

        if (side == Affiliation.Blue)
            targetMask = LayerMask.GetMask("Enemies");

        else if (side == Affiliation.Red)
            targetMask = LayerMask.GetMask("Allies");

        onStateChanged.AddListener(OnStateChanged);
        initialized = true;
    }

    public void AddUnit(AiController unit)
    {
        unit.UnitTaskForce = this;
        unitControllers.Add(unit);

        SetNewSpotDistance();
        SetNewTravelSpeed();

        if (unitControllers.Count == 1)
            commander = unit;

        unit.onUnitNeutralized.AddListener(RemoveUnitFromTaskForce);
        unit.onUnitEngaged.AddListener((attacker) => EngageTaskForce(attacker));
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

        spotDistance = 0;

        if (unitControllers.Count == 1)
        {
            spotDistance = unitControllers[0].Values.spotDistance;
            return;
        }

        else
        {
            float newSpotDistance;
            foreach (var unit in unitControllers)
            {
                newSpotDistance = unit.Values.spotDistance;
                if (newSpotDistance > spotDistance)
                    spotDistance = newSpotDistance;
            }
        }
    }

    private void RemoveUnitFromTaskForce(AiController unit)
    {
        unitControllers.Remove(unit);

        if (unitControllers.Count == 0)
            DestroyTaskForce();

        float unitSpotDistance = unit.Values.spotDistance;
        if (unitSpotDistance == spotDistance)
            SetNewSpotDistance();

        if (commander == unit)
            FindNextCommanderRecursive(0);

        UpdateStrength();

        onSizeChanged?.Invoke(unitControllers.Count);
    }

    private void UpdateStrength()
    {
        strength = (float)unitControllers.Count / maxSize;
        onStrengthChanged?.Invoke(strength);
    }

    public void DestroyTaskForce()
    {
        StopAllCoroutines();
        onTaskForceDestroyed?.Invoke(this);
        Destroy(icon);
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
        if (Units.Count == 0 || target.Units.Count == 0)
            return false;

        jobEnemies = target.CreateUnitsDataSnapshot();
        jobAllies = CreateUnitsDataSnapshot();
        targetData = new(jobAllies.Length, Allocator.Persistent);

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
                    if (targetProviderJobHandle.IsCompleted)
                        TryGetTargetProviderJob(ref targetProviderJobHandle);

                if (target == null)
                    SetIdleState();
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
        if (SpotForTargets(out TaskForceController target))
            EngageTaskForce(target);
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

    //HashSet<TaskForceController> enemyTaskForces = new();
    private bool SpotForTargets(out TaskForceController target)
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

        Collider[] targets = Physics.OverlapSphere(commander.transform.position, spotDistance + (float)Math.Sqrt(unitControllers.Count) * commander.Volume, targetMask);

        if (targets.Length == 0)
        {
            target = null;
            return false;
        }

        int index = 0;
        float distance = float.PositiveInfinity;
        for (int i = 0; i < targets.Length; i++)
        {
            float newDistance = Vector3.Distance(targets[i].transform.position, commander.transform.position);

            if (newDistance < distance)
                index = i;
        }

        if (targets[index].TryGetComponent(out AiController controller))
        {
            target = controller.UnitTaskForce;
            return true;
        }
        
        else
        {
            target = null;
            return false;
        }
    }

    public void EngageTaskForce(TaskForceController target)
    {
        if (target == null)
            return;

        this.target = target;

        CurrentState = State.Combat;

        if (targetCalculations == TargetCalculations.Coroutine)
            //StartCoroutine(RefreshTargets(target));
            StartCoroutine(RefreshTargetDataInInterval());
    }

    public void Disengage(Vector3 escapePoint)
    {
        CurrentState = State.Retreat;

        foreach (var controller in unitControllers)
        {
            if (controller != null)
                controller.SetRetreatState(GameUtils.RandomPlanePositionCircle(escapePoint, Mathf.Sqrt(unitControllers.Count) * commander.Volume));
        }

        if (commander != null)
            commander.SetRetreatState(escapePoint);
    }

    public void SetDestination(Vector3 destination)
    {
        CurrentState = State.Moving;

        foreach (var controller in unitControllers)
        {
            controller.SetMovingState(GameUtils.RandomPlanePositionCircle(destination, Mathf.Sqrt(unitControllers.Count) * commander.Volume));
        }

        if (commander != null)
            commander.SetMovingState(destination);
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

        foreach (var controller in reinforcements.unitControllers)
        {
            AddUnit(controller);
            controller.SetMovingState(GameUtils.RandomPlanePositionCircle(commander.transform.position, Mathf.Sqrt(unitControllers.Count) * commander.Volume));
        }

        maxSize = unitControllers.Count;
        strength = 1.0f;
        onSizeChanged?.Invoke(unitControllers.Count);
        onStrengthChanged?.Invoke(strength);
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

        while (target)
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
            GameUtils.DrawCircle(gameObject, spotDistance + (float)Math.Sqrt(unitControllers.Count) * commander.Volume, commander.transform);

            icon.transform.LookAt(Camera.main.transform, Vector3.up);
            icon.transform.position = commander.transform.position + iconOffset;
        }

        if (targetData.IsCreated)
        {
            // temporary solution
            // making sure targetData[i] corresponds to unitControllers[i]
            // lazy, but otherwise needs additional synchronizing which can introduce overhead
            // if false, some units can keep outdated targetData or lag behind
            // in large scale this problem can be negligible
            // in small scale TargetProvider is generally fast enough
            // side effect: can improve frame generation time
            // WILL INTRODUCE MEMORY LEAKS
            //if (unitControllers.Count != targetData.Length)
            //    return;

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
