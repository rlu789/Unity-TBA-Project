using UnityEngine;

public enum UIType { Statistics, UnitActions, TurnUI }

public class UIHelper : MonoBehaviour {
    public static UIHelper Instance;

    public GameObject statisticsCanvas;
    StatisticsUI statistics;
    public GameObject unitActionsCanvas;
    UnitActionUI unitActions;
    public GameObject turnCanvas;
    TurnUI turnUI;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("UIHelper already exists!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        statistics = statisticsCanvas.GetComponent<StatisticsUI>();
        unitActions = unitActionsCanvas.GetComponent<UnitActionUI>();
        turnUI = turnCanvas.GetComponent<TurnUI>();
    }

    public void ToggleVisible(UIType element, bool active)  //toggles the type of UI specified
    {
        switch (element)
        {
            case UIType.Statistics:
                statisticsCanvas.SetActive(active);
                break;
            case UIType.UnitActions:
                unitActions.currentUnit = null;
                unitActionsCanvas.SetActive(active);
                break;
            case UIType.TurnUI:
                turnCanvas.SetActive(active);
                break;
        }
    }

    public void ToggleAllVisible(bool active)   //well it toggles all the unit stuff
    {
        ToggleVisible(UIType.Statistics, active);
        ToggleVisible(UIType.UnitActions, active);
    }

    //StatisticsUI
    public void SetStatistics(Unit unit)    //show this units stats
    {
        if (unit == null) return;
        ToggleVisible(UIType.Statistics, true);
        statistics.SetValues(unit);
    }

    public void SetStatistics(Node node)    //if there is a unit on this node, show stats otherwise toggle off the stats window
    {
        if (node.currentUnit != null) SetStatistics(node.currentUnit);
        else ToggleVisible(UIType.Statistics, false);
    }

    //UnitActionsUI
    public void SetUnitActions(Unit unit)
    {
        if (unit == null) return;
        ToggleVisible(UIType.UnitActions, true);
        unitActions.SetValues(unit);
    }

    public void SetUnitActions(Node node)
    {
        if (node.currentUnit != null) SetUnitActions(node.currentUnit);
        else ToggleVisible(UIType.UnitActions, false);
    }

    public void NextUnitAction(Unit unit)   //shows the next action in the units array
    {
        if (unit == null) return;
        ToggleVisible(UIType.UnitActions, true);
        unitActions.NextValue(unit);
    }

    public void NextUnitAction(Node node)
    {
        if (node.currentUnit != null) NextUnitAction(node.currentUnit);
        else ToggleVisible(UIType.UnitActions, false);
    }

    public void NextUnitAction()    //(used for next action button on unitActionUI)
    {
        if (unitActions.currentUnit != null) NextUnitAction(unitActions.currentUnit);
    }

    public int GetActionIndex()
    {
        if (unitActions.currentUnit != null) return unitActions.GetCurrentActionIndex();
        return -1;
    }

    public Unit GetCurrentActingUnit()
    {
        if (unitActions.currentUnit != null) return unitActions.currentUnit;
        return null;
    }

    //TurnUI

    public void SetTurnValues(TurnHandlerStates turn)
    {
        turnUI.SetValues(turn);
    }
}
