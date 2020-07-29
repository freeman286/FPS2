using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MortarController : NetworkBehaviour
{
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (GetComponent<NetworkIdentity>().hasAuthority)
            GameManager.RegisterMortar(gameObject);

    }


}
