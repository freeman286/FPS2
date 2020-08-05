using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ProjectileShoot : NetworkBehaviour
{

    public void Shoot(Transform _muzzleTrans, Vector3 _direction, Vector3 _devience, float _velocity, GameObject _projectile, string _playerID, int _roundsPerShot)
    {
        for (int i = 0; i < _roundsPerShot; i++)
        {
            CmdProjectileShot(_muzzleTrans.position, Quaternion.LookRotation(_direction + _devience), _playerID, _velocity, _projectile.name);
        }
    }

    [Command]
    void CmdProjectileShot(Vector3 _pos, Quaternion _rot, string _playerID, float _velocity, string _projectileName)
    {
        GameObject _projectilePrefab = WeaponsUtil.NameToProjectile(_projectileName);

        if (_projectilePrefab != null)
        {
            GameObject _projectile = (GameObject)Instantiate(_projectilePrefab, _pos, _rot);
            NetworkServer.Spawn(_projectile, connectionToClient);

            ProjectileController _projectileController = _projectile.GetComponent<ProjectileController>();
            _projectileController.playerID = _playerID;

            _projectileController.RpcLaunch(_velocity);
        }
    }
}
