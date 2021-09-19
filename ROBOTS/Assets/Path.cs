using System;
using System.Collections;
using System.Collections.Generic;
using elZach.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LineRenderer))]
public class Path : MonoBehaviour
{
    public class Point
    {
        public Vector3 position;
        public PathPoint.PathAction action;
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

    public static float up = 0.8f;
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
        if (gameObject) Destroy(gameObject);
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
            int memIndex = index;
            var point = pathPoints[index];
            var pos = point.position + Vector3.up * up;
            line.SetPosition(index, pos);
            GameObject pointVisual = Instantiate(pathPointOriginal, transform);// pathPointOriginal.Spawn(transform);
            pointVisual.transform.position = pos;
            pointVisual.hideFlags = HideFlags.HideAndDontSave;
            pointVisual.SetActive(true);
            onCreateNewPath -= pointVisual.Despawn;
            onCreateNewPath += pointVisual.Despawn;
            var pointScript = pointVisual.GetComponent<PathPoint>();
            pointScript.pathAction = point.action;
            pointScript.SetSpriteFromAction();
            pointScript.onChangedAction += (action) =>
            {
                point.action = action;
                Calculate(this.assigned);
            };
            pointScript.onRemove += () =>
            {
                Debug.Log(memIndex);
                if (memIndex == 0) return;
                if (memIndex < pathPoints.Count - 1)
                {
                    Debug.Log($"{(pathPoints[memIndex - 1].position+ Vector3.up * up)} -> {(pathPoints[memIndex + 1].position+ Vector3.up * up)}");
                    if(!PathPlaner.CheckValidity(
                        pathPoints[memIndex - 1].position+ Vector3.up * up, 
                        pathPoints[memIndex + 1].position+ Vector3.up * up)) return;
                }
                pathPoints.Remove(point);
                pointVisual.Despawn();
                CreatePath();
                Calculate(this.assigned);
            };
            pointScript.onClicked -= PathPlaner.Instance.ReactivatePathBuilding;
            if (index == pathPoints.Count - 1) pointScript.onClicked += PathPlaner.Instance.ReactivatePathBuilding;
        }
    }

    public void Calculate(Robot robot)
    {
        for (int i = 1; i < pathPoints.Count; i++)
        {
            var previous = pathPoints[i - 1];
            var point = pathPoints[i];
            float speedModifier = previous.action != PathPoint.PathAction.GoTo ? 0.5f : 1f;
            point.timeStamp = previous.timeStamp + Vector3.Distance(previous.position, point.position) / (robot.speed * speedModifier);
        }
    }
    
    public Vector3 Evaluate(float time, out Vector3 direction, out PathPoint.PathAction state)
    {
        for (int i = 0; i < pathPoints.Count; i++)
        {
            var point = pathPoints[i];
            if (time >= point.timeStamp) continue;
            var previous = pathPoints[i - 1];
            float progress = time.Remap(previous.timeStamp, point.timeStamp, 0f, 1f);
            direction = (point.position - previous.position).normalized;
            state = previous.action;
            return Vector3.Lerp(previous.position, point.position, progress);
        }
        var secondToLastPoint = pathPoints[pathPoints.Count - 2];
        var lastPoint = pathPoints[pathPoints.Count - 1];

        state = lastPoint.action;
        direction = (lastPoint.position - secondToLastPoint.position).normalized;
        return lastPoint.position;
    }

    public void AddPoint(Vector3 pos)
    {
        var p = new Point(pos);
        p.action = (PathPoint.PathAction) Mathf.RoundToInt(pos.y);
        p.position.y = 0f;
        pathPoints.Add(p);
        CreatePath();
        Calculate(assigned);
    }

    public void AddPoints(List<Vector3> positions)
    {
        foreach (var pos in positions)
        {
            var p = new Point(pos);
            if(pos.y > 0)
            {
                p.action = (PathPoint.PathAction) Mathf.RoundToInt(pos.y);
                p.position.y = 0f;
                Debug.Log($"{pos.y} -> {p.action}");
            }
            
            pathPoints.Add(p);
        }

        CreatePath();
        Calculate(assigned);
    }
}
