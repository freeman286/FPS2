using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TurretController : PlaceableEquipmentController
{
    public GameObject target = null;
    
    [Header("Damage and Speed")]

    [SerializeField]
    private float range = 100f;

    [SerializeField]
    private float speed = 10f;

    [SerializeField]
    private float maxAngle = 90f;

    [SerializeField]
    private GameObject turret = null;

    [SerializeField]
    private GameObject impact = null;

    [SerializeField]
    private LayerMask mask = -1;

    private const string PLAYER_TAG = "Player";

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (GetComponent<NetworkIdentity>().hasAuthority)
            GameManager.RegisterTurret(gameObject);

    }

    public override void Update()
    {
        base.Update();
        if (ready && target != null)
        {
            Track();
        }
    }

    public void Kill()
    {
        CmdDie(transform.position, transform.forward);
    }

    [Command]
    void CmdDie(Vector3 _pos, Vector3 _dir)
    {
        RpcDie(_pos, Quaternion.LookRotation(_dir));
    }

    [ClientRpc]
    public void RpcDie(Vector3 _pos, Quaternion _rot)
    {
        GameObject _impact = (GameObject)Instantiate(impact, _pos, _rot);

        Destroy(_impact, 4f);
        NetworkServer.Destroy(gameObject);
    }


    void Track()
    {

        Vector3 _direction = (target.transform.position - turret.transform.position).normalized;

        RaycastHit _hit;

        if (Vector3.Angle(_direction, transform.forward) <= maxAngle && Physics.Raycast(turret.transform.position, _direction, out _hit, range, mask))
        {

            if (_hit.collider.tag == PLAYER_TAG)
            {
                Quaternion _lookAtRotation = Quaternion.LookRotation(_direction);

                if (transform.rotation != _lookAtRotation)
                {
                    turret.transform.rotation = Quaternion.RotateTowards(turret.transform.rotation, _lookAtRotation, speed * Time.deltaTime);
                }
            }
        }
        
    }
}
