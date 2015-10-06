using UnityEngine;

[RequireComponent(typeof(Shields))]
[RequireComponent(typeof(LineRenderer))]
public class ShieldRenderer : PoolBehaviour {
	public Shields shields;
	public LineRenderer lineRenderer;
	
	
	public float maxLineWidth = 1f;
	
	void Awake() {
		shields = GetComponent<Shields>();
		lineRenderer = GetComponent<LineRenderer>();
	}

	void Start() {
		OnShieldsChange();
	}

	public void OnShieldsEnable() {
		lineRenderer.enabled = true;
	}

	public void OnShieldsDisable() {
		lineRenderer.enabled = false;
	}

	public void OnShieldsChange() {	
		var lineWidth = (shields.health / shields.maxHealth) * maxLineWidth;
		var ellipse = shields.ellipse.Shrink(lineWidth/2f);
		
		lineRenderer.SetWidth(lineWidth, lineWidth);
		lineRenderer.SetVertexCount(ellipse.positions.Length);
		for (int i = 0; i < ellipse.positions.Length; i++) {
			lineRenderer.SetPosition(i, ellipse.positions[i]);
		}

		var color = Color.Lerp(Color.red, Color.blue, shields.health/shields.maxHealth);
		lineRenderer.SetColors(color, color);
	}
}
