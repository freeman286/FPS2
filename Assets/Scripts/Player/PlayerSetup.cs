using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Player))]
public class PlayerSetup : NetworkBehaviour
{

    [SerializeField]
    Behaviour[] componentsToDisable = null;

    [SerializeField]
    string remoteLayerName = "RemotePlayer";

    [SerializeField]
    string dontDrawLayerName = "DontDraw";

    [SerializeField]
    GameObject playerGraphics = null;

    [SerializeField]
    GameObject playerUIPrefab = null;
    [HideInInspector]
    public GameObject playerUIInstance;

    public PlayerUI ui;

    void Start()
    {
        if (!isLocalPlayer)
        {
            DisableComponents();
            AssignRemoteLayer();
            
        }
        else
        {
            Util.SetLayerRecursively(playerGraphics, LayerMask.NameToLayer(dontDrawLayerName));

            playerUIInstance = Instantiate(playerUIPrefab);
            playerUIInstance.name = playerUIPrefab.name;

            ui = playerUIInstance.GetComponent<PlayerUI>();
            if (ui == null)
                Debug.LogError("No PlayerUI component on PlayerUI prefab.");
            ui.SetPlayer(GetComponent<Player>());

            GetComponent<Player>().SetupPlayer();

            CmdSetUsername(transform.name, PlayerInfo.playerName);

            GetComponent<WeaponManager>().SyncAllWepaons();
        }
    }

    [Command]
    void CmdSetUsername(string _playerID, string _username)
    {
        Player _player = GameManager.GetPlayer(_playerID);
        if (_player != null)
        {
            _player.username = _username;
            RpcUsernameSet(_username);
        }
    }

    [ClientRpc]
    void RpcUsernameSet(string _username)
    {
        GameManager.instance.messageCallback.Invoke("<b>" + _username + "</b> joined game");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        string _netID = GetComponent<NetworkIdentity>().netId.ToString();
        Player _player = GetComponent<Player>();

        GameManager.RegisterPlayer(_netID, _player);
    }

    void AssignRemoteLayer()
    {
        Util.SetLayerRecursively(gameObject, LayerMask.NameToLayer(remoteLayerName));
    }

    void DisableComponents()
    {
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }
    }

    void OnDisable()
    {
        Destroy(playerUIInstance);

        if (isLocalPlayer)
            GameManager.instance.SetSceneCameraActive(true);

        GameManager.UnRegisterPlayer(transform.name);
    }

}
