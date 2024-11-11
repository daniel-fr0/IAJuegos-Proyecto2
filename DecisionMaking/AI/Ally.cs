using UnityEngine;

public class AllyAI : MonoBehaviour
{
    public StateMachine stateMachine;

    // State
    public State waitSafe;
    public State followPlayer;
    public State waitForPlayer;
    public State fallBackToSafeZone;

    // World information
    public GameObject player;
    public GameObject enemy;
    public RectTransform safeZoneRectangle;
    public int safeZoneLevel = 1;
    private Node safeZoneNode;

    // Transition parameters
    public float detectionRadius = 5.0f;
    public bool debugInfo = false;

    void DefineTransitions()
    {
        Transition waitSafeToFollowPlayer = new Transition
        {
            transitionName = "WaitSafeToFollowPlayer",
            targetState = followPlayer,
            condition = () => GotRescued() && NearPlayer()
        };

        Transition followPlayerToWaitForPlayer = new Transition
        {
            transitionName = "FollowPlayerToWaitForPlayer",
            targetState = waitForPlayer,
            condition = () => !NearPlayer()
        };

        Transition waitForPlayerToFollowPlayer = new Transition
        {
            transitionName = "WaitForPlayerToFollowPlayer",
            targetState = followPlayer,
            condition = () => NearPlayer() && StopWaiting()
        };

        Transition followPlayerToFallbackToSafeZone = new Transition
        {
            transitionName = "FollowPlayerToFallbackToSafeZone",
            targetState = fallBackToSafeZone,
            condition = () => NearEnemy()
        };

        Transition waitForPlayerToFallbackToSafeZone = new Transition
        {
            transitionName = "WaitForPlayerToFallbackToSafeZone",
            targetState = fallBackToSafeZone,
            condition = () => NearEnemy() && StopWaiting()
        };

        Transition fallBackToSafeZoneToWaitSafe = new Transition
        {
            transitionName = "FallBackToSafeZoneToWaitSafe",
            targetState = waitSafe,
            condition = () => InSafeZone()
        };

        waitSafe.transitions.Add(waitSafeToFollowPlayer);

        followPlayer.transitions.Add(followPlayerToFallbackToSafeZone);
        followPlayer.transitions.Add(followPlayerToWaitForPlayer);

        waitForPlayer.transitions.Add(waitForPlayerToFallbackToSafeZone);
        waitForPlayer.transitions.Add(waitForPlayerToFollowPlayer);

        fallBackToSafeZone.transitions.Add(fallBackToSafeZoneToWaitSafe);

        stateMachine.currentState = waitSafe;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (waitSafe == null || followPlayer == null || waitForPlayer == null || fallBackToSafeZone == null)
        {
            Debug.LogError("States not defined in " + gameObject.name);
            return;
        }

        DefineTransitions();

        if (safeZoneNode != null)
        {
            safeZoneNode = new Node(safeZoneLevel);
            safeZoneNode.bounds = safeZoneRectangle.rect;
            safeZoneNode.center = safeZoneRectangle.position;
        }
        else
        {
            Debug.LogError("Safe zone not defined in " + gameObject.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (debugInfo)
        {
            DebugVisuals.DrawRadius(stateMachine.stateKinematicData.position, detectionRadius, Color.yellow);
        }

        transform.position = stateMachine.stateKinematicData.position;
    }

    private bool NearPlayer()
    {
        return Vector3.Distance(transform.position, player.transform.position) < detectionRadius;
    }

    private bool NearEnemy()
    {
        return Vector3.Distance(transform.position, enemy.transform.position) < detectionRadius;
    }

    private bool InSafeZone()
    {
        return safeZoneNode.Contains(new Node(transform.position));
    }

    private bool GotRescued()
    {
        return safeZoneNode.Contains(new Node(player.transform.position));
    }

    private bool StopWaiting()
    {
        // take into account that the kinematic properties have an offset
        // make it stop completely
        stateMachine.stateKinematicData.velocity = Vector3.zero;
        stateMachine.stateKinematicData.rotation = 0;

        // take the transform's position
        Transform transform = stateMachine.stateKinematicData.transform;
        stateMachine.stateKinematicData.position = transform.position;

        return true;
    }
}
