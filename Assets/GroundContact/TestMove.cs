using System.Collections;
using System.Collections.Generic;
using GContacts;
using UnityEngine;

public class TestMove : MonoBehaviour
{
    private Rigidbody rb;
    private GroundContact gc;
    private void Start()
    {
        TryGetComponent(out rb);
        TryGetComponent(out gc);
    }

    private void FixedUpdate()
    {
        Vector3 input = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) input.z++;
        if (Input.GetKey(KeyCode.A)) input.x--;
        if (Input.GetKey(KeyCode.S)) input.z--;
        if (Input.GetKey(KeyCode.D)) input.x++;
        Debug.Log(gc.Result);
        Vector3 movement = Vector3.ProjectOnPlane(input.normalized, gc.Result.normal);
        rb.MovePosition(rb.position + movement * 8 * Time.fixedDeltaTime);
    }
}
