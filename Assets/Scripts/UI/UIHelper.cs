using UnityEngine;

public enum UIType { Statistics, UnitActions, TurnUI }

public class UIHelper : MonoBehaviour {
    public static UIHelper Instance;

    public GameObject statisticsCanvas;
    StatisticsUI statistics;
    public GameObject unitActionsCanvas;
    HandUI unitActions;
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

    public void Setup()
    {
        statistics = statisticsCanvas.GetComponent<StatisticsUI>();
        unitActions = unitActionsCanvas.GetComponent<HandUI>();
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

    public Unit GetCurrentUnit()
    {
        return statistics.currentUnit;
    }

    //UnitActionsUI
    public void SetUnitActions(Unit unit)
    {
        if (unit == null) return;
        ToggleVisible(UIType.UnitActions, true);
        unitActions.SetValues(unit);
    }

    public Transform GetZoneTransform()
    {
        return unitActions.GetZoneTransform();
    }

    public void DebugDraw()
    {
        unitActions.DebugCardDraw();
    }

    //public void SetUnitActions(Node node)
    //{
    //    if (node.currentUnit != null) SetUnitActions(node.currentUnit);
    //    else ToggleVisible(UIType.UnitActions, false);
    //}

    //TurnUI

    public void SetTurnValues(TurnHandlerStates turn)
    {
        if (turnUI == null) return;
        turnUI.SetValues(turn);
    }
}
