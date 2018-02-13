using UnityEngine;

public class FloatingTextHandler : MonoBehaviour {

    public GameObject floatingTextPrefab;
    public Canvas overlayCanvas;
    //TODO: why are these two functions just make the second one use the first
    public void SpawnFloatingText(Transform target, string text, bool randomPos = true)
    {
        GameObject ins = Instantiate(floatingTextPrefab);

        float rX = Random.Range(-0.35f, 0.35f);
        float rY = Random.Range(-0.35f, 0.35f);
        if (!randomPos)
        {
            rX = 0;
            rY = 0;
        }
        Vector2 screenPos = Camera.main.WorldToScreenPoint(new Vector3(target.position.x + rX, target.position.y + rY, target.position.z));

        ins.transform.SetParent(overlayCanvas.transform, false);
        ins.transform.position = screenPos;
        ins.GetComponent<FloatingText>().SetValues(text, 3, -10);

        ins.GetComponent<FloatingText>().pos = new Vector3(target.position.x + rX, target.position.y + rY, target.position.z);

    }

    public void SpawnFloatingText(Transform target, int number)
    {
        GameObject ins = Instantiate(floatingTextPrefab);

        float rX = Random.Range(-0.35f, 0.35f);
        float rY = Random.Range(-0.35f, 0.35f);
        Vector2 screenPos = Camera.main.WorldToScreenPoint(new Vector3(target.position.x + rX, target.position.y + rY, target.position.z));

        ins.transform.SetParent(overlayCanvas.transform, false);
        ins.transform.position = screenPos;
        if (number > 0) ins.GetComponent<FloatingText>().SetValues(number + "", 0, number);
        else if (number < 0) ins.GetComponent<FloatingText>().SetValues(Mathf.Abs(number) + "", 1, Mathf.Abs(number));
        else ins.GetComponent<FloatingText>().SetValues(number + "", 2);

        ins.GetComponent<FloatingText>().pos = new Vector3(target.position.x + rX, target.position.y + rY, target.position.z);
    }
}
