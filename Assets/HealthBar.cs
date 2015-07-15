using UnityEngine;
using System.Collections;

public class HealthBar : MonoBehaviour {
	Crew crew;

	// Use this for initialization
	void Start () {
		crew = GetComponentInParent<Crew>();
	}
	
	// Update is called once per frame
	void Update () {
		transform.localScale = new Vector3(crew.health / (float)crew.maxHealth, transform.localScale.y, transform.localScale.z);		
	}
}
