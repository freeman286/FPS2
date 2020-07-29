using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//For keeping track of player metrics from the perspective of different clients
public class PlayerMetrics : MonoBehaviour
{

    [SerializeField]
    private LayerMask mask = -1;

    [HideInInspector]
    public Vector3 lastPosition;
    [HideInInspector]
    public Vector3 velocity; // The velocity that other players think this player is travelling at

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
    }

    public bool IsGrounded()
    {
        RaycastHit _hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out _hit, 3f, mask))
        {
            if ((_hit.distance < 2.15f && Mathf.Abs(velocity.y) < 0.1f && _hit.distance != 0f))
            {
                if (Physics.Raycast(transform.position, Vector3.up, out _hit, 1f, mask))
                {
                    return false;
                }
                return true;
            }
        }
        return false;
    }

    public bool IsMoving()
    {
        return velocity.magnitude > 0.5f;
    }
}
