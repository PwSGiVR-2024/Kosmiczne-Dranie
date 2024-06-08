using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Unity.VisualScripting;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Rendering.CameraUI;

public class Outpost : MonoBehaviour, IInteractable
{
    public OutpostValues values;
    private LayerMask healMask;
    public bool isConnected;
    public PlayerHeadquarters headquarters;
    public List<Outpost> connections = new();
    public LayerMask outpostMask;

    [SerializeField] private int currentHealth;

    public int CurrentHealth
    {
        get => currentHealth;

        set {
            currentHealth = value;
            onHealthChanged.Invoke(currentHealth);
        }
    }

    // public int maxHealth;

    private GameManager gameManager;
    //public GameObject icon;
    private Affiliation affiliation;
    public int range;
    public UnityEvent onOutpostDestroy = new();
    public UnityEvent<Zone> onZoneCaptured = new();
    public UnityEvent<Zone> onZoneRelease = new();
    public List<Zone> zones;
    
    public UnityEvent<int> onHealthChanged = new();

    public Affiliation Affiliation
    {
        get => affiliation;
    }

    public static Outpost Create(Vector3 pos, GameObject prefab, Affiliation affiliation, GameManager gameManager, PlayerHeadquarters headquarters)
    {
        Outpost outpost = Instantiate(prefab, pos, Quaternion.identity).GetComponent<Outpost>();
        outpost.Init(gameManager, affiliation, headquarters);

        return outpost;
    }

    private void Init(GameManager gameManager, Affiliation affiliation, PlayerHeadquarters headquarters)
    {
        this.gameManager = gameManager;
        this.affiliation = affiliation;
        CurrentHealth = values.health;
        range = values.range;
        this.headquarters = headquarters;

        if (affiliation == Affiliation.Blue)
        {
            healMask = LayerMask.GetMask("Allies");
            outpostMask = LayerMask.GetMask("AllyOutposts");
        }

        else if (affiliation == Affiliation.Red)
        {
            healMask = LayerMask.GetMask("Enemies");
            outpostMask = LayerMask.GetMask("EnemyOutposts");
        }

        
    }

    private void Update()
    {
        GameUtils.DrawCircle(gameObject, range, transform);
    }
    public void GatherResources()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, range, LayerMask.GetMask("Resource"));
        foreach (Collider col in colliders)
        {
            Zone zone = col.GetComponent<Zone>();

            if (zone.captured)
                continue;

            onZoneCaptured.Invoke(zone);
            zones.Add(zone);
        }
        Debug.Log(colliders.Length);
    }
    void Start()
    {
        GatherResources();
        StartCoroutine(HealUnits());
        ScanForConnections();
    }

    public void Damage(Projectile projectile)
    {
        currentHealth -= projectile.Values.projectileDamage;

        if (currentHealth <= 0)
            Destroy(gameObject);
    }

    public void Damage(int value, AiController attacker)
    {
        currentHealth -= value;

        if (currentHealth <= 0)
            Destroy(gameObject);
            
    }

    private IEnumerator HealUnits()
    {
        WaitForSeconds interval = new(1);
        Collider[] colliders;

        while (true)
        {
            if (currentHealth < values.health)
            {
                currentHealth += (int)(values.health * 0.01f);

                if (currentHealth > values.health)
                    currentHealth = values.health;
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
        foreach (Zone zone in zones)
        {
            onZoneRelease.Invoke(zone);
        }

        onOutpostDestroy.Invoke();
    }

    private void ScanForConnections()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, range, outpostMask);

        foreach (Collider collider in colliders)
        {
            Outpost connection = collider.GetComponent<Outpost>();

            if (connection == this)
                continue;

            AddConnection(connection);
            connection.AddConnection(this);
        }

        UpdateConnections();
    }

    private void UpdateConnections()
    {
        GatherResources();

        Debug.Log(gameObject.name + " updating connections");
        if (Vector3.Distance(transform.position, headquarters.transform.position) <= headquarters.range)
        {
            isConnected = true;
            return;
        }

        if (connections.Count == 0)
        {
            isConnected = false;
            return;
        }

        else
        {
            bool connected = false;
            foreach (Outpost outpost in connections)
            {
                if (outpost.isConnected)
                    connected = true;
            }

            isConnected = connected;
        }
    }

    private void AddConnection(Outpost outpost)
    {
        connections.Add(outpost);
        outpost.onOutpostDestroy.AddListener(() => RemoveConnection(outpost));
    }

    private void RemoveConnection(Outpost outpost)
    {
        connections.Remove(outpost);
        UpdateConnections();
    }
}
