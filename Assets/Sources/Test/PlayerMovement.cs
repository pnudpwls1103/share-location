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
        // Input Authority를 가진 플레이어만 카메라 할당
        if (HasInputAuthority)
        {
            _camera = Camera.main;
            if (_camera != null)
            {
                var cameraController = _camera.GetComponent<FirstPersonCamera>();
                if (cameraController != null)
                {
                    cameraController.SetTarget(transform);
                }
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Input Authority를 가진 플레이어만 입력 처리
        if (HasInputAuthority == false)
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