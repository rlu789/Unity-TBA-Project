using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour {

    public Text _name;
    public Text cost;
    public Text range;
    public Text damage;
    public Text aoe;
    //public Text cooldown;
    public Text initiative;

    Unit unit;
    int index;

    public void SetCardValues(UnitAction act, Unit _unit, int _index)    //pass in index to access that action directly
    {
        _name.text = act.name;
        cost.text = act.manaCost + " Mana / " + act.healthCost + " Health";
        range.text = act.range + " Range";
        damage.text = act.damage + " Damage";
        aoe.text = act.aoe + " AOE";
        //cooldown.text = "Cooldown: " + act.currentCooldown + " (" + act.cooldown + ")";
        initiative.text = act.initiative + " Initiative";

        unit = _unit;
        index = _index;
    }

    public void UseCard()
    {
        unit.PrepareAction(index);
    }
}
