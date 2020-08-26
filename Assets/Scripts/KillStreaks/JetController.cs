using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class JetController : KillStreakController
{

    enum TrackingState
    {
        returning,
        turning,
        strafing,
    };

    [Header("Return function")]

    [SerializeField]
    private float altitude = 200f;

    [SerializeField]
    private float returnRadius = 40f;

    [Header("Strafe function")]

    [SerializeField]
    private float moveSpeed = 30f;

    [SerializeField]
    private float turnSpeed = 10f;

    [SerializeField]
    private float strafeClearance = 50f;

    [SerializeField]
    private float floor = 30f;

    [SerializeField]
    private float maxStrafeTime = 30f;

    private float strafeTime = 0f;

    private TrackingState trackingState = TrackingState.returning;

    private Vector3 returnLocation = Vector3.zero;

    public Transform currentTarget = null;

    private Vector3 lastPosition;
    private Vector3 velocity;

    public override void Start()
    {
        base.Start();
        velocity = transform.forward;
        Return();
    }

    public override void Update()
    {
        base.Update();

        if (!networkIdentity.hasAuthority)
            return;

        MoveForward();

        velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        Vector3 _forward = transform.position + transform.forward * strafeClearance;

        strafeTime += Time.deltaTime;

        if (_forward.y < floor || CheckRoute(_forward).point != Vector3.zero)
        {
            Return();
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(transform.position + Vector3.up), turnSpeed * Time.deltaTime);
            return;
        }

        if (trackingState == TrackingState.returning)
        {
            FlyToPosition(returnLocation);
        } else if (trackingState == TrackingState.turning)
        {
            FlyToPosition(altitude * Vector3.up);
        }
        else if (trackingState == TrackingState.strafing)
        {
            Strafe();
        }

    }

    void Return()
    {
        trackingState = TrackingState.returning;
        currentTarget = null;

        if (timeSinceCalledIn < killStreak.time)
        {
            returnLocation = Util.Flatten(velocity).normalized * returnRadius + altitude * Vector3.up; 
        } else
        {
            returnLocation = KillStreakSpawnManager.GetKillStreakSpawnPoint(killStreak).position;
        }
    }

    void MoveForward()
    {
        transform.position = transform.position + transform.forward * moveSpeed * Time.deltaTime;
    }

    void FlyToPosition(Vector3 _pos)
    {
        Vector3 _dir = _pos - transform.position;

        if (returnLocation == KillStreakSpawnManager.GetKillStreakSpawnPoint(killStreak).position) {

            if (transform.position.y < returnLocation.y + floor)
                Despawn();

            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_dir), turnSpeed * Time.deltaTime);

            return;
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(transform.up, _dir) * Quaternion.LookRotation(_dir), turnSpeed * Time.deltaTime);

        if (Util.Flatten(transform.position).magnitude > returnRadius && FindTarget())
        {
            trackingState = TrackingState.turning;
            strafeTime = 0f;
        } else if (Util.Flatten(transform.position).magnitude < returnRadius && trackingState == TrackingState.turning)
        {
            trackingState = TrackingState.strafing;
            strafeTime = 0f;
        }
    }

    void Strafe()
    {
        

        if (strafeTime > maxStrafeTime)
        {
            Return();
            return;
        }

        if (currentTarget == null)
        {
            FindTarget();
        }
        else
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(currentTarget.position - transform.position), turnSpeed * Time.deltaTime);
        }


    }

    bool FindTarget()
    {
        GameObject[] _targets = GameManager.GetAllPlayersAndKillStreaks();
        Util.Randomise(_targets);

        foreach (GameObject _target in _targets)
        {
            if (_target.GetComponent<Health>().playerID != playerID)
            {
                currentTarget = _target.transform;
                return true;
            }
        }

        Return();
        
        return false;
    }

}
