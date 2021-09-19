using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class Animatable : MonoBehaviour
{
    [System.Flags]
    public enum TransformOptions{position = 1 << 1, rotation = 1 << 2, scale = 1 << 3}
    [Serializable]
    public struct TransformData
    {
        public Vector3 localPos;
        public Vector3 localRotation;
        public Vector3 localScale;
    }
    [Serializable]
    public class Clip
    {
        [System.Serializable]
        public class Events
        {
            public UnityEvent OnStarted, OnEnded;
        }
        public AnimationCurve curve = new AnimationCurve(new Keyframe(0f,0f), new Keyframe(1f,1f));
        public float time=0.25f;
        public TransformOptions animate;
        public TransformData data;
        public Events events;

    }

    public bool animateAtOnEnable = false;
    public int animateAtOnEnableTo;

    public List<Clip> clips;
    [NonSerialized] public int currentIndex = 0;

    private Coroutine currentTransition;

    void Awake()
    {
        SetTo(0);
    }

    void OnEnable()
    {
        if(animateAtOnEnable) Play(animateAtOnEnableTo);
    }

    public void SetTo(int index) => SetTo(clips[index]);
    public void Play(int index) => Play(clips[index]);

    public void SetTo(Clip state)
    {
        //var state = clips[index];
        if (state.animate.HasFlag(TransformOptions.position)) transform.localPosition = state.data.localPos;
        if (state.animate.HasFlag(TransformOptions.rotation)) transform.localEulerAngles = state.data.localRotation;
        if (state.animate.HasFlag(TransformOptions.scale)) transform.localScale = state.data.localScale;
    }

    public void Play(Clip clip)
    {
        if(currentTransition != null) StopCoroutine(currentTransition);
        currentTransition = StartCoroutine(TransitionTo(clip));
    }

    IEnumerator TransitionTo(Clip clip)
    {
        Vector3 startPos = transform.localPosition;
        Vector3 targetPos = clip.data.localPos;
        
        Quaternion startRot = transform.localRotation;
        Quaternion targetRot = Quaternion.Euler(clip.data.localRotation);
        
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = clip.data.localScale;

        float progress = 0f;
        clip.events.OnStarted.Invoke();
        while (progress < 1f)
        {
            progress += Time.deltaTime / clip.time;
            float value = clip.curve.Evaluate(progress);
            if (clip.animate.HasFlag(TransformOptions.position)) transform.localPosition = Vector3.LerpUnclamped(startPos, targetPos, value);
            if (clip.animate.HasFlag(TransformOptions.rotation)) transform.localRotation = Quaternion.LerpUnclamped(startRot, targetRot, value);
            if (clip.animate.HasFlag(TransformOptions.scale)) transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, value);
            yield return null;
        }
        SetTo(clip);
        clip.events.OnEnded.Invoke();
        currentTransition = null;
    }

}
