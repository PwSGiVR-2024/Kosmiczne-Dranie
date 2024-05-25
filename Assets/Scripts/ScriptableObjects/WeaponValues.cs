using UnityEngine;

// kontener przechowuj¹cy wartoœci broni, której dotyczy

[CreateAssetMenu(fileName = "WeaponValues", menuName = "Scriptable Objects/Weapon", order = 1)]
public class WeaponValues : ScriptableObject
{
    public string prefabName;

    public int projectileDamage;
    public int projectileSpeed;
    public float projectileLifeSpan;
    public float attackCooldown;
    public float angleError;

    public string description;

    public GameObject projectile;
}