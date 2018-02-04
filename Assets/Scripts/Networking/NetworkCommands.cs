using UnityEngine.Networking;

public class NetworkCommands : NetworkBehaviour
{
    PlayerInfo playerInfo;

    private void Start()
    {
        playerInfo = GetComponent<PlayerInfo>();
    }

    #region Getting team list from host
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
        CmdSendTeamList(playerInfo.team);
    }

    [Command]
    public void CmdSendTeamList(TeamListing[] teamList) //everyone but the host receives the team list
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
    void TargetRpcUpdateTeamList(NetworkConnection target, TeamListing[] teamList)
    {
        playerInfo.ChangeTeam(teamList);
    }

    //TODO: send local team when player is ready, display each players local team on each client.
    //when everyone is ready, switch scene to battlemap with the complete team list
    #endregion
}
