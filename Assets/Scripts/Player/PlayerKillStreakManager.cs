﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerKillStreakManager : NetworkBehaviour
{
    [SerializeField]
    private ListType listType = null;

    private KillStreak killStreak;

    private Player player;

    void Start()
    {
        player = GetComponent<Player>();
        player.onPlayerSetDefaultsCallback += SetDefaults;
        GameManager.instance.onPlayerKilledCallback += OnKill;
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

        for (int i = 0; i < killStreak.instanceNumber; i++)
        {
            CmdSpawnKillStreak(transform.name, killStreak.name);
            yield return new WaitForSeconds(killStreak.spawnDelay);
        }

    }

    void SetDefaults()
    {
        killStreak = KillStreakUtil.NameToKillStreak(PlayerInfo.GetNameSelected(listType));
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
