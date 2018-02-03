using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

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
    [HideInInspector]
    public int waitingForAction = 0;

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
        Invoke("DelaySwitch", 0.5f);
    }

    void DelaySwitch()  //VERY bad code
    {
        SwitchState(TurnHandlerStates.ENEMYDRAW);
    }

    public void NextState()
    {
        if (waitingForAction != 0)   //if we are waiting for projectiles/animations to finish, try again in 0.5 seconds
        {
            Invoke("NextState", 0.5f);
            return;
        }
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
        NodeManager.Instance.Deselect();
        if (orderedActions.Count == 0)
        {
            Debug.Log("Round Ended.");
            return TurnHandlerStates.END;
        }
        if (orderedActions[orderedActions.Keys.First()] == null)    //enemy was killed
        {
            orderedActions.Remove(orderedActions.Keys.First());
            return DetermineTurn();
        }
        if (!orderedActions[orderedActions.Keys.First()].isEnemy)
        {
            return TurnHandlerStates.PLAYERTURN;
        }
        else
        {
            return TurnHandlerStates.ENEMYTURN;
        }
    }

    void SwitchState(TurnHandlerStates state)
    {
        UIHelper.Instance.SetTurnValues(state);
        switch (state)
        {
            case TurnHandlerStates.ENEMYDRAW:
                //TODO: function to have enemy draw their cards
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
                break;
            case TurnHandlerStates.PLAYERTURN:
                currentState = TurnHandlerStates.PLAYERTURN;
                orderedActions[orderedActions.Keys.First()].GetComponent<UnitStateMachine>().state = States.B_SELECTING;
                NodeManager.Instance.SetSelectedNode(orderedActions[orderedActions.Keys.First()].GetComponent<Unit>().currentNode);
                break;
            case TurnHandlerStates.ENEMYTURN:
                currentState = TurnHandlerStates.ENEMYTURN;
                orderedActions[orderedActions.Keys.First()].GetComponent<UnitStateMachine>().state = States.B_SELECTING;
                HandleEnemyTurn();
                break;
            case TurnHandlerStates.END:
                SetAllStates(States.END, States.END);
                currentState = TurnHandlerStates.END;
                NodeManager.Instance.Deselect();
                NextState();
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
            NodeManager.Instance.Deselect();
    }

    void DrawCards()
    {
        for (int i = 0; i < Map.Instance.unitDudeFriends.Count; i++)
        {
            if (Map.Instance.unitDudeFriends[i].GetComponent<Unit>().isEnemy == false)
            {
                Map.Instance.unitDudeFriends[i].GetComponent<Unit>().DrawCards(3);
            }
        }
    }

    void HandleEnemyTurn()
    {
        Unit enemy = orderedActions[orderedActions.Keys.First()];

        NodeManager.Instance.SetSelectedNode(enemy.currentNode);
        AIHelper.Instance.AIGetTurn(enemy);
        enemy.MoveUnit();
        AIHelper.Instance.ConfirmBestAction(enemy);
        enemy.PerformActionDelayed(2f);
        enemy.unitStateMachine.state = States.END;
        orderedActions.Remove(orderedActions.Keys.First());
        Invoke("NextState", 3f);
    }

    void HandleEnemyAct()
    {
        for (int i = 0; i < Map.Instance.unitDudeEnemies.Count; i++)
        {
            Unit enemy = Map.Instance.unitDudeEnemies[i].GetComponent<Unit>();
            //AIHelper.Instance.ConfirmBestAction(enemy);
            //checking if action = null
            if (enemy.readyAction != null && !enemy.readyAction.isEmpty()) actionQueue.Add(enemy);
        }
    }

    void HandleStatus()
    {
        foreach (GameObject unit in Map.Instance.unitDudeFriends)
        {
            StatusHelper.Instance.CheckStatus(unit.GetComponent<Unit>());
        }
    }

   public void DetermineTurnOrder()
    {
        haveYetToCrossTheBridge = 0.01f;
        //THIS IS COMPLETELY FUNCTIONAL REGARDLESS OF WHETHER THE COUNT IS ODD OR EVEN
        foreach (GameObject u in Map.Instance.unitDudeFriends)
        {
            List<int> hahagetonemike = new List<int>();
            foreach (UnitAction action in u.GetComponent<Unit>().selectedActions)
            {
                hahagetonemike.Add(action.initiative);
            }
            hahagetonemike.Sort();
            //BANDAID
            float theF = hahagetonemike[(int)Mathf.Floor(hahagetonemike.Count / 2)];
            if (!orderedActions.ContainsKey(theF))
                orderedActions.Add(theF, u.GetComponent<Unit>());
            else
                orderedActions.Add(theF + haveYetToCrossTheBridge, u.GetComponent<Unit>());
            haveYetToCrossTheBridge += 0.01f;
        }
        foreach (GameObject u in Map.Instance.unitDudeEnemies)
        {
            List<int> initiativeOrder = new List<int>();
            foreach (UnitAction action in u.GetComponent<Unit>().selectedActions)
            {
                initiativeOrder.Add(action.initiative);
            }
            initiativeOrder.Sort();
            //BANDAID
            float median = initiativeOrder[(int)Mathf.Floor(initiativeOrder.Count / 2)];
            if (!orderedActions.ContainsKey(median))
                orderedActions.Add(median, u.GetComponent<Unit>());
            else
                orderedActions.Add(median + haveYetToCrossTheBridge, u.GetComponent<Unit>());
            haveYetToCrossTheBridge += 0.01f;
        }
        NodeManager.Instance.SetSelectedNode(orderedActions[orderedActions.Keys.First()].currentNode);

        NextState();
    }

    public string WhosTurnIsItAnyway()
    {
        return orderedActions[orderedActions.Keys.First()].stats.displayName;
    }

    #region ah the old yeap function.. good times
    /*
    public void yeap()
    {
        //TODO WTF IS ALL OF THIS
        if (NodeManager.Instance.selectedNode.currentUnit.readyAction.type == ActionType.ACTION)
            NodeManager.Instance.selectedNode.currentUnit.unitStateMachine.state = States.B_SELECTINGACTION;
        else
        {
            //YEAP
            NodeManager.Instance.selectedNode.currentUnit.unitStateMachine.state = States.B_SELECTINGMOVE;
            NodeManager.Instance.selectedNode.currentUnit.stats.moveSpeed = NodeManager.Instance.selectedNode.currentUnit.readyAction.range;
        }
        //BUT ESPICALLY THIS
        Node CLONEDNODEgetHAckedNode = NodeManager.Instance.selectedNode;
        NodeManager.Instance.SetSelectedNode(CLONEDNODEgetHAckedNode);
    }
    */
    #endregion
}
