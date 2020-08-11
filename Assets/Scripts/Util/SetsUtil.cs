using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetsUtil : MonoBehaviour
{
    public static Set[] AllSets()
    {
        Set[] allSets = Resources.LoadAll<Set>("ScriptableObjects/Sets");

        return allSets;
    }

    public static DamageType[] AllDamageDamageTypes()
    {
        DamageType[] allDamageDamageTypes = Resources.LoadAll<DamageType>("ScriptableObjects/DamageTypes");

        return allDamageDamageTypes;
    }

    public static DamageType[] DamageTypes()
    {
        DamageType[] allDamageDamageTypes = Resources.LoadAll<DamageType>("ScriptableObjects/DamageTypes");

        return allDamageDamageTypes;
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
}
