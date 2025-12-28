using UnityEngine;

public class PlayerLocomotionManager : MonoBehaviour
{
    public PlayerManager PlayerManager { get; private set; }
    public CharacterController CharacterController { get; private set; }

    // List of variables that are read from the input manager and stored locally in this script for readability or calculated within this script.
    [Header("Input Values")]
    Vector2 _movementInput;
    float _verticalMovementInput;
    float _horizontalMovementInput;
    float _moveAmount;
    bool _jumpInput;
    bool _sprintInput;
    bool _dashInput;

    [Header("Movement Values")]
    public float WalkSpeed = 2f;
    public float RunSpeed = 4f;
    public float SprintSpeed = 6f;
    float _currentSpeed = 0f;
    Vector3 _moveDirection;

    [Header("Rotation Values")]
    public float RotationSpeed = 15f;
    Vector3 _targetRotationDirection;

    [Header("Jump & Gravity")]
    public float JumpHeight = 20f;
    public LayerMask JumpLayer;
    [SerializeField] float _groundCheckRadius = 0.15f;
    float _gravity = -9.81f;
    float _inAirTimer;
    bool _fallingVelocitySet;
    Vector3 yVelocity = Vector3.zero;

    [Header("Dash")]
    public AnimationCurve DashDistanceCurve;
    public float MaxDashDistance = 5f;
    public float DashDuration = 0.2f;
    float _dashTimer;
    float _distanceCovered = 0f;
    Vector3 _dashDirection;

    [Header("Movement Flags")]
    bool _isSprinting = false;    // Not the value of sprint input, but whether the player is currently sprinting.
    bool _isGrounded;
    bool _isJumping;
    bool _isDashing;


    private void Awake()
    {
        PlayerManager = GetComponent<PlayerManager>();
        CharacterController = GetComponent<CharacterController>();
    }

    void Update()
    { 
        GetMovementInputValues();
        GetMoveAmountValue();

        HandleSprintInput();
        HandleGroundMovement();
        HandleRotation();

        HandleGroundCheck();
        HandleGravity();
        HandleJump();
        HandleDash();
    }

    void GetMovementInputValues()
    { 
        _movementInput = PlayerInputManager.Instance.MovementInput;
        _verticalMovementInput = _movementInput.y;
        _horizontalMovementInput = _movementInput.x;
        _sprintInput = PlayerInputManager.Instance.SprintInput;

        _jumpInput = PlayerInputManager.Instance.JumpInput;
        _dashInput = PlayerInputManager.Instance.DashInput;

        // Reset certain action inputs in the input manager after reading it.
        PlayerInputManager.Instance.JumpInput = false;
        PlayerInputManager.Instance.DashInput = false;
    }

    // Calculate move amount based on movement input, cap it to specific intervals for the sake of only allowins specific movement states.
    void GetMoveAmountValue()
    {
        _moveAmount = Mathf.Clamp01(Mathf.Abs(_movementInput.x) + Mathf.Abs(_movementInput.y));

        if (_moveAmount > 0f && _moveAmount <= 0.5f)
        {
            _moveAmount = 0.5f;
        }
        else if (_moveAmount > 0.5f && _moveAmount <= 1f)
        {
            _moveAmount = 1f;
        }
    }

    void HandleSprintInput()
    {
        // We need to be moving and on the ground to be able to sprint.
        // This check prevents the player from being able to sprint in place.

        if (_sprintInput)
        {
            if (_moveAmount >= 0.5f && _isGrounded)
                _isSprinting = true;
            else
                _isSprinting = false;
        }
        else
            _isSprinting = false;
    }

    void HandleGroundMovement()
    {
        // This will be used later on when there are actions such as dashing and attacking that prevent players from moving.
        // if (PlayerManager.isPerformingAction)
        // return;

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward.Normalize();
        cameraRight.Normalize();

        _moveDirection = cameraForward * _verticalMovementInput;
        _moveDirection += cameraRight * _horizontalMovementInput;
        _moveDirection.Normalize();

        if (_isSprinting)
        {
            _currentSpeed = SprintSpeed;
        }
        else
        {
            if (_moveAmount == 0.5f)
            {
                _currentSpeed = WalkSpeed;
            }
            else if (_moveAmount == 1f)
            {
                _currentSpeed = RunSpeed;
            }
        }

        CharacterController.Move(Time.deltaTime * _currentSpeed * _moveDirection);
    }

