using UnityEngine;

public abstract class Headquarters : MonoBehaviour, IInteractable
{
    public LineRenderer lineRenderer;

    public int range;
    public int health;
    //public List<Outpost> outpostNetwork = new();
    //public FleetManager fleetManager;
    //public Spawner spawner;

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

    // Update is called once per frame
    void Update()
    {
        GameUtils.DrawCircle(lineRenderer, range, 5, transform.position);
    }
}
