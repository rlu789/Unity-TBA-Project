using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour {

    public Text _name;
    public Text range;
    public Text damage;
    public Text initiative;

    public Text description; //Use sentences and += instead of = to form the description 

    public Image classImage;

    public GameObject damageCard;
    public GameObject healingCard;
    public GameObject moveCard;

    Unit unit;
    int index;

    public bool selected = false;
    [HideInInspector]
    public int zone = 0;   //0 hand, 1 selectZone

    public Button onClick;
    public DragObjectHandler onDrag;

    public void SetCardValues(UnitAction act, Unit _unit, int _index, bool drag)    //pass in index to access that action directly
    {
        if (act.type == ActionType.MOVE)
        {
            damage.text = act.range + ""; ;
            ChangeCardType(2);
        }
        else if (act.damage > 0)
        {
            damage.text = act.damage + "";
            ChangeCardType(0);
        }
        else if (act.damage <= 0)
        {
            damage.text = -act.damage + "";
            ChangeCardType(1);
        }

        //Fill text fields
        _name.text = act.name;

        initiative.text = act.initiative + "";

        description.text = "";

        if (act.type == ActionType.ACTION) range.text = act.range + "";
        else range.text = "";

        //Fill description
        if (act.manaCost != 0) description.text += act.manaCost + " Mana cost. ";
        else if (act.healthCost != 0) description.text += act.healthCost + " Health cost. ";
        else description.text += "No cost. ";

        if (act.aoe > 0) description.text += act.aoe + " Area of Effect. ";

        if (act.status != null) description.text += "Applies " + act.status.name + ". ";

        ChangeCardClass(act.actionClass);

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

    void ChangeCardType(int cardType)
    {
        switch (cardType)
        {
            case 0:
                damageCard.SetActive(true);
                healingCard.SetActive(false);
                moveCard.SetActive(false);
                break;
            case 1:
                damageCard.SetActive(false);
                healingCard.SetActive(true);
                moveCard.SetActive(false);
                break;
            case 2:
                damageCard.SetActive(false);
                healingCard.SetActive(false);
                moveCard.SetActive(true);
                break;
        }
    }

    void ChangeCardClass(Class _class)
    {
        switch (_class)
        {
            case Class.Dude:
                classImage.sprite = PrefabHelper.Instance.sprites[0];
                classImage.color = new Color32(255, 255, 255, 75);
                break;
            case Class.HealthyBoy:
                classImage.sprite = PrefabHelper.Instance.sprites[1];
                classImage.color = new Color32(200, 200, 200, 75);
                break;
            case Class.VERYSmart:
                classImage.sprite = PrefabHelper.Instance.sprites[2];
                classImage.color = new Color32(255, 115, 30, 75);
                break;
            case Class.Pope:
                classImage.sprite = PrefabHelper.Instance.sprites[3];
                classImage.color = new Color32(255, 255, 150, 75);
                break;
            case Class.Ninja:
                classImage.sprite = PrefabHelper.Instance.sprites[4];
                classImage.color = new Color32(125, 220, 255, 75);
                break;
            case Class.GENERIC:
                classImage.sprite = PrefabHelper.Instance.sprites[5];
                classImage.color = new Color32(255, 200, 50, 75);
                //classImage.gameObject.SetActive(false);
                break;
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
