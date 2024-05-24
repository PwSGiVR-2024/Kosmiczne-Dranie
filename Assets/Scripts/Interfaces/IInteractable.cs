using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IInteractable
{
    public abstract void Damage(int dmg, AiController attacker);
    public abstract void Damage(Projectile projectile);
}
