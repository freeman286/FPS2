using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;

public class Stats : NetworkBehaviour
{
    public List<DamageAttribute> damageAttributes = new List<DamageAttribute>();

    public float GetDamageMultiplier(string _damageType, bool _self)
    {
        DamageAttribute _damageAttribute = damageAttributes.FirstOrDefault(x => x.damageType.name == _damageType && x.self == _self);

        if (_damageAttribute == null)
        {
            return 1;
        }
        else
        {
            return _damageAttribute.multiplier;
        }

    }
}
