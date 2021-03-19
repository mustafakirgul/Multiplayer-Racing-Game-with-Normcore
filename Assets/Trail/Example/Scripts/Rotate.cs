using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        const float rotationsPerSecond = 1.0f/4.0f;
        this.gameObject.transform.eulerAngles = new Vector3(
            this.gameObject.transform.eulerAngles.x,
            this.gameObject.transform.eulerAngles.y +
                Time.deltaTime * 2 * Mathf.PI * Mathf.Rad2Deg * rotationsPerSecond,
            this.gameObject.transform.eulerAngles.z
        );
    }
}
