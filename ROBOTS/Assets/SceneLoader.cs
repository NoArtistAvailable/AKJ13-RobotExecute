using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int playableScenes = SceneManager.sceneCountInBuildSettings - 1;
        var buttons = GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            int memIndex = i;
            var b = buttons[i];
            b.gameObject.SetActive(i < playableScenes);
            if (i < playableScenes)
            {
                b.onClick.AddListener( () => SceneManager.LoadScene(memIndex+1));
            }
        }
    }
}
