using UnityEngine;

// kontener przechowuj¹cy wartoœci danej jednostki
// ka¿dy TYP jednostki posada oddzieln¹ instancjê

[CreateAssetMenu(fileName = "UnitValues", menuName = "ScriptableObjects/Unit", order = 1)]
public class UnitValues : ScriptableObject
{
    public string prefabName;

    public float spotDistance;
    public int health;
    public int xp;
    public float attackDistance;
    public int unitSpeed;
    public float acceleration;
    public float angularSpeed;
    public string description;
}