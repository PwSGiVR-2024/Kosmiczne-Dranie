using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ResourceProfile
{
    public int money;
    public int crystals;
    public int metals;
    public int scrap;
    public int rareMaterials;
    public List<ResourceHolder> capturedHolders;
    public void AddResource(ResourceHolder holder)
    {
        capturedHolders.Add(holder);
    }

    public void RemoveResource(ResourceHolder holder)
    {
        capturedHolders.Remove(holder);
    }
}
