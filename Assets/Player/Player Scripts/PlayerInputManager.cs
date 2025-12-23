using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    InputSystem_Actions _inputSystemActions;

    public static PlayerInputManager Instance;

    [Header("Movement")]
    public Vector2 MovementInput = Vector2.zero;
    public float MoveAmount;
    public bool JumpInput;
    public bool SprintInput;
    public bool DashInput;

    [Header("Look & Mouse")]
    public Vector2 LookInput;

    [Header("Lock On")]
    public bool LockOnInput;
    public bool NextTargetInput;
    public bool PreviousTargetInput;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _inputSystemActions = new InputSystem_Actions();

        _inputSystemActions.Player.Enable();

        //register callbacks
        _inputSystemActions.Player.Move.performed += context => MovementInput = context.ReadValue<Vector2>();
        _inputSystemActions.Player.Look.performed += context => LookInput = context.ReadValue<Vector2>();

        _inputSystemActions.Player.Jump.performed += context => JumpInput = true;
        _inputSystemActions.Player.Dash.performed += context => DashInput = true;

        _inputSystemActions.Player.LockOn.performed += context => LockOnInput = true;
        _inputSystemActions.Player.LockOn.canceled += context => LockOnInput = false;
        _inputSystemActions.Player.Next.performed += context => NextTargetInput = true;
        _inputSystemActions.Player.Previous.performed += context => PreviousTargetInput = true;

        _inputSystemActions.Player.Sprint.performed += context => SprintInput = true;
        _inputSystemActions.Player.Sprint.canceled += context => SprintInput = false;

        LockCursor();
    }
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
