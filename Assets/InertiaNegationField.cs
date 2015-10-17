using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InertiaNegationField : MonoBehaviour {
    public float radius;
    public float drag;
    public HashSet<Rigidbody> rigids = new HashSet<Rigidbody>();

    SphereCollider collider;

    void Start() {
        transform.localScale = new Vector2(radius, radius);
        collider = GetComponent<SphereCollider>();
    }

    public void OnTriggerEnter(Collider col) {
        if (rigids.Contains(col.attachedRigidbody)) return;

        col.attachedRigidbody.drag += drag;
        rigids.Add(col.attachedRigidbody);
    }

    public void OnTriggerExit(Collider col) {
        if (!rigids.Contains(col.attachedRigidbody)) return;

        col.attachedRigidbody.drag -= drag;
        rigids.Remove(col.attachedRigidbody);
    }
}