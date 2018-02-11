using UnityEngine;
using System.Collections.Generic;
using System.Linq;
//ALERT
//OMEGA TODO:
//I made a bunch of regions with very different descriptions that a lot of the functions in this class fit into. Classes should have a single use so all the functions in seperate regions should be moved to seperate classes.
//It doesn't have to be exactly how I mapped it out into regions, I just did that quickly to show my point.
//OMEGA TODO:
//ALERT
public class Unit : MonoBehaviour {
    [Header("Stats")]
    public UnitStats stats;
    public UnitCards cards = new UnitCards();
    public bool isEnemy;

    //Setup fields
    public GameObject graphics;
    public Transform firePoint;
    public Transform statusPoint;
    [Header("For unity and debug, don't change")]
    public Vector2Int XY = new Vector2Int(0, 0);
    public int currentNodeID = -1;
    public Node currentNode;
    //Movement fields
    public List<Node> currentPath = new List<Node>();
    List<Node> movePath = new List<Node>();
    List<GameObject> pathVisual = new List<GameObject>();
    int currMoveIndex = 0;
    //Action fields
    public UnitAction readyAction;
    public int readyActionIndex;
    public Node targetActionNode;
    public UnitStateMachine unitStateMachine;

    public List<Status> statuses = new List<Status>();
    [HideInInspector]
    public bool dead = false;
    //Network fields
    public int ownerID;

    private void Start()
    {
        stats.currentHealth = stats.maxHealth;
        stats.moveSpeed = stats.baseMoveSpeed;
        stats.currentMovement = stats.moveSpeed;
        stats.currentMana = stats.maxMana;
        if (stats.displayName == "") stats.displayName = GenerateRandomNameOfPower();
        //:rage:
        unitStateMachine = GetComponent<UnitStateMachine>();

        cards.InitDeck(this);
        if (isEnemy) cards.CopySelectedActionsToDeck();
        else cards.DrawCards(2);
    }

    void Update()
    {
        if (movePath.Count != 0)
        {
            MoveStep();
        }
    }

    #region Action handling
    public void PerformAction() //TODO: clean up this code dont need two functions doing the same thing
    {
        if (readyAction == null)
        {
            targetActionNode = null;
            return;
        }
        if (targetActionNode == null)
        {
            targetActionNode = null;
            readyAction = null;
            return;
        }

        if (PlayerInfo.Instance != null && PlayerInfo.Instance.playerID == ownerID)    //why didnt i just do if the unit owner is you instead of two functions?
        {
            PlayerInfo.Instance.commands.CmdSendAction(PlayerInfo.Instance.playerID, currentNode.nodeID, targetActionNode.nodeID, readyActionIndex);
        }

        readyAction.UseAction(targetActionNode, this);
        if (!isEnemy) cards.discardedActions.Add(readyAction);

        cards.SendActionToTheShadowRealm_BYMIKE_ActuallyNotByMikeRichardWroteThisSoYeap();
    }

    public void PerformActionClient()   //doesn't try to send command to other players (can't use optional arguements in commands and rpcs)
    {
        readyAction.UseAction(targetActionNode, this);
        if (!isEnemy) cards.discardedActions.Add(readyAction);

        cards.SendActionToTheShadowRealm_BYMIKE_ActuallyNotByMikeRichardWroteThisSoYeap(true);
    }

    public void PerformActionDelayed(float delay)
    {
        if (readyAction == null || targetActionNode == null)
        {
            if (PlayerInfo.Instance != null && PlayerInfo.Instance.playerID == ownerID)    //why didnt i just do if the unit owner is you instead of two functions?
            {
                PlayerInfo.Instance.commands.CmdEndEnemyTurn(PlayerInfo.Instance.playerID);
            }
            readyAction = null;
            targetActionNode = null;
        }
        else Invoke("PerformAction", delay);
    }
    #endregion
    #region Movement handling
    //TODO: bug when trying to move to a new node while already moving to a node (i believe only when trying to move a shorter distance than the current movement distance?)
    //Make any action or movement wait until the last is finished before allowing a new one
    public void MoveUnit()    //moves unit on selected tile
    {
        List<Node> pathToFollow = currentPath;   //get the path to follow, based on the max distance the unit can move this turn
        movePath = currentPath;

        foreach (GameObject haha in pathVisual) Destroy(haha);  //clear visuals
        pathVisual.Clear();

        if (pathToFollow == null || pathToFollow.Count == 0 || (pathToFollow[0] == currentNode && pathToFollow.Count == 1)) //if this path goes nowhere, use up the card and return
        {
            if (PlayerInfo.Instance != null && PlayerInfo.Instance.playerID == ownerID)
            {
                PlayerInfo.Instance.commands.CmdSendMove(PlayerInfo.Instance.playerID, currentNode.nodeID, currentNode.nodeID, readyActionIndex);
            }
            if (!isEnemy)
            {
                cards.ActionUsed(readyAction);
            }
            return;
        }

        Node _destNode = pathToFollow[pathToFollow.Count - 1];  //the destination is the furthest node we can reach

        //if we arent the player that called this movement then send it to the other players

        if (PlayerInfo.Instance != null && PlayerInfo.Instance.playerID == ownerID)
        {
            PlayerInfo.Instance.commands.CmdSendMove(PlayerInfo.Instance.playerID, currentNode.nodeID, _destNode.nodeID, readyActionIndex);
        }

        //set values on initial and destination nodes
        _destNode.currentUnitGO = gameObject;
        _destNode.currentUnit = this;
        Map.Instance.nodes[XY.x, XY.y].currentUnitGO = null;
        Map.Instance.nodes[XY.x, XY.y].currentUnit = null;

        //set units new node values
        XY = _destNode.XY;
        currentNodeID = _destNode.nodeID;
        currentNode = _destNode;

        //BANDAID
        if (!isEnemy)
        {
            cards.ActionUsed(readyAction);
        }
    }

