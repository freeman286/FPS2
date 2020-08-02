using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : Weapon
{
    public new string name = "pistol";

    public bool primary;

    [Header("Spread")]
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

    public string[] scriptsToEnable;
}
