using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PathPoint : MonoBehaviour
{
    public enum PathAction{GoTo, TakeAim, Interact}
    public PathAction pathAction;
    public SpriteRenderer sprite;
    public Sprite[] sprites;

    public Path _path;
    public Path path{get
    {
        if (!_path) _path = GetComponentInParent<Path>();
        return _path;
    }}

    public event Action<PathPoint> onClicked;
    public event Action<PathAction> onChangedAction;
    public event Action<Vector3> onChangedPosition;
    public event Action onRemove;
    
    public void Click(BaseEventData eventData)
    {
        var pointerEvent = eventData as PointerEventData;
        if (pointerEvent.dragging) return;
        if(Input.GetMouseButtonUp(0))
        {
            pathAction += 1;
            if ((int)pathAction > 2) pathAction = 0;
            Debug.Log($"Clicked {pathAction}!", this);
            SetSpriteFromAction();
            if(onChangedAction != null) onChangedAction.Invoke(pathAction);
            if(onClicked != null) onClicked.Invoke(this);
            
        }else if (Input.GetMouseButtonUp(1))
        {
            Debug.Log($"Delete this!", this);
            if(onRemove != null) onRemove.Invoke();
        }
    }

    public void SetSpriteFromAction()
    {
        sprite.sprite = sprites[(int) pathAction];
        if((int)pathAction != 0) sprite.GetComponent<Animatable>().Play(1);
    }

    public void BeginDrag()
    {
        Debug.Log("Begin Drag");
        CameraControls.SetCameraControls(false);
    }

    public void Dragging()
    {
        
    }

    public void EndDrag()
    {
        Debug.Log("End Drag");
        CameraControls.SetCameraControls(true);
    }

    public void HideIconIfNeccessary()
    {
        if (pathAction != 0) return;
        sprite.GetComponent<Animatable>().Play(0);
    }
}
