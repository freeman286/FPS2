using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateFeet : MonoBehaviour
{
    [SerializeField]
    private GameObject leftFoot = null;
    [SerializeField]
    private GameObject rightFoot = null;

    private Vector3 leftFootRestingPos;
    private Vector3 rightFootRestingPos;

    private Vector3 leftFootPos;
    private Vector3 rightFootPos;

    [SerializeField]
    private float snapDist = 0.1f;

    [SerializeField]
    private float snapSpeed = 65f;

    private bool leftFootMoving = true;
    private bool rightFootMoving = true;

    private Rigidbody rb;

    void Start()
    {
        leftFootRestingPos = leftFoot.transform.localPosition;
        rightFootRestingPos = rightFoot.transform.localPosition;

        rb = GetComponent<Rigidbody>();
        SetDefaults();
    }

    void FixedUpdate()
    {
        PerformFootMovement();
    }

    void PerformFootMovement()
    {
        float _leftDist = Vector3.Distance(leftFootRestingPos, leftFoot.transform.localPosition);
        float _rightDist = Vector3.Distance(rightFootRestingPos, rightFoot.transform.localPosition);

        if (_leftDist > 4f || _rightDist > 4f)
        {
            leftFoot.transform.localPosition = leftFootRestingPos;
            rightFoot.transform.localPosition = rightFootRestingPos;
        }

        leftFootMoving = (_leftDist > snapDist && !rightFootMoving) || Mathf.Abs(rb.velocity.y) > 0.01f;

        rightFootMoving = (_rightDist > snapDist && !leftFootMoving) || Mathf.Abs(rb.velocity.y) > 0.01f;

        if (leftFootMoving)
        {
            leftFoot.transform.localPosition = Vector3.Lerp(leftFoot.transform.localPosition, leftFootRestingPos, Time.deltaTime * _leftDist * snapSpeed);
        }
        else
        {
            leftFoot.transform.position = leftFootPos;
        }
        leftFootPos = leftFoot.transform.position;

        if (rightFootMoving)
        {
            rightFoot.transform.localPosition = Vector3.Lerp(rightFoot.transform.localPosition, rightFootRestingPos, Time.deltaTime * _rightDist * snapSpeed);
        }
        else
        {
            rightFoot.transform.position = rightFootPos;
        }
        rightFootPos = rightFoot.transform.position;

    }

    public void SetDefaults()
    {
        leftFoot.transform.localPosition = leftFootRestingPos;
        rightFoot.transform.localPosition = rightFootRestingPos;
        leftFootPos = leftFoot.transform.position;
        rightFootPos = rightFoot.transform.position;
    }
}
