using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Push : PlayerMovementAbility
{

    private Health health;

    public override void Start()
    {
        base.Start();
        health = GetComponent<Health>();
    }

    public override void DoAbility()
    {
        timeSinceMovementAbilityUsed = 0f;
    }

    public override void LateUpdate()
    {
        base.LateUpdate();

        if (timeSinceMovementAbilityUsed < ability.effectTime)
        {
            motor.Move(transform.forward * ability.magnitude);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isLocalPlayer || timeSinceMovementAbilityUsed >= ability.effectTime)
            return;

        Health _health = collision.transform.root.GetComponent<Health>();

        if (_health != null)
        {
            health.CmdDamage(_health.transform.name, ability.damage, transform.name, ability.damageType.name);
            timeSinceMovementAbilityUsed = ability.effectTime;
        }

    }

}