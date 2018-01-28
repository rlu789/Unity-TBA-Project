using UnityEngine;
using UnityEngine.UI;

public class TurnUI : MonoBehaviour {

    public GameObject btnMove;
    public GameObject btnAct;

    public Text currentTurnText;

    public void SetValues(TurnHandlerStates turn)
    {
        switch (turn)
        {
            case TurnHandlerStates.PLAYERMOVE:
                currentTurnText.text = "Player Move";
                btnMove.SetActive(true);
                btnAct.SetActive(false);
                break;
            case TurnHandlerStates.ENEMYMOVE:
                currentTurnText.text = "Enemy Move";
                btnMove.SetActive(false);
                btnAct.SetActive(false);
                break;
            case TurnHandlerStates.PLAYERACT:
                currentTurnText.text = "Player Action";
                btnMove.SetActive(false);
                btnAct.SetActive(true);
                break;
            case TurnHandlerStates.ENEMYACT:
                currentTurnText.text = "Enemy Action";
                btnMove.SetActive(false);
                btnAct.SetActive(false);
                break;
            case TurnHandlerStates.BATTLEACT:
                currentTurnText.text = "Battle Phase";
                btnMove.SetActive(false);
                btnAct.SetActive(false);
                break;
        }
    }
}
