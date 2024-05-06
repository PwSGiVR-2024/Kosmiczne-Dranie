using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Outpost : MonoBehaviour
{
    public enum OutpostSide { Enemy, Player }
    public int range = 100;
    private OutpostSide side;

    // kiedy outpost zostanie zainstancjonowany, szuka wszystkich zasobów w pobli¿u
    // w znalezionych holderach, wywo³uje event
    void Start()
    {
        List<ResourceHolder> capturedHolders = FindAllHolders();

        foreach (var holder in capturedHolders)
        {
            holder.onCaptured.Invoke(side);
        }
    }

    // musi znaleŸæ wsystkie holdery w range
    List<ResourceHolder> FindAllHolders()
    {
        return new List<ResourceHolder>();
    }
}
