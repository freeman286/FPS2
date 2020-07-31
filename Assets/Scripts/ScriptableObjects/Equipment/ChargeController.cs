using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ChargeController : NetworkBehaviour
{
    [SyncVar]
    public string playerID;

    [SerializeField]
    private GameObject impact = null;

    [SerializeField]
    private float force = 10f;

    public Collider[] colliders;

    [SerializeField]
    private LayerMask mask = -1;

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
    private AnimationCurve damageOverAngle = null;

    private Vector3 targetPos = Vector3.zero;
    private Quaternion targetRot;
    [HideInInspector]
    public float placeSpeed;

    private bool ready = false;

    private const string PLAYER_TAG = "Player";

    private NetworkIdentity networkIdentity;

    void Start()
    {
        networkIdentity = GetComponent<NetworkIdentity>();
        foreach (Collider _collider in colliders)
        {
            _collider.enabled = false;
        }
    }

    void Update()
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

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (GetComponent<NetworkIdentity>().hasAuthority)
            GameManager.RegisterCharge(gameObject);

    }

    [ClientRpc]
    public void RpcPlace(Vector3 _pos, Quaternion _rot, float _placeSpeed)
    {
        targetPos = _pos;
        targetRot = _rot;
        placeSpeed = _placeSpeed;
    }

    public void Detonate()
    {
        if (ready)
            CmdExplode(transform.position + transform.forward * 0.01f, transform.forward);
    }

    [Command]
    void CmdExplode(Vector3 _pos, Vector3 _dir)
    {
        RpcExplode(_pos, Quaternion.LookRotation(_dir), playerID);

        Collider[] colliders = Physics.OverlapSphere(_pos, range);

        foreach (var _collider in colliders)
        {

            if (_collider.tag == PLAYER_TAG && _collider.name == "Head")
            {

                RaycastHit _hit;

                Vector3 target_vector = _collider.transform.position - _pos;

                if (Physics.Raycast(_pos, target_vector, out _hit, range, mask))
                {
                    if (_collider.tag == PLAYER_TAG)
                    {
                        float _distance = Vector3.Distance(_hit.transform.position, _pos);

                        Player player = _hit.transform.root.GetComponent<Player>();

                        if (player != null)
                        {
                            player.RpcTakeDamage((int)(damage * damageFallOff.Evaluate(_distance / range) * damageOverAngle.Evaluate(Vector3.Angle(_dir, target_vector) / 180f)), playerID, damageType.name);
                        }
                    }
                }
            }

        }

    }

    [ClientRpc]
    public void RpcExplode(Vector3 _pos, Quaternion _rot, string _playerID)
    {
        GameObject _impact = (GameObject)Instantiate(impact, _pos, _rot);

        DetonateExplosive _detonateExplosive = _impact.GetComponent<DetonateExplosive>();

        if (_detonateExplosive != null)
        {
            _detonateExplosive.playerID = _playerID;
            _detonateExplosive.Detonate();
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, range);
        foreach (Collider _collider in colliders)
        {
            Rigidbody rb = _collider.attachedRigidbody;

            if (rb != null)
                rb.AddExplosionForce(force, transform.position, range, 1.0f);
        }

        Destroy(_impact, 4f);
        NetworkServer.Destroy(gameObject);
    }
}
