using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {
	public string type;
	public string orientation;

	// Use this for initialization
	void Start() {
	
	}
	
	// Update is called once per frame
	void Update() {
	
	}

	void OnDestroy() {
		if (gameObject.transform.parent != null) {
			var ship = gameObject.transform.parent.GetComponent<Ship>();
			ship.RemoveBlock(this);
		}
	}
}
