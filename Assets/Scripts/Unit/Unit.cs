using UnityEngine;
using System.Collections.Generic;

public class Unit : MonoBehaviour {
    [Header("Stats")]
    public UnitStats stats;
    List<UnitAction> deck = new List<UnitAction>();
    public List<UnitAction> availableActions = new List<UnitAction>();
    public List<UnitAction> selectedActions = new List<UnitAction>();
    public List<UnitAction> discardedActions = new List<UnitAction>();
    public bool isEnemy;

    //Setup fields
    public Transform FirePoint;
    [Header("For unity and debug, don't change")]
    public Vector2Int XY = new Vector2Int(0, 0);
    public int currentNodeID = -1;
    public List<Node> currentPath = new List<Node>();
    public GameObject __testObject;
    //fresh fIelds
    List<Node> movePath = new List<Node>();
    List<GameObject> pathVisual = new List<GameObject>();
    int currMoveIndex = 0;
    public Node currentNode, targetActionNode;

    public UnitAction readyAction;
    public int readyActionIndex;
    public UnitStateMachine unitStateMachine;

    public List<Status> statuses = new List<Status>();

    GameObject[,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,][,,,,,,,,,,,,,,,,,,,,,,,,,,,][,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,] loadBearingArray;

    private void Start()
    {
        stats.currentHealth = stats.maxHealth;
        stats.currentMovement = stats.moveSpeed;
        stats.currentMana = stats.maxMana;
        if (stats.displayName == "") stats.displayName = GenerateRandomNameOfPower();
        //:rage:
        unitStateMachine = GetComponent<UnitStateMachine>();

        deck = CardManager.Instance.decks[(int)stats._class];
        if (isEnemy) selectedActions = deck;
        else DrawCards(2);
    }

    void Update()
    {
        if (movePath.Count != 0)
        {
            MoveStep();
        }
    }

    public void DrawCards(int no)
    {
        if (deck.Count <= 0)
        {
            Debug.Log("No cards in deck!");
            return;
        }
        for (int i = 0; i < no; i++)
        {
            availableActions.Add(deck[Random.Range(0, deck.Count)]);
        }
    }

