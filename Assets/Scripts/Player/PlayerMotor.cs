using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{

    [SerializeField]
    private Camera cam = null;

    private Vector3 velocity = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private float cameraRotationX = 0f;
    private float currentCameraRotationX = 0f;

    [SerializeField]
    private float minCameraRotationLimit = 50f;

    [SerializeField]
    private float maxCameraRotationLimit = 50f;

    private Rigidbody rb;
    private PlayerController playerController;

    private WeaponManager weaponManager;

    private AnimateFeet feet;

    private PlayerMetrics metrics;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerController>();
        metrics = GetComponent<PlayerMetrics>();
        
        feet = GetComponent<AnimateFeet>();

        GetComponent<Player>().onPlayerSetDefaultsCallback += SetDefaults;
    }

    public void Move(Vector3 _velocity)
    {
        velocity = _velocity * metrics.GetSpeed();
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
        PerformMovement();
        PerformRotation();
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
            currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -minCameraRotationLimit, maxCameraRotationLimit);

            cam.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
        }
    }

    public void Jump()
    {
        if (metrics.IsGrounded())
        {
            rb.velocity = Vector3.up * playerController.jumpForce * metrics.GetJumpMultiplier();
        }
    }

    public void SetDefaults()
    {
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        feet.enabled = true;
        feet.SetDefaults();
    }

    public void Die()
    {
        rb.isKinematic = true;
        feet.enabled = false;
    }

}
