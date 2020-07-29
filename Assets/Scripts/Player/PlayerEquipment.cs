using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Player))]
public class PlayerEquipment : NetworkBehaviour
{
    [SerializeField]
    private Transform equipmentSpawnPoint = null;

    [SerializeField]
    private LayerMask mask;

    private Equipment equipment;

    [SyncVar(hook = nameof(EquipmentNameChanged))]
    public string equipmentName;

    private float timeSinceEquipmentUsed = 0f;

    private WeaponManager weaponManager;

    void Start()
    {
        weaponManager = GetComponent<WeaponManager>();
        SetDefaults();
    }

    void Update()
    {
        timeSinceEquipmentUsed += Time.deltaTime;

        if (isLocalPlayer && Input.GetButtonDown("Equipment") && timeSinceEquipmentUsed > equipment.cooldown && !weaponManager.isReloading && !Pause.IsOn) {
            if (equipment is Grenade)
            {

                CmdThrowGrenade(equipmentSpawnPoint.position, Quaternion.LookRotation(ThrowDirection()), transform.name, ((Grenade)equipment).throwPower);
                timeSinceEquipmentUsed = 0f;

            } else if (equipment is Charge)
            {

                GameObject _charge = GameManager.GetCharge();
                if (_charge != null)
                {
                    _charge.GetComponent<ChargeController>().Detonate(false);
                } else {
                    RaycastHit _hit = EquipmentPlace();
                    if (_hit.point != Vector3.zero)
                    {
                        CmdPlaceCharge(equipmentSpawnPoint.position, Quaternion.LookRotation(ThrowDirection()), transform.name, _hit.point, Quaternion.LookRotation(_hit.normal));
                        timeSinceEquipmentUsed = 0f;
                    }
                }

            }
            
        }
    }

    [Command]
    void CmdThrowGrenade(Vector3 _pos, Quaternion _rot, string _playerID, float _velocity)
    {
        GameObject _grenade = (GameObject)Instantiate(equipment.prefab, _pos, _rot);
        NetworkServer.Spawn(_grenade, connectionToClient);

        ProjectileController _projectileController = _grenade.GetComponent<ProjectileController>();
        _projectileController.playerID = _playerID;

        _projectileController.RpcLaunch(_velocity);
    }

    [Command]
    void CmdPlaceCharge(Vector3 _pos, Quaternion _rot, string _playerID, Vector3 _placePos, Quaternion _placeRot)
    {
        GameObject _charge = (GameObject)Instantiate(equipment.prefab, _pos, _rot);
        NetworkServer.Spawn(_charge, connectionToClient);

        ChargeController _chargeController = _charge.GetComponent<ChargeController>();
        _chargeController.playerID = _playerID;

        _chargeController.RpcPlace(_placePos, _placeRot, (equipment as Charge).placeSpeed);
    }

    public void SetDefaults()
    {
        if (isLocalPlayer)
        {
            CmdSetEquipment(PlayerInfo.equipmentName);
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
    }

    Vector3 ThrowDirection()
    {

        Vector3 _direction;
        RaycastHit _hit;
        if (Physics.Raycast(transform.position, equipmentSpawnPoint.forward, out _hit, equipment.range, mask))
        {
            _direction = (_hit.point - equipmentSpawnPoint.position).normalized;
        }
        else
        {
            _direction = (transform.position + equipmentSpawnPoint.forward * equipment.range - equipmentSpawnPoint.position).normalized;
        }

        return _direction;
    }

    RaycastHit EquipmentPlace()
    {

        RaycastHit _hit;

        if (Physics.SphereCast(transform.position, (equipment as Charge).footprint, equipmentSpawnPoint.forward, out _hit, equipment.range))
        {
            return _hit;
        }

        return new RaycastHit();
    }

}
