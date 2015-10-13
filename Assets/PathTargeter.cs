using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PathTargeter : BlockAbility {
    PathTarget target;
	bool isDrawing = false;
	float lineLength;

    /*public override bool WorksWith(Block block) {
        return block.type.GetComponent<BeamCannon>() != null;
    }*/

    void Awake() {
    }

    void OnEnable() {
        InputEvent.For(MouseButton.Left).Bind(this, OnLeftClick);
    }

    public void OnLeftClick() {
		var clickedForm = Blockform.AtWorldPos(Game.mousePos);
		if (clickedForm == null) return;

		if (!isDrawing) {
            target = Pool.For("PathTarget").Take<PathTarget>();
            target.form = clickedForm;
            target.transform.SetParent(clickedForm.transform);
            target.transform.position = clickedForm.transform.position;
            target.transform.rotation = clickedForm.transform.rotation;
            target.Add(clickedForm.transform.InverseTransformPoint(Game.mousePos));
            lineLength = 0f;
            isDrawing = true;
		}
	}

    public void UpdateDrawing() {
        if (!Input.GetMouseButton(0)) {
            foreach (var block in blocks) {
                block.gameObject.SendMessage("OnPathTarget", target);
            }

            isDrawing = false;
            gameObject.SetActive(false);
            return;
        }
        
        if (!target.form.BlocksAtWorldPos(Game.mousePos).Any())
            return;
        
        var localPos = target.form.transform.InverseTransformPoint(Game.mousePos);
        
        var threshold = 0.5f;
        var maxLength = 20f;
        if (!target.path.Any((pos) => Vector2.Distance(pos, localPos) < threshold)) {
            var dist = Vector2.Distance(target.path.Last(), localPos);
            
            if (lineLength + dist <= maxLength) {
                target.Add(localPos);
                lineLength += dist;
            }
        }
        
    }
	
	public void Update() {
        if (isDrawing)
            UpdateDrawing();
	}
}
