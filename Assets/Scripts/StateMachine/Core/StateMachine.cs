using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour {
    private StateNode currentNode;
    private Dictionary<State, StateNode> transitions;
    private HashSet<Transition> anyTransitions;

    private class StateNode {
        public State State { get; }
        public HashSet<Transition> Transitions { get; }

        public StateNode(State state) {
            State = state;
            Transitions = new HashSet<Transition>();
        }

        public void AddTransition(State to, Predicate condition) {
            Transitions.Add(new Transition(to, condition));
        }
    }
}