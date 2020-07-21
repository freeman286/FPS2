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
    private float delay;

    private float timeSinceCreated;

    private bool target = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        networkIdentity = GetComponent<NetworkIdentity>();
    }

    void Update()
    {
        timeSinceCreated += Time.deltaTime;

        if (!networkIdentity.hasAuthority)
            return;

        if (laser == null && timeSinceCreated > delay)
        {

            laser = GameManager.GetLaser();

            if (laser != null)
                laserController = laser.GetComponent<LaserController>();

        }

        if (laserController != null && laserController.active)
        {
            Home(laser.transform.position);
            target = true;
        } else if (!target)
        {
            Home(transform.position + Vector3.up);
        } else
        {
            rb.angularVelocity = Vector3.zero;
        }
    }

    void Home(Vector3 _pos)
    {
        Vector3 rotate = Vector3.Cross(transform.forward, (_pos - transform.position).normalized);
        rb.angularVelocity = rotate.normalized * homingness * Time.deltaTime * Mathf.Pow(rotate.magnitude, 0.3f);
        rb.velocity = transform.forward * rb.velocity.magnitude;
    }
}
