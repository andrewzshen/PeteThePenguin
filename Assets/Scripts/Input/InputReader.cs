using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "Input/InputReader")]
public class InputReader : ScriptableObject, InputActions.IGameplayActions {

    // Gameplay 
    public event UnityAction<Vector2> MoveEvent = delegate {};
    public event UnityAction JumpEvent = delegate {};
    public event UnityAction JumpCanceledEvent = delegate {};   
    public event UnityAction SlideEvent = delegate {};

    private InputActions inputActions;

    private void OnEnable() {
        if(inputActions == null) {
            inputActions = new InputActions();
            inputActions.Gameplay.SetCallbacks(this);
        }
        inputActions.Enable();
    }

    private void OnDisable() {
        inputActions.Disable();
    }

    public void OnMove(InputAction.CallbackContext context) {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnJump(InputAction.CallbackContext context) {
        if(context.phase == InputActionPhase.Performed) {
            JumpEvent.Invoke();
        }
        if(context.phase == InputActionPhase.Canceled) {
            JumpCanceledEvent.Invoke();
        }
    }

    public void OnSlide(InputAction.CallbackContext context) {
        if(SlideEvent != null && context.started) {
            SlideEvent.Invoke();
        }
    }
}
