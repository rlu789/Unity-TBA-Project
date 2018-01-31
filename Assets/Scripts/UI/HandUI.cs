﻿using UnityEngine;
using System.Collections.Generic;

public class HandUI : MonoBehaviour {

    public GameObject cardPrefab;
    [HideInInspector]
    public List<GameObject> cardPrefabs = new List<GameObject>();
    [HideInInspector]
    public Unit currentUnit = null;

    //GameObject[] availableCards = new GameObject[0];

    public void SetValues(Unit unit)
    {
        for (int i = cardPrefabs.Count - 1; i >= 0; --i) Destroy(cardPrefabs[i]);
        cardPrefabs.Clear();

        currentUnit = unit;

        for (int i = 0; i < unit.availableActions.Count; ++i)
        {
            cardPrefabs.Add(Instantiate(cardPrefab, transform));
            cardPrefabs[i].GetComponent<CardUI>().SetCardValues(unit.availableActions[i], unit, i);
        }
    }

    public void RemoveCard(int index)
    {
        Destroy(cardPrefabs[index]);
        cardPrefabs.RemoveAt(index);
    }
}

//NodeManager.Instance.ShowUnitActionRange(unit.currentNode);