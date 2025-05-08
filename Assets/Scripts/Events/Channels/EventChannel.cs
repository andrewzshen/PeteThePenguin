using UnityEngine.Events;

public abstract class EventChannel<T> {
    public UnityAction<T> OnEventRaised = delegate {};

    public void RaiseEvent(T value) {
        OnEventRaised.Invoke(value);
    }
}