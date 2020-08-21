using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounce : PlayerMovementAbility
{
    public override void DoAbility()
    {
        if (metrics.velocity.y < -0.1f && !metrics.IsGrounded())
        {
            timeSinceMovementAbilityUsed = 0f;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isLocalPlayer || timeSinceMovementAbilityUsed >= ability.effectTime)
            return;

        rb.velocity -= Vector3.up * ability.magnitude * metrics.velocity.y;

        timeSinceMovementAbilityUsed = ability.effectTime;

    }
}