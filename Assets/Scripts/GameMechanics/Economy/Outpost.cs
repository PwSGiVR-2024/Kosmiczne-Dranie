using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class Outpost : MonoBehaviour, IInteractable
{
    public LayerMask healMask;
    public int health = 100000;
    public GameManager gameManager;
    public GameObject icon;
    public Affiliation OutpostAffiliation;
    public int range = 100;
    private ResourceManager resourceManager;
    private List<int[]> resourcesToCapture = new List<int[]>();

    public void Init(GameObject icon, Vector3 iconOffset, GameManager gameManager, Affiliation affiliation)
    {
        this.icon = icon;
        icon.transform.position = transform.position + iconOffset;
        this.gameManager = gameManager;
        OutpostAffiliation = affiliation;

        if (affiliation == Affiliation.Blue)
            healMask = LayerMask.GetMask("Allies");

        else if (affiliation == Affiliation.Red)
            healMask = LayerMask.GetMask("Enemies");
    }
    private void Update()
    {
        GameUtils.DrawCircle(gameObject, range, transform);
        icon.transform.LookAt(Camera.main.transform, Vector3.up);
    }

    void Start()
    {
        resourceManager = FindObjectOfType<ResourceManager>();
        StartCoroutine(CaptureResourcesRoutine());
        StartCoroutine(HealUnits());
    }

    IEnumerator CaptureResourcesRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(60); // Czekaj minutê

            foreach (var resources in resourcesToCapture)
            {
                resourceManager.AddResources(resources);
            }

            resourcesToCapture.Clear();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ResourceZone"))
        {
            Zone zone = other.GetComponent<Zone>();
            if (zone != null)
            {
                resourcesToCapture.Add(zone.GetResources());
            }
        }
    }

    public void Damage(Projectile projectile)
    {
        health -= projectile.Values.projectileDamage;

        if (health <= 0)
        {
            Destroy(icon);
            Destroy(gameObject);
        }
    }

    public void Damage(int value, AiController attacker)
    {
        health -= value;

        if (health <= 0)
        {
            Destroy(icon);
            Destroy(gameObject);
        }
            
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
}
