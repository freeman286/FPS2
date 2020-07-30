using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DetonateExplosive : NetworkBehaviour
{

    [SyncVar]
    public string playerID;

    [SerializeField]
    private float range = 10f;

    [SerializeField]
    private float offset = 0.01f;

    [SerializeField]
    private LayerMask mask = -1;

    private const string PROJECTILE_TAG = "Projectile";
    private const string EQUIPMENT_TAG = "Equipment";

    public void Detonate()
    {

        Vector3 _pos = transform.position + transform.forward * offset;

        Collider[] _colliders = Physics.OverlapSphere(_pos, range);

        foreach (var _collider in _colliders)
        {

            if (_collider.tag == PROJECTILE_TAG || _collider.tag == EQUIPMENT_TAG)
            {       

                RaycastHit _hit;

                Vector3 target_vector = _collider.transform.position - _pos;

                if (Physics.Raycast(_pos, target_vector, out _hit, range, mask))
                {

                    if (_collider.tag == PROJECTILE_TAG)
                    {

                        ExplosiveController _explosiveController = _hit.transform.root.GetComponent<ExplosiveController>();

                        if (_explosiveController != null)
                        {
                            _explosiveController.playerID = playerID;
                            _explosiveController.Detonate();
                        }


                    } else if (_collider.tag == EQUIPMENT_TAG)
                    {

                        ChargeController _chargeController = _hit.transform.root.GetComponent<ChargeController>();

                        if (_chargeController != null)
                        {
                            _chargeController.playerID = playerID;
                            _chargeController.Detonate();
                        }

                    }

                }
            }

        }
    }

}
