using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class Outpost : MonoBehaviour, IInteractable
{
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
}
