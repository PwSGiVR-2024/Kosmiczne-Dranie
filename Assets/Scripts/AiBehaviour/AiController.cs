using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;


// work-in-progress
// klasa odpowiada za zachowanie pojedynczej jednostyki
// wiele jedostek wchodzi w sk³ad TaskForceController

// treœæ bêdziê siê jeszcze czêsto zmieniaæ
public abstract class AiController : MonoBehaviour
{
    public struct TargetDataLite
    {
        public Vector3 position { get; private set; }
        public Quaternion rotation { get; private set; }
        public Vector3 forward { get; private set; }
        public float distance { get; private set; }
        public float angle { get; private set; }

        public TargetDataLite(Vector3 position, Quaternion rotation, Vector3 forward, float distance, float angle)
        {
            this.position = position;
            this.rotation = rotation;
            this.forward = forward;
            this.distance = distance;
            this.angle = angle;
        }
    }

    public struct TargetData
    {
        public float dataTimeStamp { get; private set; }
        public AiController targetController { get; private set; }
        public Vector3 pastPosition { get; private set; }
        public Vector3 position { get; private set; }
        public float distance { get; private set; }
        public float angle { get; private set; }
        public float relativeVelocity { get; private set; }

        public void UpdateData(Vector3 position, float distance, float angle)
        {
            this.position = position;
            this.distance = distance;
            this.angle = angle;
        }

        public bool TryLockTarget()
        {
            Ray ray = new Ray(new Vector3(position.x, -1, position.z), Vector3.up);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                targetController = hit.collider.gameObject.GetComponent<AiController>();
                return true;
            }

            else
                return false;
        }

