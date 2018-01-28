using UnityEngine;
using System.Collections.Generic;
using System.Collections.Specialized;

public enum TurnHandlerStates
{
    PLAYERMOVE,
    ENEMYMOVE,
    PLAYERACT,
    ENEMYACT,
    BATTLEACT
}

public class TurnHandler : MonoBehaviour
{
    public static TurnHandler Instance;
    public int unitTurnCount;
    public TurnHandlerStates currentState;
    public List<Unit> actionQueue = new List<Unit>();
    public SortedDictionary<float, Unit> orderedActions = new SortedDictionary<float, Unit>();

    public bool actionReady = false, singleActionReady = false;
    float haveYetToCrossTheBridge = 0.01f;
    int delay = 1;

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
            if (actionQueue.Count == 0)
            {
                actionReady = false;
                singleActionReady = false;
                Invoke("NextState", 2f);
                return;
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
                SwitchState(TurnHandlerStates.ENEMYMOVE);
                break;
            case TurnHandlerStates.ENEMYMOVE:
                SwitchState(TurnHandlerStates.PLAYERACT);
                break;
            case TurnHandlerStates.PLAYERACT:
                SwitchState(TurnHandlerStates.ENEMYACT);
                break;
            case TurnHandlerStates.ENEMYACT:
                SwitchState(TurnHandlerStates.BATTLEACT);
                break;
            case TurnHandlerStates.BATTLEACT:
                SwitchState(TurnHandlerStates.PLAYERMOVE);
                break;
        }
    }

    void SwitchState(TurnHandlerStates state)
    {
        //UIHelper.Instance.SetTurnValues(state);   //seems to break up here for some reason :omegaThinking:
        switch (state)
        {
            case TurnHandlerStates.PLAYERMOVE:
                SetAllStates(States.MOVE, States.END);
                currentState = TurnHandlerStates.PLAYERMOVE;
                break;
            case TurnHandlerStates.ENEMYMOVE:
                SetAllStates(States.END, States.MOVE);
                currentState = TurnHandlerStates.ENEMYMOVE;
                HandleEnemyTurn();
                break;
            case TurnHandlerStates.PLAYERACT:
                SetAllStates(States.ACT, States.END);
                currentState = TurnHandlerStates.PLAYERACT;
                break;
            case TurnHandlerStates.ENEMYACT:
                SetAllStates(States.END, States.ACT);
                currentState = TurnHandlerStates.ENEMYACT;
                HandleEnemyAct();
                break;
            case TurnHandlerStates.BATTLEACT:
                SetAllStates(States.END, States.END);
                currentState = TurnHandlerStates.BATTLEACT;
                BattleAct();
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
            AIHelper.Instance.AIGetTurn(Map.Instance.unitDudeEnemies[i].GetComponent<Unit>());
            Map.Instance.unitDudeEnemies[i].GetComponent<Unit>().MoveUnit();
        }
        NextState();
    }

    void HandleEnemyAct()
    {
        for (int i = 0; i < Map.Instance.unitDudeEnemies.Count; i++)
        {
            //Map.Instance.unitDudeEnemies[i].GetComponent<Unit>().PerformActionDelayed(i + 1)
            actionQueue.Add(Map.Instance.unitDudeEnemies[i].GetComponent<Unit>());
            // input enemy action here
        }
        NextState();
    }

    void BattleAct()
    {
        foreach(Unit u in actionQueue)
        {
            if (u.readyAction == null) continue;
            if (orderedActions.ContainsKey(u.readyAction.initiative))
            {
                orderedActions.Add(u.readyAction.initiative + haveYetToCrossTheBridge, u);
                haveYetToCrossTheBridge += 0.01f;
            }
            else
                orderedActions.Add(u.readyAction.initiative, u);
        }
        //TODO CROSS THIS BRIDGE WHEN WE GET THERE
        foreach (KeyValuePair<float, Unit> unit in orderedActions)
        {
            Debug.Log("Key: " + unit.Key + ", Value: {1} " + unit.Value + " using action " + unit.Value.readyAction.name);
            unit.Value.PerformActionDelayed(delay++);
        }

        haveYetToCrossTheBridge = 0.01f; delay = 1;
        actionQueue.Clear();
        orderedActions.Clear();
        NextState();
    }
}
