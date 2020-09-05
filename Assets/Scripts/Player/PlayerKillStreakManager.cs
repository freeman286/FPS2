using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerKillStreakManager : NetworkBehaviour
{
    [SerializeField]
    private ListType listType = null;

    private KillStreak killStreak;

    private Player player;

    public delegate void OnKillStreakChangedCallback(Sprite icon);
    public OnKillStreakChangedCallback onKillStreakChangedCallback;

    public float GetKillStreakPct()
    {
        if (killStreak == null)
            return 1;

        GameObject[] _killSteaks = GameManager.GetPlayersKillStreaks(transform.name);

        if (_killSteaks.Length > 0)
        {
            float _totalHealth = 0f;
            foreach(GameObject _killSteak in _killSteaks)
            {
                if (_killSteak.GetComponent<KillStreakController>().killStreak == killStreak)
                    _totalHealth += _killSteak.GetComponent<Health>().GetHealthPct();
            }

            if (_totalHealth > 0f)
                return Mathf.Clamp(_totalHealth / killStreak.instanceNumber, 0, 1);
        }

        return Mathf.Clamp((float)player.killStreak / killStreak.kills, 0, 1);
    }

    void Start()
    {
        player = GetComponent<Player>();
        player.onPlayerSetDefaultsCallback += SetDefaults;
        GameManager.instance.onPlayerKilledCallback += OnKill;
    }

    void Update()
    {
        if (isLocalPlayer && Input.GetKeyDown("k"))
        {
            StartCoroutine(KillStreakCoroutine());
        }
    }


    public void OnKill(string _playerID, string _sourceID)
    {
        if (isLocalPlayer && _sourceID == transform.name && player.killStreak == killStreak.kills)
        {
            StartCoroutine(KillStreakCoroutine());
        }
    }

    private IEnumerator KillStreakCoroutine()
    {
        CmdAnnounceKillStreak(transform.name, killStreak.name);

        yield return new WaitForSeconds(killStreak.spawnDelay);

        for (int i = 0; i < killStreak.instanceNumber; i++)
        {
            CmdSpawnKillStreak(transform.name, killStreak.name);
            yield return new WaitForSeconds(killStreak.spawnInterval);
        }

    }

    void SetDefaults()
    {
        if (isLocalPlayer)
        {
            killStreak = KillStreakUtil.NameToKillStreak(PlayerInfo.GetNameSelected(listType));
            onKillStreakChangedCallback.Invoke(killStreak.icon);
        }
    }

    [Command]
    void CmdSpawnKillStreak(string _playerID, string _killStreakName)
    {
        KillStreak _killStreak = KillStreakUtil.NameToKillStreak(_killStreakName);

        if (_killStreak != null)
        {
            Transform _spawnPos = KillStreakManager.GetKillStreakSpawnPoint(_killStreak);

            GameObject _killStreakPrefab = (GameObject)Instantiate(_killStreak.prefab, _spawnPos.position, _spawnPos.rotation);
            NetworkServer.Spawn(_killStreakPrefab, connectionToClient);

            KillStreakController _killStreakController = _killStreakPrefab.GetComponent<KillStreakController>();
            _killStreakController.playerID = _playerID;

            Health _health = _killStreakPrefab.GetComponent<Health>();
            _health.playerID = _playerID;
        }
            
    }

    [Command]
    void CmdAnnounceKillStreak(string _playerID, string _killStreakName)
    {
        Player _player = GameManager.GetPlayer(_playerID);

        if (_player == null)
            return;

        _player.killStreak = 0;

        RpcAnnounceKillStreak(_playerID, _killStreakName);
    }

    [ClientRpc]
    void RpcAnnounceKillStreak(string _playerID, string _killStreakName)
    {
        Player _player = GameManager.GetPlayer(_playerID);

        if (_player != null)
            GameManager.instance.messageCallback.Invoke("<b>" + _player.username + "</b> called in <b>" + _killStreakName + "</b>");
    }

    void OnDestroy() {
        player.onPlayerSetDefaultsCallback -= SetDefaults;
        GameManager.instance.onPlayerKilledCallback -= OnKill;
    }
}
