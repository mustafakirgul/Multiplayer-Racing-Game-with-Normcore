using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelShaker : MonoBehaviour
{
    public float max, min, speed, shellEjectionForce;
    private float time;

    [SerializeField]
    private GameObject shell;

    [SerializeField]
    private Transform shellEjector;
    public void StartShake()
    {
        time = 0;
        StartCoroutine(TriggerShake());
    }
    private IEnumerator TriggerShake()
    {
        EjectShell();
        while (time < 0.5f)
        {
            time += Time.deltaTime;
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y,
                (Mathf.PingPong(time * speed, max - min) + min) * speed);
            yield return null;
        }
    }
    private void EjectShell()
    {
        GameObject EjectedShell = Instantiate(shell, shellEjector.transform.position, shell.transform.rotation
            * Quaternion.Euler(Random.Range(0,45), Random.Range(0,45), Random.Range(0, 45)));

        Rigidbody RB = EjectedShell.GetComponent<Rigidbody>();
        RB.AddForce((shellEjector.transform.right + shellEjector.transform.up + new Vector3(Random.Range(0, 0.3f), Random.Range(0, 0.3f))) 
            * shellEjectionForce, ForceMode.Impulse);
        StartCoroutine(DelayDestroy(EjectedShell));
    }

    private IEnumerator DelayDestroy(GameObject shell)
    {
        yield return new WaitForSeconds(3f);
        Destroy(shell);
    }
}
