using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerWeapon : MonoBehaviour
{
    public string name = "pistol";

    public int damage = 10;
    public float range = 100f;

    public bool automatic;

    public float fireRate;

    public int magSize = 20;
    [HideInInspector]
    public int bullets;

    public float reloadTime = 1f;

    public GameObject graphics;

    public void Load()
    {
        bullets = magSize;
    }

}
