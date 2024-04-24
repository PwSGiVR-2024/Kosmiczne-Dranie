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
    private AiController target;
    private TaskForceController unitTaskForce;
    private GameObject projectileContainer;
    private ProjectilePool pool;

    private bool initialized = false;
    private int health;
    private float cooldownRemaining;
    private bool onCooldown;
    private float volume;

    private Vector3 closestTargetPastPosition;
    private Vector3 closestTargetPosition;
    private float targetRelativeVelocity;
    private float targetDistance;
    private float tempStoppingDistance;

    public enum AiState { Idle, Moving, Combat, Retreat }
    public enum Side { Ally, Enemy, Neutral }

    [Header("Miscellaneous:")]
    [SerializeField] protected bool disabled = false;
    [SerializeField] protected bool debug = false;
    [SerializeField] protected bool logCurrentState = false;
    [SerializeField] protected bool enablePoolLogging = false;

    [Header("Components:")]
    [SerializeField] private Transform childModel;
    [SerializeField] private Unit unitValues;
    [SerializeField] private NavMeshAgent agent;

    [Header("States:")]
    [SerializeField] private AiState currentState = AiState.Idle;
    [SerializeField] private Side unitSide;

    [Header("Events:")]
    public UnityEvent<AiController> onUnitNeutralized = new();
    //public UnityEvent<GameObject> onProjectileSpawned = new();
    public UnityEvent<TaskForceController> onUnitEngaged = new();
    public UnityEvent<AiState> onStateChanged = new();

    public int Health { get => health; }
    public TaskForceController UnitTaskForce { get => unitTaskForce; set => unitTaskForce = value; }
    public Unit Values { get => unitValues; }
    public NavMeshAgent Agent { get => agent; }
    public AiController Target { get => target; }
    public float Volume {  get => volume; }
    public Vector3 ClosestTargetPosition { get => closestTargetPosition; }
    public Vector3 ClosestTargetPastPosition { get => closestTargetPastPosition; }
    public float TargetDistance { get => targetDistance; }
    public float TargetRelativeVelocity { get => targetRelativeVelocity; }
    public AiState CurrentState { get => currentState; }
    public Side UnitSide { get => unitSide; }

    // debug
    [SerializeField] private float ownSpeed;
    [SerializeField] private GameObject[] projectiles;

    protected abstract void AdditionalInit();

    public void Init(Side side, TaskForceController taskForce, GameObject projectileContainer)
    {
        if (initialized)
            return;

        unitSide = side;
        unitTaskForce = taskForce;
        this.projectileContainer = projectileContainer;

        health = unitValues.health;
        agent.speed = unitValues.unitSpeed;
        agent.acceleration = unitValues.acceleration;
        agent.angularSpeed = unitValues.angularSpeed;

        pool = new(unitValues.projectileLifeSpan, unitValues.attackCooldown);
        projectiles = pool.GetPool();


        Collider col = gameObject.GetComponent<Collider>();
        float volX = col.bounds.size.x;
        float volZ = col.bounds.size.z;

        if (volX > volZ)
            volume = volX;
        else
            volume = volZ;

        AdditionalInit();

        initialized = true;
    }

    protected void Update()
    {
        if (disabled) return;

        if (logCurrentState)
            Debug.Log(currentState);

        if (enablePoolLogging)
            pool.EnableLogging();

        ownSpeed = Agent.velocity.magnitude;

        UpdateOperations();

        switch (currentState)
        {
            case AiState.Idle:
                IdleState();
                break;

            case AiState.Combat:
                CombatState();
                break;

            case AiState.Moving:
                MovingState();
                break;
        }
    }

    public void Damage(Projectile projectile)
    {
        health -= projectile.dmg;

        if (currentState != AiState.Combat)
            onUnitEngaged?.Invoke(projectile.shotBy.unitTaskForce);

        if (health <= 0)
        {
            //pool.SetProjectilesToDestroy();

            foreach (GameObject go in projectiles)
            {
                if (go != null)
                    unitTaskForce.gameManager.AddToExterminationCamp(go);
            }

            gameObject.SetActive(false);
            onUnitNeutralized?.Invoke(this);
        }

    }

    public void SetTargetPosition(Vector3 pos)
    {
        if (disabled) return;

        closestTargetPastPosition = closestTargetPosition;
        closestTargetPosition = pos;
        OnTargetPositionChanged();
        targetDistance = Vector3.Distance(transform.position, closestTargetPosition);
        TryLockTarget();
    }

    protected void SetState(AiState newState)
    {
        currentState = newState;
        onStateChanged?.Invoke(currentState);
    }

    /// <summary>
    /// Namierza cel (AiController) na podstawie pozycji closestTargetPosition, jeœli to mo¿liwe
    /// </summary>
    protected bool TryLockTarget()
    {
        Ray ray = new Ray(new Vector3(closestTargetPosition.x, -1, closestTargetPosition.z), Vector3.up);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (debug)
                Debug.DrawRay(ray.origin, ray.direction + new Vector3(0, 10, 0), Color.green, 1);

            target = hit.collider.gameObject.GetComponent<AiController>();
            CalculateTargetRelativeVelocity();
            return true;
        }
        else
        {
            if (debug)
                Debug.DrawRay(ray.origin, ray.direction + new Vector3(0, 10, 0), Color.red, 1);

            return false;
        }
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

    protected void ResetTarget()
    {
        target = null;
    }

    protected void ResetDestination()
    {
        agent.destination = transform.position;
        agent.speed = unitValues.unitSpeed;
    }

    private void CalculateTargetRelativeVelocity()
    {
        Vector3 velocityVector = Agent.velocity - Target.Agent.velocity;
        targetRelativeVelocity = velocityVector.magnitude;
    }

    private void PutProjectile()
    {
        bool friendly;
        if (unitSide == Side.Enemy)
            friendly = false;
        else
            friendly = true;

        GameObject projectile;
        if (pool.TryGetProjectile(out projectile))
        {
            projectile.transform.position = transform.position;
            projectile.transform.rotation = SetProjectileRotation();
            projectile.SetActive(true);
        }

        else
        {
            projectile = SpawnProjectile(friendly);
            pool.TryPutProjectileInPool(projectile);
        }
    }

    private GameObject SpawnProjectile(bool friendly)
    {
        GameObject projectile = Instantiate(unitValues.projectile, transform.position, SetProjectileRotation());
        projectile.GetComponent<Projectile>().Init(unitValues, friendly, projectileContainer, this);
        return projectile;
    }

    private Quaternion SetProjectileRotation()
    {
        Quaternion projectileRotation = gameObject.transform.rotation;

        float randomAngle = UnityEngine.Random.Range(-unitValues.angleError, unitValues.angleError);
        projectileRotation *= Quaternion.AngleAxis(randomAngle, Vector3.up);

        return projectileRotation;
    }

    private IEnumerator AttackCooldown()
    {
        onCooldown = true;

        while (cooldownRemaining > 0)
        {
            cooldownRemaining -= Time.deltaTime;

            if (cooldownRemaining < 0)
                cooldownRemaining = 0;

            yield return null;
        }

        onCooldown = false;
    }

    

    protected void FireProjectile()
    {
        if (onCooldown)
            return;

        float angleToTarget = Vector3.Angle(closestTargetPosition - transform.position, transform.forward);
        
        if (angleToTarget <= Values.angleError)
        {
            PutProjectile();
            cooldownRemaining = unitValues.attackCooldown;

            StartCoroutine(AttackCooldown());
        }
    }

    public virtual void SetIdleState()
    {
        if (disabled) return;

        ResetDestination();
        ResetTarget();
        SetState(AiState.Idle);
    }

    public virtual void SetMovingState(Vector3 destination)
    {
        if (disabled) return;

        if (agent.SetDestination(destination))
        {
            agent.acceleration = unitValues.acceleration;
            agent.speed = unitValues.unitSpeed;
            agent.stoppingDistance = unitValues.unitSpeed;
            SetState(AiState.Moving);
        }
    }
    public virtual void SetCombatState()
    {
        if (agent.pathPending || disabled)
            return;

        else
            SetState(AiState.Combat);
            
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
        if (CurrentState == AiState.Combat)
        {
            if (Agent.remainingDistance <= Values.attackDistance)
                return Values.attackDistance;

            else if (Target)
            {
                tempStoppingDistance = targetRelativeVelocity + Values.attackDistance;
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

    protected abstract void OnTargetPositionChanged();
}