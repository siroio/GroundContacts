using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public struct GroundResult
{
    public GroundResult(bool isHit, float angle, Vector3 normal)
    {
        this.isHit = isHit;
        this.angle = angle;
        this.normal = normal;
    }

    public readonly bool isHit;
    public readonly float angle;
    public readonly Vector3 normal;
}

public class CheckGround : MonoBehaviour
{
    [Header("CheckParameter")]
    [SerializeField, Range(0.0f, 2.0f)] private float radius;
    [SerializeField, Range(0.1f, 1.00f)] private float GroundCheckDistance;
    [SerializeField] private LayerMask checkMask;

    private static readonly HashSet<string> GroundTags = new HashSet<string> {
        "IceGround",
        "Ground",
        "PlayerDieArea",
        "Parts"
    };

    private CapsuleCollider Capsule;
    private Rigidbody rb;
    private RaycastHit hit;
    
    private void Awake() {
        Capsule = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        var i = onGround();
        Debug.DrawRay(Vector3.zero, i.normal, Color.magenta, 0.1f);
    }

    public GroundResult onGround() {
        // calc
        float sphereRadius = Capsule.radius * radius;
        Vector3 origin = rb.position + new Vector3(0, sphereRadius * 2, 0);
        var collider = Physics.OverlapSphere(origin, sphereRadius, checkMask);

        bool isHit = collider?.Length > 0 && collider.Any(col => GroundTags.Contains(col.tag)) ||
                     Physics.SphereCast(origin, sphereRadius, Vector3.down * 10, out hit, sphereRadius + GroundCheckDistance, checkMask);

        // result
        bool ground = isHit && GroundTags.Contains(hit.collider.tag);
        float angle = Vector3.Angle(Vector3.up, hit.normal);

        //return values
        return new GroundResult(ground, angle, hit.normal);
    }

    public GroundResult onGround(Action<GroundResult> action)
    {
        GroundResult result = onGround();
        action(result);
        return result;
    }
}
