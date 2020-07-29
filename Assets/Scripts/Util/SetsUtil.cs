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

    public static bool SetMatch(Set _set, string _primaryWeaponName, string _secondaryWeaponName)
    {
        GameObject _primaryWeapon = _set.primaryWeapon;
        GameObject _secondaryWeapon = _set.secondaryWeapon;

        if ((_primaryWeapon != null && _primaryWeapon.name != _primaryWeaponName) ||
            (_secondaryWeapon != null && _secondaryWeapon.name != _secondaryWeaponName))
            return false;

        return true;
    }
}
