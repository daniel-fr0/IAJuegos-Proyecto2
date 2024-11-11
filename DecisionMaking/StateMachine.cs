using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public State initialState;
    public State currentState;
    public Kinematic stateKinematicData;

    void Start()
    {
        currentState = initialState;
        stateKinematicData = currentState.EnterState(null);
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
                stateKinematicData = currentState.ExitState();
                currentState = transition.targetState;
                currentState.EnterState(stateKinematicData);
                break;
            }
        }
    }
}