using Fusion;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public float PlayerSpeed = 2f;
    public float JumpForce = 5f;
    public float GravityValue = -9.81f;

    private CharacterController _controller;
    private Vector3 _velocity;
    private Camera _camera;

    private bool _jumpPressed;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            _jumpPressed = true;
        }
    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            _camera = Camera.main;
            _camera.GetComponent<FirstPersonCamera>().SetTarget(transform);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority == false)
        {
            return;
        }

        if (_controller.isGrounded)
        {
            _velocity = new Vector3(0, -1, 0);
        }

        Quaternion cameraRotationY = Quaternion.Euler(0, _camera.transform.rotation.eulerAngles.y, 0);
        Vector3 move = cameraRotationY * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        _velocity.y += GravityValue * Runner.DeltaTime;
        if (_jumpPressed && _controller.isGrounded)
        {
            _velocity.y += JumpForce;
        }
        _controller.Move(move + _velocity * Runner.DeltaTime);

        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }

        _jumpPressed = false;
    }
}