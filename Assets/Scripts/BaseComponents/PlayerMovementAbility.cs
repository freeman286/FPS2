﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerMovementAbility : EnableDuringRuntime
{
    public MovementAbility ability;

    protected float timeSinceMovementAbilityUsed = Mathf.Infinity;

    protected PlayerMotor motor;

    protected PlayerMetrics metrics;

    protected Rigidbody rb;

    private bool active = true;

    void Start()
    {
        motor = GetComponent<PlayerMotor>();
        metrics = GetComponent<PlayerMetrics>();
        rb = GetComponent<Rigidbody>();

        Player _player = GetComponent<Player>();
        _player.onPlayerSetDefaultsCallback += SetDefaults;
        _player.onPlayerDieCallback += Die;
    }

    public virtual void LateUpdate()
    {
        timeSinceMovementAbilityUsed += Time.deltaTime;

        if (Pause.IsOn)
            return;

        if (isLocalPlayer && active && Input.GetButton("MovementAbility") && timeSinceMovementAbilityUsed >= ability.cooldown)
        {
            DoAbility();
        }
    }

    public virtual void DoAbility()
    {
        Debug.LogError("No ability assigned");
    }

    void SetDefaults()
    {
        active = true;
        timeSinceMovementAbilityUsed = ability.cooldown;
    }

    void Die()
    {
        active = false;
    }
}
