using UnityEngine;

public enum States
{
    MOVE,
    END
}
public class UnitStateMachine : MonoBehaviour {
    public States state;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetState(States s)
    {
        state = s; 
    }
}
