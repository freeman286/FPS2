using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Push : PlayerMovementAbility
{

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
            CmdDamage(_health.transform.name, ability.damage, transform.name, ability.damageType.name);
            timeSinceMovementAbilityUsed = ability.effectTime;
        }

    }

    [Command]
    void CmdDamage(string _healthID, int _damage, string _sourceID, string _damageType)
    {
        Health _health = GameManager.GetHealth(_healthID);
        _health.RpcTakeDamage(_damage, _sourceID, _damageType);
    }

}