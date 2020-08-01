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

    public virtual void Start()
    {
        networkIdentity = GetComponent<NetworkIdentity>();
        foreach (Collider _collider in colliders)
        {
            _collider.enabled = false;
        }
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
            ready = true;
            foreach (Collider _collider in colliders)
            {
                _collider.enabled = true;
            }
        }
    }

    [ClientRpc]
    public void RpcPlace(Vector3 _pos, Quaternion _rot, float _placeSpeed)
    {
        targetPos = _pos;
        targetRot = _rot;
        placeSpeed = _placeSpeed;
    }
}
