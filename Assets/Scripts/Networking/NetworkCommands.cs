using UnityEngine.Networking;
using UnityEngine;  //Debug.Log()
using UnityEngine.SceneManagement;

public class NetworkCommands : NetworkBehaviour
{
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
        CmdSendTeamList(PlayerInfo.Instance.team);
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
        PlayerInfo.Instance.ChangeTeam(teamList);
    }

    [Command]
    public void CmdLobbyReady(UnitListing[] localTeam)  //send other players your local team
    {
        RpcLobbyReady(localTeam);
    }

    [ClientRpc]
    public void RpcLobbyReady(UnitListing[] localTeam) //set the team units at a preset location for the owner
    {
        int ownerID = localTeam[0].ownerID; //get the owner ID from one of the sent units
        PlayerInfo.Instance.team[ownerID * 2] = localTeam[0];
        PlayerInfo.Instance.team[(ownerID * 2) + 1] = localTeam[1];
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
    public void CmdSendMove(int playerID, int startNodeID, int endNodeID)
    {
        foreach (NetworkConnection target in NetworkServer.connections)
        {
            if (target.connectionId != playerID)    //if the target is not that player who sent the command, send the RPC
            {
                TargetRpcSendMove(target, startNodeID, endNodeID);
            }
        }
    }

    [TargetRpc]
    void TargetRpcSendMove(NetworkConnection target, int startNodeID, int endNodeID)
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
    public void CmdSendAction(int playerID, int startNodeID, int targetNodeID, int actionID)
    {
        foreach (NetworkConnection target in NetworkServer.connections)
        {
            if (target.connectionId != playerID)
            {
                TargetRpcSendAction(target, startNodeID, targetNodeID, actionID);
            }
        }
    }

    [TargetRpc]
    void TargetRpcSendAction(NetworkConnection target, int startNodeID, int targetNodeID, int actionID)
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

    [Command]
    public void CmdSendSelectedCards(int playerID, int[] actionIndexes, int nodeID)
    {
        foreach (NetworkConnection target in NetworkServer.connections)
        {
            if (target.connectionId != playerID)
            {
                TargetRpcSendSelectedCards(target, actionIndexes, nodeID);
            }
        }
    }

    [TargetRpc]
    void TargetRpcSendSelectedCards(NetworkConnection target, int[] actionIndexes, int nodeID)
    {
        Node node = null;
        foreach (Node n in Map.Instance.nodes)
        {
            if (n.nodeID == nodeID) node = n;
        }
        node.currentUnit.SelectCards(actionIndexes);
    }

    #endregion
}
