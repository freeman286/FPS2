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

    [HideInInspector]
    public GameObject laserInstance;

    private bool laserExists = false;

    private bool laserActive = false;

    void Update()
    {
        if (!isLocalPlayer)
            return;

        RaycastHit _hit;
        if (Input.GetButton(laseButton) && !Pause.IsOn)
        {
            if (Physics.Raycast(transform.position, cam.transform.forward, out _hit, range, mask))
            {
                if (!laserExists)
                {
                    CmdInstantiateLaser(_hit.point, Quaternion.LookRotation(_hit.normal), transform.name);
                    laserExists = true;
                }
                else if (laserInstance == null)
                {
                    laserInstance = GameManager.GetLaser();
                }
                else
                {
                    laserInstance.transform.position = _hit.point;
                    laserInstance.transform.rotation = Quaternion.LookRotation(_hit.normal);
                }

                if (laserInstance != null && !laserActive) {
                    CmdActivate(laserInstance.transform.name, true);
                    laserActive = true;
                }

            } else if (laserInstance != null && laserActive)
            {
                CmdActivate(laserInstance.transform.name, false);
                laserActive = false;
            }
        } else if (laserInstance != null && laserActive)
        {
            CmdActivate(laserInstance.transform.name, false);
            laserActive = false;
        }
    }

    void OnDisable()
    {
        if (isLocalPlayer && laserInstance != null && laserActive)
        {
            CmdActivate(laserInstance.transform.name, false);
            laserActive = false;
        }
    }

    [Command]
    void CmdInstantiateLaser(Vector3 _pos, Quaternion _rot, string _playerID)
    {
        GameObject _laserInstance = Instantiate(laserParticle, _pos, _rot);
        NetworkServer.Spawn(_laserInstance, connectionToClient);
        _laserInstance.GetComponent<LaserController>().playerID = _playerID;
    }

    [Command]
    void CmdActivate(string _particleID, bool _active) {
        RpcActivate(_particleID, _active);
    }

    [ClientRpc]
    void RpcActivate(string _particleID, bool _active)
    {
        GameObject _particle = GameManager.GetParticle(_particleID);

        if (_particle != null)
        {
            _particle.GetComponent<LaserController>().Activate(_active);
        }
    }
}