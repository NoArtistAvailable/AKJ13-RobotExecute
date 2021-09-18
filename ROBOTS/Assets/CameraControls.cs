using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    private static Camera _cam;
    public static Camera cam{get
    {
        if (!_cam) _cam = Camera.main;
        return _cam;
    }}
    public Transform cameraPivot;
    
    [Info("Sets the sensitivity of the camera.", InfoAttribute.InfoType.Warning)]
    public Vector2 sensitivity = Vector2.one;
    public Vector3 offsetRange;
    public Vector2 minMaxX;

    Vector3? clickPivot;
    Vector3 oldMousePos;

    void Start()
    {
        oldMousePos = Input.mousePosition;
    }

    void OnApplicationFocus(bool focus)
    {
        if(focus)
            oldMousePos = Input.mousePosition;
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            oldMousePos = Input.mousePosition;
            if (GetClickPivot(out var hitPoint)) clickPivot = hitPoint;
            else clickPivot = null;
        }

        if (Input.GetMouseButton(0) && clickPivot != null)
        {
            var delta = Input.mousePosition - oldMousePos;
            delta /= Screen.width;
            var targetEuler = cameraPivot.eulerAngles + Vector3.right * delta.y * sensitivity.y + Vector3.up * delta.x * sensitivity.x;
            targetEuler += offsetRange;
            targetEuler.x = targetEuler.x % 360f;
            targetEuler.x = Mathf.Clamp(targetEuler.x, minMaxX.x, minMaxX.y);
            // Debug.Log(targetEuler);
            targetEuler -= offsetRange;
            //targetEuler.x = Mathf.Clamp(targetEuler.x, -80f, 80f);
            cameraPivot.eulerAngles = targetEuler;
        }

        if (Input.GetMouseButtonUp(0))
        {
            
        }
        
        oldMousePos = Input.mousePosition;
    }

    bool GetClickPivot(out Vector3 hitPoint)
    {

        var ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit))
        {
            hitPoint = hit.point;
        }
        else
        {
            Plane plane = new Plane(Vector3.up, 0f);
            if (plane.Raycast(ray, out float enter))
            {
                hitPoint = ray.GetPoint(enter);
            }
            else
            {
                hitPoint = Vector3.zero;
                return false;
            }
        }

        return true;
    }

}
