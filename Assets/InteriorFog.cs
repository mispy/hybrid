using UnityEngine;
using System.Collections;

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
		var colors = new Color32[form.blocks.width*form.blocks.height];

		for (var i = 0; i < colors.Length; i++)
			colors[i] = Color.clear;

		foreach (var block in form.blocks.AllBlocks) {
			if (!form.blocks.IsExternallyVisible(block.pos)) {
				var x = block.pos.x - form.blocks.minX;
				var y = block.pos.y - form.blocks.minY;
				colors[y*form.blocks.width + x] = Color.black;
			}
		}

		texture.SetPixels32(colors);
		texture.Apply();

		meshRenderer.material.SetTexture("_Visibility", texture);
		meshRenderer.sortingLayerName = "Fog";
	}
}
