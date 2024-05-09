using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

public class TaskForceController : MonoBehaviour
{
    public enum TaskForceSide { Ally, Enemy, Neutral }
    public enum TaskForceState { Idle, Moving, Combat, Retreat }
    public enum TaskForceBehaviour { Passive, Aggresive, Evasive }
    public enum TaskForceOrder { None, Engage, Disengage, Move, Patrol, Defend }

    // helper attributes
    private float secondsAfterStop = 0;
    private bool counterRunning = false;
    private readonly float waitingForSeconds = 1.0f;
    //

    private bool initialized = false;
    public GameManager gameManager;
    [SerializeField] private bool debug = false;
    
    [Header("Display info:")]
    [SerializeField] private GameObject icon;
    [SerializeField] private Vector3 iconOffset;

    [Header("Main attributes:")]
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
    [SerializeField] private TaskForceSide side = TaskForceSide.Neutral;
    [SerializeField] private TaskForceState currentState = TaskForceState.Idle;
    [SerializeField] private TaskForceBehaviour currentBehaviour = TaskForceBehaviour.Aggresive;
    [SerializeField] private TaskForceOrder currentOrder = TaskForceOrder.None;

    [Header("Events:")]
    public UnityEvent<int> onSizeChanged = new();
    public UnityEvent<float> onStrengthChanged = new();
    public UnityEvent<TaskForceState> onStateChanged = new();
    public UnityEvent<TaskForceBehaviour> onBehaviourChanged = new();
    public UnityEvent<TaskForceOrder> onOrderChanged = new();
    public UnityEvent<TaskForceController> onTaskForceDestroyed = new();

