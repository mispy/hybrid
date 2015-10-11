using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public struct PathTarget {
    public Blockform form;
    public List<Vector2> path;

    public PathTarget(Blockform form, List<Vector2> path) {
        this.form = form;
        this.path = path;
    }
}

public class PathTargeter : BlockAbility {
	Blockform targetForm;
	LineRenderer lineRenderer;
	bool isDrawing = false;
	List<Vector2> positions;
	float lineLength;

    public override bool WorksWith(Block block) {
        return block.type.GetComponent<BeamCannon>() != null;
    }

    void Awake() {
        lineRenderer = Pool.For("TargetLine").Take<LineRenderer>();
        lineRenderer.sortingLayerName = "UI";

    }

	public void OnLeftClick() {
		var clickedForm = Blockform.AtWorldPos(Game.mousePos);
		if (clickedForm == null) return;

		if (!isDrawing) {
			isDrawing = true;
			targetForm = clickedForm;
			lineLength = 0f;
			positions = new List<Vector2>() { targetForm.transform.InverseTransformPoint(Game.mousePos) };
			lineRenderer.transform.SetParent(targetForm.transform);
			lineRenderer.transform.position = targetForm.transform.position;
			lineRenderer.transform.rotation = targetForm.transform.rotation;
			lineRenderer.gameObject.SetActive(true);
		}
	}

    public void UpdateInitial() {
        if (Input.GetMouseButtonDown(0))           
            OnLeftClick();
    }

    public void UpdateDrawing() {
        if (!Input.GetMouseButton(0)) {
            var target = new PathTarget(targetForm, positions);
            Debug.Log("???");
            foreach (var block in blocks) {
                Debug.Log(block);
                block.gameObject.SendMessage("OnPathTarget", target);
            }

            isDrawing = false;
            gameObject.SetActive(false);
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
	
	public void Update() {
        if (!isDrawing) {
            UpdateInitial();
        } else {
            UpdateDrawing();
        }
	}
}
