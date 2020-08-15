using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ShootEffects))]
public class ShootEffects : MonoBehaviour
{
    private PlayerShoot shoot;

    void Start()
    {
        shoot = GetComponent<PlayerShoot>();
    }

    public void LocalShootEfftect(WeaponGraphics _graphics)
    {
        if (_graphics == null)
            return;

        Animator anim = _graphics.animator;
        if (anim != null && !anim.GetCurrentAnimatorStateInfo(0).IsName("Reload"))
        {
            anim.SetTrigger("Shoot");
        }

        if (!(shoot != null && shoot.isScoped))
        {
            _graphics.muzzleFlash.Play();
        }

        GameObject _shootSound = (GameObject)Instantiate(_graphics.shootSound, _graphics.firePoint.transform.position, Quaternion.identity);
        Destroy(_shootSound, _shootSound.GetComponent<AudioSource>().clip.length);
    }
}