        public float CalculateTargetRelativeVelocity(NavMeshAgent agent)
        {
            if (targetController)
            {
                Vector3 velocityVector = agent.velocity - targetController.Agent.velocity;
                return velocityVector.magnitude;
            }

            return -1;
        }
    }

    private TargetData target = new();
    public TargetData Target { get => target; }

    private LayerMask hostileProjectileMask;
    private GameManager gameManager;

    //private AiController target;
    private TaskForceController unitTaskForce;
    private GameObject projectileContainer;

    private bool initialized = false;
    private int health;
    private float volume;

    private float tempStoppingDistance;

    public enum UnitState { Idle, Moving, Combat, Retreat }
    public enum UnitSide { Ally, Enemy, Neutral }

    [Header("Miscellaneous:")]
    [SerializeField] protected bool disabled = false;
    [SerializeField] protected bool debug = false;
    [SerializeField] protected bool logCurrentState = false;

    [Header("Components:")]
    [SerializeField] private Transform childModel;
    [SerializeField] private UnitValues unitValues;
    [SerializeField] private NavMeshAgent agent;

    [Header("States:")]
    [SerializeField] private UnitState currentState = UnitState.Idle;
    [SerializeField] private UnitSide side;

    [Header("Events:")]
    public UnityEvent<AiController> onUnitNeutralized = new();
    public UnityEvent<TaskForceController> onUnitEngaged = new();
    public UnityEvent<UnitState> onStateChanged = new();

    /// <summary>
    /// Current health of this unit
    /// </summary>
    public int Health { get => health; }
    public TaskForceController UnitTaskForce { get => unitTaskForce; set => unitTaskForce = value; }
    public UnitValues Values { get => unitValues; }
    public NavMeshAgent Agent { get => agent; }
    public float Volume {  get => volume; }
    public UnitState CurrentState { get => currentState; }
    public UnitSide Side { get => side; }
    public GameObject ProjectileContainer { get => projectileContainer; }
    public GameManager GameManager { get => gameManager; }

    // debug
    [SerializeField] private float ownSpeed;

    protected abstract void AdditionalInit();

    public void Init(UnitSide side, TaskForceController taskForce, GameObject projectileContainer)
    {
        if (initialized)
            return;

        this.side = side;
        unitTaskForce = taskForce;
        this.projectileContainer = projectileContainer;

        health = unitValues.health;
        agent.speed = unitValues.unitSpeed;
        agent.acceleration = unitValues.acceleration;
        agent.angularSpeed = unitValues.angularSpeed;

        Collider col = gameObject.GetComponent<Collider>();
        float volX = col.bounds.size.x;
        float volZ = col.bounds.size.z;

        if (volX > volZ)
            volume = volX;
        else
            volume = volZ;

        gameManager = taskForce.gameManager;

        if (side == UnitSide.Ally)
            hostileProjectileMask = LayerMask.GetMask("EnemyProjectiles");

        else if (side == UnitSide.Enemy)
            hostileProjectileMask = LayerMask.GetMask("AllyProjectiles");

        AdditionalInit();

        initialized = true;
    }

    protected void Update()
    {
        if (disabled) return;

        if (logCurrentState)
            Debug.Log(currentState);

        ownSpeed = Agent.velocity.magnitude;

        UpdateOperations();

        switch (currentState)
        {
            case UnitState.Idle:
                IdleState();
                break;

            case UnitState.Combat:
                CombatState();
                break;

            case UnitState.Moving:
                MovingState();
                break;

            case UnitState.Retreat:
                RetreatState();
                break;
        }
    }

    public void Damage(Projectile projectile)
    {
        health -= projectile.Values.projectileDamage;

        if (currentState != UnitState.Combat)
            onUnitEngaged.Invoke(projectile.ShotBy?.UnitTaskForce);

        if (health <= 0)
        {
            //pool.SetProjectilesToDestroy();

            //foreach (Projectile proj in projectiles)
            //{
            //    if (proj != null)
            //        unitTaskForce.gameManager.AddToExterminationCamp(proj.gameObject);
            //}
            BeforeDeactivation();
            gameObject.SetActive(false);
            onUnitNeutralized?.Invoke(this);
            gameManager.AddToExterminationCamp(gameObject);
        }

    }

    public void Damage(int value, AiController attacker)
    {
        health -= value;

        if (currentState != UnitState.Combat)
            onUnitEngaged.Invoke(attacker.UnitTaskForce);

        if (health <= 0)
        {
            //pool.SetProjectilesToDestroy();

            //foreach (Projectile proj in projectiles)
            //{
            //    if (proj != null)
            //        unitTaskForce.gameManager.AddToExterminationCamp(proj.gameObject);
            //}
            BeforeDeactivation();
            gameObject.SetActive(false);
            onUnitNeutralized?.Invoke(this);
            gameManager.AddToExterminationCamp(gameObject);
        }

    }

    public void SetTargetData(Vector3 pos, float distance, float angle)
    {
        if (disabled) return;

        //target.pastPosition = target.position;
        target.UpdateData(pos, distance, angle);
        OnTargetPositionChanged();

        //if (targetDistance > Values.attackDistance)
        //    TryLockTarget();
    }

    public void SetTargetData(TargetDataLite data)
    {
        if (disabled) return;

        //target.pastPosition = target.position;
        target.UpdateData(data.position, data.distance, data.angle);
        OnTargetPositionChanged();

        //if (targetDistance > Values.attackDistance)
        //    TryLockTarget();
    }

    protected void SetState(UnitState newState)
    {
        currentState = newState;
        onStateChanged?.Invoke(currentState);
    }

    protected AiController GetFacingEnemy(float maxDistance)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance))
        {
            if (hit.collider.gameObject.CompareTag("Enemy") && gameObject.CompareTag("Ally") ||
                hit.collider.gameObject.CompareTag("Ally") && gameObject.CompareTag("Enemy"))

                return hit.collider.gameObject.GetComponent<AiController>();
        }

        return null;
    }

    protected void ResetDestination()
    {
        agent.destination = transform.position;
        //agent.speed = unitValues.unitSpeed;
    }

    public virtual void SetIdleState()
    {
        if (disabled) return;

        ResetDestination();
        //ResetTarget();
        SetState(UnitState.Idle);
    }

    public virtual void SetMovingState(Vector3 destination)
    {
        if (disabled) return;

        if (agent.SetDestination(destination))
        {
            //agent.acceleration = unitValues.acceleration;
            //agent.speed = unitValues.unitSpeed;
            agent.stoppingDistance = unitValues.unitSpeed;
            SetState(UnitState.Moving);
        }
    }
    public virtual void SetCombatState()
    {
        if (agent.pathPending || disabled)
            return;

        else
            SetState(UnitState.Combat);
            
    }

    public virtual void SetRetreatState(Vector3 escapePoint)
    {
        if (agent.SetDestination(escapePoint))
        {
            //agent.acceleration = unitValues.acceleration;
            //agent.speed = unitValues.unitSpeed;
            agent.stoppingDistance = unitValues.unitSpeed;
            SetState(UnitState.Retreat);
        }
    }

    protected virtual void UpdateOperations()
    {
        Agent.stoppingDistance = StoppingDistanceFormula();
    }

    /// <summary>
    /// Funkcja kalibruje stoppingDistance Agenta, w ka¿dej iteracji metody Update(), jeœli UpdateOperations() nie zostanie nadpisane.
    /// Bazowa formu³a wykonuje obliczenia na podstawie relatywnej prêdkoœci do celu. Zapobiega to zbytniemu zbli¿eniu siê do celu, jeœli obie jednostki poruszaj¹ sie naprzeciwko siebie. 
    /// </summary>
    protected virtual float StoppingDistanceFormula()
    {
        if (CurrentState == UnitState.Combat)
        {
            if (Agent.remainingDistance <= Values.attackDistance)
                return Values.attackDistance;

            else if (target.targetController)
            {
                tempStoppingDistance = target.relativeVelocity + Values.attackDistance;
                return tempStoppingDistance;
            }
            else
                return tempStoppingDistance;
        }
        else
            return Values.unitSpeed;
    }

    protected abstract void IdleState();

    protected abstract void MovingState();

    protected abstract void CombatState();

    protected abstract void RetreatState();

    protected abstract void OnTargetPositionChanged();

    protected abstract void BeforeDeactivation();

    //protected virtual void OnTriggerEnter(Collider collider)
    //{
    //    if ((hostileProjectileMask & (1 << collider.gameObject.layer)) != 0)
    //        Damage(collider.gameObject.GetComponent<Projectile>());
    //}
}