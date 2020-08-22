using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class KillStreakController : NetworkBehaviour
{
    [SyncVar]
    public string playerID;

    [SerializeField]
    private KillStreak killStreak = null;

    [SerializeField]
    private GameObject impact = null;

    protected NetworkIdentity networkIdentity;

    public virtual void Start()
    {
        networkIdentity = GetComponent<NetworkIdentity>();
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

    [Command]
    void CmdDie(Vector3 _pos, Vector3 _dir, string _sourceID)
    {
        RpcDie(_pos, Quaternion.LookRotation(_dir), _sourceID);
    }

    [ClientRpc]
    public void RpcDie(Vector3 _pos, Quaternion _rot, string _sourceID)
    {
        GameObject _impact = (GameObject)Instantiate(impact, _pos, _rot);

        GameManager.instance.messageCallback.Invoke("<b>" + GameManager.GetPlayer(_sourceID).username + "</b> shot down <b>" + killStreak.name + "</b>");

        Destroy(_impact, 4f);
        NetworkServer.Destroy(gameObject);
    }
}
