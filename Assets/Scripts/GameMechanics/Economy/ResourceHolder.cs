using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

public class ResourceHolder : MonoBehaviour
{
    public enum ResourceType { Crystals, Metals, Credits }
    public ResourceType resourceType;
    public int value;
    public UnityEvent<Affiliation> onCapture = new();
}