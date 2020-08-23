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
    private float tilt = 1f;

    [SerializeField]
    private TrackingMode trackingMode = TrackingMode.player;

    [SerializeField]
    private LayerMask layerMask = -1;

    private TrackingState trackingState = TrackingState.loitering;

    private float currentLoiterTime = Mathf.Infinity;

    private Vector3 loiterLocation;

    private float loiterSwayAmount;

    private Vector3 velocity = Vector3.zero;
    private Vector3 previousVelocity = Vector3.zero;
    private Vector3 acceleration = Vector3.zero;

    public override void Update()
    {
        base.Update();

        if (!networkIdentity.hasAuthority)
            return;

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
                        Vector3 _loiterLocation = Util.Flatten(_health.transform.position) + altitude * Vector3.up;
                        if (CheckRoute(_loiterLocation).point == Vector3.zero)
                        {
                            loiterLocation = _loiterLocation;
                            break;
                        }
                    }
                }
            }

            if (trackingMode == TrackingMode.random || loiterLocation == Vector3.zero)
            {
                loiterLocation = Util.Flatten(Random.insideUnitSphere * loiterRadius) + altitude * Vector3.up;
            }
        }  


        if (CheckRoute(loiterLocation).point == Vector3.zero) {
            trackingState = TrackingState.moving;
            currentLoiterTime = 0f;
        } else
        {
            trackingState = TrackingState.loitering;
            loiterLocation = Vector3.zero;
            loiterSwayAmount = Random.Range(-loiterSway, loiterSway);
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

        acceleration = (velocity - previousVelocity) / Time.deltaTime;

        previousVelocity = velocity;

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(transform.up, acceleration + Vector3.up * tilt) * Quaternion.LookRotation(_dir), turnSpeed * Time.deltaTime);

        RaycastHit _hit = CheckRoute(loiterLocation);
        if (_hit.point != Vector3.zero)
        {
            loiterLocation = _hit.point - _dir.normalized * footprint;
        }

        if (_dir.magnitude < 0.1f)
        {
            trackingState = TrackingState.loitering;
            currentLoiterTime = 0f;
            velocity = Vector3.zero;
            acceleration = Vector3.zero;

            if (loiterLocation == KillStreakSpawnManager.GetKillStreakSpawnPoint(killStreak).position)
                Kill(string.Empty); // Despawn
        }
    }

    private RaycastHit CheckRoute(Vector3 _destination)
    {
        Vector3 _dir = _destination - transform.position;

        RaycastHit _hit;
        if (Physics.SphereCast(transform.position + _dir.normalized * footprint, footprint, _dir, out _hit, _dir.magnitude - footprint, layerMask))
        {
            return _hit;
        }

        return new RaycastHit();
    }
}
