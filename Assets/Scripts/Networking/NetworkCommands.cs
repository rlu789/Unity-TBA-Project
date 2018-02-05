using UnityEngine.Networking;
using UnityEngine;  //Debug.Log()
using UnityEngine.SceneManagement;

public class NetworkCommands : NetworkBehaviour
{
    public PlayerInfo playerInfo;

    private void Start()
    {
        Debug.Log("A network commands was created");
        playerInfo = PlayerInfo.Instance;
    }

    #region Lobby/Team list
    [Command]
    public void CmdRequestTeamList()    //request team list
    {
        foreach (NetworkConnection target in NetworkServer.connections)
        {
            if (target.connectionId == 0)
            {
                TargetRpcSendTeamList(target);
            }
        }
    }

    [TargetRpc]
    void TargetRpcSendTeamList(NetworkConnection target)    //host sends the team list
    {
        if (playerInfo == null) playerInfo = FindObjectOfType<PlayerInfo>();
        CmdSendTeamList(playerInfo.team);
    }

    [Command]
    public void CmdSendTeamList(UnitListing[] teamList) //everyone but the host receives the team list
    {
        foreach (NetworkConnection target in NetworkServer.connections)
        {
            if (target.connectionId != 0)
            {
                TargetRpcUpdateTeamList(target, teamList);
            }
        }
    }

    [TargetRpc]
    void TargetRpcUpdateTeamList(NetworkConnection target, UnitListing[] teamList)
    {
        playerInfo.ChangeTeam(teamList);
    }

    [Command]
    public void CmdLobbyReady(UnitListing[] localTeam)  //send other players your local team
    {
        RpcLobbyReady(localTeam);
    }

    [ClientRpc]
    public void RpcLobbyReady(UnitListing[] localTeam) //set the team units at a preset location for the owner
    {
        if (playerInfo == null) playerInfo = FindObjectOfType<PlayerInfo>();
        int ownerID = localTeam[0].ownerID; //get the owner ID from one of the sent units
        playerInfo.team[ownerID * 2] = localTeam[0];
        playerInfo.team[(ownerID * 2) + 1] = localTeam[1];
    }

    [Command]
    public void CmdLobbyStartGame()
    {
        RpcLobbyStartGame();
    }

    [ClientRpc]
    public void RpcLobbyStartGame()
    {
        Random.InitState(666);
        SceneManager.LoadScene(1);
    }
    #endregion
    //TODO: make the RPCs targetted so i can remove the return from the initial local function and not have to return after the CMD call
    #region Unit move/action functions
    [Command]
    public void CmdSendMove(int startNodeID, int endNodeID)
    {
        RpcSendMove(startNodeID, endNodeID);
    }

    [ClientRpc]
    void RpcSendMove(int startNodeID, int endNodeID)
    {
        Node startNode = null;
        Node endNode = null;
        foreach (Node n in Map.Instance.nodes)
        {
            if (n.nodeID == startNodeID)
            {
                startNode = n;
            }
            if (n.nodeID == endNodeID)
            {
                endNode = n;
            }
            if (startNode != null && endNode != null) break;
        }
        NodeManager.Instance.AssignPath(startNode, endNode);
        Unit theU = startNode.currentUnit;
        theU.MoveUnitClient();

        NodeManager.Instance.TurnEndHandler(endNode.currentUnit);
    }

    [Command]
    public void CmdSendAction(int startNodeID, int targetNodeID, int actionID)
    {
        RpcSendAction(startNodeID, targetNodeID, actionID);
    }

    [ClientRpc]
    void RpcSendAction(int startNodeID, int targetNodeID, int actionID)
    {
        Node startNode = null;
        Node targetNode = null;
        foreach (Node n in Map.Instance.nodes)
        {
            if (n.nodeID == startNodeID)
            {
                startNode = n;
            }
            if (n.nodeID == targetNodeID)
            {
                targetNode = n;
            }
            if (startNode != null && targetNode != null) break;
        }
        Unit theU = startNode.currentUnit;
        theU.SetAction(actionID, targetNode);
        theU.PerformActionClient();

        NodeManager.Instance.TurnEndHandler(theU);
    }
    #endregion
}
