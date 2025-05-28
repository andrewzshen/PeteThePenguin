using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Input Reader", menuName = "Input/InputReader")]
public class InputReader : ScriptableObject, InputActions.IGameplayActions {
    // Gameplay 
    public event UnityAction<Vector2> MoveEvent = delegate {};
    public event UnityAction<bool> JumpEvent = delegate {};
    public event UnityAction<bool> SurfEvent = delegate {};

    private InputActions inputActions;

    private void OnEnable() {
        if(inputActions == null) {
            inputActions = new InputActions();
            inputActions.Gameplay.SetCallbacks(this);
        }
    }

    private void OnDisable() {
        inputActions.Gameplay.Disable();
    }

    public void EnableGameplayActions() {
        inputActions.Gameplay.Enable();
    }

    public void DisableGameplayActions() {
        inputActions.Gameplay.Disable();
    }

    public void OnMove(InputAction.CallbackContext context) {
        MoveEvent.Invoke(context.ReadValue<Vector2>());
    }

    public void OnJump(InputAction.CallbackContext context) {
        if(context.started) {
            JumpEvent.Invoke(true);
        } else if(context.canceled) {
            JumpEvent.Invoke(false);
        }
    }

    public void OnSurf(InputAction.CallbackContext context) {
        if(context.started) {
            SurfEvent.Invoke(true);
        } else if(context.canceled) {
            SurfEvent.Invoke(false);
        }
    }
}
