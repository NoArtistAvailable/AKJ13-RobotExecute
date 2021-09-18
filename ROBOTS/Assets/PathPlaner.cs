using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPlaner : MonoBehaviour
{
    public static Lazy<PathPlaner> Instance = new Lazy<PathPlaner>(FindObjectOfType<PathPlaner>);
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
    
    public void StartPath(GameObject target)
    {
        isPlanning = true;
        currentPath = pathPrefab.gameObject.Spawn().GetComponent<Path>();
        currentPath.pathPoints.Clear();
        AddPointToPath(currentPath, target.transform.position);
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
        }
    }

    private void AddPointToPath(Path path, Vector3 pos)
    {
        lastPlacedPoint = pos;
        line.SetPosition(0, lastPlacedPoint + Vector3.up * pathPrefab.up);
        path.pathPoints.Add(pos);
        path.CreatePath();
    }
}
