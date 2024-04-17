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

    private GameObject[] pool;
    private int getIndex = 0;
    private int putIndex = 0;
    private int length;
    private bool loggingEnabled = false;

    public ProjectilePool(float projectileLifeSpan, float attackCooldown)
    {
        if (projectileLifeSpan <= 0 || attackCooldown <= 0)
            throw new InvalidSize("[ProjectilePool] Parameters must be greater than 0. Projectile pool not initialized.");

        length = (int) ((projectileLifeSpan / attackCooldown) + 1.0f);

        pool = new GameObject[length];
    }

    public bool TryGetProjectile(out GameObject projectile)
    {
        if (loggingEnabled)
            Debug.Log("[TryGetProjectile] Trying to get projectile at index: " + getIndex);

        if (getIndex == length)
        {
            getIndex = 0;

            if (loggingEnabled)
                Debug.Log("[TryGetProjectile] Maximum index. Going back to 0");
        }
            

        if (pool[getIndex] == null ||
            pool[getIndex].activeSelf)
        {
            if (loggingEnabled)
                Debug.LogWarning("[TryGetProjectile] Projectile at index " + getIndex + " is null or active in the scene. Method returned false");

            projectile = null;
            return false;
        }

        else
        {
            projectile = pool[getIndex];
            getIndex++;

            if (loggingEnabled)
                Debug.Log("[TryGetProjectile] Projectile successfully fetched at index: " + (getIndex - 1));

            return true;
        }
    }

    public bool TryPutProjectileInPool(GameObject projectile)
    {
        if (loggingEnabled)
            Debug.Log("[TryPutProjectileInPool] Trying to put projectile at index: " + putIndex);

        if (putIndex == length)
        {
            if (loggingEnabled)
                Debug.LogWarning("[TryPutProjectileInPool] Maximum pool size reached. Pool size: " + length + ". Method returned false and projectile will be destroyed");

            projectile.GetComponent<Projectile>().MarkToDestroy();
            return false;
        }
            

        if (pool[putIndex] != null)
        {
            if (loggingEnabled)
                Debug.LogWarning("[TryPutProjectileInPool] Projectile already exists at index: " + putIndex + ". Method returned false and projectile will be destroyed");

            projectile.GetComponent<Projectile>().MarkToDestroy();
            return false;
        }
            

        pool[putIndex] = projectile;

        if (loggingEnabled)
            Debug.Log("[TryPutProjectileInPool] Projectile successfully placed in the pool at index: " + putIndex);

        putIndex++;
        return true;
    }

    public void DestroyProjectiles()
    {
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i] != null)
                pool[i].GetComponent<Projectile>().MarkToDestroy();

        }
    }

    public GameObject[] GetPool() { return pool; }

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
