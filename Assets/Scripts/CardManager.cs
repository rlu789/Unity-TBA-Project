using UnityEngine;
using System.Collections.Generic;
using System;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;
    public List<UnitAction> allCards = new List<UnitAction>();
    public Dictionary<int, Status> allStatuses = new Dictionary<int, Status>();
    public List<Status> testingStatus = new List<Status>();
    UnitAction tempAction = new UnitAction();
    Status tempStatus;
    public TextAsset cardsFile, statusFile;

    //temp variables for status reader
    int id = -1;
    string statusName = ""; int duration = 0; GameObject visual;
    List<Effect> eff = new List<Effect>();
    //Effect tempEffect = new Effect(StatusType.DOT, 0, false);
    //StatusType type; int strength; bool initialEffect;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("CardManager already exists!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Setup()
    {
        ReadStatusFile(statusFile);
        ReadTextFile(cardsFile);
    }

    void ReadTextFile(TextAsset file)
    {
        string[] textLines = file.text.Split('\n');

        foreach (string line in textLines)
        {
            if (line.Length != 0 && line[0] != '/') HandleLine(line);
        }
    }

    void HandleLine(string line)
    {
        string[] parts = line.Split(' ');
        for (int i = 0; i < parts.Length; i++)
        {
            switch (parts[i].Trim())
            {
                case "[Action]":
                    tempAction = new UnitAction();
                    break;
                case "Name:":
                    for (int x = 1; x < parts[i].Length - 2 ; x++)
                    {
                        try
                        {
                            tempAction.name += parts[x] + " ";
                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            //Debug.Log("wtf why tho");
                        }
                    }
                    break;
                case "Prefab:":
                    try
                    {
                        if (parts.Length > 1) tempAction.projectile = PrefabHelper.Instance.projectiles[Int32.Parse(parts[++i])];
                    }
                    catch (System.FormatException)
                    {
                        //Debug.Log("ur welcome mike");
                    }
                    break;
                case "Mana:":
                    tempAction.manaCost = Int32.Parse(parts[++i]);
                    break;
                case "Health:":
                    tempAction.healthCost = Int32.Parse(parts[++i]);
                    break;
                case "Range:":
                    tempAction.range = Int32.Parse(parts[++i]);
                    break;
                case "Damage:":
                    tempAction.damage = Int32.Parse(parts[++i]);
                    break;
                case "AOE:":
                    tempAction.aoe = Int32.Parse(parts[++i]);
                    break;
                case "Cooldown:":
                    tempAction.cooldown = Int32.Parse(parts[++i]);
                    break;
                case "Initiative:":
                    tempAction.initiative = Int32.Parse(parts[++i]);
                    break;
                case "Type:":
                    tempAction.type = (ActionType) Int32.Parse(parts[++i]);
                    break;
                case "ActionClass:":
                    tempAction.actionClass = (Class) Int32.Parse(parts[++i]);
                    break;
                case "StatusId:":
                    tempAction.status = allStatuses[Int32.Parse(parts[++i])];
                    break;
                case "[End]":
                    if (tempAction != null)
                    {
                        allCards.Add(tempAction);
                    }
                    break;
            }
        }
    }

    void ReadStatusFile(TextAsset file)
    {
        string[] textLines = file.text.Split('\n');

        foreach (string line in textLines)
        {
            if (line.Length != 0 && line[0] != '/') HandleStatusLine(line);
        }
        foreach (KeyValuePair<int, Status> entry in allStatuses)
        {
            testingStatus.Add(entry.Value);
        }
    }

    void HandleStatusLine(string line)
    {
        string[] parts = line.Split(' ');

        switch (parts[0].Trim())
        {
            case "[Status]":
                tempStatus = null;
                break;
            case "Id:":
                id = Int32.Parse(parts[1].Trim());
                break;
            case "Name:":
                for (int x = 1; x < parts[1].Length - 2; x++)
                {
                    try
                    {
                        statusName += parts[x] + " ";
                    }
                    catch (System.IndexOutOfRangeException)
                    {
                        //Debug.Log("wtf why tho");
                    }
                }
                break;
            case "EffectType:": //TODO account for more than one effect
                for (int i = 1; i < parts.Length; i++)
                {
                    eff.Add(new Effect(StatusType.DOT, 0, false));
                    switch (parts[i].Trim())
                    {
                        case "DOT":
                            eff[i-1].type = StatusType.DOT;
                            break;
                        case "MoveSpeed":
                            eff[i - 1].type = StatusType.MoveSpeed;
                            break;
                        case "Actions":
                            eff[i - 1].type = StatusType.Actions;
                            break;
                        case "OutgoingDamage":
                            eff[i - 1].type = StatusType.OutgoingDamage;
                            break;
                        case "IncomingDamage":
                            eff[i - 1].type = StatusType.IncomingDamage;
                            break;
                    }
                }
                break;
            case "EffectStrength:":
                for (int i = 1; i < parts.Length; i++)
                {
                    eff[i - 1].strength = Int32.Parse(parts[i].Trim());
                }
                break;
            case "InitialEffect:":
                for (int i = 1; i < parts.Length; i++)
                {
                    eff[i - 1].initialEffect = (Int32.Parse(parts[i].Trim()) == 0) ? false : true;
                }
                break;
            case "Duration:":
                duration = Int32.Parse(parts[1].Trim());
                break;
            case "Visuals:":
                visual = PrefabHelper.Instance.statusVisuals[Int32.Parse(parts[1])];
                break;
            case "[End]":
                if (id >= 0)
                {
                    tempStatus = new Status(statusName, eff.ToArray(), duration, visual);
                    allStatuses.Add(id, tempStatus);
                    eff.Clear();
                    statusName = "";
                }
                else Debug.Log("empty status or negative id");
                break;
        }
    }
}
