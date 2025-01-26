using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bash : MonoBehaviour
{
    public Button bash;
    public GameObject ShieldObject;

    void Start()
    {
        bash.onClick.AddListener(shieldBash);
    }

    void shieldBash()
    {
        Instantiate(ShieldObject, new Vector3(-10f, 0f, 0f), Quaternion.Euler(0, 90, 0));
    }
}
