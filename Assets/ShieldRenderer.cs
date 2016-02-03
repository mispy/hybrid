using UnityEngine;

[RequireComponent(typeof(Shields))]
[RequireComponent(typeof(LineRenderer))]
public class ShieldRenderer : PoolBehaviour {
	public Shields shields;
	public LineRenderer lineRenderer;
	
	public float maxLineWidth = 1f;
    public float lastLineWidth = 0f;
	
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

    public void OnShieldsResize() {
        UpdateShields();
    }

	public void OnShieldsChange() {	
        UpdateShields();
    }

    public void OnShieldsMove() {
        UpdateShields();
    }

    void UpdateShields() {
        var lineWidth = (shields.health / shields.maxHealth) * maxLineWidth;
        //if (Mathf.Abs(lineWidth - lastLineWidth) < maxLineWidth/6f)
        //    return;
        lastLineWidth = lineWidth;

        var ellipse = shields.ellipse.Shrink(lineWidth/2f);
        
        lineRenderer.SetWidth(lineWidth, lineWidth);
        lineRenderer.SetVertexCount(shields.arcPositions.Length);
        //Debug.LogFormat("{0} {1} {2}", shields.angle, shields.arcLength, ellipse.positions.Length);

        for (var i = 0; i < shields.arcPositions.Length; i++) {
            lineRenderer.SetPosition(i, shields.arcPositions[i]);
        }

        var color = Color.Lerp(Color.red, Color.blue, shields.health/shields.maxHealth);
        lineRenderer.SetColors(color, color);
    }
}
