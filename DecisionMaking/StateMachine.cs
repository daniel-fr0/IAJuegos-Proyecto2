using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public State currentState;
    public State previousState;
    public Kinematic stateKinematicData;

    void Start()
    {
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

        if (stateKinematicData == null)
        {
            Debug.LogError("Kinematic data is null!");
            return;
        }

        // Follow the first transition that is triggered
        foreach (Transition transition in currentState.transitions)
        {
            if (transition.IsTriggered())
            {
                stateKinematicData = currentState.ExitState();
                previousState = currentState;
                currentState = transition.targetState;
                stateKinematicData = currentState.EnterState(stateKinematicData);
                break;
            }
        }
    }
}