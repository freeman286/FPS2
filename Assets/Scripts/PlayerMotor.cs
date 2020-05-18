﻿using System.Collections;
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

    private WeaponManager weaponManager;

    [SerializeField]
    private LayerMask mask;

    void Start()
    {
        SetDefaults();
        rb = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerController>();
        weaponManager = GetComponent<WeaponManager>();
    }

    public void Move(Vector3 _velocity)
    {
        velocity = _velocity.normalized * playerController.speed * weaponManager.GetCurrentWeapon().speed;
    }

    public void Rotate(Vector3 _rotation)
    {
        rotation = _rotation;
    }

    public void AddRotation(Vector3 _rotation)
    {
        rotation += _rotation;
    }

    public void RotateCamera(float _cameraRotationX)
    {
        cameraRotationX = _cameraRotationX;
    }

    public void AddRotationCamera(float _cameraRotationX)
    {
        cameraRotationX += _cameraRotationX;
    }

    void FixedUpdate()
    {
        CheckGrounded();
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

        if (_leftDist > 4f || _rightDist > 4f)
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

    void CheckGrounded()
    {
        RaycastHit _hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out _hit, 3f, mask));
        {
            isGrounded = _hit.distance < 2.5f && Mathf.Abs(rb.velocity.y) < 1f && _hit.distance != 0f;
            if (Physics.Raycast(transform.position, Vector3.up, out _hit, 1f, mask))
            {
                isGrounded = _hit.distance > 1f;
            }
        }
        
    }

    public void SetDefaults()
    {
        leftFoot.transform.localPosition = leftFootRestingPos;
        rightFoot.transform.localPosition = rightFootRestingPos;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public bool IsMoving()
    {
        return leftFootMoving || rightFootMoving;
    }

}
