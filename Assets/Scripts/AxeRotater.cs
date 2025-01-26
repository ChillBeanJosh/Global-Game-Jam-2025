using UnityEngine;

public class AxeRotater : MonoBehaviour
{
    public float speed;
    public float destroyTime;

    private void Update()
    {
        transform.Rotate(0f, 0f, -200f * Time.deltaTime * speed, Space.Self);
        Destroy(gameObject, destroyTime);
    }

}
