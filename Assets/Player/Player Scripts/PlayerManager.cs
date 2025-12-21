using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public PlayerLocomotionManager PlayerLocomotionManager { get; private set; }

    private void Awake()
    {
        PlayerLocomotionManager = GetComponent<PlayerLocomotionManager>();
    }
}
