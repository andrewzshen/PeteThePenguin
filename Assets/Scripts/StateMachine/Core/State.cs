using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "State", menuName = "State Machine/State")]
public class State : ScriptableObject {
    [SerializeField] private Action[] actions;

    public void OnEnter() {

    }

    public void Update() {
        foreach(Action action in actions) {
            action.Update();
        }
    }

    public void FixedUpdate() {

    }

    public void OnExit() {

    }
}
