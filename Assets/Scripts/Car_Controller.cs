using UnityEngine;

public class Car_Controller : MonoBehaviour
{
    Rigidbody Rb => GetComponent<Rigidbody>();
    public float forwardThrust;
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            Rb.AddForce(
                transform.forward *
                Time.deltaTime *
                forwardThrust,
                ForceMode.Acceleration
                );
        }

/*        Rb.MovePosition(
            new Vector3(
             transform.position.x,
             transform.position.y + (Mathf.Sin(Time.timeSinceLevelLoad) * Time.deltaTime),
             transform.position.z
             ));*/
    }
}
