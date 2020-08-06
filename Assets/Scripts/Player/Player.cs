using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerSetup))]
public class Player : NetworkBehaviour
{
    [Header("Behaviours")]

    [SerializeField]
    private Behaviour[] disableOnDeath = null;
    private bool[] wasEnabled;

    [SerializeField]
    private GameObject[] disableGameObjectsOnDeath = null;

    [SerializeField]
    private Collider[] disableCollidersOnDeath = null;

    [SerializeField]
    private GameObject spawnEffect = null;

    private bool firstSetup = true;
    
    [Header("Score")]

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
    public GameObject[] rigidbodyOnDeath;

    public Vector3[] rigidbodyPosition;

    [SerializeField]
    private float[] rigidbodyMass = null;

    private PlayerMotor motor;

    private PlayerShoot shoot;

    private PlayerEquipment equipment;

    private PlayerMetrics metrics;

    private PlayerStats stats;

    private Health health;

    void Start()
    {
        motor = GetComponent<PlayerMotor>();
        shoot = GetComponent<PlayerShoot>();
        equipment = GetComponent<PlayerEquipment>();
        metrics = GetComponent<PlayerMetrics>();
        stats = GetComponent<PlayerStats>();
        health = GetComponent<Health>();

        for (int i = 0; i < rigidbodyOnDeath.Length; i++)
        {
            rigidbodyPosition[i] = rigidbodyOnDeath[i].transform.localPosition;
        }
    }

    public void SetupPlayer()
    {

        if (isLocalPlayer)
        {
            GameManager.instance.SetSceneCameraActive(false);
            GetComponent<PlayerSetup>().ui.Alive();
        }

        CmdBroadcastPlayerSetup();
    }

    [Command]
    private void CmdBroadcastPlayerSetup ()
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

    public void Die(string _sourceID)
    {
        CmdDie(_sourceID);
    }

    [Command]
    private void CmdDie(string _sourceID)
    {
        RpcDie(_sourceID);
    }

    [ClientRpc]
    private void RpcDie(string _sourceID)
    {

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
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rigidbody.velocity = metrics.velocity;
        }

        weaponManager.Die();
        motor.Die();

        if (isLocalPlayer)
        {
            GameManager.instance.SetSceneCameraActive(true);
            Environment.instance.UnStun();
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
        health.SetDefaults();

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

        Util.DeleteTagRecursively(gameObject, "Projectile");

        GameObject _spawnEffect = (GameObject)Instantiate(spawnEffect, transform.position, Quaternion.identity);
        Destroy(_spawnEffect, 5f);

        motor.SetDefaults();
        weaponManager.SetDefaults();
        equipment.SetDefaults();
    }
}
