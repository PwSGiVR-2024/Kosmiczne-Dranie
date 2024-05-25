using UnityEngine;

// kontener przechowuj�cy warto�ci broni, kt�rej dotyczy

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