using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(ProjectileController))]
public class Homing : NetworkBehaviour
{
    private GameObject laser;
    private LaserController laserController;
    private Rigidbody rb;
    private NetworkIdentity networkIdentity;

    [SerializeField]
    private float homingness;

    [SerializeField]
    private float homingInterval;

    [SerializeField]
    private float delay;

    private float timeSinceCreated;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        networkIdentity = GetComponent<NetworkIdentity>();
    }

    void Update()
    {
        timeSinceCreated += Time.deltaTime;

        if (!networkIdentity.hasAuthority || timeSinceCreated < delay)
            return;

        if (laser == null)
        {
            laser = GameManager.GetLaser();

            if (laser != null)
                laserController = laser.GetComponent<LaserController>();


        } else if (laserController.active && !IsInvoking("Home"))
        {
            InvokeRepeating("Home", 0f, homingInterval);
        } else if (IsInvoking("Home"))
        {
            CancelInvoke("Home");
        }
    }

    void Home()
    {
        Quaternion targetRotation = Quaternion.LookRotation(laser.transform.position - transform.position);
        rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, homingInterval * homingness));
        rb.velocity = transform.forward * rb.velocity.magnitude;
    }
}
