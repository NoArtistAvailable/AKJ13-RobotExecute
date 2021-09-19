using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class VisionCone : MonoBehaviour
{
    public float angle = 30f;
    public float range = 8f;
    
    private LineRenderer _line;
    public LineRenderer line{get
    {
        if (!_line) _line = GetComponent<LineRenderer>();
        return _line;
    }}

    void Start()
    {
        SetUp();
    }

    void Update()
    {
        //line.startColor = Color.green;
    }
    
    public void SetUp()
    {
        Vector3 centerPos = transform.position + Vector3.up * 0.5f;
        Vector3 pointA = centerPos + Quaternion.Euler(0f,angle,0f) * transform.forward * range;
        Vector3 pointB = centerPos + Quaternion.Euler(0f,-angle,0f) * transform.forward * range;

        Vector3 transformScale = transform.localScale;
        transform.localScale = Vector3.one;
        centerPos = transform.InverseTransformPoint(centerPos);
        pointA = transform.InverseTransformPoint(pointA);
        pointB = transform.InverseTransformPoint(pointB);
        transform.localScale = transformScale;
        
        line.positionCount = 3;
        line.SetPositions(new Vector3[] {pointA, centerPos, pointB});
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawRay(transform.position+Vector3.up*0.5f,Quaternion.Euler(0f,angle,0f) * transform.forward * range);
        Gizmos.DrawRay(transform.position+Vector3.up*0.5f,Quaternion.Euler(0f,-angle,0f) * transform.forward * range);
    }
}
