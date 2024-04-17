using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;


// work-in-progress
// klasa odpowiada za zachowanie pojedynczej jednostyki
// wiele jedostek wchodzi w sk³ad TaskForceController

// treœæ bêdziê siê jeszcze czêsto zmieniaæ
public class AiController : MonoBehaviour
{
    public TaskForceController unitTaskForce;
    public enum AiState { Idle, Moving, Combat, Retreat }
    public AiState currentState = AiState.Idle;
    public enum Side { Ally, Enemy }
    public Side unitSide;
    public bool logCurrentState = false;
    public Transform childModel;
    public Unit unitValues;
    public NavMeshAgent agent;
    public bool disabled;
    public bool enablePoolLogging;
    public int health;
    private float cooldownRemaining;
    private bool onCooldown;
    public UnityEvent<GameObject> onUnitDestroyed = new();
    public UnityEvent<GameObject> onProjectileSpawned = new();
    public GameObject projectileContainer;
    public Vector3 closestTargetPastPosition;
    public Vector3 closestTarget;
    public GameObject[] projectiles;
    public float targetSpeed;
    public float ownSpeed;
    public GameObject facingObject;
    private ProjectilePool pool;
    public AiController target;
    public bool debug = false;
    public float relativeVelocity;
    public float targetDistance;

    public UnityEvent<TaskForceController> onUnitEngaged = new();


    public Collider[] colliders = new Collider[1];
    public LayerMask targetMask = new();



    public static AiController Create(string name, GameObject prefab, TaskForceController taskForce, Vector3 pos, GameObject projectileContainer, GameObject allyContainer)
    {
        GameObject instance = Instantiate(prefab, pos, Quaternion.identity, allyContainer.transform);
        instance.name = name;
        AiController controller = instance.GetComponent<AiController>();
        controller.gameObject.transform.SetParent(allyContainer.transform);
        controller.unitTaskForce = taskForce;
        controller.projectileContainer = projectileContainer;

        if (controller.CompareTag("Ally"))
        {
            controller.unitSide = Side.Ally;
            instance.layer = 7;
        }
            

        else if (controller.CompareTag("Enemy"))
        {
            controller.unitSide = Side.Enemy;
            instance.layer = 6;
        }
        
        controller.health = controller.unitValues.health;
        controller.agent = controller.GetComponent<NavMeshAgent>();
        controller.agent.speed = controller.unitValues.unitSpeed;
        controller.agent.stoppingDistance = controller.unitValues.attackDistance;
        controller.agent.acceleration = controller.unitValues.acceleration;


        controller.childModel = controller.GetComponentInChildren<Transform>();

        controller.pool = new(controller.unitValues.projectileLifeSpan, controller.unitValues.attackCooldown);
        controller.projectiles = controller.pool.GetPool();

        //controller.StartCoroutine(controller.SpotForTargets());
        return controller;
    }

    //public void Init(TaskForceController taskForce, AiManager manager, GameObject projectileContainer)
    //{
    //    this.unitTaskForce = taskForce;
    //    this.aiManager = manager;
    //    this.projectileContainer = projectileContainer;

    //    health = unitValues.health;

    //    if (gameObject.CompareTag("Ally"))
    //        unitSide = Side.Ally;

    //    else if (gameObject.CompareTag("Enemy"))
    //        unitSide = Side.Enemy;

    //    agent = GetComponent<NavMeshAgent>();
    //    agent.speed = unitValues.unitSpeed;
    //    agent.stoppingDistance = unitValues.attackDistance;
    //    agent.acceleration = unitValues.acceleration;


    //    childModel = GetComponentInChildren<Transform>();

    //    pool = new(unitValues.projectileLifeSpan, unitValues.attackCooldown);
    //    projectiles = pool.GetPool();

    //    StartCoroutine(SpotForTargets());
    //}


    //private IEnumerator SpotForTargets()
    //{
    //    if (debug)
    //        Debug.Log("starting spotting coroutine");

    //    WaitForSeconds interval = new(1);


            

    //    if (gameObject.CompareTag("Ally"))
    //        targetMask = LayerMask.GetMask("Enemies");
    //    else if (gameObject.CompareTag("Enemy"))
    //        targetMask = LayerMask.GetMask("Allies");


    //    while (true)
    //    {
    //        if (currentState != AiState.Combat)
    //        {
    //            if (debug)
    //                Debug.Log("spotting...");

    //            int count = Physics.OverlapSphereNonAlloc(transform.position, unitValues.spotDistance, colliders, targetMask);
    //            if (count > 0)
    //            {
    //                if (debug)
    //                    Debug.Log("enemy spotted");

    //                TaskForceController enemyTaskForce = colliders[0].gameObject.GetComponent<AiController>().unitTaskForce;
    //                onTaskForceSpotted?.Invoke(enemyTaskForce);
    //            }
    //            else if (count == 0)
    //            {
    //                if (debug)
    //                    Debug.Log("no enemies spotted");
    //            }
    //        }

    //        else if (currentState == AiState.Combat)
    //        {
    //            if (debug)
    //                Debug.Log("combat state, waiting to exit state");
    //        }

    //        yield return interval;
    //    }
    //}





