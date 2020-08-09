using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

[RequireComponent(typeof(ProjectileController))]
public class Homing : NetworkBehaviour
{
    public UnityEvent proximityEvent;

    [SerializeField]
    private float proximityFuse = 8f;

    private GameObject laser;
    private LaserController laserController;
    private Rigidbody rb;
    private NetworkIdentity networkIdentity;

    [SerializeField]
    private float homingness = 50f;

    [SerializeField]
    private AnimationCurve homingnessOverAngle = null;

    [SerializeField]
    private float delay = 1f;

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
            Home(laser.transform.position);

            if ((laser.transform.position - transform.position).magnitude < proximityFuse)
                proximityEvent.Invoke();
        }
    }

    void Home(Vector3 _pos)
    {
        Vector3 target_vector = (_pos - transform.position).normalized;

        Vector3 rotate = Vector3.Cross(transform.forward, target_vector);
        rb.angularVelocity = rotate.normalized * homingness * Time.deltaTime * homingnessOverAngle.Evaluate(Vector3.Dot(transform.forward, target_vector));
        rb.velocity = transform.forward * rb.velocity.magnitude;
    }
}
