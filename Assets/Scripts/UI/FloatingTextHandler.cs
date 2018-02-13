using UnityEngine;

public class FloatingTextHandler : MonoBehaviour {

    public GameObject floatingTextPrefab;
    public Canvas overlayCanvas;

    public void SpawnFloatingText(Transform target, string text)
    {
        GameObject ins = Instantiate(floatingTextPrefab);

        float rX = Random.Range(-0.35f, 0.35f);
        float rY = Random.Range(-0.35f, 0.35f);
        Vector2 screenPos = Camera.main.WorldToScreenPoint(new Vector3(target.position.x + rX, target.position.y + rY, target.position.z));

        ins.transform.SetParent(overlayCanvas.transform, false);
        ins.transform.position = screenPos;
        ins.GetComponent<FloatingText>().SetValues(text);
    }
}
