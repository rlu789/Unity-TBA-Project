using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HandUI : MonoBehaviour {

    public GameObject cardPrefab;
    [HideInInspector]
    public List<GameObject> cardPrefabs = new List<GameObject>();
    [HideInInspector]
    public Unit currentUnit = null;

    public Transform hand;
    public Transform selectedZone;
    public GameObject selectedZoneGO;

    //GameObject[] availableCards = new GameObject[0];

    public void SetValues(Unit unit)
    {
        //remove previous cards from the hand
        Transform[] children = transform.GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; ++i) if (children[i].tag == "PlaceHolderCard") Destroy(children[i].gameObject);
        for (int i = cardPrefabs.Count - 1; i >= 0; --i) Destroy(cardPrefabs[i]);
        cardPrefabs.Clear();
        currentUnit = unit;

        if (TurnHandler.Instance.currentState == TurnHandlerStates.PLAYERSELECT)
        {
            selectedZoneGO.SetActive(true);
            UnitAction[] actionsClone = (UnitAction[])unit.selectedActions.ToArray().Clone();   //need to clone for shallow copy
            List<UnitAction> selectedList = actionsClone.ToList();

            for (int i = 0; i < unit.availableActions.Count; ++i)
            {
                cardPrefabs.Add(Instantiate(cardPrefab, transform));
                cardPrefabs[i].GetComponent<CardUI>().SetCardValues(unit.availableActions[i], unit, i, true);
                if (selectedList.Contains(unit.availableActions[i]))
                {
                    cardPrefabs[i].GetComponent<CardUI>().ChangeZone(1, false);
                    selectedList.Remove(unit.availableActions[i]); //to deal with adding multiple of the same card even if there is only one
                }

            }
        }
        else if (TurnHandler.Instance.currentState == TurnHandlerStates.PLAYERTURN)
        {
            selectedZoneGO.SetActive(false);
            for (int i = 0; i < unit.selectedActions.Count; ++i)
            {
                cardPrefabs.Add(Instantiate(cardPrefab, transform));
                cardPrefabs[i].GetComponent<CardUI>().SetCardValues(unit.selectedActions[i], unit, i, false);
            }
        }
        else
        {
            //not player turn or player selection
            UIHelper.Instance.ToggleVisible(UIType.UnitActions, false);
            return;
        }
    }

    public void RemoveCard(int index)
    {
        Destroy(cardPrefabs[index]);
        cardPrefabs.RemoveAt(index);
    }

    public Transform GetZoneTransform()
    {
        return selectedZone;
    }
}