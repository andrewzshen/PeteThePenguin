using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Int Event Channel")]
public class IntEventChannel : ScriptableObject {
    
    public UnityAction<int> OnEventRaised = delegate {}; 

    public void RaiseEvent(int value) {
        OnEventRaised.Invoke(value);
    }
}