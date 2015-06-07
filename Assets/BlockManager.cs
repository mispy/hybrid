using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class BlockManager : MonoBehaviour, ISerializationCallbackReceiver {
	public List<Texture2D> sprites = new List<Texture2D>();
	public List<BlockType> allTypes = new List<BlockType>();

	public void OnBeforeSerialize() {
	}
	
	public void OnAfterDeserialize() {
		foreach (var type in allTypes) {
			Block.types[type.name] = type;
		}


		Debug.LogFormat("{0} {1}", allTypes.Count, Block.types.Count);
	}


	public void Setup() {
		Block.manager = this;

		var resources = Resources.LoadAll("Blocks");
		foreach (var obj in resources) {
			sprites.Add(obj as Texture2D);
		}

		// let's compress all the block sprites into a single tilesheet texture
		var atlas = new Texture2D(Block.pixelSize*100, Block.pixelSize*100);
		
		var boxes = atlas.PackTextures(sprites.ToArray(), 0, Block.pixelSize*sprites.Count);

		Block.tileWidth = (float)Block.pixelSize / atlas.width;
		Block.tileHeight = (float)Block.pixelSize / atlas.height;
		
		new BlockType("tractorBeam", prefab: Game.Prefab("TractorBeam"));
		new BlockType("beamCannon", prefab: Game.Prefab("BeamCannon"));
		new BlockType("thruster", prefab: Game.Prefab("Thruster"));
		
		for (var i = 0; i < sprites.Count; i++) {			
			var name = sprites[i].name;
			if (!Block.types.ContainsKey(name)) {
				new BlockType(name);
			}
			var type = Block.types[name];
			
			var box = boxes[i];
			
			type.upUVs = new Vector2[] {
				new Vector2(box.xMin, box.yMin + Block.tileHeight),
				new Vector2(box.xMin + Block.tileWidth, box.yMin + Block.tileHeight),
				new Vector2(box.xMin + Block.tileWidth, box.yMin),
				new Vector2(box.xMin, box.yMin)
			};
			
			type.downUVs = new Vector2[] {
				new Vector2(box.xMin, box.yMin),
				new Vector2(box.xMin + Block.tileWidth, box.yMin),
				new Vector2(box.xMin + Block.tileWidth, box.yMin + Block.tileHeight),
				new Vector2(box.xMin, box.yMin + Block.tileHeight)
			};
			
			type.leftUVs = new Vector2[] {
				new Vector2(box.xMin + Block.tileWidth, box.yMin),
				new Vector2(box.xMin + Block.tileWidth, box.yMin + Block.tileHeight),
				new Vector2(box.xMin, box.yMin + Block.tileHeight),
				new Vector2(box.xMin, box.yMin)
			};
			
			type.rightUVs = new Vector2[] {
				new Vector2(box.xMin, box.yMin + Block.tileHeight),
				new Vector2(box.xMin, box.yMin),
				new Vector2(box.xMin + Block.tileWidth, box.yMin),
				new Vector2(box.xMin + Block.tileWidth, box.yMin + Block.tileHeight)
			};
		}
		
		Block.wallLayer = LayerMask.NameToLayer("Block");
		Block.floorLayer = LayerMask.NameToLayer("Floor");
		Game.Prefab("Ship").GetComponent<MeshRenderer>().sharedMaterial.mainTexture = atlas;
		Game.Prefab("Blueprint").GetComponent<MeshRenderer>().sharedMaterial.mainTexture = atlas;
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
