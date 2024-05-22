using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    public int crystals;
    public int metals;
    public int scrap;
    public int rareMaterials;
    public int money;
    public TMP_Text resourceText;

    void Start()
    {
        UpdateResourceText();
    }

    public void AddResources(int[] resources)
    {
        crystals += resources[0];
        metals += resources[1];
        scrap += resources[2];
        rareMaterials += resources[3];
        money += resources[0] + resources[1] + resources[2] + resources[3]/10;

        UpdateResourceText();
    }

    private void UpdateResourceText()
    {
        resourceText.text = $"Space Credits: {money} Crystals: {crystals} Metals: {metals} Scrap: {scrap} Rare Materials: {rareMaterials}";
    }
}