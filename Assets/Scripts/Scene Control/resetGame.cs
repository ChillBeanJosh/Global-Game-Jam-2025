using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class resetGame : MonoBehaviour
{
    public Button reset;

    void Start()
    {
        reset.onClick.AddListener(ResetGame);
    }

    void ResetGame()
    {
        sceneManager.Instance.RestartScene();
    }
}
