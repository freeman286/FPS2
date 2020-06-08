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

    private WeaponManager weaponManager;

    [SerializeField]
    private LayerMask mask;

    private Vector3 lastPos;

    private Vector3 currentPos;

    void Start()
    {
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
    }

    void PerformMovement()
    {
        if (velocity != Vector3.zero)
        {
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }
        lastPos = rb.transform.position;
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
            isGrounded = _hit.distance < 2.15f && Mathf.Abs(rb.velocity.y) < 1f && _hit.distance != 0f;   
            if (Physics.Raycast(transform.position, Vector3.up, out _hit, 1f, mask))
            {
                isGrounded = _hit.distance > 1f;
            }
        }
        
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public bool IsMoving()
    {
        currentPos = rb.position;
        return currentPos != lastPos;
    }

    public void SetDefaults()
    {
        rb.velocity = Vector3.zero;
    }

}
