using UnityEngine;
using System.Collections;
using System.Linq;

public class InteriorFog : MonoBehaviour {
	Blockform form;
	MeshRenderer meshRenderer;

	// Use this for initialization
	void Start () {
		form = GetComponentInParent<Blockform>();
		meshRenderer = GetComponent<MeshRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		transform.localPosition = form.bounds.center;
		transform.localScale = form.bounds.size;

		var texture = new Texture2D(form.blocks.width, form.blocks.height);
		texture.filterMode = FilterMode.Point;
		var colors = new Color32[(form.blocks.width)*(form.blocks.height)];

		for (var i = 0; i < colors.Length; i++)
			colors[i] = Color.black;

		var blocks = form.blocks;

		// Find an open space directly on the exterior
		IntVector2 startPos = new IntVector2(0, 0);
		for (var i = blocks.minX-1; i <= blocks.maxX+1; i++) {
			for (var j = blocks.minY-1; j <= blocks.maxY+1; j++) {
				var bp = new IntVector2(i, j);
				if (blocks.CollisionLayer(bp) == Block.spaceLayer && blocks.IsPassable(bp)) {
					startPos = bp;
					break;
				}
			}
		}

		foreach (var pos in BlockPather.Floodsight(blocks, startPos)) {
			var x = pos.x - blocks.minX;
			var y = pos.y - blocks.minY;

			if (x >= 0 && y >= 0 && x < blocks.width && y < blocks.height)
				colors[y*form.blocks.width + x] = Color.clear;
		}

		texture.SetPixels32(colors);
		texture.Apply();

		meshRenderer.material.SetTexture("_Visibility", texture);
		meshRenderer.sortingLayerName = "Fog";
	}
}
