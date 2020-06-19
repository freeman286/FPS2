using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerWeapon : MonoBehaviour
{
    public string name = "pistol";

    public bool primary;

    public int damage = 10;
    public float headShotMultiplier = 1f;
    public float range = 100f;
    public AnimationCurve damageFallOff;

    public bool automatic;

    public bool projectileWeapon;

    public bool scoped;

    public float fireRate;

    public int roundsPerShot;

    public float coneOfFire;
    public float spreadDefault;
    public float spreadWhileMoving;
    public float spreadWhileJumping;
    public float spreadWhileScoped;

    public float recoilAmount;
    public float recoilTime;

    public int magSize = 20;

    public float speed = 1;
    public float scopedSpeed = 1;


    [HideInInspector]
    public int bullets;

    public float reloadTime = 1f;

    public GameObject graphics;

    public GameObject projectile;

    public void Load()
    {
        bullets = magSize;
    }

}
