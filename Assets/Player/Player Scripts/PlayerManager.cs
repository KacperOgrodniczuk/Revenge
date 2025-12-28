using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public PlayerLocomotionManager PlayerLocomotionManager { get; private set; }
    public PlayerCameraControlManager PlayerCameraControlManager{ get; private set; }
    public PlayerLockOnManager PlayerLockOnManager { get; private set; }

    private void Awake()
    {
        PlayerLocomotionManager = GetComponent<PlayerLocomotionManager>();
        PlayerCameraControlManager = GetComponent<PlayerCameraControlManager>();
        PlayerLockOnManager = GetComponent<PlayerLockOnManager>();
    }
}
