using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public State initialState;
    public State currentState;

    void Start()
    {
        currentState = initialState;
        currentState.EnterState();
    }

    void Update()
    {
        // Make sure the current state is not null
        if (currentState == null)
        {
            Debug.LogError("Current state is null!");
            return;
        }

        // Follow the first transition that is triggered
        foreach (Transition transition in currentState.transitions)
        {
            if (transition.IsTriggered())
            {
                currentState.ExitState();
                currentState = transition.targetState;
                currentState.EnterState();
                break;
            }
        }
    }
}