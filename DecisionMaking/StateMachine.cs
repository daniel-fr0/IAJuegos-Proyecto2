using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public State state;
    public Kinematic stateKinematicData;

    void Start()
    {
        stateKinematicData = state.EnterState(null);
    }

    void Update()
    {
        // Make sure the current state is not null
        if (state == null)
        {
            Debug.LogError("Current state is null!");
            return;
        }

        // Follow the first transition that is triggered
        foreach (Transition transition in state.transitions)
        {
            if (transition.IsTriggered())
            {
                stateKinematicData = state.ExitState();
                state = transition.targetState;
                state.EnterState(stateKinematicData);
                break;
            }
        }
    }
}