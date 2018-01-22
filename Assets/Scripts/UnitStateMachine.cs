using UnityEngine;

public enum States
{
    MOVE,
    END
}
public class UnitStateMachine : MonoBehaviour {
    public States state;

    public void SetState(States s)
    {
        state = s; 
    }
}
