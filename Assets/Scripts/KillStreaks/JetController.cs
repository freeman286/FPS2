using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class JetController : KillStreakController
{

    enum TrackingState
    {
        returning,
        strafing,
    };

    [Header("Loiter function")]

    [SerializeField]
    private float altitude = 200f;

    [SerializeField]
    private float returnRadius = 40f;

    [Header("Tracking function")]

    [SerializeField]
    private float moveSpeed = 30f;

    [SerializeField]
    private float turnSpeed = 10f;

    [SerializeField]
    private float strafeClearance = 50f;

    private TrackingState trackingState = TrackingState.returning;

    private Vector3 returnLocation = Vector3.zero;

    public Transform currentTarget = null;

    public override void Start()
    {
        base.Start();
        Return();
    }

    public override void Update()
    {
        base.Update();

        if (!networkIdentity.hasAuthority)
            return;

        MoveForward();

        if (CheckRoute(transform.position + transform.forward * strafeClearance).point != Vector3.zero)
        {
            currentTarget = null;
            Return();
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(transform.position + Vector3.up), turnSpeed * Time.deltaTime);
            return;
        }

        if (trackingState == TrackingState.returning)
        {
            FlyToPosition(returnLocation);
        }
        else if (trackingState == TrackingState.strafing)
        {
            Strafe();
        }

    }

    void Return()
    {
        trackingState = TrackingState.returning;

        if (timeSinceCalledIn < killStreak.time)
        {
            returnLocation = -Util.Flatten(transform.position).normalized * returnRadius + altitude * Vector3.up; 
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

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(transform.up, _dir) * Quaternion.LookRotation(_dir), turnSpeed * Time.deltaTime);

        if (_dir.magnitude < 1f || Util.Flatten(transform.position).magnitude > returnRadius && FindTarget())
        {
            trackingState = TrackingState.strafing;
            
            if (returnLocation == KillStreakSpawnManager.GetKillStreakSpawnPoint(killStreak).position)
                Kill(string.Empty); // Despawn
        }
    }

    void Strafe()
    {
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
