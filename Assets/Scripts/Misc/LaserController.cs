using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LaserController : NetworkBehaviour
{
    [SyncVar]
    public string playerID;

    public bool active = true;

    public override void OnStartClient()
    {
        base.OnStartClient();

        GameManager.RegisterParticle(GetComponent<NetworkIdentity>().netId.ToString(), gameObject);

        if (GetComponent<NetworkIdentity>().hasAuthority)
            GameManager.RegisterLaser(gameObject);

    }

    public void Activate(bool _active)
    {
        active = _active;

        foreach (Transform _child in transform)
        {
            _child.gameObject.SetActive(_active);
        }
        
    }

    void OnDestroy()
    {
        GameManager.UnRegisterParticle(transform.name);
    }

}