    public void MoveUnitClient()    //doesnt send commands to other players
    {
        List<Node> pathToFollow = currentPath;   //get the path to follow, based on the max distance the unit can move this turn
        movePath = currentPath;
        foreach (GameObject haha in pathVisual)
            Destroy(haha);
        pathVisual.Clear();
        if (pathToFollow == null)
            return;
        if (pathToFollow.Count == 0)
            return;
        if (pathToFollow[0] == currentNode && pathToFollow.Count == 1) return;

        Node _destNode = pathToFollow[pathToFollow.Count - 1];  //the destination is the furthest node we can reach

        //set values on initial and destination nodes
        _destNode.currentUnitGO = gameObject;
        _destNode.currentUnit = this;
        Map.Instance.nodes[XY.x, XY.y].currentUnitGO = null;
        Map.Instance.nodes[XY.x, XY.y].currentUnit = null;

        //set units new node values
        XY = _destNode.XY;
        currentNodeID = _destNode.nodeID;
        currentNode = _destNode;

        //BANDAID
        if (!isEnemy)
        {
            cards.ActionUsed(readyAction);
        }
    }
    #endregion
    #region Movement helper functions
    //animating movement
    void MoveStep()
    {
        Vector3 dir = movePath[currMoveIndex].transform.position - transform.position;      //sets our target direction to the next node along the path
        transform.Translate(dir.normalized * Time.deltaTime * (stats.moveSpeed), Space.World);    //moves towards our target
        if (dir != Vector3.zero) transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), (stats.moveSpeed * 2) * Time.deltaTime); //rotates in the direction we are going

        if (Vector3.Distance(transform.position, movePath[currMoveIndex].transform.position) <= 0.1f)  //if we are close to the node, we can start moving towards the next node
        {
            GetNextStep();
        }
    }

    void GetNextStep()
    {
        currMoveIndex++;
        if (currMoveIndex == movePath.Count)
        {
            transform.position = movePath[currMoveIndex - 1].transform.position;    //make sure we are right on the node when we are finished
            movePath.RemoveRange(0, movePath.Count);                                //clear out the list of nodes to move to
            currMoveIndex = 0;                                                      //reset our move index when finished
            return;
        }
    }
    //getting valid movement paths
    public void SetUnitPath(List<Node> path)
    {
        stats.currentMovement = stats.moveSpeed;    //reset movement when assigning a new path
        List<Node> _currentPath = GetValidPath(path, true);
        foreach (GameObject haha in pathVisual)
            Destroy(haha);
        pathVisual.Clear();

        pathVisual = PathHelper.Instance.DrawActualPath(_currentPath);

        currentPath = _currentPath;
    }

    public void DeleteUnitPath()
    {
        foreach (GameObject haha in pathVisual)
            Destroy(haha);
        pathVisual.Clear();

        currentPath.Clear();
        stats.currentMovement = stats.moveSpeed;
        UIHelper.Instance.SetStatistics(this);
    }

    public bool IsPathValid(List<Node> path)
    {
        List<Node> _currentPath = GetValidPath(path);

        Node nodeToCheck = _currentPath[_currentPath.Count - 1];

        return (nodeToCheck.currentUnit != null) ? false : true;
    }

    public List<Node> GetValidPath(List<Node> path, bool moving = false)    //optional arguement to update units movement when getting valid path
    {
        List<Node> _currentPath = new List<Node>();
        int movementRemaining = stats.moveSpeed;
        int currentNodeInt = 0;

        _currentPath.Add(currentNode);
        while (currentNodeInt < path.Count - 1 && movementRemaining > 0)
        {
            movementRemaining -= path[currentNodeInt + 1].moveCost;    //reduce our remaning movement by the cost

            if (movementRemaining < 0) break;   //if you can't make it to the node, stop adding to the path

            currentNodeInt++;
            _currentPath.Add(path[currentNodeInt]);
        }
        if (moving) stats.currentMovement = movementRemaining;
        return _currentPath;
    }
    #endregion
    #region Action helper functions
    public List<Node> FindRange()
    {
        if (readyAction == null) return null;
        else return readyAction.GetNodesInRange(currentNode);
    }

    public void PrepareAction(int actionIndex, bool calledFromClient = false)   //Don't forget to call this before using a movement!
    {
        readyActionIndex = actionIndex;
        readyAction = cards.selectedActions[actionIndex];
        if (readyAction.type == ActionType.ACTION) unitStateMachine.SetState(States.B_SELECTINGACTION);
        else
        {
            unitStateMachine.SetState(States.B_SELECTINGMOVE);
            stats.moveSpeed = cards.selectedActions[actionIndex].range + stats.modMove;
        }
        if (!calledFromClient) NodeManager.Instance.SetSelectedNode(currentNode);
    }

    public void SetAction(int actionIndex, Node _target)
    {
        readyActionIndex = actionIndex;
        readyAction = cards.selectedActions[actionIndex];
        targetActionNode = _target;
    }

    public void SetAction(UnitAction action, Node _target)
    {
        for (int i = 0; i < cards.selectedActions.Count; ++i)
        {
            if (cards.selectedActions[i] == action)
            {
                SetAction(i, _target);
                return;
            }
        }
    }
    #endregion
    #region Stats and status functions
    public void TakeDamage(int amount)
    {
        stats.currentHealth -= amount + stats.modIncDamage;
        //TODO: display a damage number above the unit or in a log
        if (stats.currentHealth <= 0)
        {
            Debug.Log("Unit killed! " + stats.displayName + " is dead now... :(");
            Die();
        }
        if (stats.currentHealth > stats.maxHealth) stats.currentHealth = stats.maxHealth;
        if (UIHelper.Instance.GetCurrentUnit() == this) UIHelper.Instance.SetStatistics(this);
    }

    public void ApplyStatus(Status status)
    {
        Status s = new Status(status);
        if (s.visual != null) s.visualIns = Instantiate(s.visual, statusPoint.transform);
        StatusHelper.Instance.ApplyInitialEffects(s, this);
        statuses.Add(s);
    }

    void Die()
    {
        dead = true;
        StatusHelper.Instance.RemoveAllStatuses(this);
        NodeManager.Instance.unitsWithAssignedPaths.Remove(this);
        Map.Instance.unitDudeEnemies.Remove(gameObject);
        Map.Instance.unitDudeFriends.Remove(gameObject);
        currentNode.SetHexReady(false);
        currentNode.currentUnit = null;
        currentNode.currentUnitGO = null;
        //its still this units turn, end it
        if (TurnHandler.Instance.orderedActions.Count != 0 && TurnHandler.Instance.orderedActions[TurnHandler.Instance.orderedActions.Keys.First()] == this) NodeManager.Instance.TurnEndHandler(this);
        //TODO: Play death animation
        Destroy(graphics);
        Destroy(gameObject, 2f);
    }

    public void StartTurn()
    {
        unitStateMachine.SetState(States.B_SELECTING);
        currentNode.SetHexReady(false);
        stats.ResetStats();
        StatusHelper.Instance.CheckStatuses(this);
    }

    public void EndTurn()
    {
        StatusHelper.Instance.ResolveStatuses(this);
    }
    #endregion

    string GenerateRandomNameOfPower()
    {
        string name = "";
        string[] letters = { "mic", "ric", "jo", "hae", "har", "n", "el", "ard", "oj", "ri", "on", "rd", "cha", "ich", "j", "rich", "jon", "mich", " The Great, ", " of Power..." };
        int nameLength = Random.Range(2, 6);
        int randIndex = 0;

        for (int i = 0; i < nameLength; ++i)
        {
            randIndex = Random.Range(0, letters.Length);
            if (i > 0) name += letters[randIndex];
            else name += letters[randIndex].ToUpper();
        }
        return name;
    }
}

