using UnityEngine;
using UnityEngine.EventSystems;

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

    Vector3 _moveDirection;
    Vector3 _targetRotationDirection;
    public bool IsSprinting = false;    // Not the value of sprint input, but whether the player is currently sprinting.

    [Header("Speed & Rotation Values")]
    public float WalkSpeed = 2f;
    public float RunSpeed = 4f;
    public float SprintSpeed = 6f;
    public float RotationSpeed = 15f;
    float _currentSpeed = 0f;

    //Header("Jump & Gravity")      // TODO: Implement jump and gravity after base locomotion and dashing is done.

    //Header("Dashing")     //TODO: Implement dashing after base locomotion is done.

    private void Awake()
    {
        PlayerManager = GetComponent<PlayerManager>();
        CharacterController = GetComponent<CharacterController>();
    }

    void Update()
    { 
        GetMovementInputValues();
        GetMoveAmountValue();
    }

    void GetMovementInputValues()
    { 
        _movementInput = PlayerInputManager.Instance.MovementInput;
        _verticalMovementInput = _movementInput.y;
        _horizontalMovementInput = _movementInput.x;
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
}
