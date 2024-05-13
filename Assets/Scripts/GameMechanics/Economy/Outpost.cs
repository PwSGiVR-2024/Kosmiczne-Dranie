using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Outpost : MonoBehaviour
{
    public enum OutpostSide { Enemy, Player }
    public int range = 100;
    private OutpostSide side;

    // kiedy outpost zostanie zainstancjonowany, szuka wszystkich zasob�w w pobli�u
    // w znalezionych holderach, wywo�uje event
    void Start()
    {
        List<ResourceHolder> capturedHolders = FindAllHolders();

        foreach (var holder in capturedHolders)
        {
            holder.onCaptured.Invoke(side);
        }
        Collider[] colliders=Physics.OverlapSphere(transform.position, range,LayerMask.GetMask("Resources"));
        for (int i=0;i<colliders.Length;i++)
        {
            colliders[i].GetComponent<ResourceHolder>().onCaptured.Invoke(side);
        }
        
    }

    // musi znale�� wsystkie holdery w range
    List<ResourceHolder> FindAllHolders()
    {
        return new List<ResourceHolder>();
    }
}
