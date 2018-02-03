using UnityEngine;
using UnityEngine.UI;

public class TurnUI : MonoBehaviour {

    public Text currentTurnText;

    public void SetValues(TurnHandlerStates turn)
    {
        switch (turn)
        {
            case TurnHandlerStates.ENEMYDRAW:
                currentTurnText.text = "Enemy Drawing Cards";
                break;
            case TurnHandlerStates.PLAYERDRAW:
                currentTurnText.text = "Player Drawing Cards";
                break;
            case TurnHandlerStates.PLAYERSELECT:
                currentTurnText.text = "Player Selecting Cards";
                break;
            case TurnHandlerStates.PLAYERTURN:
                currentTurnText.text = "<color=#00ff007f>" + TurnHandler.Instance.WhosTurnIsItAnyway() + " Is Up</color>";
                break;
            case TurnHandlerStates.ENEMYTURN:
                currentTurnText.text = "<color=#ff00007f>" + TurnHandler.Instance.WhosTurnIsItAnyway() + " Is Up</color>";
                break;
            case TurnHandlerStates.END:
                currentTurnText.text = "THE TURN HAS ENDED";
                break;
        }
    }
}
