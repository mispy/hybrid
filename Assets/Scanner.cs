using UnityEngine;
using System.Collections;

public class Scanner : BlockComponent {
	[HideInInspector]
	GameObject scanCircle;

	public float scanRadius;
	
	public void Start() {
		scanCircle = Pool.For("ScanCircle").TakeObject();
	}

	public void Scan(Vector2 targetPos) {		
		var form = Blockform.AtWorldPos(targetPos);
		if (form == null) return;

		scanCircle.SetActive(true);
		scanCircle.transform.SetParent(form.transform);
		scanCircle.transform.position = Game.mousePos;
		scanCircle.transform.localScale = new Vector3(scanRadius*2, scanRadius*2, scanRadius*2);
	}
}
