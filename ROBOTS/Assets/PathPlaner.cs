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
            line.SetPosition(1,pos + Vector3.up * pathPrefab.up);
            line.SetPosition(2, pos);
            if (Input.GetMouseButtonDown(0))
            {
                AddPointToPath(currentPath, pos);
            }

            if (Input.GetMouseButtonDown(1))
            {
                isPlanning = false;
                line.positionCount = 0;
            }
        }
    }

    private void AddPointToPath(Path path, Vector3 pos)
    {
        lastPlacedPoint = pos;
        line.SetPosition(0, pos + Vector3.up * path.up);
        path.AddPoint(pos);
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
