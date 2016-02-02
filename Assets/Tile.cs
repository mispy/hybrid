using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Tileable {
    public string name;
    public int tileWidth;
    public int tileHeight;
    public Texture2D texture;

    public Tile up;
    public Tile right;
    public Tile down;
    public Tile left;

    public Tileable(Texture2D texture) {
        this.texture = texture;
    }

    public Tile GetRotatedTile(Facing facing) {
        if (facing == Facing.up)
            return up;
        else if (facing == Facing.right)
            return right;
        else if (facing == Facing.down)
            return down;
        else if (facing == Facing.left)
            return left;
        return up;
    }
}

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class Tile {
    public static List<Texture2D> textures = new List<Texture2D>();
    public static Dictionary<string, Tileable> tileablesByName = new Dictionary<string, Tileable>();
    public static List<Tileable> allTileables = new List<Tileable>();
    
    public static int pixelSize = 32; // the size of a tile in pixels
    public static float worldSize = 1f; // the size of a tile in worldspace coordinates
    
    // size of a tile as fraction of tilesheet size
    public static float fracWidth = 0; 
    public static float fracHeight = 0;
    
    static Tile() {
        foreach (var texture in Game.LoadTextures("Tileables")) {
            var tileable = new Tileable(texture);
            tileablesByName[texture.name] = tileable;
            allTileables.Add(tileable);
        }
        
        // let's compress all the textures into a single tilesheet
        Texture2D[] textures = allTileables.Select(tileable => tileable.texture).ToArray();
        var atlas = new Texture2D(pixelSize*100, pixelSize*100, TextureFormat.RGBA32, false);
		atlas.filterMode = FilterMode.Point;
		atlas.wrapMode = TextureWrapMode.Clamp;
        var boxes = atlas.PackTextures(textures, 5, pixelSize*textures.Length);
        
        Tile.fracWidth = (float)pixelSize / atlas.width;
        Tile.fracHeight = (float)pixelSize / atlas.height;
        
		// Texture extrusion hack
		// This seems to be the only way to stop occasional weird lines occurring between
		// tiles as a result of interpolation between neighboring pixels on the atlas
		var colors = atlas.GetPixels32();
		foreach (var box in boxes) {
			var xMin = Mathf.FloorToInt(box.xMin * atlas.width);
			var xMax = Mathf.FloorToInt(box.xMax * atlas.width);
			var yMin = Mathf.FloorToInt(box.yMin * atlas.height);
			var yMax = Mathf.FloorToInt(box.yMax * atlas.height);

			for (var i = xMin; i <= xMax; i++) {
				for (var j = yMin; j <= yMax; j++) {
					var color = colors[j*atlas.width + i];

					if (i == xMin && i > 1) {
						colors[j*atlas.width + i - 1] = color;
					}
					if (i == xMax) {
						colors[j*atlas.width + i + 1] = color;
					}
					if (j == yMin && j > 1) {
						colors[(j-1)*atlas.width + i] = color;
					}
					if (j == yMax) {
						colors[(j+1)*atlas.width + i] = color;
					}
				}
			}
		}
		atlas.SetPixels32(colors);
		atlas.Apply();
        
        for (var i = 0; i < allTileables.Count; i++) {            
            var tileable = allTileables[i];
            var box = boxes[i];
            var upUVs = new Vector2[] {
                new Vector2(box.xMin, box.yMax),
                new Vector2(box.xMax, box.yMax),
                new Vector2(box.xMax, box.yMin),
                new Vector2(box.xMin, box.yMin)
            };
            
            var rightUVs = new Vector2[] {
                new Vector2(box.xMax, box.yMin),
                new Vector2(box.xMax, box.yMax),
                new Vector2(box.xMin, box.yMax),
                new Vector2(box.xMin, box.yMin)
            };
            
            var downUVs = new Vector2[] {
                new Vector2(box.xMin, box.yMin),
                new Vector2(box.xMax, box.yMin),
                new Vector2(box.xMax, box.yMax),
                new Vector2(box.xMin, box.yMax)
            };
            
            var leftUVs = new Vector2[] {
                new Vector2(box.xMin, box.yMax),
                new Vector2(box.xMin, box.yMin),
                new Vector2(box.xMax, box.yMin),
                new Vector2(box.xMax, box.yMax)
            };
            
            tileable.up = new Tile(tileable, Facing.up, upUVs);
            tileable.right = new Tile(tileable, Facing.right, rightUVs);
            tileable.down = new Tile(tileable, Facing.down, downUVs);
            tileable.left = new Tile(tileable, Facing.left, leftUVs);
        }
        
        Game.Prefab("TileChunk").GetComponent<MeshRenderer>().sharedMaterial.mainTexture = atlas;
    }

    public readonly Tileable tileable;
    public readonly Facing rot;    
    public readonly Vector2[] uvs;

    public Tile(Tileable tileable, Facing rot, Vector2[] uvs) {
        this.tileable = tileable;
        this.rot = rot;
        this.uvs = uvs;
    }
}
