using UnityEngine;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour {

    public int lifeTime;
    public Text text;
    //public Outline outline;   //TODO: use these later on
    //public Image image;

    public void SetValues(string _text)
    {
        text.fontSize += Random.Range(-3, 3);
        text.text = _text;

        Destroy(gameObject, lifeTime);
    }
}
