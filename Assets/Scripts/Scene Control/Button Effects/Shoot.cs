using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shoot : MonoBehaviour
{
    public Button shoot;
    public GameObject OrbObject;

    void Start()
    {
        shoot.onClick.AddListener(axeSlash);
    }

    void axeSlash()
    {
        Instantiate(OrbObject, new Vector3(0f, 0f, -10f), Quaternion.identity);
    }
}
