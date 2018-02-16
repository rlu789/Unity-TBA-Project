using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
    int sanityCheck;    //we need to know when a unit dies from a status effect at the top of their turn, so we check the count of actions before and after status effects are activated

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
            return TurnHandlerStates.END;
        }
        if (orderedActions[orderedActions.Keys.First()] == null || orderedActions[orderedActions.Keys.First()].dead)    //enemy was killed
        {
            orderedActions.Remove(orderedActions.Keys.First());
            return DetermineTurn();
        }
        if (orderedActions[orderedActions.Keys.First()].team == 0)
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

                sanityCheck = orderedActions.Count;
                orderedActions[orderedActions.Keys.First()].GetComponent<Unit>().StartTurn();
                if (sanityCheck == orderedActions.Count) NodeManager.Instance.SetSelectedNode(orderedActions[orderedActions.Keys.First()].GetComponent<Unit>().currentNode);
                else Debug.Log("Sanity check failed");
                break;
            case TurnHandlerStates.ENEMYTURN:
                currentState = TurnHandlerStates.ENEMYTURN;

                sanityCheck = orderedActions.Count;
                orderedActions[orderedActions.Keys.First()].GetComponent<Unit>().StartTurn();
                if (sanityCheck == orderedActions.Count) HandleEnemyTurn();
                else Debug.Log("Sanity check failed");
                //else NextState();

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
        for (int i = 0; i < Map.Instance.teamZero.Count; i++)
        {
            Map.Instance.teamZero[i].GetComponent<UnitStateMachine>().SetState(playerState);
        }
        for (int i = 0; i < Map.Instance.teamOne.Count; i++)
        {
            Map.Instance.teamOne[i].GetComponent<UnitStateMachine>().SetState(enemyState);
        }
    }

    void DrawCards()
    {
        for (int i = 0; i < Map.Instance.teamZero.Count; i++)
        {
            if (Map.Instance.teamZero[i].GetComponent<Unit>().team == 0)
            {
                Map.Instance.teamZero[i].GetComponent<Unit>().cards.DrawCards(3);
            }
        }
    }

    public void HandleEnemyTurn()
    {
        if (PlayerInfo.Instance != null && PlayerInfo.Instance.playerID != 0) return;   //only host should handle enemy turns

        if (orderedActions[orderedActions.Keys.First()] == null)
        {
            NextState();
            return;
        }

        Unit enemy = orderedActions[orderedActions.Keys.First()];
        if (enemy.team == 0 && enemy.playerControlled == true)  //since i changed the enemy teams to numbers, theres a bug when an enemy dies from status then an ally takes a turn, it will end up here and have the AI control it.
        {                                                       //TODO: figure out why instead of this bandaid code
            NextState();
            return;
        }
        NodeManager.Instance.SetSelectedNode(enemy.currentNode);
        AIHelper.Instance.AIGetTurn(enemy);
        enemy.MoveUnit();
        AIHelper.Instance.ConfirmBestAction(enemy);
        enemy.PerformActionDelayed(2f);
        enemy.unitStateMachine.state = States.END;
        enemy.EndTurn();
        orderedActions.Remove(orderedActions.Keys.First());

        Invoke("NextState", 3f);
    }

    public void EndEnemyTurn(float delay)
    {
        orderedActions[orderedActions.Keys.First()].unitStateMachine.state = States.END;
        orderedActions.Remove(orderedActions.Keys.First());
        Invoke("NextState", delay);
    }

   public void DetermineTurnOrder()
    {
        haveYetToCrossTheBridge = 0.01f;
        //THIS IS COMPLETELY FUNCTIONAL REGARDLESS OF WHETHER THE COUNT IS ODD OR EVEN
        foreach (GameObject u in Map.Instance.teamZero)
        {
            List<int> hahagetonemike = new List<int>();
            foreach (UnitAction action in u.GetComponent<Unit>().cards.selectedActions)
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
        foreach (GameObject u in Map.Instance.teamOne)
        {
            float initiative = u.GetComponent<Unit>().stats.baseInitiative;

            if (!orderedActions.ContainsKey(initiative)) orderedActions.Add(initiative, u.GetComponent<Unit>());
            else
            {
                orderedActions.Add(initiative + haveYetToCrossTheBridge, u.GetComponent<Unit>());
                haveYetToCrossTheBridge += 0.01f;
            }

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
