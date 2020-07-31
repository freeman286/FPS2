using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TurretController : NetworkBehaviour
{
    [SyncVar]
    public string playerID;

    [SerializeField]
    private int maxHealth = 100;

    [SyncVar]
    private float currentHealth;

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

    public Collider[] colliders;

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
        SetDefaults();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (GetComponent<NetworkIdentity>().hasAuthority)
            GameManager.RegisterTurret(gameObject);

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
        } else if (ready && target != null)
        {
            Track();
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

    [ClientRpc]
    public void RpcTakeDamage(int _amount, string _sourceID, string _damageType)
    {

        Player sourcePlayer = GameManager.GetPlayer(_sourceID);

        currentHealth -= _amount * sourcePlayer.GetComponent<PlayerStats>().GetDamageMultiplier(_damageType, false);

        if (currentHealth <= 0)
        {
            Kill();
        }
    }

    public void SetDefaults()
    {
        currentHealth = maxHealth;

        foreach (Collider _collider in colliders)
        {
            _collider.enabled = false;
        }
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
