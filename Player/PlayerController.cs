using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float sprintSpeed = 6f;
    public float speedChangeRate = 8f;
    public float gravityChangeRate = 9.8f;
    public float checkGroundedOffset = 0.1f;
    public float speedOffset = 0.1f;
    public float rotationSpeed = 1f;
    public float rotationSmoothTime = 0.2f;
    public float jumpHeight = 1f;
    public float maxFallSpeed = 50f;
    public float jumpInterval = 0.5f;
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isFall;
    [SerializeField]private Vector3 _horizontalVelocity;
    [SerializeField]private Vector3 _verticalVelocity;
    [SerializeField] private float _rotationVelocity;
    
    private InputManager _input;
    private CharacterController _characterController;
    private Animator _animator;
    public GameObject _mainCamera;
    private float timer;
    private const float _threshold = 0.01f; 
    [SerializeField]
    private float _moveSpeed;

    private float _targetRotation;
    private float _fallSpeed;
    private float _upSpeed;
    private float _jumpTimer;
    private float _cinemachineTargetPitch;

    public GameObject CinemachineCameraTarget;
    public float topClamp=75f;
    [FormerlySerializedAs("BottomClamp")] public float bottomClamp=-85f;
    
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _input = GetComponent<InputManager>();
        _animator = GetComponentInChildren<Animator>();
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        CheckIsGrounded();
        JumpAndGravity();
        Move();
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void Move()
    {
        if (isGrounded)
        {
            float targetSpeed = _input.isRun ? runSpeed : walkSpeed;
            if (_input.moveDirection == Vector2.zero)
            {
                targetSpeed = 0f;
            }
            float currentSpeed = new Vector3(_characterController.velocity.x, 0, _characterController.velocity.z)
                .magnitude;

            if (currentSpeed < targetSpeed - speedOffset || currentSpeed > targetSpeed + speedOffset)
            {
                _moveSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * speedChangeRate);
                _moveSpeed = Mathf.Round(_moveSpeed * 1000f) / 1000f;
            }
            else
            {
                _moveSpeed = targetSpeed;
            }

        }
        _horizontalVelocity = (_input.moveDirection.x*transform.right+_input.moveDirection.y*transform.forward).normalized * _moveSpeed;
        if (timer > 0.5)
        {
            Debug.Log(transform.forward);
            timer = 0;
        }
        _animator.SetFloat("Speed",_horizontalVelocity.magnitude);
        
        Vector3 finalVelocity = _horizontalVelocity + _verticalVelocity;
        _characterController.Move(finalVelocity*Time.deltaTime);
    }

    private void CheckIsGrounded()
    {
        Vector3 position = new Vector3(transform.position.x, transform.position.y+_characterController.radius+checkGroundedOffset, transform.position.z);
        isGrounded=Physics.CheckSphere(position, _characterController.radius,LayerMask.GetMask("Ground","Interactable"));
        _animator.SetBool("Grounded",isGrounded);
        Debug.DrawLine(position,new Vector3(position.x,position.y-_characterController.radius,position.z),Color.green);
        // if (timer >= 0.5)
        // {
        //     Debug.Log("isGrounded="+isGrounded);
        //     timer = 0;
        // }
    }

    private void JumpAndGravity()
    {
        
        if (isGrounded)
        {
            _fallSpeed = 2f;
            _upSpeed = 0f;
            isFall = false;
            _animator.SetBool("Fall",false);
            _jumpTimer += Time.deltaTime;
            if (_input.isJump&&_jumpTimer>jumpInterval)
            {
                _upSpeed = Mathf.Sqrt(2 * jumpHeight * gravityChangeRate)+2;
                _animator.SetBool("Jump",true);
                _jumpTimer = 0;
            }
        }
        else
        {
            if (_fallSpeed < maxFallSpeed)
            {
                _fallSpeed += gravityChangeRate*Time.deltaTime;
            }
            isFall = true;
            _animator.SetBool("Fall",true);
            _animator.SetBool("Jump",false);
        }

        _verticalVelocity = Vector3.up * _upSpeed + Vector3.down * _fallSpeed;
    }

    private void CameraRotation()
    {
        if (_input.lookDirection.sqrMagnitude >= _threshold)
        {
            float deltaMultiplier = 1f;
            _cinemachineTargetPitch += _input.lookDirection.y * rotationSpeed * deltaMultiplier;
            _rotationVelocity = _input.lookDirection.x * rotationSpeed * deltaMultiplier;

            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);
            //handle camera up and down
            CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0f, 0f);
            //handle player rotate left and right
            transform.Rotate(Vector3.up*_rotationVelocity);
        }
    }
    
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}
