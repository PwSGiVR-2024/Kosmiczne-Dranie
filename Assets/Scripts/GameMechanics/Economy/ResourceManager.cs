using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using UnityEngine.UIElements;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    public int money;
    public int crystals;
    public int metals;
    public int scrap;
    public int rareMaterials;
    public int redFactionAffiliation;//Frakcje todo na potem albo nie
    public int blueFactionAffiliation;//Frakcje todo na potem albo nie
    public TMP_Text ResourceText;
    List<ResourceHolder> holders;

    // oddzielne profile �eby zachowa� porz�dek
    ResourceProfile playerProfile;
    ResourceProfile enemyProfile;

    void Start()
    {
        /*foreach (var holder in holders)
        {
            holder.onCaptured.AddListener((side) => UpdateResource(side, holder));
        }*/
    }
    private void FixedUpdate()
    {
        ResourceCount();
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
    private void ResourceCount()
    {
        ResourceText.text = "Space Credits:" + money + " Crystals:" + crystals + " Metals:" + metals + " Scrap:" + scrap + " RareMaterials:" + rareMaterials;
    }
}
