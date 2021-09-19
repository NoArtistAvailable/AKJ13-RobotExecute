using System.Collections;
using System.Collections.Generic;
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
        PathPlaner.Instance.StartPath(GetComponentInParent<Robot>());
    }
}
