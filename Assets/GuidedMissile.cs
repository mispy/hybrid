using UnityEngine;
using System.Collections;

public class GuidedMissile : MonoBehaviour {
    public Block targetBlock;
    Rigidbody rigid;
    float velocity = 10.0f;
    float rotationSpeed = 90.0f;
    Collider collider;
    Explosive explosive;

    void Awake() {
        rigid = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        explosive = GetComponent<Explosive>();
    }

	void Start () {
	    
	}
	
	// Update is called once per frame
	void FixedUpdate() {
        if (targetBlock.isDestroyed)
            return;

        Vector2 targetDirection = targetBlock.worldPos - (Vector2)transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, targetDirection);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);

        rigid.velocity = transform.up * velocity;

        if (collider.bounds.Contains(targetBlock.worldPos)) {
            explosive.Explode();
        }
	}
}
