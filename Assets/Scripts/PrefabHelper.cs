using UnityEngine;

public class PrefabHelper : MonoBehaviour {

    public static PrefabHelper Instance;

    public GameObject[] projectiles;
    [Space(10)]
    public GameObject[] statusVisuals;
    [Space(10)]
    public GameObject[] units;
    [Space(10)]
    public Sprite[] sprites;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("PrefabHelper already exists!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}
