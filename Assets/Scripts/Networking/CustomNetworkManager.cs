using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager
{
    public void StartupHost()
    {
        SetPort();
        StartHost();
        //Debug.Log(NetworkServer.listenPort);
        //Debug.Log(client.serverPort);
        //Debug.Log(NetworkManager.singleton.networkAddress);
        gameObject.SetActive(false);
    }

    public void JoinGame()
    {
        SetIPAddress();
        SetPort();
        StartClient();

        gameObject.SetActive(false);
    }

    public void SetIPAddress()
    {
        string ipAddress = GameObject.Find("InputFieldIPAddress").transform.Find("Text").GetComponent<Text>().text;
        networkAddress = ipAddress;
    }

    void SetPort()
    {
        networkPort = 6666;
    }
}