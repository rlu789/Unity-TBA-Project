using UnityEngine;
using System.Collections.Generic;
using System.Collections;

//public enum TurnHandlerStates2
//{
//    PLAYERMOVE,
//    ENEMYMOVE,
//    PLAYERACT,
//    ENEMYACT,
//    BATTLEACT
//}

public enum TurnHandlerStates
{
    ENEMYDRAW,
    PLAYERDRAW,
    PLAYERSELECT,
    PLAYERTURN,
    ENEMYTURN,
    END
}

public class TurnHandler : MonoBehaviour
{
    public static TurnHandler Instance;
    public int unitTurnCount;
    public TurnHandlerStates currentState;
    public List<Unit> actionQueue = new List<Unit>();
    public SortedDictionary<float, Unit> orderedActions = new SortedDictionary<float, Unit>();
    
    float haveYetToCrossTheBridge = 0.01f;

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
        SwitchState(TurnHandlerStates.ENEMYDRAW);
    }

    public void NextState()
    {
        switch (currentState)
        {
            case TurnHandlerStates.ENEMYDRAW:
                SwitchState(TurnHandlerStates.PLAYERDRAW);
                break;
            case TurnHandlerStates.PLAYERDRAW:
                SwitchState(TurnHandlerStates.PLAYERSELECT);
                break;
            case TurnHandlerStates.PLAYERSELECT:
                SwitchState(DetermineTurn());
                break;
            case TurnHandlerStates.PLAYERTURN:
                SwitchState(DetermineTurn());
                break;
            case TurnHandlerStates.ENEMYTURN:
                SwitchState(DetermineTurn());
                break;
            case TurnHandlerStates.END:
                SwitchState(TurnHandlerStates.ENEMYDRAW);
                break;
        }
    }

    TurnHandlerStates DetermineTurn()
    {
        if (orderedActions.Count == 0)
        {
            Debug.Log("Dgf");
            return TurnHandlerStates.END;
        }
        if (!orderedActions[0].isEnemy)
        {
            orderedActions[0].GetComponent<UnitStateMachine>().state = States.B_SELECTING;
            NodeManager.Instance.SetSelectedNode(orderedActions[0].GetComponent<Unit>().currentNode);
            return TurnHandlerStates.PLAYERTURN;
        }
        else
        {
            orderedActions[0].GetComponent<UnitStateMachine>().state = States.B_SELECTING;
            return TurnHandlerStates.ENEMYTURN;
        }
    }

    void SwitchState(TurnHandlerStates state)
    {
        //UIHelper.Instance.SetTurnValues(state);
        switch (state)
        {
            case TurnHandlerStates.ENEMYDRAW:
                //function to have enemy draw their cards
                SetAllStates(States.START, States.DRAW);
                currentState = TurnHandlerStates.ENEMYDRAW;
                //HandleStatus();
                NextState();
                break;
            case TurnHandlerStates.PLAYERDRAW:
                SetAllStates(States.DRAW, States.WAIT);
                currentState = TurnHandlerStates.PLAYERDRAW;
                DrawCards();
                NextState();
                break;
            case TurnHandlerStates.PLAYERSELECT:
                SetAllStates(States.SELECT, States.WAIT);
                currentState = TurnHandlerStates.PLAYERSELECT;
                //defs not cheating lol
                LazyTestingCBFFunction();
                DetermineTurnOrder();
                NextState();
                break;
            case TurnHandlerStates.PLAYERTURN:
                //SetAllStates(States.END, States.ACT);
                currentState = TurnHandlerStates.PLAYERTURN;
                break;
            case TurnHandlerStates.ENEMYTURN:
                //SetAllStates(States.END, States.ACT);
                currentState = TurnHandlerStates.ENEMYTURN;
                HandleEnemyAct();
                break;
            case TurnHandlerStates.END:
                SetAllStates(States.END, States.END);
                currentState = TurnHandlerStates.END;
                break;
        }
        //UIHelper.Instance.SetTurnValues(currentState);
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

    void DrawCards()
    {
        for (int i = 0; i < Map.Instance.unitDudeFriends.Count; i++)
        {
            if (Map.Instance.unitDudeFriends[i].GetComponent<Unit>().isEnemy == false)
            {
                Map.Instance.unitDudeFriends[i].GetComponent<Unit>().DrawCards(1);
            }
        }
    }

    public void PerformButton()
    {
        NextState();
    }

    void HandleEnemyTurn()
    {
        for (int i = 0; i < Map.Instance.unitDudeEnemies.Count; i++)
        {
            AIHelper.Instance.AIGetTurn(Map.Instance.unitDudeEnemies[i].GetComponent<Unit>());
            Map.Instance.unitDudeEnemies[i].GetComponent<Unit>().MoveUnit();
        }
        NextState();
    }

    void HandleEnemyAct()
    {
        for (int i = 0; i < Map.Instance.unitDudeEnemies.Count; i++)
        {
            Unit enemy = Map.Instance.unitDudeEnemies[i].GetComponent<Unit>();
            AIHelper.Instance.ConfirmBestAction(enemy);
            if (enemy.readyAction != null && !enemy.readyAction.isEmpty()) actionQueue.Add(enemy);
        }
        NextState();
    }

    //IEnumerator BattleAct() //TODO: need to ignore units that were killed
    //{
    //    foreach(Unit u in actionQueue)
    //    {
    //        if (u.readyAction == null) continue;
    //        if (orderedActions.ContainsKey(u.readyAction.initiative))
    //        {
    //            orderedActions.Add(u.readyAction.initiative + haveYetToCrossTheBridge, u);
    //            haveYetToCrossTheBridge += 0.01f;
    //        }
    //        else
    //            orderedActions.Add(u.readyAction.initiative, u);
    //    }
    //    //TODO CROSS THIS BRIDGE WHEN WE GET THERE
    //    foreach (KeyValuePair<float, Unit> unit in orderedActions)
    //    {
    //        //Debug.Log("Key: " + unit.Key + ", Value: {1} " + unit.Value + " using action " + unit.Value.readyAction.name);
    //        if (unit.Value == null) continue;
    //        unit.Value.PerformAction();
    //        yield return new WaitForSeconds(1f);
    //    }

    //    haveYetToCrossTheBridge = 0.01f;
    //    actionQueue.Clear();
    //    orderedActions.Clear();
    //    NextState();
    //}

    void HandleStatus()
    {
        foreach (GameObject unit in Map.Instance.unitDudeFriends)
        {
            StatusHelper.Instance.CheckStatus(unit.GetComponent<Unit>());
        }
    }

    void DetermineTurnOrder()
    {
        for (int i = 0; i < Map.Instance.unitDudeFriends.Count; i++)
        {
            orderedActions.Add(i, Map.Instance.unitDudeFriends[i].GetComponent<Unit>());
        }
        for (int i = 0; i < Map.Instance.unitDudeEnemies.Count; i++)
        {
            orderedActions.Add(i + 10, Map.Instance.unitDudeEnemies[i].GetComponent<Unit>());
        }
        Debug.Log(orderedActions.Count);
    }

    void LazyTestingCBFFunction()
    {
        for (int i = 0; i < Map.Instance.unitDudeFriends.Count; i++)
        {
            Map.Instance.unitDudeFriends[i].GetComponent<Unit>().selectedActions = Map.Instance.unitDudeFriends[i].GetComponent<Unit>().availableActions;
            Map.Instance.unitDudeFriends[i].GetComponent<Unit>().GetComponent<UnitStateMachine>().state = States.WAIT;
        }
        for (int i = 0; i < Map.Instance.unitDudeEnemies.Count; i++)
        {
            Map.Instance.unitDudeEnemies[i].GetComponent<Unit>().selectedActions = Map.Instance.unitDudeEnemies[i].GetComponent<Unit>().availableActions;
            Map.Instance.unitDudeEnemies[i].GetComponent<Unit>().GetComponent<UnitStateMachine>().state = States.WAIT;
        }
    }
}
