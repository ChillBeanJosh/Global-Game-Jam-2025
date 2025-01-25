using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
    public Button play;


    void Start()
    {
        play.onClick.AddListener(StartPlay);
    }

    void StartPlay()
    {
        sceneManager.Instance.LoadNewGame();
    }
}
