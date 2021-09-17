using UnityEngine;

public class Thrust : MonoBehaviour
{

    [SerializeField] float speed;
    [SerializeField] float boostFactor = 0.03f;

    [SerializeField] JoystickController controller;

    public float boost;

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime * (1 + (boost * boostFactor)));
    }

    void OnCollisionEnter(Collision col)
    {
        controller.Shake();
        controller.Blink();
    }
}
