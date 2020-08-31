using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


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
    private float AggroRange = 10f;

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

    void PickLoiterLocation(Vector3 _locationOveride = default(Vector3))
    {

        if (timeSinceCalledIn > killStreak.time && loiterLocation != Vector3.zero)
        {
            loiterLocation = KillStreakManager.GetKillStreakSpawnPoint(killStreak).position;
        }
        else
        {

            List<Collider> _hitColliders = new List<Collider>(Physics.OverlapSphere(transform.position, footprint, layerMask));

            _hitColliders.RemoveAll(collider => collider.transform.root == transform);

            if (_hitColliders.Count() == 0 && _locationOveride != default(Vector3))
            {
                //A player has shot at us
                if (CheckRoute(_locationOveride).point == Vector3.zero)
                {
                    loiterLocation = _locationOveride;
                }
                else
                {
                    return;
                }

            }
            else if (_hitColliders.Count() == 0 && trackingMode == TrackingMode.protect)
            {
                //We are protecting a player
                Player _player = GameManager.GetPlayer(playerID);

                if (_player == null)
                    return;

                Vector3 _loiterLocation = Util.Flatten(_player.transform.position) + altitude * Vector3.up;

                if (CheckRoute(_loiterLocation).point == Vector3.zero)
                {
                    loiterLocation = _loiterLocation;
                }
                else
                {
                    loiterLocation = Vector3.zero;
                }

            }
            else if (_hitColliders.Count() == 0 && trackingMode == TrackingMode.player)
            {
                //We are attacking a player
                loiterLocation = Vector3.zero;

                Player[] _players = GameManager.GetAllPlayers();
                Util.Randomise(_players);

                foreach (Player _player in _players)
                {
                    if (_player.transform.name != playerID)
                    {
                        Vector3 _dir = Util.Flatten(_player.transform.position - transform.position);
                        Vector3 _loiterLocation = Util.Flatten(_player.transform.position) + altitude * Vector3.up - _dir.normalized * AggroRange;
                        if (CheckRoute(_loiterLocation).point == Vector3.zero)
                        {
                            loiterLocation = _loiterLocation;
                            break;
                        }
                    }
                }
            }
            else
            {
                loiterLocation = Vector3.zero;
            }

            if ((trackingMode == TrackingMode.random && _locationOveride == default(Vector3)) || loiterLocation == Vector3.zero)
            {
                //Moving randomly or unable to find target
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

            if (loiterLocation == KillStreakManager.GetKillStreakSpawnPoint(killStreak).position)
                Despawn();
        }
    }

    public void AggroPlayer(string _playerID)
    {
        if (networkIdentity.hasAuthority && currentLoiterTime > 1f)
        {

            Player _player = GameManager.GetPlayer(_playerID);

            if (_player == null || _playerID == playerID)
                return;

            Vector3 _dir = Util.Flatten(_player.transform.position - transform.position);

            Vector3 _loiterLocation = Util.Flatten(_player.transform.position) + altitude * Vector3.up - _dir.normalized * AggroRange;

            if ((_dir).magnitude > AggroRange)
                PickLoiterLocation(_loiterLocation);

        }
    }

}
