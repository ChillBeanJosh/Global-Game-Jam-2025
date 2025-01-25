using UnityEngine;

public class MainMenuObjectTransform : MonoBehaviour
{

    public Vector3 myStartPosition = new Vector3(-5, 0, 0);
    public Vector3 myEndPosition = new Vector3(5, 0, 0);

    public float speed = 3;
    public bool foward = true;

    void Start()
    {
        gameObject.transform.position = myStartPosition;
    }

    void Update()
    {
        if (gameObject.transform.position.x >= myEndPosition.x)
        {
            foward = false;
            Destroy(gameObject);
        }

        if (foward == true)
        {
            gameObject.transform.position = new Vector3(gameObject.transform.position.x + (Time.deltaTime * speed), gameObject.transform.position.y, gameObject.transform.position.z);
        }

    }
}
