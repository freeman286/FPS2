﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RaycastShoot : NetworkBehaviour
{
    public RaycastHit Shoot(Transform _muzzleTrans, Vector3 _direction, Vector3 _devience, Weapon _weapon, LayerMask _mask)
    {
        Vector3 _cone = Random.insideUnitSphere * _weapon.coneOfFire;

        RaycastHit _hit;
        if (Physics.Raycast(_muzzleTrans.position + _direction * 0.2f, _direction + _devience + _cone, out _hit, _weapon.range, _mask))
        {

            int _damage = Mathf.RoundToInt(_weapon.damageFallOff.Evaluate(_hit.distance / _weapon.range) * _weapon.damage);

            Rigidbody rb = _hit.collider.attachedRigidbody;

            if (rb != null && rb.GetComponent<Player>() == null)
                rb.AddForceAtPosition((_direction + _devience + _cone) * _damage, _hit.point);

            if (_hit.collider.transform.name == "Head")
                _damage = (int)(_damage * _weapon.headShotMultiplier);

            Health _health = _hit.collider.transform.root.GetComponent<Health>();

            if (_health != null)
                CmdDamage(_health.transform.name, _damage, transform.name, _weapon.damageType.name);

            return _hit;
        }

        return new RaycastHit();
    }

    [Command]
    void CmdDamage(string _healthID, int _damage, string _sourceID, string _damageType)
    {
        Health _health = GameManager.GetHealth(_healthID);
        _health.RpcTakeDamage(_damage, _sourceID, _damageType);
    }

    public Vector3 ShootDirection(Transform _camTrans, Transform _muzzleTrans, float _range, LayerMask _mask)
    {
        Vector3 _direction;

        RaycastHit _hit;
        if (Physics.Raycast(_camTrans.position + _camTrans.forward * 2f, _camTrans.forward, out _hit, _range, _mask))
        {
            _direction = (_hit.point - _muzzleTrans.position).normalized;
        }
        else
        {
            _direction = (_camTrans.position + _camTrans.forward * _range - _muzzleTrans.position).normalized;
        }

        return _direction;
    }
}
