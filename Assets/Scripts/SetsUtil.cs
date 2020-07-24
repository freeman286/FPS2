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

    public static bool SetMatch(Set _set, WeaponManager _weaponManager)
    {
        GameObject _primaryWeapon = _set.primaryWeapon;
        GameObject _secondaryWeapon = _set.secondaryWeapon;

        if ((_primaryWeapon != null && _primaryWeapon.GetComponent<PlayerWeapon>() != _weaponManager.primaryWeapon) ||
            (_secondaryWeapon != null && _secondaryWeapon.GetComponent<PlayerWeapon>() != _weaponManager.secondaryWeapon))
            return false;

        return true;
    }
}
