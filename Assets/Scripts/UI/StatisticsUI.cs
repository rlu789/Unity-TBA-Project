using UnityEngine;
using UnityEngine.UI;

public class StatisticsUI : MonoBehaviour {

    public Text _name;
    public Text _class;
    public Text health;
    public Text mana;
    public Text movement;
    public Text armor;

    public void SetValues(Unit unit)
    {
        _name.text = "Name: " + unit.displayName;
        _class.text = "Class: " + unit._class;
        health.text = "Health: " + unit.currentHealth + "/" + unit.maxHealth;
        mana.text = "Mana: " + unit.currentMana + "/" + unit.maxMana;
        movement.text = "Movement: " + unit.currentMovement + "/" + unit.moveSpeed; ;
        armor.text = "Armor: " + unit.armor;
    }
}
