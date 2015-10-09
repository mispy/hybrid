using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Tileable {
    public string name;
    public int tileWidth;
    public int tileHeight;
    public BaseTile[,] tiles;
    public Texture2D texture;
    
    public Tileable(int width, int height) {
        tileWidth = width;
        tileHeight = height;
        tiles = new BaseTile[width, height];
    }

    
    public Tile GetRotatedTile(int x, int y, Facing facing) {
        if (facing == Facing.right) {
            var t = x;
            x = y;
            y = tileHeight - t - 1;
            return tiles[x, y].right;
        } else if (facing == Facing.down) {
            x = tileWidth - x - 1;
            y = tileHeight - y - 1;
            return tiles[x, y].down;
        } else if (facing == Facing.left) {
            var t = x;
            x = tileWidth - y - 1;
            y = t;
            return tiles[x, y].left;
        }

        return tiles[x, y].up;
    }
}

public class BaseTile {
    public readonly Texture2D texture; 
    
    public Tile up;
    public Tile right;
    public Tile down;
    public Tile left;
    
    public BaseTile(Texture2D texture) {
        this.texture = texture;
    }
}

public class Tile {
    public static List<Texture2D> textures = new List<Texture2D>();
    public static List<BaseTile> baseTiles = new List<BaseTile>();
    public static Dictionary<string, Tileable> tileables = new Dictionary<string, Tileable>();
    
    public static int pixelSize = 32; // the size of a tile in pixels
    public static float worldSize = 1f; // the size of a tile in worldspace coordinates
    
    // size of a tile as fraction of tilesheet size
    public static float fracWidth = 0; 
    public static float fracHeight = 0;
    
    public static void Setup() {
        foreach (var texture in Game.LoadTextures("Tileables")) {
            var tileWidth = texture.width / Tile.pixelSize;
            var tileHeight = texture.height / Tile.pixelSize;
            var tileable = new Tileable(tileWidth, tileHeight);
            tileable.texture = texture;
            
            if (texture.width == Tile.pixelSize && texture.height == Tile.pixelSize) {
                var baseTile = new BaseTile(texture);
                tileable.tiles[0, 0] = baseTile;
                Tile.baseTiles.Add(baseTile);
            } else {
                // tileable images bigger than pixelSize will be broken up into multiple tiles
                for (var x = 0; x < tileWidth; x++) {
                    for (var y = 0; y < tileHeight; y++) {
                        Color[] pixels = texture.GetPixels(x*Tile.pixelSize, y*Tile.pixelSize, Tile.pixelSize, Tile.pixelSize);
                        var tileTex = new Texture2D(Tile.pixelSize, Tile.pixelSize, texture.format, false, false);
                        tileTex.SetPixels(pixels);
                        tileTex.Apply();
                        var baseTile = new BaseTile(tileTex);
                        tileable.tiles[x, y] = baseTile;
                        //Debug.LogFormat("{0} {1} {2} {3} {4}", x, y, tileWidth, tileHeight, texture.name);
                        Tile.baseTiles.Add(baseTile);
                    }
                }
            }
            
            tileables[texture.name] = tileable;
        }
        
        // let's compress all the textures into a single tilesheet
        Texture2D[] textures = baseTiles.Select(type => type.texture).ToArray();
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
        
        for (var i = 0; i < baseTiles.Count; i++) {            
            var baseTile = baseTiles[i];
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
            
            baseTile.up = new Tile(baseTile, Facing.up, upUVs);
            baseTile.right = new Tile(baseTile, Facing.right, rightUVs);
            baseTile.down = new Tile(baseTile, Facing.down, downUVs);
            baseTile.left = new Tile(baseTile, Facing.left, leftUVs);
        }
        
        Game.Prefab("TileChunk").GetComponent<MeshRenderer>().sharedMaterial.mainTexture = atlas;
    }
    
    public readonly BaseTile baseTile;
    public readonly Facing rot;    
    public readonly Vector2[] uvs;
    
    public Tile(BaseTile baseTile, Facing rot, Vector2[] uvs) {
        this.baseTile = baseTile;
        this.rot = rot;
        this.uvs = uvs;
    }
}
