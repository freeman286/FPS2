using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{

    [SerializeField]
    private Camera cam;

    private Vector3 velocity = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private float cameraRotationX = 0f;
    private float currentCameraRotationX = 0f;

    [SerializeField]
    private float cameraRotationLimit = 50f;

    private Rigidbody rb;
    private PlayerController playerController;

    public bool isGrounded = true;

    [SerializeField]
    private GameObject leftFoot;
    [SerializeField]
    private GameObject rightFoot;

    [SerializeField]
    private Vector3 leftFootRestingPos;
    [SerializeField]
    private Vector3 rightFootRestingPos;

    private Vector3 leftFootPos;
    private Vector3 rightFootPos;

    [SerializeField]
    private float snapDist;

    [SerializeField]
    private float snapSpeed = 100f;


    private bool leftFootMoving = true;
    private bool rightFootMoving = true;

    void Start()
    {
        SetDefaults();
        rb = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerController>();
    }

    public void Move(Vector3 _velocity)
    {
        velocity = _velocity.normalized * playerController.speed;
    }

    public void Rotate(Vector3 _rotation)
    {
        rotation = _rotation;
    }

    public void RotateCamera(float _cameraRotationX)
    {
        cameraRotationX = _cameraRotationX;
    }

    void FixedUpdate()
    {
        PerformMovement();
        PerformRotation();
        PerformFootMovement();
    }

    void PerformMovement()
    {
        if (velocity != Vector3.zero)
        {
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }
    }

    void PerformRotation()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
        if (cam != null)
        {
            currentCameraRotationX -= cameraRotationX;
            currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);

            cam.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
        }
    }

    void PerformFootMovement()
    {
        float _leftDist = Vector3.Distance(leftFootRestingPos, leftFoot.transform.localPosition);
        float _rightDist = Vector3.Distance(rightFootRestingPos, rightFoot.transform.localPosition);

        if (_leftDist > 2f || _rightDist > 2f)
        {
            leftFoot.transform.localPosition = leftFootRestingPos;
            rightFoot.transform.localPosition = rightFootRestingPos;
        }

        leftFootMoving = (_leftDist > snapDist && !rightFootMoving) || !isGrounded;

        rightFootMoving = (_rightDist > snapDist && !leftFootMoving) || !isGrounded;

        if (leftFootMoving)
        {
            leftFoot.transform.localPosition = Vector3.Lerp(leftFoot.transform.localPosition, leftFootRestingPos, Time.deltaTime * _leftDist * snapSpeed);
        } else
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

    public void Jump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * playerController.jumpForce);
            isGrounded = false;
        }
    }

    void OnCollisionEnter(Collision collisionInfo)
    {
        isGrounded = true;
    }

    void OnCollisionExit(Collision collisionInfo)
    {
        isGrounded = false;
    }

    public void SetDefaults()
    {
        leftFoot.transform.localPosition = leftFootRestingPos;
        rightFoot.transform.localPosition = rightFootRestingPos;
        Debug.Log("Setup");
    }
}
