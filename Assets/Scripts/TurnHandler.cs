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
    public TurnHandlerStates state;

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
        state = TurnHandlerStates.PLAYERTURN;
        // Set up states for all units
        for (int i = 0; i < Map.Instance.unitDudes.Count; i++)
        {
            Debug.Log(Map.Instance.unitDudes[i]);
            if (Map.Instance.unitDudes[i].GetComponent<Unit>().isEnemy == false)
                Map.Instance.unitDudes[i].GetComponent<UnitStateMachine>().SetState(States.MOVE);
            else if (Map.Instance.unitDudes[i].GetComponent<Unit>().isEnemy == true)
                Map.Instance.unitDudes[i].GetComponent<UnitStateMachine>().SetState(States.END);
        }
    }

    // Update is called once per frame
    void Update () {

	}

    public void MoveButton()
    {
        for (int i = 0; i < Map.Instance.unitDudes.Count; i++)
        {
            if (Map.Instance.unitDudes[i].GetComponent<Unit>().isEnemy == false)
                Map.Instance.unitDudes[i].GetComponent<Unit>().MoveUnit();
        }
    }
}
