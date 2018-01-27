using UnityEngine;
using System.Collections.Generic;

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
    public TurnHandlerStates currentState;
    public List<Unit> actionQueue = new List<Unit>();

    public bool actionReady = false, singleActionReady = false;

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

    public void Update()
    {
        if (actionReady && singleActionReady)
        {
            if (actionQueue.Count <= 1)
            {
                actionReady = false;
                singleActionReady = false;
            }
            singleActionReady = false;
            actionQueue[actionQueue.Count - 1].PerformAction();
            actionQueue.RemoveAt(actionQueue.Count - 1);
        }
    }

    public void Setup()
    {
        SwitchState(TurnHandlerStates.PLAYERMOVE);
    }

    public void NextState()
    {
        switch (currentState)
        {
            case TurnHandlerStates.PLAYERMOVE:
                SwitchState(TurnHandlerStates.PLAYERACT);
                break;
            case TurnHandlerStates.PLAYERACT:
                SwitchState(TurnHandlerStates.ENEMYMOVE);
                break;
            case TurnHandlerStates.ENEMYMOVE:
                SwitchState(TurnHandlerStates.ENEMYACT);
                break;
            case TurnHandlerStates.ENEMYACT:
                SwitchState(TurnHandlerStates.PLAYERMOVE);
                break;
        }
    }

    void SwitchState(TurnHandlerStates state)
    {
        switch (state)
        {
            case TurnHandlerStates.PLAYERMOVE:
                SetAllStates(States.MOVE, States.END);
                currentState = TurnHandlerStates.PLAYERMOVE;
                break;
            case TurnHandlerStates.PLAYERACT:
                SetAllStates(States.ACT, States.END);
                currentState = TurnHandlerStates.PLAYERACT;
                break;
            case TurnHandlerStates.ENEMYMOVE:
                SetAllStates(States.END, States.MOVE);
                currentState = TurnHandlerStates.ENEMYMOVE;
                HandleEnemyTurn();
                break;
            case TurnHandlerStates.ENEMYACT:
                SetAllStates(States.END, States.ACT);
                currentState = TurnHandlerStates.ENEMYACT;
                HandleEnemyAct();
                break;
        }
        UIHelper.Instance.SetTurnValues(currentState);
    }

    void SetAllStates(States playerState, States enemyState)
    {
        for (int i = 0; i < Map.Instance.unitDudeFriends.Count; i++)
        {
            Map.Instance.unitDudeFriends[i].GetComponent<UnitStateMachine>().SetState(playerState);
        }
        for (int i = 0; i < Map.Instance.unitDudeEnemies.Count; i++)
        {
            Map.Instance.unitDudeEnemies[i].GetComponent<UnitStateMachine>().SetState(enemyState);
        }
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
        NextState();
        if (NodeManager.Instance.selectedNode != null)
            NodeManager.Instance.Deselect(true);
    }

    public void PerformButton()
    {
        actionReady = true; singleActionReady = true;
        NextState();
    }

    void HandleEnemyTurn()
    {
        for (int i = 0; i < Map.Instance.unitDudeEnemies.Count; i++)
        {
            NodeManager.Instance.AssignPath(Map.Instance.unitDudeEnemies[i].GetComponent<Unit>().currentNode, Map.Instance.unitDudeFriends[0].GetComponent<Unit>().currentNode);
            Map.Instance.unitDudeEnemies[i].GetComponent<Unit>().MoveUnit();
        }
        NextState();
    }

    //TODO DO DIS
    void HandleEnemyAct()
    {
        for (int i = 0; i < Map.Instance.unitDudeEnemies.Count; i++)
        {
            NodeManager.Instance.AssignPath(Map.Instance.unitDudeEnemies[i].GetComponent<Unit>().currentNode, Map.Instance.unitDudeFriends[0].GetComponent<Unit>().currentNode);
            Map.Instance.unitDudeEnemies[i].GetComponent<Unit>().MoveUnit();
        }
        NextState();
    }
}