[System.Serializable]
public class UnitCards
{
    List<UnitAction> deck = new List<UnitAction>();
    public List<UnitAction> availableActions = new List<UnitAction>();
    public List<UnitAction> selectedActions = new List<UnitAction>();
    public List<int> selectedActionIndexes = new List<int>();
    public List<UnitAction> discardedActions = new List<UnitAction>();
    Unit unit;

    public void CopySelectedActionsToDeck()
    {
        selectedActions = deck;
    }

    public void ActionUsed(UnitAction action)
    {
        selectedActions.Remove(action);
        discardedActions.Add(action);
    }


    #region Card/turn related functions
    public void InitDeck(Unit u)
    {
        unit = u;
        for (int i = 0; i < CardManager.Instance.allCards.Count; i++)
        {
            if (CardManager.Instance.allCards[i].actionClass == u.stats._class || (CardManager.Instance.allCards[i].actionClass == Class.GENERIC && u.isEnemy == false))
            {
                deck.Add(CardManager.Instance.allCards[i]);
            }
        }
    }

    public void DrawCards(int amount)
    {
        if (PlayerInfo.Instance != null && PlayerInfo.Instance.playerID != unit.ownerID) return;
        if (deck.Count <= 0)
        {
            Debug.Log("No cards in deck!");
            return;
        }
        for (int i = 0; i < amount; i++)
        {
            if (availableActions.Count >= 5) break;
            availableActions.Add(deck[Random.Range(0, deck.Count)]);
        }
        if (PlayerInfo.Instance != null && PlayerInfo.Instance.playerID == unit.ownerID) //if im the owner of this unit, send my hand to the other players
        {
            List<int> cardIndexesToSend = new List<int>();

            foreach (UnitAction availableAct in availableActions)
            {
                cardIndexesToSend.Add(GetDeckIndex(availableAct));
            }

            PlayerInfo.Instance.commands.CmdSendUnitHand(PlayerInfo.Instance.playerID, cardIndexesToSend.ToArray(), unit.currentNode.nodeID);
        }
    }

