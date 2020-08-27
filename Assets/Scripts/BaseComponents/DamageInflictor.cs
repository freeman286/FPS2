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
        if (_health != null)
            _health.RpcTakeDamage(_damage, _sourceID, _damageType);
    }
}
