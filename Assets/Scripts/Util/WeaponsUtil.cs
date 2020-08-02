using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class WeaponsUtil : MonoBehaviour
{
    public static PlayerWeapon[] AllWeapons()
    {
        GameObject[] allWeaponObjects = Util.GetPrefabs("Assets/Resources/Prefabs/Weapons");

        PlayerWeapon[] allWeapons = new PlayerWeapon[allWeaponObjects.Length];

        for (int i = 0; i < allWeaponObjects.Length; i++)
        {
            allWeapons[i] = ((GameObject)allWeaponObjects[i]).GetComponent<PlayerWeapon>();
        }

        return allWeapons;
    }

    public static PlayerWeapon NameToWeapon(string _name)
    {
        PlayerWeapon[] allWeapons = AllWeapons();

        foreach (var weapon in allWeapons)
        {
            if (weapon.name == _name)
            {
                return weapon;
            }
        }
        return null;
    }

    public static GameObject[] AllProjectiles()
    {

        GameObject[] _projectiles = Util.GetPrefabs("Assets/Resources/Prefabs/Projectiles");

        GameObject[] _grenades = Util.GetPrefabs("Assets/Resources/Prefabs/Equipment/Grenades");

        return _projectiles.Concat(_grenades).ToArray();
    }

    public static GameObject NameToProjectile(string _name)
    {
        GameObject[] allProjectiles = AllProjectiles();

        foreach (var projectile in allProjectiles)
        {
            if (projectile.name == _name)
            {
                return projectile;
            }
        }
        return null;
    }

}
