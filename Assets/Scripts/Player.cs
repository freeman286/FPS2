﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerSetup))]
public class Player : NetworkBehaviour
{
    [SyncVar]
    private bool _isDead = false;
    public bool isDead
    {
        get { return _isDead; }
        protected set { _isDead = value; }
    }

    [SerializeField]
    private int maxHealth = 100;

    [SyncVar]
    private int currentHealth;

    public float GetHealthPct()
    {
        return Mathf.Clamp((float)currentHealth / maxHealth, 0, 1);
    }

    [SerializeField]
    private Behaviour[] disableOnDeath;
    private bool[] wasEnabled;

    [SerializeField]
    private GameObject[] disableGameObjectsOnDeath;

    [SerializeField]
    private Collider[] disableCollidersOnDeath;

    [SerializeField]
    private GameObject spawnEffect;

    private bool firstSetup = true;

    [SerializeField]
    private PlayerShoot shoot;

    [SyncVar]
    public int kills;

    [SyncVar]
    public int deaths;

    [SyncVar]
    public string username;

    public WeaponManager weaponManager;
    public PlayerController playerController;

    [Header("Rigidbody On Death Info")]

    [SerializeField]
    private GameObject[] rigidbodyOnDeath;

    [SerializeField]
    private Vector3[] rigidbodyPosition;

    [SerializeField]
    private float[] rigidbodyMass;

    public void SetupPlayer()
    {

        if (isLocalPlayer)
        {
            GameManager.instance.SetSceneCameraActive(false);
            GetComponent<PlayerSetup>().ui.Alive();
        }

        CmdBroadcastNewPlayerSetup();
    }

    [Command]
    private void CmdBroadcastNewPlayerSetup ()
    {
        RpcSetupPlayerOnAllClients();
    }

    [ClientRpc]
    private void RpcSetupPlayerOnAllClients ()
    {
        if (firstSetup)
        {
            wasEnabled = new bool[disableOnDeath.Length];
            for (int i = 0; i < wasEnabled.Length; i++)
            {
                wasEnabled[i] = disableOnDeath[i].enabled;
            }

            firstSetup = false;
        }
        SetDefaults();
    }

    void Update()
    {
        if (!isLocalPlayer)
            return;

        if (Input.GetKeyDown(KeyCode.K))
        {
            RpcTakeDamage(10, transform.name);
        }
    }


    [ClientRpc]
    public void RpcTakeDamage(int _amount, string _sourceID)
    {
        if (isDead)
            return;

        currentHealth -= _amount;

        if (currentHealth <= 0)
        {
            CmdDie(_sourceID);
        }
    }

    [Command]
    private void CmdDie(string _sourceID)
    {
        RpcDie(_sourceID);
    }

    [ClientRpc]
    private void RpcDie(string _sourceID)
    {
        if (isDead)
            return;

        isDead = true;

        shoot.CancelInvoke("Shoot");

        Player sourcePlayer = GameManager.GetPlayer(_sourceID);
        if (sourcePlayer != null)
        {
            sourcePlayer.kills++;
            GameManager.instance.onPlayerKilledCallback.Invoke(username, sourcePlayer.username);
        }

        

        deaths++;

        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = false;
        }

        for (int i = 0; i < disableGameObjectsOnDeath.Length; i++)
        {
            disableGameObjectsOnDeath[i].SetActive(false);
        }

        for (int i = 0; i < disableCollidersOnDeath.Length; i++)
        {
            disableCollidersOnDeath[i].enabled = false;
        }

        for (int i = 0; i < rigidbodyOnDeath.Length; i++)
        {
            Rigidbody rigidbody = rigidbodyOnDeath[i].AddComponent<Rigidbody>();
            rigidbody.mass = rigidbodyMass[i];
            rigidbody.drag = 1;
            rigidbody.velocity = Vector3.zero;
        }

        if (isLocalPlayer)
        {
            GameManager.instance.SetSceneCameraActive(true);
            GetComponent<PlayerSetup>().ui.Death();
        }

        StartCoroutine(Respawn());
    }


    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(GameManager.instance.matchSettings.respawnTime);

        Transform _spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = _spawnPoint.position;
        transform.rotation = _spawnPoint.rotation;

        yield return new WaitForSeconds(0.1f);

        SetupPlayer();
    }

    public void SetDefaults()
    {
        isDead = false;

        currentHealth = maxHealth;

        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = wasEnabled[i];
        }

        for (int i = 0; i < disableGameObjectsOnDeath.Length; i++)
        {
            disableGameObjectsOnDeath[i].SetActive(true);
        }

        for (int i = 0; i < disableCollidersOnDeath.Length; i++)
        {
            disableCollidersOnDeath[i].enabled = true;
        }

        for (int i = 0; i < rigidbodyOnDeath.Length; i++)
        {
            Destroy(rigidbodyOnDeath[i].GetComponent<Rigidbody>());
            rigidbodyOnDeath[i].transform.localPosition = rigidbodyPosition[i];
            rigidbodyOnDeath[i].transform.localRotation = Quaternion.identity;
        }


        GameObject _spawnEffect = (GameObject)Instantiate(spawnEffect, transform.position, Quaternion.identity);
        Destroy(_spawnEffect, 5f);

        playerController.SetDefaults();
        weaponManager.SetDefaults();
    }
}
