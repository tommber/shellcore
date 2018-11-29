﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Every entity that can move is a craft. This includes drones and ShellCores.
/// </summary>
public abstract class Craft : MonoBehaviour {

    protected float[] currentHealth;
    protected float[] maxHealth;
    protected float[] regenRate;
    protected Ability[] abilities; // abilities
    public int enginePower; // craft's engine power, determines how fast it goes
    public Rigidbody2D craftBody; // craft to modify with this script
    private static readonly float reciprocalSqrt2 = 1 / Mathf.Sqrt(2); // store this cancerous value for unit vector specification
    // the following are diagonal vectors used for better precision on rotation
    private Vector2 upRight = new Vector2(reciprocalSqrt2, reciprocalSqrt2);
    private Vector2 bottomRight = new Vector2(reciprocalSqrt2, -reciprocalSqrt2);
    private Vector2 upLeft = new Vector2(-reciprocalSqrt2, reciprocalSqrt2);
    private Vector2 bottomLeft = new Vector2(-reciprocalSqrt2, -reciprocalSqrt2);
    public Collider2D hitbox; // the hitbox of the core (excluding extra parts)
    protected TargetingSystem targeter;
    protected bool isInCombat;
    protected bool isBusy;
    protected bool isDead;
    protected bool isImmobile;
    protected float busyTimer;
    protected float combatTimer;
    public GameObject explosionCirclePrefab;
    public GameObject explosionLinePrefab;
    protected Vector3 spawnPoint;

    public bool GetIsDead() {
        return isDead;
    }

    protected void OnDeath() {
        isDead = true;
        isImmobile = true;
        GameObject deadShell = transform.Find("Shell Sprite").gameObject;
        if (deadShell)
        {
            deadShell.GetComponent<ShellPart>().Detach();
        }
        GameObject tmp = Instantiate(explosionCirclePrefab);
        tmp.transform.SetParent(transform, false);
        Destroy(tmp, 2);
        for (int i = 0; i < 3; i++) {
            tmp = Instantiate(explosionLinePrefab);
            tmp.transform.SetParent(transform, false);
            Destroy(tmp, 2);
        }
    }

    virtual protected void Awake() {
        currentHealth = new float[3];
        maxHealth = new float[3];
        regenRate = new float[3];
        isBusy = true;
        targeter = new TargetingSystem();
        isInCombat = false;
    }

    virtual protected void Start()
    {
        GetComponentInChildren<MinimapLockRotationScript>().Initialize();
        transform.rotation = Quaternion.identity;
        var tmp = GetComponent<SpriteRenderer>();
        if (tmp) {
            tmp.enabled = true;
        }
        busyTimer = 0;
    }

    protected virtual void Update() {
        TickState();
    }

    protected void RegenHealth(ref float currentHealth, float regenRate, float maxHealth) {
        if (currentHealth + regenRate * Time.deltaTime <= maxHealth)
        {
            currentHealth += regenRate * Time.deltaTime;
        }
        else
        {
            currentHealth = maxHealth;
        }
    }

    protected virtual void Respawn() {
        isDead = false;
        isBusy = false;
        isImmobile = false;
        transform.rotation = Quaternion.identity;
        foreach (Transform child in transform) {
            child.transform.rotation = Quaternion.identity;
            var tmp = child.gameObject.GetComponent<ShellPart>();
            if (tmp) {
                tmp.Start();
            }
        }
        Start();
    }

    protected void TickState() {

        if (currentHealth[1] <= 0 && !isDead) {
            OnDeath();
            MakeBusy();
        }
        if (isDead) {
            if (busyTimer >= 1) {
                GetComponent<SpriteRenderer>().enabled = false;
                transform.Find("Minimap Image").GetComponent<SpriteRenderer>().enabled = false;
            }
            if (busyTimer >= 2) {
                Respawn();
            }
        }
        if (targeter.GetTarget() != null) {
            RotateCraft(targeter.GetTarget().transform.position - transform.position);
        }
        RegenHealth(ref currentHealth[0], regenRate[0], maxHealth[0]);
        RegenHealth(ref currentHealth[2], regenRate[2], maxHealth[2]);

        combatTimer += Time.deltaTime;
        busyTimer += Time.deltaTime;
        if (busyTimer > 5) {
            isBusy = false;
        }
        if (combatTimer > 5) {
            isInCombat = false;
        }
    }

    public void MakeBusy() {
        isBusy = true;
        busyTimer = 0;
    }

    public bool GetIsBusy() {
        return isBusy;
    }

    public void SetIntoCombat() {
        isInCombat = true;
        isBusy = true;
        busyTimer = 0;
        combatTimer = 0;
    }

    public bool GetIsInCombat()
    {
        return isInCombat;
    }

    public Ability[] GetAbilities() {
        return abilities;
    }

    public TargetingSystem GetTargetingSystem() {
        return targeter;
    }

