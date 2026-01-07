using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    public PlayerManager PlayerManager { get; private set; }
    
    public Animator Animator { get; private set; }
    
    protected float dampTime = 0.1f;

    private void Awake()
    {
        PlayerManager = GetComponent<PlayerManager>();
        Animator = GetComponentInChildren<Animator>();
    }

    public void UpdateMovementParameters(float horizontalInput, float verticalInput, bool isSprinting)
    {
        float horizontal = horizontalInput;
        float vertical = verticalInput;

        if (isSprinting)
        {
            vertical = 2;
        }

        Animator.SetFloat("Horizontal Input", horizontal, dampTime, Time.deltaTime);
        Animator.SetFloat("Vertical Input", vertical, dampTime, Time.deltaTime);
    }
}
