﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The main bullet ability that every shellcore has as a basic ability (this will inherit from the base bullet ability)
/// </summary>
public class MainBullet : WeaponAbility {

    public GameObject bulletPrefab; // the prefabbed sprite for a bullet with a BulletScript
    private float bulletSpeed; // the speed of the bullet
    private float survivalTime; // the time the bullet takes to delete itself


    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        bulletSpeed = 50; 
        survivalTime = 0.5F;
        range = bulletSpeed * survivalTime;
        ID = 3;
        cooldownDuration = 0.4F;
        CDRemaining = cooldownDuration;
        energyCost = 10;
    }

    /// <summary>
    /// Fires the bullet using the helper method
    /// </summary>
    /// <param name="victimPos">The position to fire the bullet to</param>
    protected override bool Execute(Vector3 victimPos)
    {
        if (Core.GetTargetingSystem().GetTarget() != null) // check if there is actually a target, do not fire if there is not
        {
            Entity targetCraft = Core.GetTargetingSystem().GetTarget().GetComponent<Entity>();
            if (targetCraft && targetCraft.faction != Core.faction) // check what type the target is & check the faction
            {
                FireBullet(victimPos); // fire if there is
                isOnCD = true; // set on cooldown
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Helper method for Execute() that creates a bullet and modifies it to be shot
    /// </summary>
    /// <param name="targetPos">The position to fire the bullet to</param>
    void FireBullet(Vector3 targetPos)
    {
        Vector3 originPos = part ? part.transform.position : Core.transform.position;
        // Create the Bullet from the Bullet Prefab
        var bullet = Instantiate(bulletPrefab, originPos + Vector3.Normalize(targetPos - originPos) * 1.5F, Quaternion.identity);

        // Update its damage to match main bullet
        bullet.GetComponent<BulletScript>().SetDamage(100);

        bullet.GetComponent<BulletScript>().SetShooterFaction(Core.faction);

        // Add velocity to the bullet
        bullet.GetComponent<Rigidbody2D>().velocity = Vector3.Normalize(targetPos - originPos) * bulletSpeed;

        // Destroy the bullet after survival time
        Destroy(bullet, survivalTime);
    }
}
