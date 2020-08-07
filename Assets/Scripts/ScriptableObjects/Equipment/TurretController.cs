using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.VFX;

enum TargetMode
{
    Player,
    Projectile
}

public class TurretController : PlaceableEquipmentController
{
    public GameObject target = null;

    [Header("Operation")]

    [SerializeField]
    private TargetMode targetMode = TargetMode.Player;

    [SerializeField]
    private float speed = 10f;

    [SerializeField]
    private float trackingAngle = 0.01f;

    [SerializeField]
    private float maxAngle = 90f;

    [Header("Transforms and GameObjects")]

    [SerializeField]
    private GameObject turret = null;

    [SerializeField]
    private Transform muzzle = null;

    [SerializeField]
    private GameObject impact = null;

    [SerializeField]
    private VisualEffect muzzleFlash;

    private Weapon weapon;
    private WeaponGraphics weaponGraphics;
    private RaycastShoot raycastShoot;
    private ShootEffects shootEffects;



    [SerializeField]
    private LayerMask mask = -1;

    private const string PLAYER_TAG = "Player";

    public override void Start()
    {
        base.Start();
        weapon = GetComponent<Weapon>();
        raycastShoot = GetComponent<RaycastShoot>();
        weaponGraphics = GetComponent<WeaponGraphics>();
        shootEffects = GetComponent<ShootEffects>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (GetComponent<NetworkIdentity>().hasAuthority)
            GameManager.RegisterTurret(gameObject);

    }

    public override void Update()
    {
        base.Update();
        if (ready && target != null)
        {
            Track();
        } else if (ready)
        {
            Search();
        }
    }

    public void Kill()
    {
        CmdDie(transform.position, transform.forward);
    }

    [Command]
    void CmdDie(Vector3 _pos, Vector3 _dir)
    {
        RpcDie(_pos, Quaternion.LookRotation(_dir));
    }

    [ClientRpc]
    public void RpcDie(Vector3 _pos, Quaternion _rot)
    {
        GameObject _impact = (GameObject)Instantiate(impact, _pos, _rot);

        Destroy(_impact, 4f);
        NetworkServer.Destroy(gameObject);
    }


    void Track()
    {
        Vector3 _dir = VectorToTarget(target);

        if (_dir != Vector3.zero) {
            Quaternion _lookAtRotation = Quaternion.LookRotation(_dir);

            if (Quaternion.Angle(turret.transform.rotation, _lookAtRotation) > trackingAngle)
            {
                turret.transform.rotation = Quaternion.RotateTowards(turret.transform.rotation, _lookAtRotation, speed * Time.deltaTime);
            } else if (!IsInvoking("Shoot"))
            {
                InvokeRepeating("Shoot", 0f, 1f / weapon.fireRate);
            }

        } else
        {
            target = null;
        }
    }

    void Search()
    {
        CancelInvoke("Shoot");

        if (targetMode == TargetMode.Player)
        {
            foreach (Player _player in GameManager.GetAllPlayers())
            {
                if (_player.transform.name != playerID && VectorToTarget(_player.gameObject) != Vector3.zero)
                {
                    target = _player.gameObject;
                    return;
                }
            }
        } else if (targetMode == TargetMode.Projectile)
        {
            foreach (GameObject _projectile in GameManager.GetAllProjectile())
            {
                ExplosiveController _explosiveController = _projectile.GetComponent<ExplosiveController>();

                if (_explosiveController != null && VectorToTarget(_projectile) != Vector3.zero && _explosiveController.playerID != playerID)
                {
                    target = _projectile;
                    return;
                }
            }
        }

        target = null;
    }

    Vector3 VectorToTarget(GameObject _target)
    {
        if (_target == null)
            return Vector3.zero;

        Vector3 _direction = (_target.transform.position - turret.transform.position).normalized;

        RaycastHit _hit;

        if (Vector3.Angle(_direction, transform.forward) <= maxAngle && Physics.Raycast(turret.transform.position, _direction, out _hit, weapon.range, mask) && _hit.transform.root == _target.transform)
        {
            return _direction;
        }

        return Vector3.zero;
    }

    void Shoot()
    {
        if (!networkIdentity.hasAuthority)
            return;

        CmdOnShoot();

        raycastShoot.Shoot(muzzle, turret.transform.forward, Vector3.zero, weapon, mask, playerID);
    }

    [Command]
    public void CmdOnShoot()
    {
        RpcDoShootEfftect();
    }

    [ClientRpc]
    void RpcDoShootEfftect()
    {
        shootEffects.LocalShootEfftect(weaponGraphics);
    }
}
