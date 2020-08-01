using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlaceableEquipmentController : NetworkBehaviour
{

    [SyncVar]
    public string playerID;

    public Collider[] colliders;

    private Vector3 targetPos = Vector3.zero;
    private Quaternion targetRot;
    [HideInInspector]
    public float placeSpeed;

    [HideInInspector]
    public bool ready = false;

    private NetworkIdentity networkIdentity;

    public override void OnStartClient()
    {
        base.OnStartClient();

        GameManager.RegisterEquipment(GetComponent<NetworkIdentity>().netId.ToString(), gameObject);
    }

    public virtual void Start()
    {
        networkIdentity = GetComponent<NetworkIdentity>();
        CmdReady(false);
    }

    public virtual void Update()
    {
        if (!ready && targetPos != Vector3.zero)
        {
            if (networkIdentity.hasAuthority)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * placeSpeed);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * placeSpeed);
            }
        }

        if (transform.position == targetPos)
        {
            CmdReady(true);
        }
    }

    [ClientRpc]
    public void RpcPlace(Vector3 _pos, Quaternion _rot, float _placeSpeed)
    {
        targetPos = _pos;
        targetRot = _rot;
        placeSpeed = _placeSpeed;
    }

    void OnDestroy()
    {
        GameManager.UnRegisterEquipment(transform.name);
    }

    [Command]
    void CmdReady(bool _ready)
    {
        RpcReady(_ready);
    }

    [ClientRpc]
    void RpcReady(bool _ready)
    {
        ready = _ready;
        foreach (Collider _collider in colliders)
        {
            _collider.enabled = _ready;
        }
    }
}
