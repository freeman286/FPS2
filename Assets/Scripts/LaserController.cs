using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LaserController : NetworkBehaviour
{
    [SyncVar]
    public string playerID;

    private bool ready = false;

    public override void OnStartClient()
    {
        base.OnStartClient();

        GameManager.RegisterParticle(GetComponent<NetworkIdentity>().netId.ToString(), gameObject);

        if (GetComponent<NetworkIdentity>().hasAuthority)
            GameManager.RegisterLaser(gameObject);

    }

}
