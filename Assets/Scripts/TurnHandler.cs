using UnityEngine;

public enum TurnHandlerStates
{
    PLAYERMOVE,
    PLAYERACT,
    ENEMYMOVE,
    ENEMYACT
}

public class TurnHandler : MonoBehaviour
{
    public static TurnHandler Instance;
    public int unitTurnCount;
    public TurnHandlerStates currentState, previousState;

    // REMOVE THESE
    public int friendCount, enemyCount;
    public bool[] friendBool, enemyBool;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("TurnHandler already exists!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Setup()
    {
        currentState = TurnHandlerStates.PLAYERMOVE;
        // Set up states for all units
        for (int i = 0; i < Map.Instance.unitDudeFriends.Count; i++)
        {
            if (currentState == TurnHandlerStates.PLAYERMOVE)
            {
                Map.Instance.unitDudeFriends[i].GetComponent<UnitStateMachine>().SetState(States.MOVE);
                friendCount++;
            }
            else if (currentState == TurnHandlerStates.ENEMYMOVE)
            {
                Map.Instance.unitDudeFriends[i].GetComponent<UnitStateMachine>().SetState(States.END);
                friendCount++;
            }
        }
        for (int i = 0; i < Map.Instance.unitDudeEnemies.Count; i++)
        {
            if (currentState == TurnHandlerStates.PLAYERMOVE)
            {
                Map.Instance.unitDudeEnemies[i].GetComponent<UnitStateMachine>().SetState(States.END);
                enemyCount++;
            }
            else if (currentState == TurnHandlerStates.ENEMYMOVE)
            {
                Map.Instance.unitDudeEnemies[i].GetComponent<UnitStateMachine>().SetState(States.MOVE);
                enemyCount++;
            }
        }
        friendBool = new bool[friendCount];
        enemyBool = new bool[enemyCount];
    }

    void Update () {    //just have a function that goes to next turn state
        if (currentState != previousState)
        {
            if (currentState == TurnHandlerStates.ENEMYMOVE)
            {
                previousState = TurnHandlerStates.ENEMYMOVE;
                for (int i = 0; i < Map.Instance.unitDudeFriends.Count; i++)
                {
                    Map.Instance.unitDudeFriends[i].GetComponent<UnitStateMachine>().SetState(States.END);
                }
                for (int i = 0; i < Map.Instance.unitDudeEnemies.Count; i++)
                {
                    Map.Instance.unitDudeEnemies[i].GetComponent<UnitStateMachine>().SetState(States.MOVE);
                }
            }
            else if (currentState == TurnHandlerStates.PLAYERMOVE)
            {
                previousState = TurnHandlerStates.PLAYERMOVE;
                for (int i = 0; i < Map.Instance.unitDudeFriends.Count; i++)
                {
                    Map.Instance.unitDudeFriends[i].GetComponent<UnitStateMachine>().SetState(States.MOVE);
                }
                for (int i = 0; i < Map.Instance.unitDudeEnemies.Count; i++)
                {
                    Map.Instance.unitDudeEnemies[i].GetComponent<UnitStateMachine>().SetState(States.END);
                }
            }
        }
        if (currentState == TurnHandlerStates.ENEMYMOVE)
            HandleEnemyTurn();
    }

    public void MoveButton()
    {
        for (int i = 0; i < Map.Instance.unitDudeFriends.Count; i++)
        {
            if (Map.Instance.unitDudeFriends[i].GetComponent<Unit>().isEnemy == false)
            {
                Map.Instance.unitDudeFriends[i].GetComponent<Unit>().MoveUnit();
            }
        }
        currentState = TurnHandlerStates.ENEMYMOVE;
    }

    void HandleEnemyTurn()
    {
        for (int i = 0; i < Map.Instance.unitDudeEnemies.Count; i++)
        {
            NodeManager.Instance.AssignPath(Map.Instance.unitDudeEnemies[i].GetComponent<Unit>().currentNode, Map.Instance.unitDudeFriends[0].GetComponent<Unit>().currentNode);
            Map.Instance.unitDudeEnemies[i].GetComponent<Unit>().MoveUnit();
        }
        currentState = TurnHandlerStates.PLAYERMOVE;
    }
}
