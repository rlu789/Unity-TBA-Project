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
            case TurnHandlerStates.ENEMYDRAW:
                currentTurnText.text = "Enemy Drawing Cards";
                btnMove.SetActive(false);
                btnAct.SetActive(false);
                break;
            case TurnHandlerStates.PLAYERDRAW:
                currentTurnText.text = "Player Drawing Cards";
                btnMove.SetActive(false);
                btnAct.SetActive(false);
                break;
            case TurnHandlerStates.PLAYERSELECT:
                currentTurnText.text = "Player Selecting Cards";
                btnMove.SetActive(false);
                btnAct.SetActive(false);
                break;
            case TurnHandlerStates.PLAYERTURN:
                currentTurnText.text = "Player INDEXOFPLAYER Turn";
                btnMove.SetActive(true);
                btnAct.SetActive(true);
                break;
            case TurnHandlerStates.ENEMYTURN:
                currentTurnText.text = "Player INDEXOFENEMY Turn";
                btnMove.SetActive(false);
                btnAct.SetActive(false);
                break;
            case TurnHandlerStates.END:
                currentTurnText.text = "THE TURN HAS ENDED";
                btnMove.SetActive(false);
                btnAct.SetActive(false);
                break;
        }
    }
}
