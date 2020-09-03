using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MissileController : KillStreakController
{

    [Header("Explosive function")]

    [SerializeField]
    private float fuse = 60f;

    [SerializeField]
    private float proximityFuse = 15f;

    [Header("Tracking function")]

    [SerializeField]
    private float moveSpeed = 30f;

    [SerializeField]
    private float turnSpeed = 20f;

    [HideInInspector]
    public GameObject currentTarget = null;

    private Explosive explosive;

    private Rigidbody rb;

    public override void Start()
    {
        base.Start();

        if (!networkIdentity.hasAuthority)
            return;

        FindTarget();
        explosive = GetComponent<Explosive>();
        rb = GetComponent<Rigidbody>();
    }

    public override void Update()
    {
        base.Update();

        if (!networkIdentity.hasAuthority)
            return;

        MoveForward();

        if (currentTarget != null)
            Home(currentTarget.transform.position);

        if (timeSinceCalledIn >= fuse)
            Detonate(playerID);
    }

    void MoveForward()
    {
        transform.position = transform.position + transform.forward * moveSpeed * Time.deltaTime;
    }

    void FindTarget()
    {
        List<GameObject> _targets = new List<GameObject>(GameManager.GetAllPlayersAndKillStreaks());
        List<GameObject> _currentTargets = new List<GameObject>();

        MissileController[] _missileControllers = GameObject.FindObjectsOfType<MissileController>();

        foreach(MissileController _missileController in _missileControllers)
        {
            if (_missileController.networkIdentity.hasAuthority && !_currentTargets.Contains(_missileController.currentTarget))
                _currentTargets.Add(_missileController.currentTarget);
        }

        SearchHealth(_targets, _currentTargets, false);

        if (currentTarget == null)
            SearchHealth(_targets, _currentTargets, true);
    }

    void SearchHealth(List<GameObject> _targets, List<GameObject> _currentTargets, bool _repeat)
    {
        float _maxHealth = 0;
        foreach (GameObject _target in _targets)
        {
            Health _health = _target.GetComponent<Health>();
            if (_currentTargets.Contains(_target) == _repeat && _health.maxHealth >= _maxHealth && _health.playerID != playerID)
            {
                _maxHealth = _health.maxHealth;
                currentTarget = _target;
            }
        }
    }

    public void Detonate(string _playerID)
    {
        explosive.Explode(transform.position, rb.velocity, timeSinceCalledIn, _playerID);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (networkIdentity.hasAuthority)
        {
            explosive.Explode(transform.position, collision.contacts[0].normal, timeSinceCalledIn, playerID);
        }
    }

    void Home(Vector3 _pos)
    {
        Vector3 _dir = _pos - transform.position;

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_dir), turnSpeed * Time.deltaTime);

        if (_dir.magnitude < proximityFuse)
        {
            Detonate(playerID);
        }
    }

}
