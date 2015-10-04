using UnityEngine;
using System.Collections;

public class Transient : MonoBehaviour {
	public float duration = 1f;

	void Start() {
		Invoke("Cleanup", duration);
	}

	void Cleanup() {
		Pool.Recycle(gameObject);
	}
}