    void HandleRotation()
    {
        // This will be used later on when there are actions such as dashing and attacking that prevent players from moving.
        //if (PlayerManager.isPerformingAction)
        //return;

        if (_isDashing)
            return;

        Quaternion targetRotation = Quaternion.identity;

        // If we are locked onto a target, rotate towards the target instead of movement direction.
        if (PlayerManager.PlayerLockOnManager.CurrentHardLockOnTarget != null)
        {
            Vector3 direction = PlayerManager.PlayerLockOnManager.CurrentHardLockOnTarget.position - transform.position;
            direction.y = 0;

            _targetRotationDirection = Vector3.zero;

            Quaternion newRotation = Quaternion.LookRotation(direction);
            targetRotation = Quaternion.Slerp(transform.rotation, newRotation, RotationSpeed * Time.deltaTime);
        }
        else
        {
            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraRight = Camera.main.transform.right;

            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            _targetRotationDirection = Vector3.zero;
            _targetRotationDirection = cameraForward * _verticalMovementInput;
            _targetRotationDirection += cameraRight * _horizontalMovementInput;
            _targetRotationDirection.y = 0f;

            if (_targetRotationDirection == Vector3.zero)
            {
                _targetRotationDirection = transform.forward;
            }

            Quaternion newRotation = Quaternion.LookRotation(_targetRotationDirection);
            targetRotation = Quaternion.Slerp(transform.rotation, newRotation, RotationSpeed * Time.deltaTime);
        }

        transform.rotation = targetRotation;
    }

    void HandleGroundCheck()
    {
        _isGrounded = Physics.CheckSphere(PlayerManager.transform.position, _groundCheckRadius, JumpLayer);
    }

    void HandleGravity()
    {
        if (_isGrounded)
        {
            if (yVelocity.y < 0)
            {
                _inAirTimer = 0;
                _fallingVelocitySet = false;
                yVelocity.y = _gravity;
                _isJumping = false;
            }
        }
        else
        {
            if (!_isJumping && !_fallingVelocitySet)
            {
                _fallingVelocitySet = true;
                yVelocity.y = 0;
            }

            _inAirTimer += Time.deltaTime;
            yVelocity.y += _gravity * Time.deltaTime;
        }

        // Update in air timer once animations are properly implemented.
        // PlayerManager.animationManager.animator.SetFloat("InAirTimer", _inAirTimer);
        CharacterController.Move(yVelocity * Time.deltaTime);
    }

    void HandleJump()
    {
        // This will be used later on when there are actions such as dashing and attacking that prevent players from jumping.
        //if (PlayerManager.isPerformingAction)
        //return;

        if (_jumpInput && _isGrounded)
        {
            _isJumping = true;
            yVelocity.y = Mathf.Sqrt(JumpHeight * -2 * _gravity);

            // Call jump start animation once animations are properly implemented.
            //PlayerManager.animationManager.PlayTargetActionAnimation("Jump Start", false);
        }

        _jumpInput = false;
    }

    void HandleDash()
    {
        if (_dashInput && !_isDashing)
        {
            // This will be used later on when there are actions such as dashing and attacking that prevent players from jumping.
            //if (PlayerManager.isPerformingAction)
            //return;

            _isDashing = true;
            _dashTimer = 0.0f;

            // If we are moving or intend to move, dash in the movement direction.
            if (_moveAmount != 0f)
            {
                Vector3 cameraForward = Camera.main.transform.forward;
                Vector3 cameraRight = Camera.main.transform.right;

                cameraForward.y = 0;
                cameraRight.y = 0;

                cameraForward.Normalize();
                cameraRight.Normalize();

                _dashDirection = cameraForward * _verticalMovementInput;
                _dashDirection += cameraRight * _horizontalMovementInput;
                _dashDirection.Normalize();
            }

            // If we are not moving just dash forward in the direction the player is facing. 
            else _dashDirection = transform.forward;
        }

        if (_isDashing)
        {
            if (_dashTimer <= DashDuration)
            {
                // Normalise the time to use with animation curve for more control
                float t = _dashTimer / DashDuration;
                float nextDashCurveStep = DashDistanceCurve.Evaluate(t) * MaxDashDistance;
                float dashDistanceThisFrame = nextDashCurveStep - _distanceCovered;

                CharacterController.Move(_dashDirection * dashDistanceThisFrame);
                
                _distanceCovered = nextDashCurveStep;
                _dashTimer += Time.deltaTime;
            }
            else
            {
                _distanceCovered = 0f;
                _isDashing = false;
            }
        }

        _dashInput = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(transform.position, _groundCheckRadius);
    }
}
