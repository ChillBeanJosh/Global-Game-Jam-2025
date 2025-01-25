using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class returnToMainMenu : MonoBehaviour
{

    public Button returnToMenu;


    void Start()
    {
        returnToMenu.onClick.AddListener(StartMenu);
    }

    void StartMenu()
    {
        sceneManager.Instance.LoadMainMenu();
    }
}
