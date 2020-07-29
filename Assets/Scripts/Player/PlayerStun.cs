using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerStun : NetworkBehaviour
{

    private IEnumerator coroutine;

    private int stunCount = 0;

    [ClientRpc]
    public void RpcStun(float _amount)
    {
        if (isLocalPlayer)
        {
            StartCoroutine(Stun(_amount));
        }
    }

    private IEnumerator Stun(float _amount)
    {
        Environment.instance.Stun();

        stunCount += 1;

        yield return new WaitForSeconds(_amount);

        stunCount -= 1;
        
        if (stunCount == 0)
            Environment.instance.UnStun();
    }
}
