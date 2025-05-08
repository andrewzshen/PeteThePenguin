public class Transition {
    public State To { get; }
    public Predicate Condition { get; }

    public Transition(State to, Predicate condition) {
        To = to;
        Condition = condition;
    }
}