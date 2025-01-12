using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public sealed class GameEnviroment
{
    private static GameEnviroment instance;
    private List<GameObject> checkPoints = new List<GameObject>();
    public List<GameObject> CheckPoints{get {return checkPoints;}}
     
    public static GameEnviroment Singleton{
        get{
            if(instance == null){
                instance = new GameEnviroment();
                instance.CheckPoints.AddRange(GameObject.FindGameObjectsWithTag("Checkpoint"));
                instance.checkPoints = instance.checkPoints.OrderBy(waypoint => waypoint.name).ToList();
            }
            return instance;
        }
    }
}
