using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour {

    public Text _name;
    public Text cost;
    public Text range;
    public Text damage;
    public Text aoe;
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
        //Fill text fields
        _name.text = act.name;

        if (act.manaCost != 0) cost.text = "<color=cyan>" + act.manaCost + " Mana Cost</color>";
        else if (act.healthCost != 0) cost.text = "<color=red>" + act.healthCost + " Health Cost</color>";
        else cost.text = "No cost";

        range.text = act.range + " Range";

        if (act.damage > 0) damage.text = "<color=red>" + act.damage + " Damage</color>";
        else if (act.damage < 0) damage.text = "<color=green>" + -act.damage + " Healing</color>";
        else damage.text = "No Damage";

        aoe.text = act.aoe + " AOE";
        initiative.text = "<color=yellow>" + act.initiative + " Initiative</color>";
        //Keep track of unit this card belongs to (at which index)
        unit = _unit;
        index = _index;
        //For card use vs selection
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
            if (!selected) unit.cards.SelectCard(index);
            else unit.cards.DeselectCard(index);
            selected = !selected;
        }
        else unit.PrepareAction(index);
    }

    public void ChangeZone(int _zone, bool activate)    //switch into a different zone (0 - hand, 1 - selected). activate = player dragging into zone (true), player returning to this units actions (false)
    {
        zone = _zone;
        gameObject.transform.SetParent(UIHelper.Instance.GetZoneTransform());
        if (activate) UseCard();
        else selected = true;
    }
}
