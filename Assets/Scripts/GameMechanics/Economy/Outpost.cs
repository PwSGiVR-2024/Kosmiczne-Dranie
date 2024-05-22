using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Outpost : MonoBehaviour
{
    public enum OutpostSide { Enemy, Player }
    public OutpostSide side;
    public int range = 100;
    private ResourceManager resourceManager;
    private List<int[]> resourcesToCapture = new List<int[]>();

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
}
