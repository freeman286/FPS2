using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum DeadEyeState
{
    off,
    targeting,
    shooting
};

public class Revolver : EnableDuringRuntime
{
    [SerializeField]
    private Camera cam = null;

    [SerializeField]
    private float deadEyeSpeed = 30f;

    private DeadEyeState deadEyeState = DeadEyeState.off;
    private List<Transform> targets = new List<Transform>();

    private Transform currentTarget;

    private PlayerShoot shoot;
    private WeaponManager weaponManager;
    private PlayerMotor motor;


    void Start()
    {
        shoot = GetComponent<PlayerShoot>();
        weaponManager = GetComponent<WeaponManager>();
        motor = GetComponent<PlayerMotor>();

        GetComponent<Player>().onPlayerDieCallback += Disable;
    }

    void Update()
    {
        if (!isLocalPlayer || Pause.IsOn || weaponManager.isReloading)
            return;
        
        if (Input.GetButton("Fire2") && deadEyeState == DeadEyeState.off && shoot.CanShoot())
        {
            deadEyeState = DeadEyeState.targeting;
        }

        if (Input.GetButtonDown("Fire1") && deadEyeState == DeadEyeState.targeting && targets.Count < weaponManager.GetCurrentWeapon().bullets)
        {
            shoot.enabled = false;
            RaycastHit _hit;
            if (Physics.Raycast(cam.transform.position + cam.transform.forward * 2f, cam.transform.forward, out _hit, weaponManager.GetCurrentWeapon().range, shoot.mask)) {
                GameObject tmpTarget = new GameObject();
                tmpTarget.transform.position = _hit.point;
                tmpTarget.transform.parent = _hit.transform;
                targets.Add(tmpTarget.transform);
            }
        }
        
        if (!Input.GetButton("Fire2") && deadEyeState == DeadEyeState.targeting)
        {
            deadEyeState = DeadEyeState.shooting;
            shoot.enabled = true;
        }

        if (deadEyeState == DeadEyeState.shooting)
        {
            DoShooting();
        }

        
    }

    void DoShooting()
    {
        if (deadEyeState == DeadEyeState.shooting && targets.Count > 0)
        {
            if (currentTarget == null)
                currentTarget = targets[targets.Count - 1];


            Vector3 _dir = currentTarget.position - transform.position;

            Quaternion _lookAtRotation = Quaternion.LookRotation(_dir);

            motor.LerpRotation(Util.NormalizeAngle(_lookAtRotation.eulerAngles), deadEyeSpeed * Time.deltaTime);

            if (shoot.CanShoot() && Vector3.Angle(_dir, cam.transform.forward) < 0.1f)
            {
                shoot.Shoot();

                targets.Remove(currentTarget);
                Destroy(currentTarget.gameObject);
                currentTarget = null;
            }
        }
        else
        {
            deadEyeState = DeadEyeState.off;
        }
    }

    void Disable()
    {
        foreach(Transform _target in targets)
        {
            Destroy(_target.gameObject);
        }
        targets.Clear();
        currentTarget = null;
        deadEyeState = DeadEyeState.off;
    }

    void OnDisable()
    {
        Disable();
    }
}
