using System.Collections;
using System.Collections.Generic;
using elZach.Robots;
using UnityEngine;
using UnityEngine.UI;

public class NamingField : MonoBehaviour
{
    void Start()
    {
        GetComponent<InputField>().onValueChanged.AddListener((val) => GameManager.ManufacturerName = val);
    }
}
