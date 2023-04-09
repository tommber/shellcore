﻿using UnityEngine;
/// <summary>
/// Temporarily increases the craft's damage multiplier
/// </summary>
public class DamageBoost : ActiveAbility
{
    public const float damageAddition = 100;
    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        ID = AbilityID.DamageBoost;
        cooldownDuration = 20;
        activeDuration = 5;
        energyCost = 200;
    }

    /// <summary>
    /// Returns the engine power to the original value
    /// </summary>
    public override void Deactivate()
    {
        if (Core)
        {
            Core.DamageBoostStacks -= Mathf.Max(1, abilityTier);
            base.Deactivate();
        }
    }

    public override void ActivationCosmetic(Vector3 targetPos)
    {

        AudioManager.PlayClipByID("clip_buff", targetPos);
        base.ActivationCosmetic(targetPos);
    }

    /// <summary>
    /// Increases core engine power to speed up the core
    /// </summary>
    protected override void Execute()
    {
        if (Core)
        {
            Core.DamageBoostStacks += Mathf.Max(1, abilityTier);
            ActivationCosmetic(transform.position);
            base.Execute();
        }
    }
}
