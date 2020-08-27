using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class HelicopterController : KillStreakController
{

    enum TrackingMode
    {
        player,
        protect,
        random
    };

    enum TrackingState
    {
        moving,
        loitering
    };

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
    private float tilt = 1f;

    [SerializeField]
    private TrackingMode trackingMode = TrackingMode.player;

    private TrackingState trackingState = TrackingState.loitering;

    private float currentLoiterTime = Mathf.Infinity;

    private Vector3 loiterLocation;

    private float loiterSwayAmount;
    private float currentLoiterSwayAmount;
    private float deltaSway = 0f;

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

            if (trackingMode == TrackingMode.protect)
            {
                if (playerID == null)
                    return;

                Player _player = GameManager.GetPlayer(playerID);
                Vector3 _loiterLocation = Util.Flatten(_player.transform.position) + altitude * Vector3.up;

                if (CheckRoute(_loiterLocation).point == Vector3.zero)
                {
                    loiterLocation = _loiterLocation;
                } else
                {
                    loiterLocation = Vector3.zero;
                }

            }
            else if (trackingMode == TrackingMode.player)
            {
                loiterLocation = Vector3.zero;

                Player[] _players = GameManager.GetAllPlayers();
                Util.Randomise(_players);

                foreach (Player _player in _players)
                {
                    if (_player.transform.name != playerID)
                    {
                        Vector3 _loiterLocation = Util.Flatten(_player.transform.position) + altitude * Vector3.up;
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
        }
    }

    void Loiter()
    {
        currentLoiterTime += Time.deltaTime;

        currentLoiterSwayAmount = Mathf.SmoothDamp(currentLoiterSwayAmount, loiterSwayAmount, ref deltaSway, Time.deltaTime * turnSpeed);

        transform.Rotate(0.0f, currentLoiterSwayAmount * Time.deltaTime, 0.0f, Space.World);
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
            loiterSwayAmount = Random.Range(-loiterSway, loiterSway);
            currentLoiterTime = 0f;
            currentLoiterSwayAmount = 0f;
            deltaSway = 0f;
            velocity = Vector3.zero;
            acceleration = Vector3.zero;

            if (loiterLocation == KillStreakSpawnManager.GetKillStreakSpawnPoint(killStreak).position)
                Despawn();
        }
    }

    
}
