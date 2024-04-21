using UnityEngine;

// kontener przechowuj¹cy wartoœci danej jednostki
// ka¿dy TYP jednostki posada oddzieln¹ instancjê

[CreateAssetMenu(fileName = "Unit", menuName = "ScriptableObjects/Unit", order = 1)]
public class Unit : ScriptableObject
{
    public string prefabName;

    public float spotDistance;
    public int health;
    public int xp;
    public int projectileDamage;
    public int projectileSpeed;
    public float projectileLifeSpan;
    public float attackCooldown;
    public float attackDistance;
    public int unitSpeed;
    public float stoppingDistance;
    public float angleError;
    public float acceleration;
    public string description;

    public GameObject projectile;
}