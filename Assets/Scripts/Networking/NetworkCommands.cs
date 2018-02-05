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

    #region Unit move/action functions
    [Command]
    public void CmdSendMove(int startNodeID, int endNodeID, bool endTurn)
    {
        RpcSendMove(startNodeID, endNodeID, endTurn);
    }

    [ClientRpc]
    void RpcSendMove(int startNodeID, int endNodeID, bool endTurn)
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
        theU.MoveUnit();

        if (endTurn) NodeManager.Instance.TurnEndHandler(endNode.currentUnit);
    }

    [Command]
    public void CmdSendAction(int startNodeID, int targetNodeID, int actionID, bool endTurn)
    {
        RpcSendAction(startNodeID, targetNodeID, actionID, endTurn);
    }

    [ClientRpc]
    void RpcSendAction(int startNodeID, int targetNodeID, int actionID, bool endTurn)
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
        startNode.currentUnit.SetAction(actionID, targetNode);
        startNode.currentUnit.PerformAction();

        if (endTurn) NodeManager.Instance.TurnEndHandler(targetNode.currentUnit);
    }
    #endregion
}
