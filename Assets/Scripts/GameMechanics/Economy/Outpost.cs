using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.PlayerSettings;

public class Outpost : MonoBehaviour, IInteractable
{
    public LayerMask healMask;

    private int health;

    public int Health
    {
        get => health;

        set {
            health = value;
            onHealthChanged.Invoke(health);
        }
    }

    public int maxHealth;

    public GameManager gameManager;
    //public GameObject icon;
    public Affiliation Affiliation;
    public int range = 100;
    private List<int[]> resourcesToCapture = new List<int[]>();
    public UnityEvent onOutpostDestroy = new();
    public UnityEvent<Zone> onZoneCaptured = new();
    public UnityEvent<Zone> onZoneRelease = new();
    public List<Zone> zones;
    
    public UnityEvent<int> onHealthChanged = new();

    public static Outpost Create(Vector3 pos, GameObject prefab, Affiliation affiliation, GameManager gameManager)
    {
        Outpost outpost = Instantiate(prefab, pos, Quaternion.identity).GetComponent<Outpost>();
        outpost.Init(gameManager, affiliation);

        return outpost;
    }

    private void Init(GameManager gameManager, Affiliation affiliation)
    {
        this.gameManager = gameManager;
        Affiliation = affiliation;

        if (affiliation == Affiliation.Blue)
            healMask = LayerMask.GetMask("Allies");

        else if (affiliation == Affiliation.Red)
            healMask = LayerMask.GetMask("Enemies");

        // temporary
        Health = 10000;
        maxHealth = 10000;
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
            onZoneCaptured.Invoke(zone);
            zones.Add(zone);
        }
        Debug.Log(colliders.Length);
    }
    void Start()
    {
        GatherResources();
        StartCoroutine(HealUnits());
    }

    public void Damage(Projectile projectile)
    {
        health -= projectile.Values.projectileDamage;

        if (health <= 0)
            Destroy(gameObject);
    }

    public void Damage(int value, AiController attacker)
    {
        health -= value;

        if (health <= 0)
            Destroy(gameObject);
            
    }

    private IEnumerator HealUnits()
    {
        WaitForSeconds interval = new(1);
        Collider[] colliders;

        while (true)
        {
            colliders = Physics.OverlapSphere(transform.position, range, healMask);

            foreach (Collider collider in colliders)
            {
                if (collider.TryGetComponent(out AiController unit) && unit.UnitTaskForce.Commander == unit) // checks if this unit is a commander
                {
                    foreach (AiController member in unit.UnitTaskForce.Units)
                    {
                        if (member.Health < member.Values.health)
                            member.Health += (int)(member.Values.health * 0.1f);

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
}