    public void SelectCard(int index)
    {
        selectedActions.Add(availableActions[index]);
        selectedActionIndexes.Add(index);
        if (selectedActions.Count == 3)
        {
            UIHelper.Instance.SetUnitActions(unit);
            if (PlayerInfo.Instance != null) PlayerInfo.Instance.commands.CmdSendSelectedCards(PlayerInfo.Instance.playerID, selectedActionIndexes.ToArray(), unit.currentNode.nodeID);
            selectedActionIndexes.Clear();

            SelectDone();

            foreach (UnitAction action in selectedActions)
            {
                availableActions.Remove(action);
            }
        }
    }

    public void SelectCards(int[] actionIndexes)
    {
        foreach (int index in actionIndexes)
        {
            selectedActions.Add(availableActions[index]);
        }
        foreach (UnitAction action in selectedActions)
            availableActions.Remove(action);

        selectedActionIndexes.Clear();
        SelectDone(true);
    }
    //used to match client hand with the owner of the unit (set at the top of each turn)
    public void SetUnitHand(int[] cardIndexes)  //may need to refactor when we implement an actual deck
    {
        availableActions.Clear();   //put back into deck?
        foreach (int index in cardIndexes) if (index != -1) availableActions.Add(deck[index]);
    }

    int GetDeckIndex(UnitAction action) //works because we dont actually remove anything from the deck
    {
        for (int i = 0; i < deck.Count; ++i)
        {
            if (action == deck[i]) return i;
        }
        return -1;
    }

    public void DeselectCard(int index)
    {
        selectedActions.Remove(availableActions[index]);
    }

    void SelectDone(bool calledFromClient = false)
    {
        if (!calledFromClient) NodeManager.Instance.Deselect(); //don't deselect what the local player is doing if another player called this function
        unit.unitStateMachine.state = States.WAIT;
        unit.currentNode.SetHexReady(true);
        foreach (GameObject u in Map.Instance.unitDudeFriends)
        {
            if (u.GetComponent<Unit>().unitStateMachine.state != States.WAIT) return;
        }
        TurnHandler.Instance.DetermineTurnOrder();
    }

    public void IEndMyEndTurnPegasus()
    {
        if (!unit.isEnemy)
        {
            foreach (UnitAction action in selectedActions)
            {
                availableActions.Add(action);
            }
            selectedActions.Clear();
        }
        unit.unitStateMachine.state = States.END;
    }

    public void SendActionToTheShadowRealm_BYMIKE_ActuallyNotByMikeRichardWroteThisSoYeap(bool calledFromClient = false)
    {
        if (!unit.isEnemy) selectedActions.Remove(unit.readyAction);
        unit.targetActionNode = null;
        unit.readyAction = null;
        if (!calledFromClient) UIHelper.Instance.SetUnitActions(unit);
    }
    #endregion
}