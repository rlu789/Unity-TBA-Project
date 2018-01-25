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

    [HideInInspector]
    public Unit currentUnit = null;
    int currentActionIndex = 0;

    public void SetValues(Unit unit, int index = -1)    //pass in index to access that action directly
    {
        currentUnit = unit; //keep track of the current unit so we can move through its action array at anytime

        if (index == -1) index = currentActionIndex;    //if we arent passing in an index for direct access, access the index we are up to

        if (currentActionIndex >= unit.actions.Length)  //end of units action list, start from beginning
        {
            currentActionIndex = 0;
            index = 0;
        }

        actionNo.text = "Action " + (index + 1) + ":";
        _name.text = "Name: " + unit.actions[index].name;
        cost.text = "Cost: " + unit.actions[index].manaCost + " Mana / " + unit.actions[index].healthCost + " Health";
        range.text = "Range: " + unit.actions[index].range;
        damage.text = "Damage: " + unit.actions[index].damage;
        aoe.text = "AOE: " + unit.actions[index].aoe;
        cooldown.text = "Cooldown: " + unit.actions[index].currentCooldown + " (" + unit.actions[index].cooldown + ")";

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
