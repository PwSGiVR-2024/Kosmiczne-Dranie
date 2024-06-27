using System;
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
    private int index = 0;
    private bool loggingEnabled = false;

    public Projectile[] Projectiles { get => projectiles; }

    public ProjectilePool(int size, WeaponController weapon)
    {
        if (size < 1)
            throw new InvalidSize("[ProjectilePool] Size must be greater than 0. Projectile pool not initialized.");

        projectiles = new Projectile[size];

        for (int i = 0; i < size; i++)
        {
            projectiles[i] = Projectile.Create(weapon);
            projectiles[i].gameObject.SetActive(false);
        }
    }

    public Projectile GetProjectile()
    {
        if (loggingEnabled)
            Debug.Log("[TryGetProjectile] Trying to get projectile at index: " + index);

        if (index == projectiles.Length)
        {
            if (loggingEnabled)
                Debug.Log("[TryGetProjectile] Maximum index. Going back to 0");

            index = 0;
            return GetProjectile();
        }

        else if (projectiles[index].gameObject.activeInHierarchy)
        {
            if (loggingEnabled)
                Debug.LogWarning("[TryGetProjectile] Projectile at index " + index + " is active in the scene");

            index++;
            return GetProjectile();
        }

        else
        {
            if (loggingEnabled)
                Debug.Log("[TryGetProjectile] Projectile successfully fetched at index: " + index);

            index++;
            projectiles[index - 1].gameObject.SetActive(true);
            return projectiles[index - 1];
        }
    }

    public void Clean()
    {
        for (int i = 0; i < projectiles.Length; i++)
        {
            if (projectiles[i].gameObject.activeInHierarchy)
                projectiles[i].gameManager.AddToTemporaryCamp(projectiles[i].gameObject);

            else
                projectiles[i].gameManager.AddToExterminationCamp(projectiles[i].gameObject);
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
}
