using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsUtil : MonoBehaviour
{
    public static PlayerWeapon[] AllWeapons()
    {
        Object[] allWeaponObjects = Util.GetPrefabs("Prefabs/Weapons");

        PlayerWeapon[] allWeapons = new PlayerWeapon[allWeaponObjects.Length];

        for (int i = 0; i < allWeaponObjects.Length; i++)
        {
            allWeapons[i] = ((GameObject)allWeaponObjects[i]).GetComponent<PlayerWeapon>();
        }

        return allWeapons;
    }

}
