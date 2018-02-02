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

    public bool selected = false;
    [HideInInspector]
    public int zone = 0;   //0 hand, 1 selectZone

    public Button onClick;
    public DragObjectHandler onDrag;

    public void SetCardValues(UnitAction act, Unit _unit, int _index, bool drag)    //pass in index to access that action directly
    {
        _name.text = act.name;
        if (act.manaCost != 0) cost.text = act.manaCost + " Mana Cost";
        else if (act.healthCost != 0) cost.text = act.healthCost + " Health Cost";
        range.text = act.range + " Range";
        damage.text = act.damage + " Damage";
        aoe.text = act.aoe + " AOE";
        //cooldown.text = "Cooldown: " + act.currentCooldown + " (" + act.cooldown + ")";
        initiative.text = act.initiative + " Initiative";

        unit = _unit;
        index = _index;

        if (drag)
        {
            onClick.enabled = false;
            onDrag.enabled = true;
        }
        else
        {
            onClick.enabled = true;
            onDrag.enabled = false;
        }
    }
    
    public void UseCard()
    {
        if (unit.unitStateMachine.state == States.SELECT)
        {
            if (!selected) unit.SelectCard(index);
            else unit.DeselectCard(index);
            selected = !selected;
        }
        else unit.PrepareAction(index);
    }

    public void ChangeZone(int _zone, bool activate)    //switch into a different zone (0 - hand, 1 - selected). activate = use card or just set as selected
    {
        zone = _zone;
        gameObject.transform.SetParent(UIHelper.Instance.GetZoneTransform());
        if (activate) UseCard();
        else selected = true;
    }
}