    //private void Start()
    //{
    //    health = unitValues.health;

    //    if (gameObject.CompareTag("Ally"))
    //        unitSide = Side.Ally;

    //    else if (gameObject.CompareTag("Enemy"))
    //        unitSide = Side.Enemy;

    //    agent = GetComponent<NavMeshAgent>();
    //    agent.speed = unitValues.unitSpeed;
    //    agent.stoppingDistance = unitValues.attackDistance;
    //    agent.acceleration = unitValues.acceleration;


    //    childModel = GetComponentInChildren<Transform>();

    //    pool = new(unitValues.projectileLifeSpan, unitValues.attackCooldown);
    //    projectiles = pool.GetPool();
    //}


    private void Update()
    {
        if (enablePoolLogging)
            pool.EnableLogging();

        if (disabled) return;

        if (target)
        {
            Vector3 velocityVector = agent.velocity - target.agent.velocity;
            relativeVelocity = velocityVector.magnitude;
        } 
        else
        {
            relativeVelocity = 0;
        }
        ownSpeed = agent.velocity.magnitude;

        AiController tmp = GetTargetEnemy();
        if (tmp != null)
            target = tmp;

        if (logCurrentState)
            Debug.Log(currentState);

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

    void IdleState()
    {

    }

    void MovingState()
    {

    }

    void CombatState()
    {
        if (agent.hasPath)
        {

            targetDistance = Vector3.Distance(transform.position, agent.destination);


            if (targetDistance <= unitValues.attackDistance)
            {
                Vector3 direction = closestTarget - transform.position;
                direction.y = 0f;
                direction.Normalize();
                Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 10f * Time.deltaTime);
                
                agent.speed = 0;
                Attack();
            } 
            else
            {
                agent.speed = unitValues.unitSpeed;
            }

            //if (childModel)
            //{
            //    childModel.LookAt(target.transform.position);
            //}
                
        }

        else
        {
            SetState(AiState.Idle);
        }

    }

    void SetState(AiState newState)
    {
        currentState = newState;
    }

    private void Attack()
    {
        if (onCooldown)
            return;

        PutProjectile();
        cooldownRemaining = unitValues.attackCooldown;

        StartCoroutine(AttackCooldown());
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

    private void PutProjectile()
    {
        bool friendly;
        if (unitSide == Side.Enemy)
            friendly = false;
        else
            friendly = true;

        //GameObject projectile = ProjectilePooling.GetProjectile(friendly);
        //if (projectile)
        //{
        //    projectile.transform.position = transform.position;
        //    projectile.transform.rotation = SetProjectileRotation();
        //    projectile.SetActive(true);
        //}

        //else
        //{
        //    SpawnProjectile(friendly);
        //}

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

    public void Damage(Projectile projectile)
    {
        health -= projectile.dmg;

        if (currentState != AiState.Combat)
            onUnitEngaged?.Invoke(projectile.shotBy.unitTaskForce);

        if (health <= 0)
        {
            pool.DestroyProjectiles();
            gameObject.SetActive(false);
            onUnitDestroyed?.Invoke(gameObject);
            //aiManager.DeactivateUnit(gameObject);
            //unitTaskForce.DeactivateUnit(this);
            // Destroy(gameObject);
        }

    }

    public void SetIdleState()
    {
        if (agent.hasPath)
            agent.ResetPath();

        agent.speed = unitValues.unitSpeed;
        agent.stoppingDistance = 0;
        SetState(AiState.Idle);
    }

    public void SetMovingState(Vector3 destination)
    {
        if (agent.SetDestination(destination))
        {
            agent.acceleration = unitValues.acceleration;
            agent.speed = unitValues.unitSpeed;
            agent.stoppingDistance = unitValues.unitSpeed;
            SetState(AiState.Moving);
        }
            
    }

    public void SetCombatState(Vector3 attackPos)
    {
        closestTargetPastPosition = closestTarget;
        closestTarget = attackPos;

        if (agent.pathPending || disabled)
            return;

        else if (agent.SetDestination(attackPos))
        {
            //agent.acceleration = 10000;
            if (target != null && targetDistance > unitValues.attackDistance)
                agent.stoppingDistance = relativeVelocity * 2;

            SetState(AiState.Combat);
        }
    }

    public AiController GetFacingEnemy(float maxDistance)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance))
        {
            if (hit.collider.gameObject.CompareTag("Enemy"))
                return hit.collider.gameObject.GetComponent<AiController>();
        }

        return null;
    }

    public AiController GetTargetEnemy()
    {
        Ray ray = new Ray(new Vector3(closestTarget.x, -1, closestTarget.z), Vector3.up);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (debug)
                Debug.DrawRay(ray.origin, ray.direction, Color.green, 2.0f);

            return hit.collider.gameObject.GetComponent<AiController>();
        }

        if (debug)
            Debug.DrawRay(ray.origin, ray.direction, Color.red, 2.0f);

        return null;
    }

    public void DestroyUnit()
    {
        Destroy(gameObject);
    }

    //public void ClearProjectilePool()
    //{
    //    Debug.Log("clearing pool for unit: " + name);
    //    pool.DestroyProjectiles();
    //}
}
