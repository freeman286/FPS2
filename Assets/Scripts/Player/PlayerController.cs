using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{

    public float speed = 2f;


    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;

    public float lookSensitivity = 3f;

    [SerializeField]
    public float jumpForce = 100f;

    private PlayerMotor motor;
    private PlayerSetup setup;
    private PlayerShoot shoot;

    private Rigidbody rb;

    [SerializeField]
    private float jumpCooldown = 1f;

    private float timeSinceJumped;

    void Start()
    {
        motor = GetComponent<PlayerMotor>();
        setup = GetComponent<PlayerSetup>();
        shoot = GetComponent<PlayerShoot>();
        rb = GetComponent<Rigidbody>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    void Update()
    {

        if (Pause.isOn)
        {
            motor.Move(Vector3.zero);
            motor.Rotate(Vector3.zero);
            motor.RotateCamera(0f);

            return;
        }

        //WASD movement

        float _xMov = Input.GetAxisRaw("Horizontal");
        float _zMov = Input.GetAxisRaw("Vertical");

        Vector3 _movHorizontal = transform.right * _xMov;
        Vector3 _movVertical = transform.forward * _zMov;

        Vector3 _velocity = (_movHorizontal + _movVertical).normalized * speed;

        motor.Move(_velocity);

        //Mouse movement

        float _yRot = Input.GetAxisRaw("Mouse X");

        Vector3 _rotation = Vector3.up * _yRot * lookSensitivity;

        //Calculate camera rotation as a 3D vector (turning around)
        float _xRot = Input.GetAxisRaw("Mouse Y");

        float _cameraRotationX = _xRot * lookSensitivity;

        if (shoot.isScoped)
        {
            float _scopedSensitivityFactor = shoot.scopedFOV / shoot.defaultFOV;
            _rotation *= _scopedSensitivityFactor;
            _cameraRotationX *= _scopedSensitivityFactor;
        }

        //Apply rotation
        motor.Rotate(_rotation);

        //Apply camera rotation
        motor.RotateCamera(_cameraRotationX);

        //Jump

        if (Input.GetButton("Jump") && timeSinceJumped > jumpCooldown)
        {
            motor.Jump();
            timeSinceJumped = 0;
        }
        timeSinceJumped += Time.deltaTime;
    }
}
