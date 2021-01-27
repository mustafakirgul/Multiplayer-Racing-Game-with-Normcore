using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
 private Rigidbody rb => GetComponent<Rigidbody>();

 public void GoDown()
 {
  rb.isKinematic = false;
 }
}
