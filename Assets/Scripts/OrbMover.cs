using UnityEngine;

public class OrbMover : MonoBehaviour
{
    public float speed;
    public float destroyTime;

    private void Update()
    {
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z + (Time.deltaTime * speed));

        Destroy(gameObject, destroyTime);
    }
}
