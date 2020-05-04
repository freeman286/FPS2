using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class ProjectileController : NetworkBehaviour
{
    [SyncVar]
    public string playerID;

    public float initialVelocity;

    void Start()
    {
        GetComponent<Rigidbody>().velocity = transform.forward * initialVelocity;
    }

    
}
