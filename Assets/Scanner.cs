using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Scanner : BlockComponent {
	[HideInInspector]
	GameObject scanCircle;

	public float scanRadius;

	[HideInInspector]
	public Blockform targetForm;

	float scanDuration = 2f;
	float scanElapsed = 0f;
	List<Vector2> scanPath;
	int scanPathIndex;
	float scanPathLength;

	public void Start() {
		scanCircle = Pool.For("ScanCircle").TakeObject();
	}

	public void OnLineTarget() {
		var lineTargeter = GetComponent<LineTargeter>();	
		Scan(lineTargeter.targetForm, lineTargeter.positions);
	}

	public void Scan(Blockform targetForm, List<Vector2> points) {		
		scanPath = points;
		scanPathIndex = 0;
		scanPathLength = Util.GetPathLength(scanPath);
		scanElapsed = 0f;

		scanCircle.SetActive(true);
		scanCircle.transform.SetParent(targetForm.transform);
		scanCircle.transform.position = points[0];
		scanCircle.transform.localScale = new Vector3(scanRadius*2, scanRadius*2, scanRadius*2);
	}

	public void Update() {
		if (scanPath == null) return;
		scanElapsed += Time.deltaTime;
		scanCircle.transform.localPosition = Util.PathLerp(scanPath, scanElapsed/scanDuration);

		if (scanElapsed > scanDuration) {
			scanPath = null;
			scanCircle.SetActive(false);
		}
	}
}
