using UnityEngine;

public enum UIType { Statistics }

public class UIHelper : MonoBehaviour {
    public static UIHelper Instance;

    public GameObject statisticsCanvas;
    StatisticsUI statistics;

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
    }

    public void ToggleVisible(UIType element, bool active)  //toggles the type of UI specified
    {
        switch (element)
        {
            case UIType.Statistics:
                statisticsCanvas.SetActive(active);
                break;
        }
    }

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
}
