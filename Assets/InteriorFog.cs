using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InteriorFog : MonoBehaviour {
	Blockform form;
	MeshRenderer meshRenderer;
	Texture2D visibilityMap;
	public bool needsVisibilityUpdate;
	public HashSet<IntVector2> revealedPositions = new HashSet<IntVector2>();

	// Use this for initialization
	void Start () {
		form = GetComponentInParent<Blockform>();
		meshRenderer = GetComponent<MeshRenderer>();
        revealedPositions.Clear();

		form.blocks.OnBlockAdded += OnBlockUpdate;
		form.blocks.OnBlockRemoved += OnBlockUpdate;

		needsVisibilityUpdate = true;
	}

	public void OnBlockUpdate(Block block) {
		needsVisibilityUpdate = true;
	}

	void Update() {
		//if (needsVisibilityUpdate) {
            transform.localEulerAngles = new Vector3(90, 180, 0);
            transform.localPosition = form.localBounds.center;
            transform.localScale = new Vector3(form.localBounds.size.x, 0, form.localBounds.size.y) / 10f;

            if (Game.shipControl.isActiveAndEnabled) {
                //UpdateVisibility();                
            } else {
        		UpdateCrewVisibility();
            }
			needsVisibilityUpdate = false;
		//}
	}

    public void UpdateCrewVisibility() {
        var crew = Game.localPlayer.crew;
        var blocks = form.blocks;

        float sightRange = 5f;
        var texture = new Texture2D(blocks.width, blocks.height);
        texture.filterMode = FilterMode.Point;
        // texture map corresponding to tile positions. Color.clear for visible, Color.black for not
        var colors = new Color32[(blocks.width)*(blocks.height)];

        for (var i = 0; i < colors.Length; i++) {
            colors[i] = Color.black;
        }

        for (var i = blocks.minX; i <= blocks.maxX; i++) {
            for (var j = blocks.minY; j <= blocks.maxY; j++) {
                var bp = new IntVector2(i, j);
                var x = bp.x - blocks.minX;
                var y = bp.y - blocks.minY;

                if (blocks.CollisionLayer(bp) != Block.floorLayer) {
                    colors[y*blocks.width + x] = Color.clear;
                }
            }
        }

        foreach (var pos in BlockPather.Floodsight(blocks, crew.currentBlockPos)) {
            var x = pos.x - blocks.minX;
            var y = pos.y - blocks.minY;

            if (x >= 0 && y >= 0 && x < blocks.width && y < blocks.height) {
                colors[y*blocks.width + x] = Color.clear;
                revealedPositions.Add(pos);
            }
        }

        foreach (var pos in revealedPositions) {
            var x = pos.x - blocks.minX;
            var y = pos.y - blocks.minY;

            if (x >= 0 && y >= 0 && x < blocks.width && y < blocks.height && blocks.CollisionLayer(pos) == Block.floorLayer && colors[y*blocks.width + x] != Color.clear)
                colors[y*blocks.width + x] = Color.gray;
        }

        texture.SetPixels32(colors);
        texture.Apply();

        meshRenderer.material.SetTexture("_Visibility", texture);
        meshRenderer.sortingLayerName = "Fog";
        meshRenderer.material.mainTextureScale = new Vector2(form.box.bounds.size.x, form.box.bounds.size.y);           
    }

	public void UpdateVisibility() {
		var blocks = form.blocks;

		var texture = new Texture2D(blocks.width, blocks.height);
		texture.filterMode = FilterMode.Point;
        // texture map corresponding to tile positions. Color.clear for visible, Color.black for not
		var colors = new Color32[(blocks.width)*(blocks.height)];
		
		for (var i = 0; i < colors.Length; i++) {
/*			if (Game.debugVisibility) {
				colors[i] = form == Game.playerShip ? Color.black : Color.clear;
			} else {*/         
            colors[i] = Color.black;
//			}
		}

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
				colors[y*blocks.width + x] = Color.clear;
		}

		foreach (var pos in revealedPositions) {
			var x = pos.x - blocks.minX;
			var y = pos.y - blocks.minY;
			
			if (x >= 0 && y >= 0 && x < blocks.width && y < blocks.height && colors[y*blocks.width + x] != Color.clear)
				colors[y*blocks.width + x] = Color.gray;
		}

		texture.SetPixels32(colors);
		texture.Apply();
		
		meshRenderer.material.SetTexture("_Visibility", texture);
		meshRenderer.sortingLayerName = "Fog";
		meshRenderer.material.mainTextureScale = new Vector2(form.box.bounds.size.x, form.box.bounds.size.y);			
	}
}