    public TaskForceSide Side { get => side; }
    public TaskForceState CurrentState {
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


    // provides target information for every unit in this TaskForce
    struct TargetProvider : IJobParallelFor
    {
        [ReadOnly] // this TaskForce units locations
        public NativeArray<Vector3> unitsLocations;

        [ReadOnly] // this TaskForce units forward vectors (necessary for angle calculations)
        public NativeArray<Vector3> unitsForwardVectors;

        [ReadOnly] // units locations of enemy TaskForce
        public NativeArray<Vector3> potentialTargets;

        // calculated data
        // the paradigm is that value at given index corresponds to the unit from this taskForce at this exact index (in the List<AiController>)
        public NativeArray<Vector3> outcomeTargets;
        public NativeArray<float> outcomeTragetDistances;
        public NativeArray<float> outcomeTragetAngles;


        public void Execute(int index)
        {
            // index is index of unit from this TaskForce
            // j is index of unit from enemy TaskForce
            // this method executes ownUnits * enemyUnits times

            float distance = -1;
            float newDistance;
            int chosenTargetIndex = 0;


            for (int j = 0; j < potentialTargets.Length; j++)
            {
                newDistance = Vector3.Distance(unitsLocations[index], potentialTargets[j]);

                if (newDistance < distance || distance == -1)
                {
                    distance = newDistance;
                    outcomeTargets[index] = potentialTargets[j];
                    chosenTargetIndex = j;
                }
            }

            outcomeTragetDistances[index] = distance;
            outcomeTragetAngles[index] = Vector3.Angle(unitsForwardVectors[index], potentialTargets[chosenTargetIndex] - unitsLocations[index]);
        }
    }

    private IEnumerator RefreshTargets(TaskForceController target)
    {
        WaitForSeconds interval = new(0.25f);
        List<AiController> enemies = target.unitControllers;

        while (CurrentState == TaskForceState.Combat)
        {
            if (target == null)
            {
                SetIdleState();
                yield break;
            }

            NativeArray<Vector3> unitsLocations = new(unitControllers.Count, Allocator.Persistent);
            NativeArray<Vector3> unitsForwardVectors = new(unitControllers.Count, Allocator.Persistent);
            NativeArray<Vector3> potentialTargets = new(enemies.Count, Allocator.Persistent);
            NativeArray<Vector3> outcomeTargets = new(unitControllers.Count, Allocator.Persistent);
            NativeArray<float> outcomeTragetDistances = new(unitControllers.Count, Allocator.Persistent);
            NativeArray<float> outcomeTragetAngles = new(unitControllers.Count, Allocator.Persistent);

            for (int i = 0; i < unitControllers.Count; i++)
            {
                unitsLocations[i] = unitControllers[i].transform.position;
                unitsForwardVectors[i] = unitControllers[i].transform.forward;
            }

            for (int i = 0; i < enemies.Count; i++)
            {
                potentialTargets[i] = enemies[i].transform.position;
            }

            var job = new TargetProvider
            {
                unitsLocations = unitsLocations,
                unitsForwardVectors = unitsForwardVectors,
                potentialTargets = potentialTargets,
                outcomeTargets = outcomeTargets,
                outcomeTragetDistances = outcomeTragetDistances,
                outcomeTragetAngles = outcomeTragetAngles,
            };

            JobHandle handle = job.Schedule(unitControllers.Count, 1);
            handle.Complete();

            for (int i = 0; i < unitControllers.Count; i++)
            {
                if (unitControllers[i].gameObject.activeSelf)
                    unitControllers[i].SetTargetData(outcomeTargets[i], outcomeTragetDistances[i], outcomeTragetAngles[i]);
            }

            unitsLocations.Dispose();
            unitsForwardVectors.Dispose();
            potentialTargets.Dispose();
            outcomeTargets.Dispose();
            outcomeTragetDistances.Dispose();
            outcomeTragetAngles.Dispose();

            yield return interval;
        }
    }

    public void Init(string name, int maxSize, GameObject icon, Vector3 iconOffset, GameManager gameManager, TaskForceSide side)
    {
        if (initialized)
            return;

        taskForceName = name;
        this.maxSize = maxSize;
        this.icon = icon;
        this.iconOffset = iconOffset;
        this.gameManager = gameManager;
        this.side = side;

        if (side == TaskForceSide.Ally)
            targetMask = LayerMask.GetMask("Enemies");

        else if (side == TaskForceSide.Enemy)
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

    private void Update()
    {
        if (commander == null)
            return;

        GameUtils.DrawCircle(gameObject, spotDistance + (float)Math.Sqrt(unitControllers.Count) * commander.Volume, commander.transform);

        icon.transform.LookAt(Camera.main.transform, Vector3.up);
        icon.transform.position = commander.transform.position + iconOffset;

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

        switch (CurrentState)
        {
            case TaskForceState.Idle:
                break;

            case TaskForceState.Combat:
                break;

            case TaskForceState.Moving:
                MovingState();
                break;

            case TaskForceState.Retreat:
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
                    CurrentState = TaskForceState.Idle;
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

    private void OnStateChanged(TaskForceState state)
    {
        switch (state)
        {
            case TaskForceState.Idle:
                OnIdleChange();
                break;

            case TaskForceState.Moving:
                OnMovingChange();
                break;

            case TaskForceState.Combat:
                OnCombatChange();
                break;

            case TaskForceState.Retreat:
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


    private bool SpotForTargets(out TaskForceController target)
    {
        if (CurrentState == TaskForceState.Combat || commander == null)
        {
            target = null;
            return false;
        }

        int count = Physics.OverlapSphereNonAlloc(commander.transform.position, spotDistance + (float)Math.Sqrt(unitControllers.Count) * commander.Volume, targetCollider, targetMask);
        if (count > 0)
        {
            TaskForceController enemyTaskForce = targetCollider[0].gameObject.GetComponent<AiController>().UnitTaskForce;
            target = enemyTaskForce;
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

        CurrentState = TaskForceState.Combat;
        StartCoroutine(RefreshTargets(target));
    }

    public void Disengage(Vector3 escapePoint)
    {
        CurrentState = TaskForceState.Retreat;

        foreach (var controller in unitControllers)
        {
            if (controller != null)
                controller.SetRetreatState(GameUtils.RandomPlanePositionCircle(escapePoint, Mathf.Sqrt(unitControllers.Count) * commander.Volume));
        }

        if (commander != null)
            commander.GetComponent<AiController>().SetRetreatState(escapePoint);
    }

    public void SetDestination(Vector3 destination)
    {
        CurrentState = TaskForceState.Moving;

        foreach (var controller in unitControllers)
        {
            if (controller != null)
                controller.SetMovingState(GameUtils.RandomPlanePositionCircle(destination, Mathf.Sqrt(unitControllers.Count) * commander.Volume));
        }

        if (commander != null)
            commander.GetComponent<AiController>().SetMovingState(destination);
    }

    public void SetIdleState()
    {
        CurrentState = TaskForceState.Idle;

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
}
