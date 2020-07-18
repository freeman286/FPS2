using UnityEngine;
using System.Linq;
using System.Collections;
using Mirror;

public class ProjectLaser : NetworkBehaviour
{
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private LayerMask mask;

    [SerializeField]
    private string laseButton = "Fire2";

    [SerializeField]
    private float range;

    [SerializeField]
    private GameObject laserParticle;

    //[HideInInspector]
    public GameObject laserInstance;

    private bool laserExists = false;

    void Update()
    {
        if (!isLocalPlayer)
            return;

        RaycastHit _hit;
        if (Input.GetButton(laseButton) && !Pause.IsOn && Physics.Raycast(transform.position, cam.transform.forward, out _hit, range, mask))
        {
            if (!laserExists)
            {
                CmdInstantiateLaser(_hit.point, Quaternion.LookRotation(_hit.normal), transform.name);
                laserExists = true;
            } else if (laserInstance == null)
            {
                laserInstance = GameManager.GetLaser();
            }
            else
            {
                laserInstance.transform.position = _hit.point;
                laserInstance.transform.rotation = Quaternion.LookRotation(_hit.normal);
            }
        }

        if (laserInstance != null && Input.GetButtonDown(laseButton) && !Pause.IsOn)
        {
            CmdActivate(laserInstance.transform.name, true);
        }

        if (laserInstance != null && (Input.GetButtonUp(laseButton)))
        {
            CmdActivate(laserInstance.transform.name, false);
        }
    }

    void OnDisable()
    {
        if (isLocalPlayer)
            CmdActivate(laserParticle.transform.name, false);
    }

    [Command]
    void CmdInstantiateLaser(Vector3 _pos, Quaternion _rot, string _playerID)
    {
        GameObject _laserInstance = Instantiate(laserParticle, _pos, _rot);
        NetworkServer.Spawn(_laserInstance, connectionToClient);
        _laserInstance.GetComponent<LaserController>().playerID = _playerID;
    }

    [Command]
    void CmdActivate(string _particleID, bool _active)
    {
        RpcActivate(_particleID, _active);
    }

    [ClientRpc]
    void RpcActivate(string _particleID, bool _active)
    {
        Debug.Log(_particleID);

        GameObject _particle = GameManager.GetParticle(_particleID);

        if (_particle != null)
        {
            foreach (Transform _child in _particle.transform)
            {
                _child.gameObject.SetActive(_active);
            }
        }
    }
}