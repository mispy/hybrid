using UnityEngine;
using System.Collections;

public class InteriorFog : MonoBehaviour {
	Blockform form;

	// Use this for initialization
	void Start () {
		form = GetComponentInParent<Blockform>();
	}
	
	// Update is called once per frame
	void Update () {
		transform.localPosition = form.bounds.center;
		transform.localScale = form.bounds.size;
	}
}
