using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Outpost : MonoBehaviour, IInteractable
{
    public GameObject explosionPrefab;
    public LineRenderer lineRenderer;

    public Collider[] targetNonAlloc = new Collider[12];
    public LayerMask targetMask;

    [SerializeField] private WeaponController weapon_1;
    [SerializeField] private WeaponController weapon_2;
    [SerializeField] private WeaponController weapon_3;
    [SerializeField] private WeaponController weapon_4;

    public OutpostValues values;
    private LayerMask healMask;
    //public bool isConnected;
    public List<Outpost> connections = new();

    [SerializeField] private int currentHealth;

    public int CurrentHealth
    {
        get => currentHealth;

        set {
            currentHealth = value;
            onHealthChanged.Invoke(currentHealth);
        }
    }

    public GameManager gameManager;
    private Affiliation affiliation;
    public int range;
    public UnityEvent onOutpostDestroy = new();
    public UnityEvent<ResourceHolder> onZoneCaptured = new();
    public UnityEvent<ResourceHolder> onZoneRelease = new();
    public List<ResourceHolder> zones;
    
    public UnityEvent<int> onHealthChanged = new();

    public Affiliation Affiliation
    {
        get => affiliation;
    }

    public static Outpost Create(Vector3 pos, GameObject prefab, Affiliation affiliation, GameManager gameManager)
    {
        Outpost outpost = Instantiate(prefab, pos, Quaternion.identity).GetComponent<Outpost>();
        outpost.Init(gameManager, affiliation);

        return outpost;
    }

    private void Init(GameManager gameManager, Affiliation affiliation)
    {
        this.gameManager = gameManager;
        this.affiliation = affiliation;
        CurrentHealth = values.health;
        range = values.range;

        if (affiliation == Affiliation.Blue)
        {
            healMask = LayerMask.GetMask("Allies");
            gameObject.layer = LayerMask.NameToLayer("AllyOutposts");
            targetMask = LayerMask.GetMask("Enemies");

        }

        else if (affiliation == Affiliation.Red)
        {
            healMask = LayerMask.GetMask("Enemies");
            gameObject.layer = LayerMask.NameToLayer("EnemyOutposts");
            targetMask = LayerMask.GetMask("Allies");
        }
    }

    private void Update()
    {
        GameUtils.DrawCircle(lineRenderer, range, 5, transform.position);
        EngageClosestTarget();


    }

    private void EngageClosestTarget()
    {
        int collidersNum = Physics.OverlapSphereNonAlloc(transform.position, range, targetNonAlloc, targetMask);
        if (collidersNum == 0) return;

        int random_1 = Random.Range(0, collidersNum);
        int random_2 = Random.Range(0, collidersNum);
        int random_3 = Random.Range(0, collidersNum);
        int random_4 = Random.Range(0, collidersNum);

        weapon_1.RotateWeaponAtPosition(targetNonAlloc[random_1].transform.position);
        weapon_2.RotateWeaponAtPosition(targetNonAlloc[random_2].transform.position);
        weapon_3.RotateWeaponAtPosition(targetNonAlloc[random_3].transform.position);
        weapon_4.RotateWeaponAtPosition(targetNonAlloc[random_4].transform.position);

        if (weapon_1.CheckIfFacingTarget(targetNonAlloc[random_1].transform.position))
            weapon_1.FireProjectile(targetNonAlloc[random_1]);

        if (weapon_2.CheckIfFacingTarget(targetNonAlloc[random_2].transform.position))
            weapon_2.FireProjectile(targetNonAlloc[random_2]);

        if (weapon_3.CheckIfFacingTarget(targetNonAlloc[random_3].transform.position))
            weapon_4.FireProjectile(targetNonAlloc[random_3]);

        if (weapon_4.CheckIfFacingTarget(targetNonAlloc[random_4].transform.position))
            weapon_4.FireProjectile(targetNonAlloc[random_4]);
    }



    //public void GatherResources()
    //{
    //    Collider[] colliders = Physics.OverlapSphere(transform.position, range, LayerMask.GetMask("Resource"));
    //    foreach (Collider col in colliders)
    //    {
    //        ResourceHolder zone = col.GetComponent<ResourceHolder>();

    //        if (zone.captured)
    //            continue;

    //        onZoneCaptured.Invoke(zone);
    //        zones.Add(zone);
    //    }
    //    Debug.Log(colliders.Length);
    //}

    void Start()
    {
        weapon_1.Init(this);
        weapon_2.Init(this);
        weapon_3.Init(this);
        weapon_4.Init(this);

        StartCoroutine(HealUnits());
    }

    public void Damage(Projectile projectile)
    {
        CurrentHealth -= projectile.Values.projectileDamage;

        if (currentHealth <= 0)
            Destroy(gameObject);
    }

    public void Damage(int value, AiController attacker)
    {
        CurrentHealth -= value;

        if (currentHealth <= 0)
            Destroy(gameObject);
            
    }

    private IEnumerator HealUnits()
    {
        WaitForSeconds interval = new(1);
        Collider[] colliders;

        while (true)
        {
            if (CurrentHealth < values.health)
            {
                CurrentHealth += (int)(values.health * 0.01f);

                if (CurrentHealth > values.health)
                    CurrentHealth = values.health;
            }

            colliders = Physics.OverlapSphere(transform.position, range, healMask);

            foreach (Collider collider in colliders)
            {
                if (collider.TryGetComponent(out AiController unit) && unit.UnitTaskForce.Commander == unit) // checks if this unit is a commander
                {
                    foreach (AiController member in unit.UnitTaskForce.Units)
                    {
                        if (member.Health < member.Values.health)
                            member.Health += (int)(member.Values.health * 0.1f * values.healModifier);

                        if (member.Health > member.Values.health)
                            member.Health = member.Values.health;
                    }
                }
            }

            yield return interval;
        }
    }

    private void OnDestroy()
    {
        //foreach (ResourceHolder zone in zones)
        //{
        //    onZoneRelease.Invoke(zone);
        //}
        ActivateDestroyEffects();
        onOutpostDestroy.Invoke();
    }

    //private void ScanForConnections()
    //{
    //    Collider[] colliders = Physics.OverlapSphere(transform.position, range, outpostMask);

    //    foreach (Collider collider in colliders)
    //    {
    //        Outpost connection = collider.GetComponent<Outpost>();

    //        if (connection == this)
    //            continue;

    //        AddConnection(connection);
    //        connection.AddConnection(this);
    //    }

    //    UpdateConnections();
    //}

    //private void UpdateConnections()
    //{
    //    GatherResources();

    //    Debug.Log(gameObject.name + " updating connections");
    //    if (Vector3.Distance(transform.position, headquarters.transform.position) <= headquarters.range)
    //    {
    //        isConnected = true;
    //        return;
    //    }

    //    if (connections.Count == 0)
    //    {
    //        isConnected = false;
    //        return;
    //    }

    //    else
    //    {
    //        bool connected = false;
    //        foreach (Outpost outpost in connections)
    //        {
    //            if (outpost.isConnected)
    //                connected = true;
    //        }

    //        isConnected = connected;
    //    }
    //}

    //private void AddConnection(Outpost outpost)
    //{
    //    connections.Add(outpost);
    //    outpost.onOutpostDestroy.AddListener(() => RemoveConnection(outpost));
    //}

    //private void RemoveConnection(Outpost outpost)
    //{
    //    connections.Remove(outpost);
    //    UpdateConnections();
    //}

    private void ActivateDestroyEffects()
    {
        if (explosionPrefab)
        {
            Debug.LogWarning("activating");
            explosionPrefab.transform.parent = null;
            explosionPrefab.SetActive(true);
            gameManager.AddToTemporaryCamp(explosionPrefab);
        }
    }
}
