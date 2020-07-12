using Mirror;
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

    private Rigidbody rb;

    private ProjectileController projectileController;

    void Start()
    {
        weaponManager = GetComponent<WeaponManager>();
    }

    void Update()
    {
        if (!isLocalPlayer || weaponManager == null)
            return;

        holdPosition = weaponManager.GetCurrentGraphics().firePoint.transform;

        if (heldObject == null)
        {
            if (Input.GetButton(catchButton) && !Pause.IsOn)
            {
                RaycastHit hit;
                if (Physics.SphereCast(transform.position, grabRadius, cam.transform.forward, out hit, grabDistance, layerMask))
                {
                    heldObject = hit.collider.transform.root.gameObject;
                    projectileController = heldObject.GetComponent<ProjectileController>();
                    rb = heldObject.GetComponent<Rigidbody>();


                    CmdServerAssignClient(heldObject.name);
                }
            }
        }
        else
        {
            heldObject.transform.position = holdPosition.position;
            heldObject.transform.rotation = holdPosition.rotation;

            if (Input.GetButtonDown(fireButton) && !Pause.IsOn)
            {
                CmdActivateProjectile(heldObject.name, true);

                projectileController.CmdLaunch(throwForce);

                heldObject = null;
                projectileController = null;
                rb = null;
            }
        }
    }

    void OnDisable()
    {
        if (heldObject != null)
        {
            CmdActivateProjectile(heldObject.name, true);
        }
    }

    [Command]
    void CmdServerAssignClient(string _name)
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
            RpcActivateProjectile(_name, false);
        } 
    }

    [Command]
    void CmdActivateProjectile(string _name, bool _active)
    {
        RpcActivateProjectile(_name, _active);
    }

    [ClientRpc]
    void RpcActivateProjectile(string _name, bool _active)
    {
        ProjectileController _projectileController = GameObject.Find(_name).GetComponent<ProjectileController>();

        if (_projectileController == null)
        {
            Debug.LogError("No projectile found with name " + _name);
        }
        else
        {

            foreach (Collider _collider in _projectileController.colliders)
            {
                _collider.enabled = _active;
            }

            _projectileController.rb.isKinematic = !_active;
        }
    }
}
