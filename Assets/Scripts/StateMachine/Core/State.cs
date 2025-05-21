using UnityEngine;

public interface State {
    public void OnEnter();
    public void Update();
    public void FixedUpdate();
    public void OnExit();
}
