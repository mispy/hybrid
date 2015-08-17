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
				for (var x = 0; x < tileWidth; x++) {
					for (var y = 0; y < tileHeight; y++) {
						Color[] pixels = texture.GetPixels(x*Tile.pixelSize, y*Tile.pixelSize, Tile.pixelSize, Tile.pixelSize);
						var tileTex = new Texture2D(Tile.pixelSize, Tile.pixelSize, texture.format, false);
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
		var atlas = new Texture2D(pixelSize*100, pixelSize*100);
		var boxes = atlas.PackTextures(textures, 1, pixelSize*textures.Length*1);
		
		Tile.fracWidth = (float)pixelSize / atlas.width;
		Tile.fracHeight = (float)pixelSize / atlas.height;
		
		/* There's some fiddliness here to do with texture bleeding and padding. We need a pixel
		 * of padding around each tile to prevent white lines (possibly as a result of bilinear filtering?)
		 * but we then need to remove that padding in the UVs to prevent black lines. - mispy */
		var fracX = 1f/atlas.width;
		var fracY = 1f/atlas.height;
		
		for (var i = 0; i < baseTiles.Count; i++) {			
			var baseTile = baseTiles[i];
			var box = boxes[i];
			
			var upUVs = new Vector2[] {
				new Vector2(box.xMin + fracX, box.yMin + fracHeight - fracY),
				new Vector2(box.xMin + fracWidth - fracX, box.yMin + fracHeight - fracY),
				new Vector2(box.xMin + fracWidth - fracX, box.yMin + fracY),
				new Vector2(box.xMin + fracX, box.yMin + fracY)
			};
			
			var rightUVs = new Vector2[] {
				new Vector2(box.xMin + fracWidth - fracX, box.yMin + fracY),
				new Vector2(box.xMin + fracWidth - fracX, box.yMin + fracHeight - fracY),
				new Vector2(box.xMin + fracX, box.yMin + fracHeight - fracY),
				new Vector2(box.xMin + fracX, box.yMin + fracY)
			};
			
			var downUVs = new Vector2[] {
				new Vector2(box.xMin + fracX, box.yMin + fracY),
				new Vector2(box.xMin + fracWidth - fracX, box.yMin + fracY),
				new Vector2(box.xMin + fracWidth - fracX, box.yMin + fracHeight - fracY),
				new Vector2(box.xMin + fracX, box.yMin + fracHeight - fracY)
			};
			
			var leftUVs = new Vector2[] {
				new Vector2(box.xMin + fracX, box.yMin + fracHeight - fracY),
				new Vector2(box.xMin + fracX, box.yMin + fracY),
				new Vector2(box.xMin + fracWidth - fracX, box.yMin + fracY),
				new Vector2(box.xMin + fracWidth - fracX, box.yMin + fracHeight - fracY)
			};
			
			baseTile.up = new Tile(baseTile, Rot4.Up, upUVs);
			baseTile.right = new Tile(baseTile, Rot4.Right, rightUVs);
			baseTile.down = new Tile(baseTile, Rot4.Down, downUVs);
			baseTile.left = new Tile(baseTile, Rot4.Left, leftUVs);
		}
		
		Game.Prefab("TileChunk").GetComponent<MeshRenderer>().sharedMaterial.mainTexture = atlas;
	}
	
	public readonly BaseTile baseTile;
	public readonly Rot4 rot;	
	public readonly Vector2[] uvs;
	
	public Tile(BaseTile baseTile, Rot4 rot, Vector2[] uvs) {
		this.baseTile = baseTile;
		this.rot = rot;
		this.uvs = uvs;
	}
}
