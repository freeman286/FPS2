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
                CmdGrenadeThrow(equipmentSpawnPoint.position, Quaternion.LookRotation(ThrowDirection()), transform.name, ((Grenade)equipment).throwPower);
                timeSinceEquipmentUsed = 0f;
            }
        }
    }

    [Command]
    void CmdGrenadeThrow(Vector3 _pos, Quaternion _rot, string _playerID, float _velocity)
    {
        GameObject _grenade = (GameObject)Instantiate(equipment.prefab, _pos, _rot);
        NetworkServer.Spawn(_grenade, connectionToClient);

        ProjectileController _projectileController = _grenade.GetComponent<ProjectileController>();
        _projectileController.playerID = _playerID;

        _projectileController.RpcLaunch(_velocity);
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
        if (Physics.Raycast(transform.position + equipmentSpawnPoint.forward * 2f, equipmentSpawnPoint.forward, out _hit, equipment.range, mask))
        {
            _direction = (_hit.point - equipmentSpawnPoint.position).normalized;
        }
        else
        {
            _direction = (transform.position + equipmentSpawnPoint.forward * equipment.range - equipmentSpawnPoint.position).normalized;
        }

        return _direction;
    }

}
