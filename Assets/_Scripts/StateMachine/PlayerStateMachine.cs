using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
    [SerializeField] private Transform _cameraFollowTarget;
    private PlayerInput _playerInput;
    private CharacterController _characterController;
    private Animator _animator;

    private int _isWalkingHash;
    private int _isRunningHash;

    private Vector2 _currentMovementInput;
    private Vector3 _currentMovement;
    private Vector3 _currentRunMovement;
    private Vector3 _appliedMovement;
    private bool _isMovementPressed;
    private bool _isRunPressed;

    private float _rotationFactorPreFrame = 15.0f;
    private float _speedMultiplier = 3.0f;
    private int _zero = 0;


    private float _gravity = -9.8f;
    private float _groundedGravity = -.05f;

    private bool _isJumpPressed = false;
    private float _initiaJumVelocity;
    private float _maxJumpHeight = 4.0f;
    private float _maxJumpTime = 0.75f;
    private bool _isJumping = false;
    private int _isJumpingHash;
    private int _jumpCountHash;
    private bool _requireNewJumpPress = false;
    private int _jumpCount = 0;
    Dictionary<int, float> _initialJumpVelocities = new Dictionary<int, float>();
    Dictionary<int, float> _jumpGravities = new Dictionary<int, float>();
    private Coroutine _currentJumpResetRoutine = null;

    PlayerBaseState _currentState;
    PlayerStateFactory _states;
    private float _xRotation;
    private float _yRotation;
    private Vector2 _look;
    private Camera _mainCam;
    private float _targetRotation;
    private float rotationSpeed = 0.3f;
    private float smoothSpeed = 1000f;
    public PlayerBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }
    public CharacterController CharacterController { get { return _characterController; } }
    public Animator Animator { get { return _animator; } }
    public Coroutine CurrentJumpResetRoutine { get { return _currentJumpResetRoutine; } set { _currentJumpResetRoutine = value; } }
    public Dictionary<int, float> InitialJumpVelocities { get { return _initialJumpVelocities; } }
    public Dictionary<int, float> JumpGravities { get { return _jumpGravities; } }
    public int JumpCount { get { return _jumpCount; } set { _jumpCount = value; } }
    public int IsWalkingHash { get { return _isWalkingHash; } set { _isWalkingHash = value; } }
    public int IsRunningHash { get { return _isRunningHash; } set { _isRunningHash = value; } }
    public int IsJumpingHash { get { return _isJumpingHash; } set { _isJumpingHash = value; } }
    public int JumpCountHash { get { return _jumpCountHash; } }
    public bool IsMovementPressed { get { return _isMovementPressed; } }
    public bool IsRunPressed { get { return _isRunPressed; } }
    public bool RequireNewJumpPress { get { return _requireNewJumpPress; } set { _requireNewJumpPress = value; } }
    public bool IsJumping { set { _isJumping = value; } }
    public bool IsJumpPressed { get { return _isJumpPressed; } }
    public float GroundedGravity { get { return _groundedGravity; } }
    public float CurrentMovementY { get { return _currentMovement.y; } set { _currentMovement.y = value; } }
    public float AppliedMovementY { get { return _appliedMovement.y; } set { _appliedMovement.y = value; } }
    public float AppliedMovementX { get { return _appliedMovement.x; } set { _appliedMovement.x = value; } }
    public float AppliedMovementZ { get { return _appliedMovement.z; } set { _appliedMovement.z = value; } }
    public Vector3 AppliedMovement { get { return _appliedMovement; } set { _appliedMovement = value; } }
    public float SpeedMultiplier { get { return _speedMultiplier; } }
    public float TargetRotation { get { return _targetRotation; } }
    public Vector2 CurrentMovementInput { get { return _currentMovementInput; } }


    private void Awake()
    {
        _playerInput = new PlayerInput();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _mainCam = Camera.main;

        _states = new PlayerStateFactory(this);
        _currentState = _states.Grounded();
        _currentState.EnterState();

        _isWalkingHash = Animator.StringToHash("isWalking");
        _isRunningHash = Animator.StringToHash("isRunning");
        _isJumpingHash = Animator.StringToHash("isJumping");
        _jumpCountHash = Animator.StringToHash("jumpCount");

        _playerInput.CharacterControls.Move.started += OnMovementInput;
        _playerInput.CharacterControls.Move.canceled += OnMovementInput;
        _playerInput.CharacterControls.Move.performed += OnMovementInput;

        _playerInput.CharacterControls.Run.started += OnRun;
        _playerInput.CharacterControls.Run.canceled += OnRun;

        _playerInput.CharacterControls.Jump.started += OnJump;
        _playerInput.CharacterControls.Jump.canceled += OnJump;

        _playerInput.CharacterControls.Look.started += OnLook;
        _playerInput.CharacterControls.Look.canceled += OnLook;
        _playerInput.CharacterControls.Look.performed += OnLook;


        SetupJumpVariables();
    }
    void SetupJumpVariables()
    {
        float timeToApex = _maxJumpTime / 2;
        _gravity = (-2 * _maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        _initiaJumVelocity = (2 * _maxJumpHeight) / timeToApex;
        float secondJumpGravity = (-2 * (_maxJumpHeight + 2)) / Mathf.Pow(timeToApex * 1.25f, 2);
        float secondJumpInitialVelocity = (2 * (_maxJumpHeight + 2)) / (timeToApex * 1.25f);
        float thirdJumpGravity = (-2 * (_maxJumpHeight + 4)) / Mathf.Pow((timeToApex * 1.5f), 2);
        float thirdJumpInitialVelocity = (2 * (_maxJumpHeight + 4)) / (timeToApex * 1.5f);

        _initialJumpVelocities.Add(1, _initiaJumVelocity);
        _initialJumpVelocities.Add(2, secondJumpInitialVelocity);
        _initialJumpVelocities.Add(3, thirdJumpInitialVelocity);

        _jumpGravities.Add(0, _gravity);
        _jumpGravities.Add(1, _gravity);
        _jumpGravities.Add(2, secondJumpGravity);
        _jumpGravities.Add(3, thirdJumpGravity);
    }

    void Update()
    {
        HandleRotation();
        _currentState.UpdateStates();
        _characterController.Move(_appliedMovement * Time.deltaTime);
    }
    private void LateUpdate()
    {
        CameraRotation();

    }
    private void HandleRotation()
    {
        Vector3 positionToLookAt;

        positionToLookAt.x = _currentMovementInput.x;
        positionToLookAt.y = _zero;
        positionToLookAt.z = _currentMovementInput.y;
        Quaternion currentRotation = transform.rotation;
        _targetRotation = 0;
        _speedMultiplier = 0;

        if (_isMovementPressed)
        {
            _speedMultiplier = 4f;
            _targetRotation = Quaternion.LookRotation(positionToLookAt).eulerAngles.y + _mainCam.transform.rotation.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(0, _targetRotation, 0);
            transform.rotation = Quaternion.Slerp(currentRotation, rotation, _rotationFactorPreFrame * Time.deltaTime);
        }
        else 
        {
            _targetRotation = _mainCam.transform.rotation.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(0, _targetRotation, 0);
            transform.rotation = Quaternion.Slerp(currentRotation, rotation, _rotationFactorPreFrame * Time.deltaTime);
        }
    }
    private void CameraRotation()
    {
        _xRotation += _look.y * rotationSpeed;
        _yRotation += _look.x * rotationSpeed;
   
        _xRotation = Mathf.Clamp(_xRotation, -18, 50);
        Quaternion targetRotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        _cameraFollowTarget.rotation = Quaternion.Slerp(_cameraFollowTarget.rotation, targetRotation, Time.deltaTime * smoothSpeed);
    }
    public void OnMovementInput(InputAction.CallbackContext context)
    {
        _currentMovementInput = context.ReadValue<Vector2>();
        _currentMovement.x = !_isRunPressed ? _currentMovementInput.x : _currentMovementInput.x * _speedMultiplier;
        _currentMovement.z = !_isRunPressed ? _currentMovementInput.y : _currentMovementInput.y * _speedMultiplier;
        _isMovementPressed = _currentMovementInput.x != _zero || _currentMovementInput.y != _zero;
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        _isJumpPressed = context.ReadValueAsButton();
        _requireNewJumpPress = false;
    }
    public void OnRun(InputAction.CallbackContext context)
    {
        _isRunPressed = context.ReadValueAsButton();
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        _look = context.ReadValue<Vector2>();
    }

    private void OnEnable()
    {
        _playerInput.CharacterControls.Enable();
    }
    private void OnDisable()
    {
        _playerInput.CharacterControls.Disable();
    }
}

