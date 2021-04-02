using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelShaker : MonoBehaviour
{
    public float shakeDistance, shellEjectionForce;
    private float time;

    public bool isEjectingShells = true;

    [SerializeField]
    private GameObject shell;

    [SerializeField]
    private Transform shellEjector;

    [SerializeField]
    Transform Parent;
    public void StartShake()
    {
        time = 0;
        StartCoroutine(TriggerShake());
    }
    private IEnumerator TriggerShake()
    {
        if (isEjectingShells)
        {
            EjectShell();
        }
        while (time < 1f)
        {
            time += Time.deltaTime;
            Parent.localPosition = new Vector3(Parent.localPosition.x, Parent.localPosition.y,
                Mathf.Sin(time * 180f * Mathf.Deg2Rad) * 2f);

            //transform.localPosition = Vector3.Lerp(transform.localPosition, (transform.localPosition + new Vector3(0,0,1)), time);

            yield return null;
        }
    }
    private void EjectShell()
    {
        GameObject EjectedShell = Instantiate(shell, shellEjector.transform.position, shell.transform.rotation
            * Quaternion.Euler(Random.Range(0, 45), Random.Range(0, 45), Random.Range(0, 45)));

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

    private void OnDisable()
    {
        StopCoroutine(TriggerShake());
    }
}
