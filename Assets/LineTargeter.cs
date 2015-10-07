using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LineTargeter : MonoBehaviour {
	[HideInInspector]
	public Blockform form { get; private set; }

	public Blockform targetForm;
	public LineRenderer lineRenderer;
	public bool isTargeting = false;
	public List<Vector2> positions;
	public float lineLength;

	void Start() {
		form = GetComponentInParent<Blockform>();
		lineRenderer = Pool.For("TargetLine").Take<LineRenderer>();
		lineRenderer.sortingLayerName = "UI";
	}

	public void OnLeftClick() {
		var clickedForm = Blockform.AtWorldPos(Game.mousePos);
		if (clickedForm == null) return;

		if (!isTargeting) {
			isTargeting = true;
			targetForm = clickedForm;
			lineLength = 0f;
			positions = new List<Vector2>() { targetForm.transform.InverseTransformPoint(Game.mousePos) };
			lineRenderer.transform.SetParent(targetForm.transform);
			lineRenderer.transform.position = targetForm.transform.position;
			lineRenderer.transform.rotation = targetForm.transform.rotation;
			lineRenderer.gameObject.SetActive(true);
		}
	}
	
	public void Update() {
		if (isTargeting) {
			lineRenderer.enabled = true;
		} else {
			lineRenderer.enabled = false;
			return;
		}

		if (!Input.GetMouseButton(0)) {
			isTargeting = false;
			SendMessage("OnLineTarget");
			return;
		}

		if (!targetForm.BlocksAtWorldPos(Game.mousePos).Any())
			return;

		var localPos = targetForm.transform.InverseTransformPoint(Game.mousePos);

		var threshold = 0.5f;
		var maxLength = 20f;
		if (!positions.Any((pos) => Vector2.Distance(pos, localPos) < threshold)) {
			var dist = Vector2.Distance(positions.Last(), localPos);

			if (lineLength + dist <= maxLength) {
				positions.Add(localPos);
				lineLength += dist;
			}
		}

		lineRenderer.SetWidth(0.5f, 0.5f);
		lineRenderer.SetVertexCount(positions.Count);
		for (int i = 0; i < positions.Count; i++) {
			lineRenderer.SetPosition(i, positions[i]);
		}
		
		lineRenderer.SetColors(Color.cyan, Color.cyan);
	}
}
