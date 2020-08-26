using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunshipController : KillStreakController
{
    enum TrackingState
    {
        returning,
        cruise,
    };

    [Header("Cruise function")]

    [SerializeField]
    private float altitude = 500f;

    [SerializeField]
    private float cruiseRadius = 500f;

    [SerializeField]
    private float cruiseAngle = 45f;

    [SerializeField]
    private float cruiseClearance = 50f;

    [Header("Flying function")]

    [SerializeField]
    private float moveSpeed = 60f;

    [SerializeField]
    private float slowDownSpeed = 5f;

    [SerializeField]
    private float turnSpeed = 0.3f;

    private float speed;

    private TrackingState trackingState = TrackingState.returning;

    private Vector3 returnLocation;

    private float theta;

    public override void Start()
    {
        base.Start();
        returnLocation = new Vector3(cruiseRadius, altitude, 0f);
    }

    public override void Update()
    {
        base.Update();

        if (!networkIdentity.hasAuthority)
            return;

        if (timeSinceCalledIn >= killStreak.time)
        {
            returnLocation = KillStreakSpawnManager.GetKillStreakSpawnPoint(killStreak).position;
            trackingState = TrackingState.returning;
        }

        Vector3 _forward = transform.position + transform.forward * cruiseClearance;

        if (CheckRoute(_forward).point == Vector3.zero)
        {
            speed = moveSpeed;
        } else
        {
            speed = moveSpeed - slowDownSpeed;
        }

        if (trackingState == TrackingState.returning)
        {
            FlyToPosition(returnLocation);
        }
        else if (trackingState == TrackingState.cruise)
        {
            Cruise();
        }
    }

    void FlyToPosition(Vector3 _pos)
    {
        Vector3 _dir = _pos - transform.position;

        if (_dir.magnitude < 1f)
        {
            trackingState = TrackingState.cruise;
            return;
        } else if (_dir.magnitude < 10f && returnLocation == KillStreakSpawnManager.GetKillStreakSpawnPoint(killStreak).position)
        {
            Despawn();
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_dir), turnSpeed * Time.deltaTime);
        transform.position = transform.position + transform.forward * speed * Time.deltaTime;
    }

    void Cruise()
    {
        transform.rotation = Quaternion.Euler(0, theta, Mathf.Lerp(transform.rotation.eulerAngles.z, cruiseAngle, turnSpeed * Time.deltaTime));

        float _deltaTheta = - Mathf.Rad2Deg * speed * Time.deltaTime / cruiseRadius;

        transform.position = Quaternion.Euler(0, theta, 0) * Vector3.right * cruiseRadius + altitude * Vector3.up;

        theta += _deltaTheta;

    }

}
