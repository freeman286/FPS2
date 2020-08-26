using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class KillStreakController : NetworkBehaviour
{
    [SyncVar(hook = nameof(playerIDChanged))]
    public string playerID;

    [SerializeField]
    private TurretController turret;

    [SerializeField]
    protected KillStreak killStreak = null;

    [SerializeField]
    private GameObject impact = null;

    [SerializeField]
    protected float footprint = 2f;

    [SerializeField]
    protected LayerMask layerMask = -1;

    protected NetworkIdentity networkIdentity;

    protected float timeSinceCalledIn = 0f;

    public virtual void Start()
    {
        networkIdentity = GetComponent<NetworkIdentity>();
    }

    public virtual void Update()
    {
        timeSinceCalledIn += Time.deltaTime;
    }

    void playerIDChanged(string _oldID, string _newID)
    {
        playerID = _newID;
        
        turret.playerID = _newID;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        GameManager.RegisterKillStreak(GetComponent<NetworkIdentity>().netId.ToString(), gameObject);
    }

    void OnDestroy()
    {
        GameManager.UnRegisterKillStreak(transform.name);
    }

    public void Kill(string _sourceID)
    {
        CmdDie(transform.position, transform.forward, _sourceID);
    }

    public void Despawn()
    {
        Kill(string.Empty);
    }

    [Command]
    void CmdDie(Vector3 _pos, Vector3 _dir, string _sourceID)
    {
        RpcDie(_pos, Quaternion.LookRotation(_dir), _sourceID);
    }

    [ClientRpc]
    public void RpcDie(Vector3 _pos, Quaternion _rot, string _sourceID)
    {
        Player _source = GameManager.GetPlayer(_sourceID);

        if (_source != null)
        {
            GameManager.instance.messageCallback.Invoke("<b>" + _source.username + "</b> shot down <b>" + killStreak.name + "</b>");
            GameObject _impact = (GameObject)Instantiate(impact, _pos, _rot);
            Destroy(_impact, 4f);
        }

        NetworkServer.Destroy(gameObject);
    }

    protected RaycastHit CheckRoute(Vector3 _destination)
    {
        Vector3 _dir = _destination - transform.position;

        RaycastHit _hit;
        if (Physics.SphereCast(transform.position + _dir.normalized * footprint, footprint, _dir, out _hit, _dir.magnitude - footprint, layerMask))
        {
            return _hit;
        }

        return new RaycastHit();
    }
}
