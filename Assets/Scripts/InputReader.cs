using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "Input/InputReader")]
public class InputReader : ScriptableObject, InputActions.IPlayerActions {

    public event UnityAction<Vector2> moveEvent;
    public event UnityAction jumpEvent;
    public event UnityAction slideEvent;

    private InputActions inputActions;

    private void OnEnable() {
        if(inputActions == null) {
            inputActions = new InputActions();
            inputActions.Player.SetCallbacks(this);
        }
        inputActions.Enable();
    }

    private void OnDisable() {
        inputActions.Disable();
    }

    public void OnMove(InputAction.CallbackContext context) {
        moveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnJump(InputAction.CallbackContext context) {
        if(jumpEvent != null && context.started) {
            jumpEvent.Invoke();
        }
    }

    public void OnSlide(InputAction.CallbackContext context) {
        if(slideEvent != null && context.started) {
            slideEvent.Invoke();
        }
    }
}
