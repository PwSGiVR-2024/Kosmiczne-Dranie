using UnityEngine;

// kontener przechowuj¹cy wartoœci danej jednostki
// ka¿dy TYP jednostki posada oddzieln¹ instancjê

[CreateAssetMenu(fileName = "UnitValues", menuName = "Scriptable Objects/Unit", order = 1)]
public class UnitValues : ScriptableObject
{
    public string prefabName;

    [Header("Misc:")]
    public ShipClass shipClass;
    public int size;
    public int power;
    public string description;

    [Header("Stats:")]
    public float spotDistance;
    public int health;
    public float attackDistance;
    public int unitSpeed;
    public float acceleration;
    public float angularSpeed;

    [Header("Economy:")]
    public int crystalPrice;
    public int metalPrice;
    public int maintenancePrice;
    public int upgradePrice;
}