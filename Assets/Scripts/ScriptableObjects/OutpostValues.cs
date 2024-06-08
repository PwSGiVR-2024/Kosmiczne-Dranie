using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OutpostValues", menuName = "Scriptable Objects/Outpost", order = 1)]
public class OutpostValues : ScriptableObject
{
    public int metalPrice;
    public int crystalPrice;
    public int maintenancePrice;
    public int upgradePrice;

    public int health;
    public int range;
    public int healModifier;
}
