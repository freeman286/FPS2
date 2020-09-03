using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DamageUtil : MonoBehaviour
{
    public static DamageType[] AllDamageTypes()
    {
        DamageType[] allDamageTypes = Resources.LoadAll<DamageType>("ScriptableObjects/DamageTypes");

        return allDamageTypes;
    }

    public static float GetDamageModifier(List<DamageModifier> _damageModifiers, HealthType _healthType)
    {
        DamageModifier _damageModifier = _damageModifiers.FirstOrDefault(x => x.healthType == _healthType);

        if (_damageModifier == null)
        {
            return 1;
        }
        else
        {
            return _damageModifier.multiplier;
        }
    }
}
