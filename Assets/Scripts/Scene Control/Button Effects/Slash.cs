using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slash : MonoBehaviour
{
    public Button slash;
    public GameObject AxeObject;

    void Start()
    {
        slash.onClick.AddListener(axeSlash);
    }

    void axeSlash()
    {
        Instantiate(AxeObject, new Vector3(0f, 0f, 3f), Quaternion.Euler(0, 90, 90));
    }
}
