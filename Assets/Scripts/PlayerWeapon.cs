using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerWeapon : MonoBehaviour
{
    public new string name = "pistol";

    public bool primary;

    [Header("Damage")]
    public int damage = 10;
    public float headShotMultiplier = 1f;
    public AnimationCurve damageFallOff;
    public float range = 100f;

    [Header("Fire Mode(s)")]
    public bool automatic;
    public float fireRate;
    public bool scoped;
    public bool special;
    public int roundsPerShot;

    [Header("Projectile")]
    public bool projectileWeapon;
    public float throwPower;
    public GameObject projectile;

    [Header("Spread")]
    public float coneOfFire;
    public float spreadDefault;
    public float spreadWhileMoving;
    public float spreadWhileJumping;
    public float spreadWhileScoped;
    public float timeTillMaxSpread;
    public AnimationCurve spreadCurve;

    [Header("Recoil")]
    public float recoilAmount;
    public float horizontalRecoilMultiplier;
    public float recoilTime;

    [Header("Misc")]
    public int magSize = 20;
    public float speed = 1;
    public float scopedSpeed = 1;

    [HideInInspector]
    public int bullets;

    public float reloadTime = 1f;

    public void Load()
    {
        bullets = magSize;
    }

}
