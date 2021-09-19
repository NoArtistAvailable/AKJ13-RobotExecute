using System.Collections;
using System.Collections.Generic;
using elZach.Robots;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectFaction : MonoBehaviour
{
    public List<Button> factionButtons;
    public UnityEvent<int> OnSelection;

    public Transform[] groupTargets;
    
    IEnumerator Start()
    {
        yield return null;
        for (int i = 0; i < factionButtons.Count; i++)
        {
            int memIndex = i;
            factionButtons[i].onClick.AddListener(()=>Selection(memIndex));
            if(!GameManager.Instance.currentRobots.Exists(x => (int) x.faction == i)) factionButtons[i].gameObject.SetActive(false);
        }
    }

    public void Selection(int index)
    {
        PathPlaner.SelectFaction(index);
        OnSelection.Invoke(index);
        Transform tar = groupTargets[index];
        CameraControls.Instance.SetCameraTo(tar.position);
    }
}
