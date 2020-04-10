using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerWeapon
{
    public string name = "pistol";

    public int damage = 10;
    public float range = 100f;

    public float fireRate;

    public GameObject graphics;

}
