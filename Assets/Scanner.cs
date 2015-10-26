using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Scanner : BlockComponent {
	[HideInInspector]
	Transform scanCircle;

	public float scanRadius;

	[HideInInspector]
	public Blockform targetForm;

	float scanDuration = 2f;
	float scanElapsed = 0f;
	List<Vector2> scanPath;
	int scanPathIndex;
	float scanPathLength;


	public void Start() {
		scanCircle = Pool.For("ScanCircle").Attach<Transform>(transform, false);
	}

	public void OnLineTarget() {
		// Delayed to prevent scan start while paused
		Invoke("ScanFromLineTarget", 0.01f);
	}

	public void Scan(Blockform targetForm, List<Vector2> points) {		
		this.targetForm = targetForm;
		scanPath = points;
		scanPathIndex = 0;
		scanPathLength = Util.GetPathLength(scanPath);
		scanElapsed = 0f;

		scanCircle.gameObject.SetActive(true);
		scanCircle.transform.SetParent(targetForm.transform);
		scanCircle.transform.position = points[0];
		scanCircle.transform.localScale = new Vector3(scanRadius*2.5f, scanRadius*2.5f, scanRadius*2.5f);
	}

	public void Update() {
		if (scanPath == null) return;
		scanElapsed += Time.deltaTime;
		scanCircle.transform.localPosition = Util.PathLerp(scanPath, scanElapsed/scanDuration);

		foreach (var block in targetForm.BlocksInLocalRadius(scanCircle.transform.localPosition, scanRadius)) {
			targetForm.fog.revealedPositions.Add(block.pos);
			targetForm.fog.needsVisibilityUpdate = true;
		}

		if (scanElapsed > scanDuration) {
			scanPath = null;
			scanCircle.gameObject.SetActive(false);
		}
	}
}
