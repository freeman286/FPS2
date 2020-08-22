using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DamageInflictor : NetworkBehaviour
{
    [Command]
    public void CmdDamage(string _healthID, int _damage, string _sourceID, string _damageType)
    {
        Health _health = GameManager.GetHealth(_healthID);
        _health.RpcTakeDamage(_damage, _sourceID, _damageType);
    }
}
