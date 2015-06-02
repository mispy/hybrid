using UnityEngine;
using System.Collections;

public class Shields : MonoBehaviour {
	public static GameObject prefab;

	public float effectTime;
	public MeshRenderer renderer;
	public Vector3 contactPoint;
	private float elapsedTime = 0.0f;
	private float duration = 0.0f;
	private bool staying = false;

	void Awake() {
		renderer = GetComponent<MeshRenderer>();
	}

	void Update(){
		renderer.sharedMaterial.SetFloat("_Offset", Mathf.Repeat(Time.time, 1));
		//renderer.sharedMaterial.SetFloat("_RadialFactor", Mathf.Repeat(Time.time, 1));

		var easeIn = Interpolate.Ease(Interpolate.EaseType.EaseInCubic);
		var easeOut = Interpolate.Ease(Interpolate.EaseType.EaseOutCubic);

		if (elapsedTime < duration) {
			if (elapsedTime < duration/2.0f) {
				renderer.sharedMaterial.SetFloat("_RadialFactor", easeIn(0.0f, 1.0f, elapsedTime, duration/2.0f));
			} else {
				renderer.sharedMaterial.SetVector("_Position", -contactPoint);
				renderer.sharedMaterial.SetFloat("_RadialFactor", 1.0f - easeOut(0.0f, 1.0f, elapsedTime - duration/2.0f, duration/2.0f));
			}
			if (!staying)
				elapsedTime += Time.deltaTime;
		} else {
			renderer.sharedMaterial.SetFloat("_RadialFactor", 0.0f);
		}
	}
	
	public void OnCollisionEnter(Collision collision) {
		contactPoint = transform.InverseTransformPoint(collision.contacts[0].point);
		renderer.sharedMaterial.SetVector("_Position", contactPoint);

		if (elapsedTime > duration) {
			elapsedTime = 0.0f;
		} else if (elapsedTime > 0.4f) {
			elapsedTime = 0.39f;
		}
		duration = 0.8f;
	}

	public void OnCollisionStay(Collision collision) {
		if (elapsedTime >= 0.19f) {
			elapsedTime = 0.2f;
			staying = true;
		}
	}

	public void OnCollisionExit(Collision collision) {
		staying = false;
	}
}
