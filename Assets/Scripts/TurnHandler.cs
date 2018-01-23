using UnityEngine;

public enum TurnHandlerStates
{
    PLAYERTURN,
    ENEMYTURN
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

    // Use this for initialization
    public void Setup()
    {
        currentState = TurnHandlerStates.PLAYERTURN;
        // Set up states for all units
        for (int i = 0; i < Map.Instance.unitDudeFriends.Count; i++)
        {
            if (currentState == TurnHandlerStates.PLAYERTURN)
            {
                Map.Instance.unitDudeFriends[i].GetComponent<UnitStateMachine>().SetState(States.MOVE);
                friendCount++;
            }
            else if (currentState == TurnHandlerStates.ENEMYTURN)
            {
                Map.Instance.unitDudeFriends[i].GetComponent<UnitStateMachine>().SetState(States.END);
                friendCount++;
            }
        }
        for (int i = 0; i < Map.Instance.unitDudeEnemies.Count; i++)
        {
            if (currentState == TurnHandlerStates.PLAYERTURN)
            {
                Map.Instance.unitDudeEnemies[i].GetComponent<UnitStateMachine>().SetState(States.END);
                enemyCount++;
            }
            else if (currentState == TurnHandlerStates.ENEMYTURN)
            {
                Map.Instance.unitDudeEnemies[i].GetComponent<UnitStateMachine>().SetState(States.MOVE);
                enemyCount++;
            }
        }
        friendBool = new bool[friendCount];
        enemyBool = new bool[enemyCount];
    }

    // Update is called once per frame
    void Update () {
        if (currentState != previousState)
        {
            if (currentState == TurnHandlerStates.ENEMYTURN)
            {
                previousState = TurnHandlerStates.ENEMYTURN;
                for (int i = 0; i < Map.Instance.unitDudeFriends.Count; i++)
                {
                    Map.Instance.unitDudeFriends[i].GetComponent<UnitStateMachine>().SetState(States.END);
                }
                for (int i = 0; i < Map.Instance.unitDudeEnemies.Count; i++)
                {
                    Map.Instance.unitDudeEnemies[i].GetComponent<UnitStateMachine>().SetState(States.MOVE);
                }
            }
        }
        if (currentState == TurnHandlerStates.ENEMYTURN)
            handleEnemyTurn();
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
        currentState = TurnHandlerStates.ENEMYTURN;
    }
    void handleEnemyTurn()
    {
        for (int i = 0; i < Map.Instance.unitDudeEnemies.Count; i++)
        {
            NodeManager.Instance.AssignPath(Map.Instance.unitDudeEnemies[i].GetComponent<Unit>().currentNode, Map.Instance.unitDudeFriends[0].GetComponent<Unit>().currentNode);
            Map.Instance.unitDudeEnemies[i].GetComponent<Unit>().MoveUnit();
        }
        currentState = TurnHandlerStates.PLAYERTURN;
    }
}
