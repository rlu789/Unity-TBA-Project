using UnityEngine;

public enum States
{
    START,
    DRAW,
    SELECT,
    WAIT,
    B_SELECTING,
    B_SELECTINGMOVE,
    B_SELECTINGACTION,
    END
}
public class UnitStateMachine : MonoBehaviour {
    public States state;

    public void SetState(States s)
    {
        state = s; 
    }
}