    public Transform GetTransform() {
        return transform;
    }

    public float[] GetHealth() {
        return currentHealth;
    }

    public float[] GetMaxHealth() {
        return maxHealth;
    }
    /* /// <summary>
    /// Get the current shell
    /// </summary>
    /// <returns>shell</returns>
    public float GetShell() {
        return currentHealth[0];
    }

    /// <summary>
    /// Get the max shell
    /// </summary>
    /// <returns>max shell</returns>
    public float GetShellMax() {
        return maxHealth[0];
    }

    /// <summary>
    /// Get the current core
    /// </summary>
    /// <returns>core</returns>
    public float GetCore()
    {
        return currentHealth[1];
    }


    /// <summary>
    /// Get the max core
    /// </summary>
    /// <returns>max core</returns>
    public float GetCoreMax()
    {
        return maxHealth[1];
    }

    /// <summary>
    /// Get the current energy
    /// </summary>
    /// <returns>energy</returns>
    public float GetEnergy()
    {
        return currentHealth[2];
    }

    /// <summary>
    /// Get the max energy
    /// </summary>
    /// <returns>max energy</returns>
    public float GetEnergyMax()
    {
        return maxHealth[2];
    }*/

    /// <summary>
    /// The method that moves the craft based on the integer input it receives
    /// Movement tries to emulate original Shellcore Command movement (specifically episode 1) but is not perfect
    /// </summary>
    /// <param name="direction">integer that specifies the direction of movement</param>
    protected void MoveCraft(int direction)
    {
        if (!isImmobile)
        {
            switch (direction)
            {
                // switch based on the direction
                // although it seems like these case statements run extremely similar code, I decided not to give a crap since it would be painful
                // to initialize multiple integers just to dunk them into one piece of code. Besides, this makes actually seeing the logic pretty fun
                case 1: // northeast
                    CraftMover(upRight);
                    break;
                case 2: // northwest
                    CraftMover(upLeft);
                    break;
                case 3: // north
                    CraftMover(Vector2.up);
                    break;
                case 4: // southeast
                    CraftMover(bottomRight);
                    break;
                case 5: // southwest
                    CraftMover(bottomLeft);
                    break;
                case 6: // south
                    CraftMover(-Vector2.up);
                    break;
                case 7: // west
                    CraftMover(-Vector2.right);
                    break;
                case 8: // east
                    CraftMover(Vector2.right);
                    break;
            }
        }
    }
    private void RotateCraft(Vector2 directionVector) {
        float angle = Vector2.Angle(directionVector, Vector2.up);
        if (directionVector.x < 0)
        {
            angle = -angle;
        }
        if ((int)(Vector2.Angle(craftBody.transform.right, directionVector)) > 90) // if this is true move the craft ANTICLOCKWISE (+ve is anticlockwise)
        {
            craftBody.transform.Rotate(0, 0, 2 * enginePower / craftBody.mass * Time.deltaTime);
            if ((int)(Vector2.Angle(craftBody.transform.right, directionVector)) < 90) // check if the rotation went too far anticlockwise, reset if it did
            {
                craftBody.transform.rotation = Quaternion.Euler(0, 0, -angle);
            }
        }
        else if ((int)(Vector2.Angle(craftBody.transform.right, directionVector)) < 90 || Vector2.Angle(craftBody.transform.up, directionVector) > 0) // if this is true move the craft CLOCKWISE (-ve is clockwise)
        {
            craftBody.transform.Rotate(0, 0, -2 * enginePower / craftBody.mass * Time.deltaTime); // check if the rotation went too far clockwise, reset if it did
            if ((int)(Vector2.Angle(craftBody.transform.right, directionVector)) > 90)
            {
                craftBody.transform.rotation = Quaternion.Euler(0, 0, -angle);
            }
        }
    }
    /// <summary>
    /// Applies a force to the craft on the vector given
    /// </summary>
    /// <param name="directionVector">vector given</param>
    private void CraftMover(Vector2 directionVector)
    {
        if (targeter.GetTarget() == null)
        {
            RotateCraft(directionVector);
        }
        craftBody.AddForce(enginePower * directionVector); // actual force applied to craft; independent of angle rotation
        // the nested if structure that resets if the core went too far seems optimizable, I'm just too tired in the night. For now it works with all cases.
    }

    /// <summary>
    /// Check if craft is moving
    /// </summary>
    /// <returns></returns>
    public virtual bool IsMoving() {
        return craftBody.velocity != Vector2.zero;
    }

    public void TakeDamage(float amount, float shellPiercingFactor) {
        currentHealth[0] -= amount * (1 - shellPiercingFactor);
        if (currentHealth[0] < 0) {
            currentHealth[1] += currentHealth[0];
            currentHealth[0] = 0;
        }
        currentHealth[1] -= amount * shellPiercingFactor;
    }

    public void TakeEnergy(float amount) {
        currentHealth[2] -= amount;
    }
}
