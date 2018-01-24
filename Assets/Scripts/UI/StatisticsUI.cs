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
        _name.text = "Name: " + unit.stats.displayName;
        _class.text = "Class: " + unit.stats._class;
        health.text = "Health: " + unit.stats.currentHealth + "/" + unit.stats.maxHealth;
        mana.text = "Mana: " + unit.stats.currentMana + "/" + unit.stats.maxMana;
        movement.text = "Movement: " + unit.stats.currentMovement + "/" + unit.stats.moveSpeed;
        armor.text = "Armor: " + unit.stats.armor;
    }
}
