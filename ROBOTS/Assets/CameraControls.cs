using System;
using System.Collections;
using System.Collections.Generic;
using elZach.Common;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    static Lazy<CameraControls> _instance = new Lazy<CameraControls>(FindObjectOfType<CameraControls>); 
    public static CameraControls Instance => _instance.Value;
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

    public AnimationCurve zoomCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
    public Vector2 minMaxZoom = new Vector2(3f, 15f);
    public float zoomSpeed = 2f;
    private float currentZoom = 0.5f;
    private Vector3 localZoomPosition;

    Vector3? clickPivot;
    Vector3 oldMousePos;

    private Vector3 pivotTargetPosition;

    private float lastClickTimeStamp;
    private float clickTimeStamp;

    public static bool active = true;
    public static void SetCameraControls(bool value) => active = value;

    public void SetCameraTo(Vector3 pos)
    {
        pivotTargetPosition = pos;
    }

    void Start()
    {
        oldMousePos = Input.mousePosition;
        localZoomPosition = Vector3.back * zoomCurve.Evaluate(currentZoom).Remap(0f, 1f, minMaxZoom.x, minMaxZoom.y);
    }

    void OnApplicationFocus(bool focus)
    {
        if(focus)
            oldMousePos = Input.mousePosition;
    }
    
    void Update()
    {
        if (active)
        {
            if (Input.GetMouseButtonDown(0))
            {
                lastClickTimeStamp = clickTimeStamp;
                clickTimeStamp = Time.time;
                oldMousePos = Input.mousePosition;
                if (GetClickPivot(out var hitPoint)) clickPivot = hitPoint;
                else clickPivot = null;
            }

            if (Input.GetMouseButton(0) && clickPivot != null)
            {
                var delta = Input.mousePosition - oldMousePos;
                delta /= Screen.width;
                var targetEuler = cameraPivot.eulerAngles + Vector3.right * delta.y * sensitivity.y +
                                  Vector3.up * delta.x * sensitivity.x;
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
                if (Time.time - clickTimeStamp < 0.2f && clickTimeStamp - lastClickTimeStamp < 0.3f &&
                    clickPivot != null) pivotTargetPosition = clickPivot.Value;
            }

            if (Mathf.Abs(Input.mouseScrollDelta.y) > 0f)
            {
                currentZoom -= Input.mouseScrollDelta.y * Time.deltaTime * zoomSpeed;
                currentZoom = Mathf.Clamp01(currentZoom);
                localZoomPosition =
                    Vector3.back * zoomCurve.Evaluate(currentZoom).Remap(0f, 1f, minMaxZoom.x, minMaxZoom.y);
            }
        }

        cameraPivot.position = Vector3.Lerp(cameraPivot.position, pivotTargetPosition, Time.deltaTime*5f);
        cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, localZoomPosition, Time.deltaTime*5f);
        oldMousePos = Input.mousePosition;
    }

    bool GetClickPivot(out Vector3 hitPoint)
    {

        var ray = cam.ScreenPointToRay(Input.mousePosition);
        // if (Physics.Raycast(ray, out var hit))
        // {
        //     hitPoint = hit.point;
        // }
        // else
        // {
        //     
        // }
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

        return true;
    }

}
