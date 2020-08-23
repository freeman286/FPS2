using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum TrackingMode
{
    player,
    random
};

enum TrackingState
{
    moving,
    loitering
};

public class HelicopterController : KillStreakController
{
    [Header("Loiter function")]

    [SerializeField]
    private float altitude = 20f;

    [SerializeField]
    private float loiterTime = 30f;

    [SerializeField]
    private float loiterRadius = 40f;

    [SerializeField]
    private float loiterSway = 1f;

    [Header("Tracking function")]

    [SerializeField]
    private float moveTime = 5f;

    [SerializeField]
    private float moveSpeed = 10f;

    [SerializeField]
    private float turnSpeed = 10f;

    [SerializeField]
    private float footprint = 2f;

    [SerializeField]
    private TrackingMode trackingMode = TrackingMode.player;

    [SerializeField]
    private LayerMask layerMask = -1;

    private TrackingState trackingState = TrackingState.loitering;

    private float currentLoiterTime = Mathf.Infinity;

    private Vector3 loiterLocation;

    private float loiterSwayAmount;

    private Vector3 velocity = Vector3.zero;

    public override void Update()
    {
        base.Update();

        if (trackingState == TrackingState.moving)
        {
            Move();
        } else if (trackingState == TrackingState.loitering)
        {
            Loiter();
        }

        if (currentLoiterTime > loiterTime)
        {
            PickLoiterLocation();
        }

    }

    void PickLoiterLocation()
    {

        if (timeSinceCalledIn > killStreak.time && loiterLocation != Vector3.zero)
        {
            loiterLocation = KillStreakSpawnManager.GetKillStreakSpawnPoint(killStreak).position;
        }
        else
        {

            if (trackingMode == TrackingMode.player)
            {
                loiterLocation = Vector3.zero;
                foreach (Health _health in GameManager.GetAllHealth())
                {
                    if (_health.gameObject.tag != "Projectile" && _health.playerID != playerID)
                    {
                        loiterLocation = Util.Flatten(_health.transform.position) + altitude * Vector3.up;
                        break;
                    }
                }
            }

            if (trackingMode == TrackingMode.random || loiterLocation == Vector3.zero)
            {
                loiterLocation = Util.Flatten(Random.insideUnitSphere * loiterRadius) + altitude * Vector3.up;
            }
        }

        Vector3 _dir = loiterLocation - transform.position;

        RaycastHit _hit;
        if(Physics.SphereCast(transform.position + _dir.normalized * footprint, footprint, _dir, out _hit, _dir.magnitude - footprint, layerMask)) {
            trackingState = TrackingState.loitering;
            loiterLocation = Vector3.zero;
            loiterSwayAmount = Random.Range(-loiterSway, loiterSway);
        } else
        {
            trackingState = TrackingState.moving;
            currentLoiterTime = 0f;
        }
    }

    void Loiter()
    {
        currentLoiterTime += Time.deltaTime;
        transform.Rotate(0.0f, loiterSwayAmount * Time.deltaTime, 0.0f, Space.World);
        velocity = Vector3.zero;
    }

    void Move()
    {
        Vector3 _dir = loiterLocation - transform.position;

        transform.position = Vector3.SmoothDamp(transform.position, loiterLocation, ref velocity, moveTime, moveSpeed);

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_dir), turnSpeed * Time.deltaTime);

        RaycastHit _hit;
        if (Physics.SphereCast(transform.position + _dir.normalized * footprint, footprint, _dir, out _hit, _dir.magnitude - footprint, layerMask))
        {
            loiterLocation = _hit.point - _dir.normalized * footprint;
        }

        if (_dir.magnitude < 0.1f)
        {
            trackingState = TrackingState.loitering;
            currentLoiterTime = 0f;

            if (loiterLocation == KillStreakSpawnManager.GetKillStreakSpawnPoint(killStreak).position)
                Kill(string.Empty); // Despawn
        }
    }
}
