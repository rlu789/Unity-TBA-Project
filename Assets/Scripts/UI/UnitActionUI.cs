using UnityEngine;
using UnityEngine.UI;

public class UnitActionUI : MonoBehaviour {

    public Text actionNo;
    public Text _name;
    public Text cost;
    public Text range;
    public Text damage;
    public Text aoe;
    public Text cooldown;
    public Text initiative;

    [HideInInspector]
    public Unit currentUnit = null;
    int currentActionIndex = 0;

    public void SetValues(Unit unit, int index = -1)    //pass in index to access that action directly
    {
        currentUnit = unit; //keep track of the current unit so we can move through its action array at anytime

        if (index == -1) index = currentActionIndex;    //if we arent passing in an index for direct access, access the index we are up to

        if (currentActionIndex >= unit.availableActions.Length)  //end of units action list, start from beginning
        {
            currentActionIndex = 0;
            index = 0;
        }

        actionNo.text = "Action " + (index + 1) + ":";
        _name.text = "Name: " + unit.availableActions[index].name;
        cost.text = "Cost: " + unit.availableActions[index].manaCost + " Mana / " + unit.availableActions[index].healthCost + " Health";
        range.text = "Range: " + unit.availableActions[index].range;
        damage.text = "Damage: " + unit.availableActions[index].damage;
        aoe.text = "AOE: " + unit.availableActions[index].aoe;
        cooldown.text = "Cooldown: " + unit.availableActions[index].currentCooldown + " (" + unit.availableActions[index].cooldown + ")";
        initiative.text = "Initiative: " + unit.availableActions[index].initiative;

        NodeManager.Instance.ShowUnitActionRange(unit.currentNode);
    }

    public void NextValue(Unit unit)
    {
        currentActionIndex++;
        SetValues(unit);
    }

    public int GetCurrentActionIndex()
    {
        return currentActionIndex;
    }
}
