using UnityEngine;

public enum States
{
    MOVE,
    ACT,
    END
}
public class UnitStateMachine : MonoBehaviour {
    public States state;

    public void SetState(States s)
    {
        state = s; 
    }
}
