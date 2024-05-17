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
// wiele jedostek wchodzi w sk�ad TaskForceController

// tre�� b�dzi� si� jeszcze cz�sto zmienia�
public abstract class AiController : MonoBehaviour
{
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

    [Header("Miscellaneous:")]
    [SerializeField] protected bool disabled = false;
    [SerializeField] protected bool debug = false;
    [SerializeField] protected bool logCurrentState = false;

    [Header("Components:")]
    [SerializeField] private Transform childModel;
    [SerializeField] private UnitValues unitValues;
    [SerializeField] private NavMeshAgent agent;

    [Header("States:")]
    [SerializeField] private State currentState = State.Idle;
    [SerializeField] private Affiliation side;

    [Header("Events:")]
    public UnityEvent<AiController> onUnitNeutralized = new();
    public UnityEvent<TaskForceController> onUnitEngaged = new();
    public UnityEvent<State> onStateChanged = new();

    /// <summary>
    /// Current health of this unit
    /// </summary>
    public int Health { get => health; }
    public TaskForceController UnitTaskForce { get => unitTaskForce; set => unitTaskForce = value; }
    public UnitValues Values { get => unitValues; }
    public NavMeshAgent Agent { get => agent; }
    public float Volume {  get => volume; }
    public State CurrentState { get => currentState; }
    public Affiliation Side { get => side; }
    public GameObject ProjectileContainer { get => projectileContainer; }
    public GameManager GameManager { get => gameManager; }

    // debug
    [SerializeField] private float ownSpeed;

    protected abstract void AdditionalInit();

    public void Init(Affiliation side, TaskForceController taskForce, GameObject projectileContainer)
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

        if (side == Affiliation.Blue)
            hostileProjectileMask = LayerMask.GetMask("EnemyProjectiles");

        else if (side == Affiliation.Red)
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
            case State.Idle:
                IdleState();
                break;

            case State.Combat:
                CombatState();
                break;

            case State.Moving:
                MovingState();
                break;

            case State.Retreat:
                RetreatState();
                break;
        }
    }

    public void Damage(Projectile projectile)
    {
        health -= projectile.Values.projectileDamage;

        if (currentState != State.Combat)
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

        if (currentState != State.Combat)
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

    public void SetTargetData(Vector3 pos, float distance, float angle, Vector3 forward)
    {
        if (disabled) return;

        //target.pastPosition = target.position;
        target.UpdateData(pos, distance, angle, forward);
        OnTargetPositionChanged();

        //if (targetDistance > Values.attackDistance)
        //    TryLockTarget();
    }

    public void SetTargetData(TargetDataLite data)
    {
        if (disabled) return;

        //target.pastPosition = target.position;
        target.UpdateData(data.position, data.distance, data.angle, data.forward);
        OnTargetPositionChanged();

        //if (targetDistance > Values.attackDistance)
        //    TryLockTarget();
    }

    protected void SetState(State newState)
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
        SetState(State.Idle);
    }

    public virtual void SetMovingState(Vector3 destination)
    {
        if (disabled) return;

        if (agent.SetDestination(destination))
        {
            //agent.acceleration = unitValues.acceleration;
            //agent.speed = unitValues.unitSpeed;
            agent.stoppingDistance = unitValues.unitSpeed;
            SetState(State.Moving);
        }
    }
    public virtual void SetCombatState()
    {
        if (agent.pathPending || disabled)
            return;

        else
            SetState(State.Combat);
            
    }

    public virtual void SetRetreatState(Vector3 escapePoint)
    {
        if (agent.SetDestination(escapePoint))
        {
            //agent.acceleration = unitValues.acceleration;
            //agent.speed = unitValues.unitSpeed;
            agent.stoppingDistance = unitValues.unitSpeed;
            SetState(State.Retreat);
        }
    }

    protected virtual void UpdateOperations()
    {
        Agent.stoppingDistance = StoppingDistanceFormula();
    }

    /// <summary>
    /// Funkcja kalibruje stoppingDistance Agenta, w ka�dej iteracji metody Update(), je�li UpdateOperations() nie zostanie nadpisane.
    /// Bazowa formu�a wykonuje obliczenia na podstawie relatywnej pr�dko�ci do celu. Zapobiega to zbytniemu zbli�eniu si� do celu, je�li obie jednostki poruszaj� sie naprzeciwko siebie. 
    /// </summary>
    protected virtual float StoppingDistanceFormula()
    {
        if (CurrentState == State.Combat)
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