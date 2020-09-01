using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Stomp : PlayerMovementAbility
{

    private DamageInflictor damageInflictor;

    public override void Start()
    {
        base.Start();
        damageInflictor = GetComponent<DamageInflictor>();
    }

    public override void DoAbility()
    {
        if (!metrics.IsGrounded())
        {
            timeSinceMovementAbilityUsed = 0f;
        }
    }

    public override void LateUpdate()
    {
        base.LateUpdate();

        if (timeSinceMovementAbilityUsed < ability.effectTime)
        {
            rb.velocity += -Vector3.up * ability.magnitude * metrics.GetJumpMultiplier();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isLocalPlayer || timeSinceMovementAbilityUsed >= ability.effectTime)
            return;

        rb.velocity = Vector3.zero;

        CmdStomp(timeSinceMovementAbilityUsed);

        Health _health = collision.transform.root.GetComponent<Health>();

        if (_health != null)
        {
            damageInflictor.CmdDamage(_health.transform.name, ability.damage, transform.name, ability.damageType.name);
        }

        List<Transform> _hitTransforms = new List<Transform>();

        Collider[] colliders = Physics.OverlapSphere(transform.position, ability.magnitude);

        foreach (Collider _collider in colliders)
        {
            Transform _baseTransform = _collider.transform.root;

            _health = _baseTransform.GetComponent<Health>();

            if (_baseTransform != transform && _health != null && !_hitTransforms.Contains(_baseTransform))
            {

                _hitTransforms.Add(_baseTransform);

                damageInflictor.CmdDamage(_health.transform.name, (int)(ability.damage * timeSinceMovementAbilityUsed), transform.name, ability.damageType.name);

            }
        }

        timeSinceMovementAbilityUsed = ability.effectTime;

    }

    [Command]
    void CmdStomp(float _timeSinceMovementAbilityUsed)
    {
        RpcStomp(_timeSinceMovementAbilityUsed);
    }

    [ClientRpc]
    public void RpcStomp(float _timeSinceMovementAbilityUsed)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, ability.magnitude);
        foreach (Collider _collider in colliders)
        {
            Rigidbody rb = _collider.attachedRigidbody;
            if (rb != null && _collider.transform.root != transform)
                rb.AddExplosionForce(ability.damage * _timeSinceMovementAbilityUsed, transform.position, ability.magnitude, 1.0f);
        }
    }
}