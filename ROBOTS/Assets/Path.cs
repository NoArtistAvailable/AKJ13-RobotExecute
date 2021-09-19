using System;
using System.Collections;
using System.Collections.Generic;
using elZach.Common;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Path : MonoBehaviour
{
    public class Point
    {
        public Vector3 position;
        public float timeStamp;

        public Point(Vector3 position)
        {
            this.position = position;
        }
    }
    
    //public List<Vector3> pathPositions;
    public List<Point> pathPoints = new List<Point>();
    public Robot assigned;
    
    [SerializeField] GameObject pathPointOriginal;

    public float up = 1f;
    private LineRenderer _line;
    public LineRenderer line{get
    {
        if (!_line) _line = GetComponent<LineRenderer>();
        return _line;
    }}
    
    void Start()
    {
        pathPointOriginal.SetActive(false);
    }

    public void AssignToRobot(Robot target)
    {
        if (target.path != null && target.path != this)
        {
            target.path.SelfDestruct();
        }
        target.path = this;
        target.onDestroy += SelfDestruct;
        assigned = target;
    }

    public void SelfDestruct()
    {
        if (assigned) assigned.onDestroy -= SelfDestruct;
        Destroy(gameObject);
    }

    public event Action onCreateNewPath;
    public event Action<Robot> onAddNewPathPoint;

    [SerializeField] Button<Path> createPathButton = new Button<Path>((p) => p.CreatePath());
    public void CreatePath()
    {
        if(onCreateNewPath!=null) onCreateNewPath.Invoke();
        line.positionCount = pathPoints.Count;
        for (var index = 0; index < pathPoints.Count; index++)
        {
            var point = pathPoints[index];
            var pos = point.position + Vector3.up * up;
            line.SetPosition(index, pos);
            GameObject pointVisual = Instantiate(pathPointOriginal, transform);// pathPointOriginal.Spawn(transform);
            pointVisual.transform.position = pos;
            pointVisual.hideFlags = HideFlags.HideAndDontSave;
            pointVisual.SetActive(true);
            onCreateNewPath -= pointVisual.Despawn;
            onCreateNewPath += pointVisual.Despawn;
        }
    }

    public void Calculate(Robot robot)
    {
        for (int i = 1; i < pathPoints.Count; i++)
        {
            var previous = pathPoints[i - 1];
            var point = pathPoints[i];
            point.timeStamp = previous.timeStamp + Vector3.Distance(previous.position, point.position) / robot.speed;
        }
    }
    
    public Vector3 Evaluate(float time)
    {
        for (int i = 0; i < pathPoints.Count; i++)
        {
            var point = pathPoints[i];
            if (time >= point.timeStamp) continue;
            var previous = pathPoints[i - 1];
            float progress = time.Remap(previous.timeStamp, point.timeStamp, 0f, 1f);
            return Vector3.Lerp(previous.position, point.position, progress);
        }

        return pathPoints[pathPoints.Count - 1].position;
    }

    public void AddPoint(Vector3 pos)
    {
        pathPoints.Add(new Point(pos));
        CreatePath();
        Calculate(assigned);
    }

    public void AddPoints(List<Vector3> positions)
    {
        foreach(var pos in positions)
            pathPoints.Add(new Point(pos));
        CreatePath();
        Calculate(assigned);
    }
}
