﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Health))]
public class RaycastShoot : NetworkBehaviour
{
    public GameObject hitEffectPrefab;

    private Health health;

    void Start()
    {
        health = GetComponent<Health>();
    }

    public void Shoot(Transform _muzzleTrans, Vector3 _direction, Vector3 _devience, Weapon _weapon, LayerMask _mask, string _sourceID)
    {
        for (int i = 0; i < _weapon.roundsPerShot; i++)
        {

            Vector3 _cone = Random.insideUnitSphere * _weapon.coneOfFire;

            RaycastHit _hit;
            if (Physics.Raycast(_muzzleTrans.position + _direction * 0.2f, _direction + _devience + _cone, out _hit, _weapon.range, _mask))
            {

                int _damage = Mathf.RoundToInt(_weapon.damageFallOff.Evaluate(_hit.distance / _weapon.range) * _weapon.damage);

                if (_hit.collider.transform.name == "Head")
                    _damage = (int)(_damage * _weapon.headShotMultiplier);

                Health _health = _hit.collider.transform.root.GetComponent<Health>();

                if (_health != null)
                    health.CmdDamage(_health.transform.name, _damage, _sourceID, _weapon.damageType.name);

                Rigidbody rb = _hit.collider.attachedRigidbody;

                if (rb != null && rb.GetComponent<Player>() == null)
                    rb.AddForceAtPosition((_direction + _devience + _cone) * _damage, _hit.point);


                CmdOnHit(_hit.point, _hit.normal);
            }
        }
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

    [Command]
    void CmdOnHit(Vector3 _pos, Vector3 _normal)
    {
        RpcDoHitEfftect(_pos, _normal, transform.name);
    }

    [ClientRpc]
    void RpcDoHitEfftect(Vector3 _pos, Vector3 _normal, string _playerID)
    {
        GameObject _hitEffect = (GameObject)Instantiate(hitEffectPrefab, _pos, Quaternion.LookRotation(_normal));

        Destroy(_hitEffect, 2f);
    }
}