    void MoveStep()
    {
        Vector3 dir = movePath[currMoveIndex].transform.position - transform.position;      //sets our target direction to the next node along the path
        transform.Translate(dir.normalized * Time.deltaTime * (stats.moveSpeed), Space.World);    //moves towards our target
        if (dir != Vector3.zero) transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), (stats.moveSpeed * 2) * Time.deltaTime); //rotates in the direction we are going

        if ( Vector3.Distance(transform.position, movePath[currMoveIndex].transform.position) <= 0.1f)  //if we are close to the node, we can start moving towards the next node
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

    public void MoveUnit()    //moves unit on selected tile
    {
        //if ((TurnHandler.Instance.currentState == TurnHandlerStates.PLAYERMOVE && !isEnemy) || (TurnHandler.Instance.currentState == TurnHandlerStates.ENEMYMOVE && isEnemy))
        //
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
            _destNode.potentialUnit = null;
            Map.Instance.nodes[XY.x, XY.y].currentUnitGO = null;
            Map.Instance.nodes[XY.x, XY.y].currentUnit = null;

            //set units new node values
            Unit unitComponent = _destNode.currentUnitGO.GetComponent<Unit>();
            unitComponent.XY = _destNode.XY;
            unitComponent.currentNodeID = _destNode.nodeID;
            currentNode = _destNode;

            //BANDAID
            selectedActions.Remove(readyAction);
            if (!isEnemy) discardedActions.Add(readyAction);
        // }
    }

    public void SetUnitPath(List<Node> path)
    {
        stats.currentMovement = stats.moveSpeed;    //reset movement when assigning a new path
        List<Node> _currentPath = GetValidPath(path, true);
        foreach (GameObject haha in pathVisual)
            Destroy(haha);
        pathVisual.Clear();

        pathVisual = PathHelper.Instance.DrawActualPath(_currentPath);

        if (currentPath.Count != 0) currentPath[currentPath.Count - 1].potentialUnit = null;
        currentPath = _currentPath;
        currentPath[currentPath.Count - 1].potentialUnit = this;
    }

    public void DeleteUnitPath()
    {
        foreach (GameObject haha in pathVisual)
            Destroy(haha);
        pathVisual.Clear();

        currentPath[currentPath.Count - 1].potentialUnit = null;
        currentPath.Clear();
        stats.currentMovement = stats.moveSpeed;
        UIHelper.Instance.SetStatistics(this);
    }

    public bool IsPathValid(List<Node> path)
    {
        List<Node> _currentPath = GetValidPath(path);

        Node nodeToCheck = _currentPath[_currentPath.Count - 1];

        return (nodeToCheck.potentialUnit != null || nodeToCheck.currentUnit != null) ? false : true;
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

    string GenerateRandomNameOfPower()
    {
        string name = "";
        string[] letters = { "mic", "ric", "jo", "hae", "har", "n", "el", "ard", "oj", "ri", "on", "rd", "cha", "ich", "j", "rich", "jon", "mich", " The Great, ", " of Power..." };
        int nameLength = Random.Range(2, 6);
        int randIndex = 0;

        for (int i = 0; i < nameLength; ++i)
        {
            randIndex = Random.Range(0, letters.Length - 1);
            if (i > 0) name += letters[randIndex];
            else name += letters[randIndex].ToUpper();
        }
        return name;
    }

    public List<Node> FindRange()
    {
        if (readyAction == null) return null;
        int range = readyAction.range;

        return readyAction.GetNodesInRange(currentNode);
    }

    public void PerformAction()
    {
        if (readyAction == null)
        {
            targetActionNode = null;
            Debug.Log("No action, returning...");
            return;
        }
        if (targetActionNode == null)
        {
            targetActionNode = null;
            readyAction = null;
            Debug.Log("No node selected, returning..");
            return;
        }

        if (targetActionNode.currentUnit == null)
        {
            Debug.Log("Used action on empty node.");
        }
        readyAction.UseAction(targetActionNode, this);
        if (!isEnemy) discardedActions.Add(readyAction);

        //BANGAG
        SendActionToTheShadowRealm_BYMIKE();
    }

    public void PerformActionDelayed(float delay)
    {
        Invoke("PerformAction", delay);
    }

    public void TakeDamage(int amount)
    {
        stats.currentHealth -= amount;
        if (stats.currentHealth <= 0)
        {
            Debug.Log("Unit killed! " + stats.displayName + " is dead now... :(");
            Die();
        }
        if (stats.currentHealth > stats.maxHealth) stats.currentHealth = stats.maxHealth;
    }

    void Die()
    {
        NodeManager.Instance.unitsWithAssignedPaths.Remove(this);
        Map.Instance.unitDudeEnemies.Remove(gameObject);
        Map.Instance.unitDudeFriends.Remove(gameObject);

        Destroy(gameObject);
    }

    public void PrepareAction(int actionIndex)
    {
        readyActionIndex = actionIndex;
        readyAction = selectedActions[actionIndex];
        if (readyAction.type == ActionType.ACTION) unitStateMachine.SetState(States.B_SELECTINGACTION);
        else
        {
            unitStateMachine.SetState(States.B_SELECTINGMOVE);
            stats.moveSpeed = availableActions[actionIndex].range;
        }
        NodeManager.Instance.SetSelectedNode(currentNode);
    }

    public void SetAction(int actionIndex, Node _target)
    {
        readyActionIndex = actionIndex;
        readyAction = selectedActions[actionIndex];
        targetActionNode = _target;
    }

    public void SetAction(UnitAction action, Node _target)
    {
        for (int i = 0; i < selectedActions.Count; ++i)
        {
            if (selectedActions[i] == action)
            {
                SetAction(i, _target);
                return;
            }
        }
    }

    public void ApplyStatus(Status status)
    {
        if (status.visual != null) status.visualIns = Instantiate(status.visual, transform.position, Quaternion.identity);
        statuses.Add(status);
    }
    //public void TogglePathVisual(bool toggle)
    //{
    //    if (pathVisual.Count == 0) return;

    //    foreach (GameObject GO in pathVisual)
    //    {
    //        GO.SetActive(toggle);
    //    }
    //}

    //public void AddAction(UnitAction action) { } //add action to a unit

    public void SelectCard(int index)
    {
        selectedActions.Add(availableActions[index]);
        if (selectedActions.Count == 3)
        {
            //TODO FIEN FOR NOW
            //OR IS IT
            foreach (UnitAction action in selectedActions)
                availableActions.Remove(action);
            UIHelper.Instance.SetUnitActions(this);
            SelectDone();
        }
    }

    public void DeselectCard(int index)
    {
        selectedActions.Remove(availableActions[index]);
    }

    void SelectDone()
    {
        unitStateMachine.state = States.WAIT;
        foreach (GameObject u in Map.Instance.unitDudeFriends)
        {
            if (u.GetComponent<Unit>().unitStateMachine.state != States.WAIT)
                return;
        }
        TurnHandler.Instance.DetermineTurnOrder();
    }

    public void IEndMyEndTurnPegasus()
    {
        foreach (UnitAction action in selectedActions)
        {
            availableActions.Add(action);
        }
        selectedActions.Clear();
        unitStateMachine.state = States.END;
    }

    public void SendActionToTheShadowRealm_BYMIKE()
    {
        selectedActions.Remove(readyAction);
        targetActionNode = null;
        readyAction = null;
        UIHelper.Instance.SetUnitActions(this);
    }
}