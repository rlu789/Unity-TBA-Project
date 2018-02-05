using UnityEngine;
using UnityEngine.Networking;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField]
    Behaviour[] componentsToDisable;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    void Start ()
    {
        if (!isLocalPlayer)
        {
            Debug.Log("Not the local player");
            for (int i = 0; i < componentsToDisable.Length; ++i)
            {
                componentsToDisable[i].enabled = false;
            }
            return;
        }
        GetComponent<PlayerInfo>().playerID = connectionToServer.connectionId;

        FindObjectOfType<LobbyUI>().playerInfo = GetComponent<PlayerInfo>();
    }
}
