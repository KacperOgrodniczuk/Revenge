using Unity.Cinemachine;
using UnityEngine;

public class PlayerCameraControlManager : MonoBehaviour
{
    // NOTE: This script controls the camera target group gameobject.
    // For camera settings find Virtual Camera 1 (Main Player Camera) GameObject in the hierarchy.

    public PlayerManager PlayerManager { get; private set; }

    // Assign in inspector
    [SerializeField] GameObject _cameraTargetGroupObject;
    [SerializeField] GameObject _cameraTargetGroupTarget;   // This object lerps towards the enemy position to help with camera framing.
    CinemachineTargetGroup _cinemachineTargetGroup;
    CinemachineTargetGroup.Target _target;

    [Header("Camera Settings")]
    public float CameraSmoothSpeed = 1f;
    public float RotationSpeed = 50f;
    public float MinimumPivot = -89f;
    public float MaximumPivot = 89f;
    public float LockOnSmoothSpeed = 2f;

    Vector2 _cameraInput;
    float _horizontalLookAngle;  //left and right angle
    float _verticalLookAngle;    //up and down angle
    float _horizontalLookInput;
    float _verticalLookInput;

    private void Awake()
    {
        PlayerManager = GetComponent<PlayerManager>();
        _cinemachineTargetGroup = _cameraTargetGroupObject.GetComponent<CinemachineTargetGroup>();
        _target = _cinemachineTargetGroup.Targets[1];
        _target.Object = _cameraTargetGroupTarget.transform;
    }

    void Update()
    {
        GetCameraInput();
        HandleCameraTargetGroup();
    }

    private void LateUpdate()
    {
        if (_cameraTargetGroupObject == null)
            return;

        RotateCameraTarget();
    }

    void GetCameraInput()
    {
        _cameraInput = PlayerInputManager.Instance.LookInput;
        _horizontalLookInput = _cameraInput.x;
        _verticalLookInput = _cameraInput.y;
    }

    void RotateCameraTarget()
    {
        _horizontalLookAngle += (_horizontalLookInput * RotationSpeed) * Time.deltaTime;
        _verticalLookAngle -= (_verticalLookInput * RotationSpeed) * Time.deltaTime;

        _verticalLookAngle = Mathf.Clamp(_verticalLookAngle, MinimumPivot, MaximumPivot);

        Vector3 cameraRotation = Vector3.zero;
        Quaternion targetRotation;

        cameraRotation.y = _horizontalLookAngle;
        cameraRotation.x = _verticalLookAngle;
        targetRotation = Quaternion.Euler(cameraRotation);
        _cameraTargetGroupObject.transform.rotation = targetRotation;
    }

    void HandleCameraTargetGroup()
    {
        // If the hard lock on target is not null, the player is locked onto an enemy, so we add the enemy to the camera target group.
        if (_cameraTargetGroupObject == null) return;

        if (PlayerManager.PlayerLockOnManager.CurrentHardLockOnTarget != null)
        {
            _cameraTargetGroupTarget.transform.position = Vector3.Lerp(_cameraTargetGroupTarget.transform.position,
                PlayerManager.PlayerLockOnManager.CurrentHardLockOnTarget.position,
                Time.deltaTime * LockOnSmoothSpeed);
            // Smoothly increase the weight of the lock on target.
            _target.Weight = Mathf.Lerp(_target.Weight, 1, Time.deltaTime * LockOnSmoothSpeed);
        }
        else
        {
            _cameraTargetGroupTarget.transform.position = Vector3.Lerp(_cameraTargetGroupTarget.transform.position,
                PlayerManager.transform.position,
                Time.deltaTime * CameraSmoothSpeed);
            // Smoothly decrease the weight of the lock on target if there is no target.
            _target.Weight = Mathf.Lerp(_target.Weight, 0, Time.deltaTime * LockOnSmoothSpeed);
        }

        _cinemachineTargetGroup.Targets[1] = _target;
    }
}