using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 10;
    public float headShotMultiplier = 1f;
    public AnimationCurve damageFallOff;
    public float range = 100f;
    public DamageType damageType;

    public List<DamageModifier> damageModifiers = new List<DamageModifier>();

    [Header("Fire Mode(s)")]
    public bool automatic;
    public float fireRate;
    public bool scoped;
    public bool special;
    public int roundsPerShot;
    public float coneOfFire;

    [Header("Projectile")]
    public float throwPower;
    public GameObject projectile;
}
