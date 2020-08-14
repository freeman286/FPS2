using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ChargeController : PlaceableEquipmentController
{

    private Explosive explosive;

    public override void Start()
    {
        base.Start();
        explosive = GetComponent<Explosive>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (GetComponent<NetworkIdentity>().hasAuthority)
            GameManager.RegisterCharge(gameObject);

    }

    public void Detonate()
    {
        if (ready)
            explosive.CmdExplode(transform.position + transform.forward * 0.01f, transform.forward, 0f, playerID);
    }

    public void ForceDetonate(string _sourceID)
    {
        playerID = _sourceID;

        Detonate();
    }
}
