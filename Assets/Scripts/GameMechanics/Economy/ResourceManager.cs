using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    List<ResourceHolder> holders;

    // oddzielne profile ¿eby zachowaæ porz¹dek
    ResourceProfile playerProfile;
    ResourceProfile enemyProfile;

    void Start()
    {
        foreach (var holder in holders)
        {
            holder.onCaptured.AddListener((side) => UpdateResource(side, holder));
        }
    }

    private void UpdateResource(Outpost.OutpostSide side, ResourceHolder holder)
    {
        if (side == Outpost.OutpostSide.Enemy)
        {
            playerProfile.RemoveResource(holder);
            enemyProfile.AddResource(holder);
        }
            

        else if (side == Outpost.OutpostSide.Player)
        {
            enemyProfile.RemoveResource(holder);
            playerProfile.AddResource(holder);
        }
    }
}
