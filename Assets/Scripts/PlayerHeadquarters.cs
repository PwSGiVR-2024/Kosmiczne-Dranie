using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHeadquarters : MonoBehaviour, IInteractable
{
    public int range;
    public int health;
    public List<Outpost> outpostNetwork;

    public void Damage(int dmg, AiController attacker)
    {
        health -= dmg;

        if (health <= 0)
            Destroy(gameObject);
    }

    public void Damage(Projectile projectile)
    {
        health -= projectile.Values.projectileDamage;

        if (health <= 0)
            Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GameUtils.DrawCircle(gameObject, range, transform);
    }
}
