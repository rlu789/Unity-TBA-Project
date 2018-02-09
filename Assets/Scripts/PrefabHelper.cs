using UnityEngine;

public class PrefabHelper : MonoBehaviour {

    public static PrefabHelper Instance;

    public GameObject[] projectiles;
    public GameObject[] statusVisuals;
    public GameObject[] units;

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
