using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PathPoint : MonoBehaviour
{
    public enum PathAction{GoTo, TakeAim, ReleaseAim, Wait}
    public PathAction pathAction;
    public void Click()
    {
        Debug.Log($"Clicked {pathAction}!", this);
    }
}
