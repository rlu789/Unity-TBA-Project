using UnityEngine;
using System.Collections.Generic;
using System;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;
    public List<UnitAction> allCards = new List<UnitAction>();
    UnitAction tempAction = new UnitAction();
    public TextAsset cardsFile;
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
            //Debug.Log(parts[i]);
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
                case "[End]":
                    if (tempAction != null)
                    {
                        allCards.Add(tempAction);
                    }
                    break;
            }
        }
    }

    //UnitAction HandleLine(string line)
    //{
    //    UnitAction action = new UnitAction();
    //    bool takingName = true, takingPrefab = false;
    //    string actionName = "", prefabName = "Assets/Prefabs/Projectiles/";

    //    for (int i = 0; i < line.Length; i++)
    //    {
    //        if (line[i].Equals('-'))
    //        {
    //            takingName = false; takingPrefab = true;
    //            i++;
    //        }
    //        if (line[i].Equals(':')) takingPrefab = false;

    //        if (takingName)
    //        {
    //            actionName += line[i];
    //        }
    //        else if (takingPrefab)
    //        {
    //            prefabName += line[i];
    //        }
    //        else
    //        {
    //            action.name = actionName;
    //            //Debug.Log(prefabName);
    //            action.projectile = (GameObject)AssetDatabase.LoadAssetAtPath(prefabName, typeof(GameObject));
    //            action.manaCost = (int)Char.GetNumericValue(line[i + 2]);
    //            action.healthCost = (int)Char.GetNumericValue(line[i + 4]);
    //            action.range = (int)Char.GetNumericValue(line[i + 6]);
    //            action.damage = (int)Char.GetNumericValue(line[i + 8]);
    //            action.aoe = (int)Char.GetNumericValue(line[i + 10]);
    //            action.cooldown = (int)Char.GetNumericValue(line[i + 12]);
    //            action.initiative = (int)Char.GetNumericValue(line[i + 14]);
    //            action.type = (ActionType)(int)Char.GetNumericValue(line[i + 16]);
    //            action.actionClass = (Class)(int)Char.GetNumericValue(line[i + 18]);
    //            //Debug.Log(action.name);
    //            if (action.cooldown == 9)   //richard this code is mega dodgy get on it already the team is suffering
    //            {                           //anyway i hacked together this because it doesnt read in -negative numbers the sign breaks it REAl classic stuff :clap: :smirk:
    //                action.cooldown = 0;
    //                action.damage = -action.damage;
    //            }
    //            return action;
    //        }
    //    }
    //    return null;
    //}
}
