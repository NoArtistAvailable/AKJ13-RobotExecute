using System;
using System.Collections;
using System.Collections.Generic;
using elZach.Common;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Path : MonoBehaviour
{
    public List<Vector3> pathPoints;
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

    public event Action onCreateNewPath;

    [SerializeField] Button<Path> createPathButton = new Button<Path>((p) => p.CreatePath());
    public void CreatePath()
    {
        if(onCreateNewPath!=null) onCreateNewPath.Invoke();
        line.positionCount = pathPoints.Count;
        for (var index = 0; index < pathPoints.Count; index++)
        {
            var point = pathPoints[index];
            var pos = point + Vector3.up * up;
            line.SetPosition(index, pos);
            GameObject pointVisual = Instantiate(pathPointOriginal, transform);// pathPointOriginal.Spawn(transform);
            pointVisual.transform.position = pos;
            pointVisual.hideFlags = HideFlags.HideAndDontSave;
            pointVisual.SetActive(true);
            onCreateNewPath -= pointVisual.Despawn;
            onCreateNewPath += pointVisual.Despawn;
        }
    }

}
