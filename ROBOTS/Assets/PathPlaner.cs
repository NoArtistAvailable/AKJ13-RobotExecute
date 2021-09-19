using System;
using System.Collections;
using System.Collections.Generic;
using elZach.Robots;
using UnityEngine;

public class PathPlaner : MonoBehaviour
{
    static Lazy<PathPlaner> _instance = new Lazy<PathPlaner>(FindObjectOfType<PathPlaner>);
    public static PathPlaner Instance => _instance.Value;
    public Path pathPrefab;
    public LayerMask colliderMask = -1;
    public static LayerMask ColliderMask => Instance.colliderMask;

    LineRenderer _line;
    public LineRenderer line{get
    {
        if (!_line) _line = GetComponent<LineRenderer>();
        return _line;
    }}

    private bool isPlanning;
    private Vector3 lastPlacedPoint;
    private Plane groundPlane = new Plane(Vector3.up, 0f);

    private Path currentPath;
    public Robot.Faction currentFaction;
    
    public void StartPath(Robot target)
    {
        isPlanning = true;
        line.positionCount = 3;
        currentPath = pathPrefab.gameObject.Spawn().GetComponent<Path>();
        currentPath.pathPoints.Clear();
        currentPath.AssignToRobot(target);
        AddPointToPath(currentPath, target.transform.position);
    }

    public void LoadPath(Robot target, List<Vector3> positions)
    {
        var newPath = pathPrefab.gameObject.Spawn().GetComponent<Path>();
        newPath.AssignToRobot(target);
        newPath.AddPoints(positions);
    }

    void Update()
    {
        if (isPlanning)
        {
            var ray = CameraControls.cam.ScreenPointToRay(Input.mousePosition);
            Vector3 pos;
            if (groundPlane.Raycast(ray, out var enter))
            {
                pos = ray.GetPoint(enter);
            }
            else return;
            line.SetPosition(1,pos + Vector3.up * Path.up);
            line.SetPosition(2, pos);
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 from = lastPlacedPoint + Vector3.up * Path.up;
                Vector3 to = (pos + Vector3.up * Path.up);
                //Debug.DrawRay(from,direction,Color.red,3f);
                // if (Physics.Raycast(from, direction, out var hit, direction.magnitude))
                // {
                //     Debug.Log($"Hit sth {hit.collider.name}!", hit.collider);
                // }
                // else
                // {
                //     AddPointToPath(currentPath, pos);
                // }
                if (CheckValidity(from, to)) AddPointToPath(currentPath, pos);
            }

            if (Input.GetMouseButtonDown(1))
            {
                isPlanning = false;
                line.positionCount = 0;
            }
        }
    }

    public static bool CheckValidity(Vector3 posA, Vector3 posB)
    {
        Vector3 direction = posB - posA;
        Debug.DrawRay(posA,direction,Color.red,3f);
        if (Physics.Raycast(posA, direction, out var hit ,direction.magnitude, Instance.colliderMask))
        {
            Debug.Log($"Hit sth {hit.collider.name}!", hit.collider);
            return false;
        }
        else
        {
            return true;
        }
    }

    private void AddPointToPath(Path path, Vector3 pos)
    {
        lastPlacedPoint = pos;
        line.SetPosition(0, pos + Vector3.up * Path.up);
        path.AddPoint(pos);
    }

    public void ReactivatePathBuilding(PathPoint pointScript)
    {
        var path = pointScript.path;
        currentPath = path;
        isPlanning = true;
        line.positionCount = 3;
        lastPlacedPoint = pointScript.transform.position - Vector3.up * Path.up;
        line.SetPosition(0, lastPlacedPoint);
    }

    // public void CreateFromSerializablePlan(GameManager.SerializablePlan plan)
    // {
    //     Debug.Log($"Creating {plan.manufacturer} plan.");
    //     foreach (var botString in plan.robots)
    //     {
    //         var seriBot = new Robot.SerializableRobot(botString);
    //         seriBot.Spawn(GameManager.Instance.robotPrefab);
    //     }
    // }
}
