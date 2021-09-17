using UnityEngine;

public class ContRotate : MonoBehaviour
{

    public float speed = 14;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, speed * Time.deltaTime);
    }
}
