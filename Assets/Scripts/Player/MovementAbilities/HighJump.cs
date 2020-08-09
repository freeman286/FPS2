using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighJump : PlayerMovementAbility
{

    public override void DoAbility()
    {
        if (metrics.IsGrounded())
        {
            rb.velocity = Vector3.up * ability.magnitude * metrics.GetJumpMultiplier();
            timeSinceMovementAbilityUsed = 0f;
        }
    }

}