using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// klasa odpowiada za pooling pocisków (reusing zamiast tworzenia nowych)
// zwiêksza optymalizacjê gry
// instancja klasy powinna wchodziæ w sk³ad AiController
// AiController pobiera pociski kiedy chce, korzystaj¹c z dostêpnych metod

public class ProjectilePool
{
    private class InvalidSize : Exception
    {
        public InvalidSize(string message) : base(message) { }
    }

    private Projectile[] projectiles;
    private int getIndex = 0;
    private int putIndex = 0;
    private int length;
    private bool loggingEnabled = false;

    public Projectile[] Projectiles { get => projectiles; }

    public ProjectilePool(float projectileLifeSpan, float attackCooldown)
    {
        if (projectileLifeSpan <= 0 || attackCooldown <= 0)
            throw new InvalidSize("[ProjectilePool] Parameters must be greater than 0. Projectile pool not initialized.");

        length = (int) ((projectileLifeSpan / attackCooldown) + 1.0f);

        projectiles = new Projectile[length];
    }

    public bool TryGetProjectile(out Projectile projectile)
    {
        if (loggingEnabled)
            Debug.Log("[TryGetProjectile] Trying to get projectile at index: " + getIndex);

        if (getIndex == length)
        {
            getIndex = 0;

            if (loggingEnabled)
                Debug.Log("[TryGetProjectile] Maximum index. Going back to 0");
        }
            

        if (projectiles[getIndex] == null ||
            projectiles[getIndex].gameObject.activeSelf)
        {
            if (loggingEnabled)
                Debug.LogWarning("[TryGetProjectile] Projectile at index " + getIndex + " is null or active in the scene. Method returned false");

            projectile = null;
            return false;
        }

        else
        {
            projectile = projectiles[getIndex];
            getIndex++;

            if (loggingEnabled)
                Debug.Log("[TryGetProjectile] Projectile successfully fetched at index: " + (getIndex - 1));

            return true;
        }
    }

    public bool TryPutProjectile(Projectile projectile)
    {
        if (loggingEnabled)
            Debug.Log("[TryPutProjectileInPool] Trying to put projectile at index: " + putIndex);

        if (putIndex == length)
        {
            if (loggingEnabled)
                Debug.LogWarning("[TryPutProjectileInPool] Maximum pool size reached. Pool size: " + length + ". Method returned false and projectile will be destroyed");

            projectile.DestroyAfterDeactivated();
            return false;
        }
            

        if (projectiles[putIndex] != null)
        {
            if (loggingEnabled)
                Debug.LogWarning("[TryPutProjectileInPool] Projectile already exists at index: " + putIndex + ". Method returned false and projectile will be destroyed");

            projectile.DestroyAfterDeactivated();
            return false;
        }
            

        projectiles[putIndex] = projectile;

        if (loggingEnabled)
            Debug.Log("[TryPutProjectileInPool] Projectile successfully placed in the pool at index: " + putIndex);

        putIndex++;
        return true;
    }

    public void Clean()
    {
        for (int i = 0; i < projectiles.Length; i++)
        {
            if (projectiles[i] == null)
                continue;

            if (projectiles[i].gameObject.activeSelf)
                projectiles[i].ShotBy.GameManager.AddToTemporaryCamp(projectiles[i].gameObject);

            else
                projectiles[i].ShotBy.GameManager.AddToExterminationCamp(projectiles[i].gameObject);
        }
    }

    public void EnableLogging()
    {
        loggingEnabled = true;
    }

    public void DisableLogging()
    {
        loggingEnabled = false;
    }

    public bool CheckIfloggingEnabled()
    {
        return loggingEnabled;
    }

}
