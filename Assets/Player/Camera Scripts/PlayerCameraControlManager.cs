using UnityEngine;

public class PlayerCameraControlManager : MonoBehaviour
{
    // NOTE: This script controls the camera follow target gameobject attached to the player.
    // For camera settings find virtual Camera 1 (Main Player Camera) GameObject in the hierarchy.

    // Assign in inspector
    [SerializeField] GameObject _cameraFollowTarget;

    [Header("Camera Settings")]
    public float CameraSmoothSpeed = 1f;
    public float RotationSpeed = 50f;
    public float MinimumPivot = -89f;
    public float MaximumPivot = 89f;

    Vector2 _cameraInput;
    float _horizontalLookAngle;  //left and right angle
    float _verticalLookAngle;    //up and down angle
    float _horizontalLookInput;
    float _verticalLookInput;

    void Update()
    {
        GetCameraInput();
    }

    private void LateUpdate()
    {
        if (_cameraFollowTarget == null)
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
        _cameraFollowTarget.transform.rotation = targetRotation;
    }
}