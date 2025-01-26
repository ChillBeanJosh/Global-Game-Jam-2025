using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CameraRotater : MonoBehaviour
{
    public List<Transform> positions;

    public Button nextButton;
    public Button previousButton;

    public bool adjustRotation = true;

    private int currentIndex = 0;

    void Start()
    {
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(MoveToNextPosition);
        }

        if (previousButton != null)
        {
            previousButton.onClick.AddListener(MoveToPreviousPosition);
        }

        if (positions.Count > 0)
        {
            SetPosition(0);
        }
    }

    void MoveToNextPosition()
    {
        if (positions.Count == 0) return;

        currentIndex = (currentIndex + 1) % positions.Count; 
        SetPosition(currentIndex);
    }

    void MoveToPreviousPosition()
    {
        if (positions.Count == 0) return;

        currentIndex = (currentIndex - 1 + positions.Count) % positions.Count; 
        SetPosition(currentIndex);
    }

    void SetPosition(int index)
    {
        if (index < 0 || index >= positions.Count) return;

        Transform target = positions[index];

        transform.position = target.position;

        if (adjustRotation)
        {
            transform.rotation = target.rotation;
        }
    }
}
