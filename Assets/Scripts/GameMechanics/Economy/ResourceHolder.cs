using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResourceHolder : MonoBehaviour
{
    public int value;
    public UnityEvent<Outpost.OutpostSide> onCaptured = new();
}
