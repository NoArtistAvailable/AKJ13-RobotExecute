using System.Collections;
using System.Collections.Generic;
using elZach.Robots;
using UnityEngine;
using UnityEngine.UI;

public class PlanButton : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Clicked);
    }

    private void Clicked()
    {
        if (GameManager.Instance.playSlider.value > 0)
        {
            GameManager.Instance.playSlider.value = 0f;
            GameManager.Instance.PlaySliderChanged(0f);
        }
        PathPlaner.Instance.StartPath(GetComponentInParent<Robot>());
    }
}
