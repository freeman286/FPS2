using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Player), typeof(RaycastShoot), typeof(ProjectileShoot))]
public class PlayerEquipment : NetworkBehaviour
{
    [SerializeField]
    private Camera cam = null;

    [SerializeField]
    private ListType listType = null;

    [SerializeField]
    private Transform equipmentSpawnPoint = null;

    [SerializeField]
    private LayerMask mask;

    public Equipment equipment;

    [SyncVar(hook = nameof(EquipmentNameChanged))]
    public string equipmentName;

    public float timeSinceEquipmentUsed = 0f;

    private WeaponManager weaponManager;
    private RaycastShoot raycastShoot;
    private ProjectileShoot projectileShoot;

    public delegate void OnEquipmentChangedCallback(Sprite icon);
    public OnEquipmentChangedCallback onEquipmentChangedCallback;

    public float GetEquipmentPct()
    {
        if (equipment == null || equipment.cooldown == 0)
            return 1;

        return Mathf.Clamp(timeSinceEquipmentUsed / equipment.cooldown, 0, 1);
    }

    void Start()
    {
        weaponManager = GetComponent<WeaponManager>();
        raycastShoot = GetComponent<RaycastShoot>();
        projectileShoot = GetComponent<ProjectileShoot>();

        GetComponent<Player>().onPlayerSetDefaultsCallback += SetDefaults;
    }

    void Update()
    {
        timeSinceEquipmentUsed += Time.deltaTime;

        if (isLocalPlayer && Input.GetButtonDown("Equipment") && timeSinceEquipmentUsed > equipment.cooldown && !weaponManager.isReloading && !Pause.IsOn) {

            Vector3 _throwDirection = raycastShoot.ShootDirection(cam.transform, equipmentSpawnPoint, equipment.range, mask);

            if (equipment is Grenade)
            {

                projectileShoot.Shoot(equipmentSpawnPoint, _throwDirection, Vector3.zero, ((Grenade)equipment).throwPower, equipment.prefab, transform.name, 1);
                timeSinceEquipmentUsed = 0f;

            } else if (equipment is Charge)
            {

                GameObject _charge = GameManager.GetCharge();
                if (_charge != null)
                {
                    _charge.GetComponent<ChargeController>().Detonate();
                    timeSinceEquipmentUsed = 0f;
                } else {
                    EquipmentPlace(_throwDirection);
                }

            } else if (equipment is Turret)
            {

                GameObject _turret = GameManager.GetTurret();
                if (_turret != null)
                {
                    _turret.GetComponent<TurretController>().Kill();
                    
                } else
                {
                    EquipmentPlace(_throwDirection);
                }

            }


        }
    }

    void EquipmentPlace(Vector3 _throwDirection)
    {
        RaycastHit _hit = FindEquipmentPosition();
        if (_hit.point != Vector3.zero)
        {
            CmdPlaceEquipment(equipmentSpawnPoint.position, Quaternion.LookRotation(_throwDirection), transform.name, _hit.point, Quaternion.LookRotation(_hit.normal));
            timeSinceEquipmentUsed = 0f;
        }
    }

    [Command]
    void CmdPlaceEquipment(Vector3 _pos, Quaternion _rot, string _playerID, Vector3 _placePos, Quaternion _placeRot)
    {
        GameObject _equipment = (GameObject)Instantiate(equipment.prefab, _pos, _rot);
        NetworkServer.Spawn(_equipment, connectionToClient);

        PlaceableEquipmentController _placeableEquipmentController = _equipment.GetComponent<PlaceableEquipmentController>();

        if (_placeableEquipmentController != null)
        {
            _placeableEquipmentController.playerID = _playerID;
            _placeableEquipmentController.RpcPlace(_placePos, _placeRot, (equipment as PlaceableEquipment).placeSpeed);
        }

        Health _health = _equipment.GetComponent<Health>();

        if (_health != null)
            _health.playerID = _playerID;
    }

    public void SetDefaults()
    {
        if (isLocalPlayer)
        {
            CmdSetEquipment(PlayerInfo.GetNameSelected(listType));                
        }
    }

    [Command]
    void CmdSetEquipment(string _equipmentName)
    {
        equipmentName = _equipmentName;
    }

    void EquipmentNameChanged(string _oldName, string _newName)
    {
        equipmentName = _newName;
        equipment = EquipmentUtil.NameToEquipment(equipmentName);

        if (isLocalPlayer) {
            timeSinceEquipmentUsed = equipment.cooldown;
            onEquipmentChangedCallback.Invoke(equipment.icon);
        }
    }

    RaycastHit FindEquipmentPosition()
    {

        float _footprint = 1f;

        if (equipment is Charge)
        {
            _footprint = (equipment as Charge).footprint;
        } else if (equipment is Turret)
        {
            _footprint = (equipment as Turret).footprint;
        }

        RaycastHit _hit;

        if (Physics.SphereCast(transform.position, _footprint, equipmentSpawnPoint.forward, out _hit, equipment.range, mask))
        {
            return _hit;
        }

        return new RaycastHit();
    }

}
