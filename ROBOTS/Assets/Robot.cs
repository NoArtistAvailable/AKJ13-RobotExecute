using System;
using System.Collections;
using System.Collections.Generic;
using elZach.Robots;
using UnityEngine;

public class Robot : MonoBehaviour
{
    public enum Faction{A,B,C,D}

    public Faction faction;

    public PathPoint.PathAction state;
    
    public string robotName = "Clive";
    public int lifePoints = 1;
    public float speed = 3f;
    public Path path;
    
    Rigidbody _rb;
    public Rigidbody rb{get
    {
        if (!_rb) _rb = GetComponent<Rigidbody>();
        return _rb;
    }}

    public event Action onDestroy;

    void Start()
    {
        GameManager.Register(this);
    }

    void OnDestroy()
    {
        if (GameManager.ApplicationIsQuitting) return;
        if(onDestroy!=null) onDestroy.Invoke();
    }
    
    void OnDrawGizmos()
    {
        switch (faction)
        {
            case Faction.A:
                Gizmos.color = Color.green;
                break;
            case Faction.B:
                Gizmos.color = Color.red;
                break;
            case Faction.C:
                Gizmos.color = Color.blue;
                break;
            case Faction.D:
                Gizmos.color = Color.magenta;
                break;
        }
        Gizmos.DrawSphere(transform.position+Vector3.up,0.1f);
    }

    
    public class SerializableRobot
    {
        public string name;
        public List<Vector3> pathPoints;

        public SerializableRobot(Robot robot)
        {
            name = robot.robotName;
            pathPoints = new List<Vector3>();
            foreach (var point in robot.path.pathPoints)
            {
                pathPoints.Add(point.position);
            }
        }

        public SerializableRobot(string json)
        {
            var serialized = JsonUtility.FromJson<SerializableRobot>(json);
            name = serialized.name;
            pathPoints = serialized.pathPoints;
        }

        public string ToJSON()
        {
            return JsonUtility.ToJson(this);
        }

        public Robot Spawn(Robot prefab)
        {
            var bot = Instantiate(prefab);
            bot.transform.position = pathPoints[0];
            bot.robotName = name;
            bot.name = name;

            PathPlaner.Instance.LoadPath(bot, pathPoints);
            
            return bot;
        }
    }
    
}
