using System;
using System.Collections;
using System.Collections.Generic;
using elZach.Robots;
using UnityEngine;
using UnityEngine.Events;

public class Robot : MonoBehaviour
{
    public enum Faction{A,B,C,D}

    public Faction faction;

    public PathPoint.PathAction state;
    public bool living = true;
    public float timeOfDeath;
    
    public string robotName = "Clive";
    public int lifePoints = 1;
    public float speed = 3f;
    public Path path;

    public Animatable deathAnimatable;
    
    Rigidbody _rb;
    public Rigidbody rb{get
    {
        if (!_rb) _rb = GetComponent<Rigidbody>();
        return _rb;
    }}

    private VisionCone _vis;
    public VisionCone Vision{get
    {
        if (!_vis) _vis = GetComponentInChildren<VisionCone>();
        return _vis;
    }}

    public event Action onDestroy;
    public UnityEvent OnGameDeath;

    void Start()
    {
        GameManager.Register(this);
    }

    void Update()
    {
        if (!living) return;
        if (state == PathPoint.PathAction.TakeAim)
        {
            foreach (var otherBot in GameManager.Instance.currentRobots.FindAll(x => x.faction != faction))
            {
                if (!otherBot.living) continue;
                Vector3 myPos = transform.position + Vector3.up * Path.up;
                Vector3 otherPos = otherBot.transform.position + Vector3.up * Path.up;
                Vector3 direction = otherPos - myPos;
                float distance = direction.magnitude;
                if (distance > Vision.range) continue;

                float angle = Vector3.Angle(transform.forward, direction.normalized);
                if (angle > Vision.angle) continue;
                if (Physics.Raycast(myPos, direction, out var hit, distance, PathPlaner.ColliderMask))
                {
                    if (hit.collider.GetComponentInParent<Robot>() != otherBot) continue;
                }
                //Debug.Log("PEWPEW", this);
                otherBot.GameDeath();
            }
        }
    }

    void GameDeath()
    {
        living = false;
        timeOfDeath = GameManager.Instance.playSlider.value * GameManager.Instance.gameTime;
        deathAnimatable.Play(1);
        OnGameDeath.Invoke();
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
                Vector3 serializedPosition = new Vector3(point.position.x, (int) point.action, point.position.z);
                pathPoints.Add(serializedPosition);
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

        public Robot Spawn(Robot prefab, Faction faction)
        {
            var bot = Instantiate(prefab);
            if (pathPoints.Count < 1)
            {
                pathPoints = new List<Vector3>() {Vector3.zero};
            }
            bot.transform.position = pathPoints[0];
            bot.robotName = name;
            bot.name = name;
            bot.faction = faction;

            PathPlaner.Instance.LoadPath(bot, pathPoints);
            
            return bot;
        }
    }

    public void Revive()
    {
        living = true;
        timeOfDeath = 0f;
        deathAnimatable.Play(0);
    }
}
