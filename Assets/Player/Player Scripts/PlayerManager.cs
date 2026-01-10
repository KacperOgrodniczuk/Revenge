using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public PlayerLocomotionManager PlayerLocomotionManager { get; private set; }
    public PlayerCameraControlManager PlayerCameraControlManager{ get; private set; }
    public PlayerLockOnManager PlayerLockOnManager { get; private set; }
    public PlayerAnimationManager PlayerAnimationManager { get; private set; }
    public PlayerCombatManager PlayerCombatManager { get; private set; }

    [Header("Character Flags")]
    public bool IsPerformingAction;

    private void Awake()
    {
        PlayerLocomotionManager = GetComponent<PlayerLocomotionManager>();
        PlayerCameraControlManager = GetComponent<PlayerCameraControlManager>();
        PlayerLockOnManager = GetComponent<PlayerLockOnManager>();
        PlayerAnimationManager = GetComponent<PlayerAnimationManager>();
        PlayerCombatManager = GetComponent<PlayerCombatManager>();
    }

    private void Update()
    {
        // If the player is locked on, pass both horizontal and vertical movement.
        if (PlayerLockOnManager.CurrentHardLockOnTarget != null)
        {
            Vector3 localMovementDirection = transform.InverseTransformDirection(PlayerLocomotionManager.MoveDirection);

            PlayerAnimationManager.UpdateMovementParameters(localMovementDirection.x, localMovementDirection.z, PlayerLocomotionManager.IsSprinting);
        }

        // If we're not locked onto an enemy, just pass move amount as vertical movement.
        if(PlayerLockOnManager.CurrentHardLockOnTarget == null)
            PlayerAnimationManager.UpdateMovementParameters(0, PlayerLocomotionManager.MoveAmount, PlayerLocomotionManager.IsSprinting);
    }
}
