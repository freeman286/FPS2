using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SetsUtil : MonoBehaviour
{
    public static Set[] AllSets()
    {
        Set[] allSets = Resources.LoadAll<Set>("ScriptableObjects/Sets");

        return allSets;
    }

    public static bool SetMatch(Set _set, string _primaryWeaponName, string _secondaryWeaponName, string _equipmentName, string _movementAbilityName)
    {
        GameObject _primaryWeapon = _set.primaryWeapon;
        GameObject _secondaryWeapon = _set.secondaryWeapon;
        Equipment _equipment = _set.equipment;
        MovementAbility _movementAbility = _set.movementAbility;

        if ((_primaryWeapon != null && _primaryWeapon.name != _primaryWeaponName) ||
            (_secondaryWeapon != null && _secondaryWeapon.name != _secondaryWeaponName) ||
            (_equipment != null && _equipment.name != _equipmentName) ||
            _movementAbility != null && _movementAbility.name != _movementAbilityName)
            return false;

        return true;
    }

    public static float GetDamageMultiplier(List<DamageAttribute> _damageAttributes, string _damageType, bool _self)
    {
        DamageAttribute _damageAttribute = _damageAttributes.FirstOrDefault(x => x.damageType.name == _damageType && x.self == _self);

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
