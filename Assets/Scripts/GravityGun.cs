﻿using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityGun : NetworkBehaviour
{
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private string fireButton = "Fire1";
    [SerializeField]
    private string catchButton = "Fire2";
    [SerializeField]
    private float grabDistance = 10.0f;
    [SerializeField]
    private float grabRadius = 1.0f;

    private Transform holdPosition;
    private WeaponManager weaponManager;

    [SerializeField]
    private float throwForce = 100.0f;
    [SerializeField]
    private ForceMode throwForceMode;
    [SerializeField]
    private LayerMask layerMask = -1;

    private GameObject heldObject;

    private ProjectileController projectileController;
    private PlayerShoot shoot;

    void Start()
    {
        shoot = GetComponent<PlayerShoot>();
        weaponManager = GetComponent<WeaponManager>();
    }

    void Update()
    {
        if (!isLocalPlayer || weaponManager == null)
            return;

        holdPosition = weaponManager.GetCurrentGraphics().firePoint.transform;

        if (heldObject == null)
        {
            if (Input.GetButton(catchButton) && shoot.CanShoot() && !Pause.IsOn)
            {
                RaycastHit hit;
                if (Physics.SphereCast(transform.position, grabRadius, cam.transform.forward, out hit, grabDistance, layerMask))
                {
                    heldObject = hit.collider.transform.root.gameObject;
                    if (!string.IsNullOrEmpty(heldObject.name))
                    {
                        projectileController = heldObject.GetComponent<ProjectileController>();


                        CmdServerAssignClient(heldObject.name, transform.name);
                    }
                }
            }
        }
        else
        {
            heldObject.transform.position = holdPosition.position;
            heldObject.transform.rotation = holdPosition.rotation;

            if (Input.GetButton(fireButton) && shoot.CanShoot() && !Pause.IsOn)
            {
                CmdActivateProjectile(heldObject.name, transform.name, true);

                projectileController.CmdLaunch(throwForce);

                heldObject = null;
                projectileController = null;

                shoot.CmdOnShoot();
                shoot.LocalShootEfftect();
                shoot.timeSinceShot = 0;
            }
        }
    }

    void OnDisable()
    {
        if (heldObject != null)
        {
            CmdActivateProjectile(heldObject.name, transform.name, true);
            heldObject = null;
            projectileController = null;
        }
    }

    [Command]
    void CmdServerAssignClient(string _name, string _playerID)
    {
        GameObject obj = GameObject.Find(_name);

        if (obj == null)
        {
            Debug.LogError("No object found with name " + _name);
        }
        else
        {
            obj.GetComponent<NetworkIdentity>().RemoveClientAuthority();
            obj.GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
            RpcActivateProjectile(_name, _playerID, false);
        } 
    }

    [Command]
    void CmdActivateProjectile(string _name, string _playerID, bool _active)
    {
        RpcActivateProjectile(_name, _playerID, _active);
    }

    [ClientRpc]
    void RpcActivateProjectile(string _name, string _playerID, bool _active)
    {

        GameObject _projectile = GameObject.Find(_name);

        if (_projectile == null)
        {
            Debug.LogError("No game object found with name " + _name);
        }

        ProjectileController _projectileController = _projectile.GetComponent<ProjectileController>();

        if (_projectileController == null)
        {
            Debug.LogError("No projectile found with name " + _name);
        }
        else
        {

            _projectileController.Activate(_playerID, _active);

            foreach (Collider _collider in _projectileController.colliders)
            {
                _collider.enabled = _active;
            }

            _projectileController.rb.isKinematic = !_active;
        }
    }
}
