using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Collidable {
	public Transform transform;
	public Rect rect;

	public Collidable(Transform transform, Rect rect) {
		this.transform = transform;
		this.rect = rect;
	}
}

public class SpacePather {
	public Blockform form;

	public Rect GetRect() {
		var width = (form.blocks.maxX - form.blocks.minX) * Tile.worldSize;
		var height = (form.blocks.maxY - form.blocks.minY) * Tile.worldSize;
		return new Rect(-width/2, -height/2, width, height);
	}

	public bool IsPassable(Vector2 pos) {
		foreach (var form in Game.activeSector.blockforms) {
			var rect = form.pather.GetRect();
			if (rect.Contains(form.transform.InverseTransformPoint(pos))) {
				return false;
			}
		}

		return true;
	}

	public IEnumerable<Vector2> PathBetween(Vector2 start, Vector2 end) {
		return new List<Vector2>();
	}

	public void Update() {
		DebugUtil.DrawRect(form.transform, GetRect());
	}

	public SpacePather(Blockform form) {
		this.form = form;
	}
}
