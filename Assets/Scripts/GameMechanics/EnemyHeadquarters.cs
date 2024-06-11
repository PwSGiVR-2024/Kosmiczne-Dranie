using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHeadquarters : MonoBehaviour, IInteractable
{
    GameManager manager;

    private enum TargetPriority { Low, Medium, High }
    private enum Objective { Defend, Attack }
    private enum Behaviour { Defensive, Offensive }



    public int range;
    public int health;
    public List<Outpost> outpostNetwork = new();
    public List<TaskForceController> taskForces = new();

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

    private void EvaluateStatus()
    {
    }
}
