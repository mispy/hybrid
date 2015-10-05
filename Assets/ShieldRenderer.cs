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
	
	void Update() {	
		var lineWidth = (shields.health / shields.maxHealth) * maxLineWidth;
		var ellipse = shields.ellipse.Shrink(lineWidth/2f);
		
		lineRenderer.SetWidth(lineWidth, lineWidth);
		lineRenderer.SetVertexCount(ellipse.positions.Length);
		for (int i = 0; i < ellipse.positions.Length; i++) {
			lineRenderer.SetPosition(i, ellipse.positions[i]);
		}
		lineRenderer.SetColors(Color.green, Color.green);
	}
}
