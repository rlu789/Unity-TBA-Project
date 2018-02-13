using UnityEngine;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour {

    public int lifeTime;
    public Text text;
    public Outline outline;

    public Vector3 pos;

    private void Update()
    {
        transform.position = Camera.main.WorldToScreenPoint(pos);   //Counters camera movement so the text (screen space) stays in the same place. Not performant but its okay for this game.
    }

    public void SetValues(string _text, int damageType = 3, int fontSizeMod = 0) //TODO: replace damageType with an enum
    {
        if (damageType == 0)    //== dealt damage
        {
            text.color = Color.red;
            outline.effectColor = Color.black;
        }
        else if (damageType == 1)   //== dealt healing
        {
            text.color = Color.green;
            outline.effectColor = Color.white;
        }
        else
        {
            text.color = Color.white;   //== 0 damage or text
            outline.effectColor = Color.black;
        }

        text.fontSize += Random.Range(-2, 3);   //randomize fontsize slightly
        text.fontSize += fontSizeMod;           //increase fontsize (by how much damage it does)
        text.fontSize = Mathf.Clamp(text.fontSize, 1, 100);

        text.text = _text;

        Destroy(gameObject, lifeTime);
    }
}
