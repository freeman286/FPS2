using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerStun : NetworkBehaviour
{

    private IEnumerator coroutine;

    [ClientRpc]
    public void RpcStun(float _amount)
    {
        if (isLocalPlayer)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            coroutine = Stun(_amount);
            StartCoroutine(coroutine);
        }
    }

    private IEnumerator Stun(float _amount)
    {
        Environment.instance.Stun();

        yield return new WaitForSeconds(_amount);

        Environment.instance.UnStun();

    }
}
