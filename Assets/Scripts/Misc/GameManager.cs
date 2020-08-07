using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    public MatchSettings matchSettings;

    [SerializeField]
    private GameObject sceneCamera = null;

    public delegate void OnPlayerKilledCallback(string player, string source);
    public OnPlayerKilledCallback onPlayerKilledCallback;

    void Awake ()
    {
        if (instance != null)
        {
            Debug.LogError("More than one GameManager in scene.");
        } else
        {
            instance = this;
        }
    }

    public void SetSceneCameraActive(bool isActive)
    {
        if (sceneCamera == null)
            return;

        sceneCamera.SetActive(isActive);
    }

    #region Player tracking

    private const string PLAYER_ID_PREFIX = "Player_";

    private static Dictionary<string, Player> players = new Dictionary<string, Player>();

    public static void RegisterPlayer(string _netID, Player _player)
    {
        string _playerID = PLAYER_ID_PREFIX + _netID;
        players.Add(_playerID, _player);
        _player.transform.name = _playerID;
        RegisterHealth(_playerID, _player.GetComponent<Health>());
    }

    public static void UnRegisterPlayer(string _playerID)
    {
        players.Remove(_playerID);
        UnRegisterHealth(_playerID);
    }

    public static Player GetPlayer(string _playerID)
    {
        return players[_playerID];
    }

    public static Player[] GetAllPlayers()
    {
        return players.Values.ToArray();
    }

    #endregion

    #region Projectile tracking

    private const string PROJECTILE_ID_PREFIX = "Projectile_";

    private static Dictionary<string, GameObject> projectiles = new Dictionary<string, GameObject>();

    public static void RegisterProjectile(string _netID, GameObject _projectile)
    {
        string _projectileID = PROJECTILE_ID_PREFIX + _netID;
        projectiles.Add(_projectileID, _projectile);
        _projectile.transform.name = _projectileID;
        RegisterHealth(_projectileID, _projectile.GetComponent<Health>());
    }

    public static void UnRegisterProjectile(string _projectileID)
    {
        projectiles.Remove(_projectileID);
        UnRegisterHealth(_projectileID);
    }

    public static GameObject GetProjectile(string _projectileID)
    {
        return projectiles[_projectileID];
    }

    public static GameObject[] GetAllProjectile()
    {
        return projectiles.Values.ToArray();
    }


    #endregion

    #region Equipment tracking

    private const string EQUIPMENT_ID_PREFIX = "Equipment_";

    private static Dictionary<string, GameObject> equipment = new Dictionary<string, GameObject>();

    public static void RegisterEquipment(string _netID, GameObject _equipment)
    {
        string _equipmentID = EQUIPMENT_ID_PREFIX + _netID;
        equipment.Add(_equipmentID, _equipment);
        _equipment.transform.name = _equipmentID;
        RegisterHealth(_equipmentID, _equipment.GetComponent<Health>());
    }

    public static void UnRegisterEquipment(string _equipmentID)
    {
        equipment.Remove(_equipmentID);
        UnRegisterHealth(_equipmentID);
    }

    #endregion

    #region Mortar tracking

    private static GameObject mortar;

    public static void RegisterMortar(GameObject _mortar)
    {
        mortar = _mortar;
    }

    public static GameObject GetMortar()
    {
        return mortar;
    }

    #endregion

    #region Charge tracking

    private static GameObject charge;

    public static void RegisterCharge(GameObject _charge)
    {
        charge = _charge;
    }

    public static GameObject GetCharge()
    {
        return charge;
    }

    #endregion

    #region Turret tracking

    private static GameObject turret;

    public static void RegisterTurret(GameObject _turret)
    {
        turret = _turret;
    }

    public static GameObject GetTurret()
    {
        return turret;
    }

    #endregion

    #region Particle tracking

    private const string PARTICLE_ID_PREFIX = "Particle_";

    private static Dictionary<string, GameObject> particles = new Dictionary<string, GameObject>();

    public static void RegisterParticle(string _netID, GameObject _particle)
    {
        string _particleID = PARTICLE_ID_PREFIX + _netID;
        particles.Add(_particleID, _particle);
        _particle.transform.name = _particleID;
    }

    public static void UnRegisterParticle(string _particleID)
    {
        particles.Remove(_particleID);
    }

    public static GameObject GetParticle(string _particleID)
    {
        if (!particles.ContainsKey(_particleID))
            return null;

        return particles[_particleID];
    }

    #endregion

    #region Laser tracking

    private static GameObject laser;

    public static void RegisterLaser(GameObject _laser)
    {
        laser = _laser;
    }

    public static GameObject GetLaser()
    {
        return laser;
    }

    #endregion

    #region Health tracking

    private static Dictionary<string, Health> healths = new Dictionary<string, Health>();

    public static void RegisterHealth(string _name, Health _health)
    {
        healths.Add(_name, _health);
    }

    public static void UnRegisterHealth(string _name)
    {
        healths.Remove(_name);
    }

    public static Health GetHealth(string _name)
    {
        return healths[_name];
    }

    #endregion
}
