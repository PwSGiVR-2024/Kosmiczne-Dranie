using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHeadquarters : Headquarters
{
    private void Start()
    {
        spawner.onAllyOutpostSpawned.AddListener((outpost) => outpostNetwork.Add(outpost));
    }
}
