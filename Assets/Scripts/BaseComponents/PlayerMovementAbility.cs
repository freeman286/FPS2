using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerMovementAbility : EnableDuringRuntime
{
    public MovementAbility ability;

    protected float timeSinceMovementAbilityUsed = Mathf.Infinity;

    protected PlayerMotor motor;

    protected PlayerMetrics metrics;

    protected Rigidbody rb;

    void Start()
    {
        motor = GetComponent<PlayerMotor>();
        metrics = GetComponent<PlayerMetrics>();
        rb = GetComponent<Rigidbody>();

        GetComponent<Player>().onPlayerSetDefaultsCallback += SetDefaults;
    }

    public virtual void Update()
    {
        timeSinceMovementAbilityUsed += Time.deltaTime;

        if (isLocalPlayer && Input.GetButton("MovementAbility") && timeSinceMovementAbilityUsed >= ability.cooldown)
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
        timeSinceMovementAbilityUsed = ability.cooldown;
    }
}
