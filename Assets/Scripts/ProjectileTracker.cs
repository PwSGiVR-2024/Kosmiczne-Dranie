using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Jobs;

public static class ProjectileTracker
{
    // Keeps track of every active projectile in the scene
    // WeaponController must be registered using ProjectileTracker.RegisterWeapon() for the projectiles to be tracked
    // WeaponController must have projectiles initialized in ProjectilePool

    public static List<WeaponTypeData> weaponsData { get; } = new();

    // Statistics ----------------------------------------------------
    public static int currentTrackedActive { get; private set; } = 0;
    public static int currentTrackedAll { get; private set; } = 0;
    public static int maxTrackedActive { get; private set; } = 0;
    public static int maxTrackedAll { get; private set; } = 0;
    public static int registeredEver { get; private set; } = 0;
    // ----------------------------------------------------------------

    // this struct represents data of every TYPE of weapon, NOT every weapon in the scene
    // type of weapon is determined by WeaponValues referenced in this weapon
    // if they are the same, data (projectiles) of each weapon is stored in single struct, even if weapons are from different sides (allies, enemies)
    // it means that single struct can contain data of multiple weapons
    public struct WeaponTypeData
    {
        public WeaponValues weaponValues; // this represents weapon type
        public HashSet<Projectile> activeProjectiles; // every active projectile of this weapon type

        public WeaponTypeData(WeaponController weapon)
        {
            weaponValues = weapon.Values;
            activeProjectiles = new HashSet<Projectile>();

            Extend(weapon);
        }

        // Adds every projectile of the weapon to HashSet
        public void Extend(WeaponController weapon)
        {
            foreach (var proj in weapon.Pool.Projectiles)
            {
                AddTracker(proj);
            }
        }

        // Based on projectile events, keeps track of this projectile
        private void AddTracker(Projectile projectile)
        {
            registeredEver++;
            currentTrackedAll++;

            if (currentTrackedAll > maxTrackedAll)
                maxTrackedAll = currentTrackedAll;

            projectile.OnProjectileEnable += AddToActive;
            projectile.OnProjectileDisable += RemoveFromActive;
            projectile.OnProjectileDestroy += RemoveTracker;

            if (projectile.gameObject.activeInHierarchy)
                activeProjectiles.Add(projectile);
        }

        private void RemoveFromActive(Projectile projectile)
        {
            activeProjectiles.Remove(projectile);
            currentTrackedActive--;
        }


        private void AddToActive(Projectile projectile)
        {
            activeProjectiles.Add(projectile);
            currentTrackedActive++;

            if (currentTrackedActive > maxTrackedActive)
                maxTrackedActive = currentTrackedActive;
        }

        private void RemoveTracker(Projectile projectile)
        {
            currentTrackedAll--;
            projectile.OnProjectileEnable -= AddToActive;
            projectile.OnProjectileDisable -= RemoveFromActive;
        }

        public Transform[] GetTransforms()
        {
            Transform[] transforms = new Transform[activeProjectiles.Count];
            int i = 0;
            foreach (Projectile proj in activeProjectiles)
            {
                transforms[i] = proj.transform;
                i++;
            }
            return transforms;
        }

        public TransformAccessArray GetTransformAccessArray()
        {
            TransformAccessArray array = new TransformAccessArray(activeProjectiles.Count);
            foreach (Projectile proj in activeProjectiles)
            {
                array.Add(proj.transform);
            }
            return array;
        }
    }

    // creates new record for the weapon, or adds projectiles from the weapon to existing record
    public static void RegisterWeapon(WeaponController weapon)
    {
        if (weaponsData.Count == 0)
        {
            weaponsData.Add(new WeaponTypeData(weapon));
            return;
        }

        foreach (var data in weaponsData)
        {
            if (data.weaponValues == weapon.Values)
            {
                data.Extend(weapon);
                return;
            }
        }

        weaponsData.Add(new WeaponTypeData(weapon));
    }
}
