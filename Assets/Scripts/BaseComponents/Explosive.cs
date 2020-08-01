using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Explosive : NetworkBehaviour
{
    [SerializeField]
    private GameObject impact = null;

    [SerializeField]
    private LayerMask mask = -1;

    [SerializeField]
    private float force = 10f;

    [Header("Damage")]

    [SerializeField]
    private int damage = 100;

    [SerializeField]
    private float range = 10f;

    [SerializeField]
    private DamageType damageType;

    [SerializeField]
    private AnimationCurve damageFallOff = null;

    [SerializeField]
    private AnimationCurve damageOverTime = null;

    [SerializeField]
    private AnimationCurve damageOverAngle = null;

    private const string PLAYER_TAG = "Player";

    [Command]
    public void CmdExplode(Vector3 _pos, Vector3 _dir, float _timeSinceCreated, string _playerID)
    {
        List<Transform> _hitTransforms = new List<Transform>();

        RpcExplode(_pos, Quaternion.LookRotation(_dir), _playerID);

        Collider[] colliders = Physics.OverlapSphere(_pos, range);

        foreach (var _collider in colliders)
        {
            Health _health = _collider.transform.root.GetComponent<Health>();

            if (_health != null && !_hitTransforms.Contains(_collider.transform.root))
            {

                _hitTransforms.Add(_collider.transform.root);

                RaycastHit _hit;

                Vector3 target_vector = _collider.transform.position - _pos;

                if (Physics.Raycast(_pos, target_vector, out _hit, range, mask))
                {
                    float _distance = Vector3.Distance(_hit.transform.position, _pos);

                    _health.RpcTakeDamage((int)(damage * damageFallOff.Evaluate(_distance / range) * damageOverTime.Evaluate(_timeSinceCreated) * damageOverAngle.Evaluate(Vector3.Angle(_dir, target_vector) / 180f)), _playerID, damageType.name);
                }
            }

        }

    }

    [ClientRpc]
    public void RpcExplode(Vector3 _pos, Quaternion _rot, string _playerID)
    {
        GameObject _impact = (GameObject)Instantiate(impact, _pos, _rot);

        Collider[] colliders = Physics.OverlapSphere(_pos, range);
        foreach (Collider _collider in colliders)
        {
            Rigidbody rb = _collider.attachedRigidbody;

            if (rb != null)
                rb.AddExplosionForce(force, _pos, range, 1.0f);
        }

        Destroy(_impact, 4f);
        NetworkServer.Destroy(gameObject);
    }
}
