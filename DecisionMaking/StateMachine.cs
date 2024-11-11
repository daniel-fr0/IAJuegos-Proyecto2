using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public State currentState;
    private State _previousState;
    public State previousState
    {
        get { return _previousState; }
        set
        {
            if (value != null)
                _previousState = value;
            else
                Debug.LogError("Assigning a null state to previous state!");
        }
    }
    private Kinematic _stateKinematicData;
    public Kinematic stateKinematicData
    {
        get { return _stateKinematicData; }
        set
        {
            if (value != null)
                _stateKinematicData = value;
            else
                Debug.LogError("Assigning a null kinematic data to state kinematic data!");
        }
    }

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