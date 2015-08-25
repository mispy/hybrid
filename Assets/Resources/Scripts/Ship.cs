using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.IO;
using System.Xml;
using System.Xml.Serialization;


public class Ship : PoolBehaviour {
	public static GameObject prefab;
	public static List<Ship> allActive = new List<Ship>();
	public static Dictionary<string, Ship> templates = new Dictionary<string, Ship>();

	public static IEnumerable<Ship> ClosestTo(Vector2 worldPos) {
		return Ship.allActive.OrderBy((ship) => Vector2.Distance(ship.transform.position, worldPos));
	}
	
	public static Ship AtWorldPos(Vector2 worldPos) {
		foreach (var ship in Ship.allActive) {
			var blockPos = ship.WorldToBlockPos(worldPos);
			if (ship.blocks[blockPos, BlockLayer.Base] != null || ship.blueprint.blocks[blockPos, BlockLayer.Base] != null) {
				return ship;
			}
		}
		
		return null;
	}

	public static Ship Template(string name) {
		return templates[name];
	}

	public static void LoadTemplates() {
		foreach (var path in Directory.GetFiles(Application.dataPath + "/Ships/", "*.xml")) {

			var serializer = new XmlSerializer(typeof(ShipData));
			
			ShipData data;
			using (var stream = new FileStream(path, FileMode.Open))
			{
				data = serializer.Deserialize(stream) as ShipData;
			}
			
			var ship = Save.LoadShip(data);
			templates[ship.name] = ship;
		}
	}

	public BlockMap blocks;
	public TileRenderer tiles;
	public Blueprint blueprint;
	
	public Rigidbody rigidBody;

	public bool hasCollision = true;
	public bool hasGravity = false;
	
	public Shields shields = null;
	
	public Vector3 localCenter;

	public List<Crew> maglockedCrew = new List<Crew>();

	public float scrapAvailable = 1000;

	private bool needsMassUpdate = true;

	public int size = 0;

	public GameObject blockComponentHolder;

	public IEnumerable<T> GetBlockComponents<T>() {
		return GetComponentsInChildren<T>().Where((comp) => (comp as BlockComponent).block.ship == this);
	}

	public bool HasBlockComponent<T>() {
		return GetBlockComponents<T>().ToList().Count > 0;
	}

    public override void OnCreate() {
		rigidBody = GetComponent<Rigidbody>();
		tiles = GetComponent<TileRenderer>();
		blocks = GetComponent<BlockMap>();
		blocks.OnBlockRemoved += OnBlockRemoved;
		blocks.OnBlockAdded += OnBlockAdded;

		var obj = Pool.For("Blueprint").TakeObject();
		obj.transform.parent = transform;
		obj.transform.position = transform.position;
		obj.SetActive(true);
		blueprint = obj.GetComponent<Blueprint>();
		blueprint.ship = this;

		obj = Pool.For("Holder").TakeObject();
		obj.transform.parent = transform;
		obj.transform.position = transform.position;
		obj.name = "BlockComponents";
		obj.SetActive(true);
		blockComponentHolder = obj;
	}
	
	void Start() {								
		UpdateMass();	
		UpdateShields();	
		UpdateGravity();

		foreach (var block in blocks.AllBlocks) {
			if (block.type.isComplexBlock)
				AddBlockComponent(block);
		}

		Ship.allActive.Add(this);

		InvokeRepeating("UpdateMass", 0.0f, 0.05f);
	}
		
	public override void OnRecycle() {
		Ship.allActive.Remove(this);
	}

	public void SetBlock<T>(int x, int y) {
		var block = Block.Make<T>();
		blocks[x, y, block.layer] = block;
		var block2 = BlueprintBlock.Make<T>();
		blueprint.blocks[x, y, block2.layer] = block2;
	}

	public void SetBlock(IntVector2 pos, BlockType type) {
		var block = new Block(type);
		blocks[pos, block.layer] = block;
		var block2 = new BlueprintBlock(type);
		blueprint.blocks[pos, block2.layer] = block2;
	}

	public void SetBlock(int x, int y, BlockType type) {
		var block = new Block(type);
		blocks[x, y, block.layer] = block;
		var block2 = new BlueprintBlock(type);
		blueprint.blocks[x, y, block2.layer] = block2;
	}

	public void SetBlock<T>(int x, int y, Orientation orientation) {
		var block = Block.Make<T>();
		block.orientation = orientation;
		blocks[x, y, block.layer] = block;

		var block2 = BlueprintBlock.Make<T>();
		block2.orientation = orientation;
		blueprint.blocks[x, y, block2.layer] = block2;
	}

	public void ReceiveImpact(Rigidbody fromRigid, Block block) {
		var impactVelocity = rigidBody.velocity - fromRigid.velocity;
		var impactForce = impactVelocity.magnitude * fromRigid.mass;
		//if (impactForce < 5) return;

		// break it off into a separate fragment
		//BreakBlock(block);
	}

	public GameObject BreakBlock(Block block) {
		blocks[block.pos, block.layer] = null;
		
		/*var newShipObj = Pool.For("Ship").TakeObject();
		newShipObj.transform.position = BlockToWorldPos(block.pos);
		var newShip = newShipObj.GetComponent<Ship>();
		newShip.blocks[0, 0] = block;
		newShipObj.SetActive(true);
		newShip.rigidBody.velocity = rigidBody.velocity;
		newShip.rigidBody.angularVelocity = rigidBody.angularVelocity;*/
		//newShip.hasCollision = false;

		var obj = Pool.For("Item").TakeObject();
		obj.transform.position = BlockToWorldPos(block.pos);
		obj.SetActive(true);
		var rigid = obj.GetComponent<Rigidbody>();
		rigid.velocity = rigidBody.velocity;
		rigid.angularVelocity = rigidBody.angularVelocity;

		if (blocks.Count == 0) Pool.Recycle(gameObject);

		return obj;
	}

	public void OnBlockRemoved(Block oldBlock) {
		Profiler.BeginSample("OnBlockRemoved");

		if (oldBlock.layer == BlockLayer.Base)
			this.size -= 1;

		UpdateBlock(oldBlock);

		Profiler.EndSample();
	}

	public void OnBlockAdded(Block newBlock) {
		newBlock.ship = this;

		if (newBlock.layer == BlockLayer.Base)
			this.size += 1;

		UpdateBlock(newBlock);

		if (newBlock.type.isComplexBlock) {
			AddBlockComponent(newBlock);
		}	
	}

	public void UpdateBlock(Block block) {
		if (block.mass != 0)
			needsMassUpdate = true;
		
		if (Block.Is<ShieldGenerator>(block))
			UpdateShields();

		if (Block.Is<InertiaStabilizer>(block))
			UpdateGravity();
	}

	public void AddBlockComponent(Block block) {
		Vector2 worldOrient = transform.TransformVector(Util.orientToCardinal[block.orientation]);

		var obj = Pool.For(block.type.gameObject).TakeObject();		
		obj.transform.parent = blockComponentHolder.transform;
		obj.transform.position = BlockToWorldPos(block);
		obj.transform.up = worldOrient;
		block.gameObject = obj;
		foreach (var comp in block.gameObject.GetComponents<BlockComponent>()) {
			comp.block = block;
		}

		obj.SetActive(true);

	}

	public void UpdateMass() {		
		if (!needsMassUpdate) return;

		var totalMass = 0.0f;
		var avgPos = new IntVector2(0, 0);
		
		foreach (var block in blocks.AllBlocks) {
			totalMass += block.mass;
			avgPos.x += block.pos.x;
			avgPos.y += block.pos.y;
		}
		
		rigidBody.mass = totalMass;

		if (blocks.Count > 0) {
			avgPos.x /= blocks.Count;
			avgPos.y /= blocks.Count;
		}
		localCenter = BlockToLocalPos(avgPos);
		rigidBody.centerOfMass = localCenter;

		needsMassUpdate = false;
	}

	public void UpdateShields() {
		if (blocks.Has<ShieldGenerator>() && shields == null) {
			var shieldObj = Pool.For("Shields").TakeObject();
			shields = shieldObj.GetComponent<Shields>();
			shieldObj.transform.parent = transform;
			shieldObj.transform.localPosition = localCenter;
			shieldObj.SetActive(true);
		} else if (!blocks.Has<ShieldGenerator>() && shields != null) {
			shields.gameObject.SetActive(false);
			shields = null;
		}
	}

	public void UpdateGravity() {
		if (blocks.Has<InertiaStabilizer>() && hasGravity == false) {
			//hasGravity = true;
			rigidBody.drag = 5;
			rigidBody.angularDrag = 5;
		} else if (!blocks.Has<InertiaStabilizer>() && hasGravity == true) {
			//hasGravity = false;
			rigidBody.drag = 0;
			rigidBody.angularDrag = 0;
		}
		hasGravity = true;
	}

	public void RotateTowards(Vector2 worldPos) {
		var dir = (worldPos - (Vector2)transform.position).normalized;
		float angle = Mathf.Atan2(dir.y,dir.x)*Mathf.Rad2Deg - 90;
		var currentAngle = transform.localEulerAngles.z;

		if (Math.Abs(360+angle - currentAngle) < Math.Abs(angle - currentAngle)) {
			angle = 360+angle;
		}

		if (angle > currentAngle + 10) {
			FireAttitudeThrusters(Orientation.right);
		} else if (angle < currentAngle - 10) {
			FireAttitudeThrusters(Orientation.left);
		}

	}

	public void MoveTowards(Vector3 worldPos) {
		var dist = (worldPos - transform.position).magnitude;
		if ((worldPos - (transform.position + transform.up)).magnitude < dist) {
			FireThrusters(Orientation.down);
		}
		/*var localDir = transform.InverseTransformDirection((worldPos - (Vector2)transform.position).normalized);
		var orient = Util.cardinalToOrient[Util.Cardinalize(localDir)];
		FireThrusters((Orientation)(-(int)orient));*/
	}

	public IntVector2 WorldToBlockPos(Vector2 worldPos) {
		return LocalToBlockPos(transform.InverseTransformPoint(worldPos));
	}

	public IntVector2 LocalToBlockPos(Vector3 localPos) {
		// remember that blocks go around the center point of the center block at [0,0]		
		return new IntVector2(Mathf.FloorToInt((localPos.x + Tile.worldSize/2.0f) / Tile.worldSize),
		                      Mathf.FloorToInt((localPos.y + Tile.worldSize/2.0f) / Tile.worldSize));
	}


	public Vector2 BlockToLocalPos(IntVector2 blockPos) {
		return new Vector2(blockPos.x*Tile.worldSize, blockPos.y*Tile.worldSize);
	}

	public Vector2 BlockToLocalPos(Block block) {
		var centerX = block.pos.x + block.Width/2.0f;
		var centerY = block.pos.y + block.Height/2.0f;
		return new Vector2(centerX * Tile.worldSize - Tile.worldSize/2.0f, centerY * Tile.worldSize - Tile.worldSize/2.0f);
	}

	public Vector2 BlockToWorldPos(IntVector2 blockPos) {
		return transform.TransformPoint(BlockToLocalPos(blockPos));
	}

	public Vector2 BlockToWorldPos(Block block) {
		return transform.TransformPoint(BlockToLocalPos(block));
	}

	public IEnumerable<Block> BlocksAtWorldPos(Vector2 worldPos) {
		return blocks[WorldToBlockPos(worldPos)];
	}

	public void FireThrusters(Orientation orientation) {
		foreach (var thruster in GetBlockComponents<Thruster>()) {
			if (thruster.block.orientation == orientation)
				thruster.Fire();
		}
	}

	public void FireAttitudeThrusters(Orientation orientation) {
		foreach (var thruster in GetBlockComponents<Thruster>()) {
			if (thruster.block.orientation == orientation)
				thruster.FireAttitude();
		}
	}

	void OnCollisionEnter(Collision collision) {
		var obj = collision.rigidbody.gameObject;

		if (collision.contacts.Length == 0) return;

		if (shields != null && collision.contacts[0].thisCollider.gameObject == shields.gameObject) {
			shields.OnCollisionEnter(collision);
			return;
		}

		if (obj.tag == "Item") {
			scrapAvailable += 10;
			Pool.Recycle(obj);
			//foreach (var beam in GetBlockComponents<TractorBeam>()) {
				//if (beam.captured.Contains(obj.GetComponent<Collider>())) {
				//}
			//}
		}

		var otherShip = obj.GetComponent<Ship>();
		if (otherShip != null) {
			foreach (var block in otherShip.BlocksAtWorldPos(collision.collider.transform.position)) {
				otherShip.ReceiveImpact(rigidBody, block);
			}
		}
	}

	public Crew FindPilot() {
		foreach (var crew in maglockedCrew) {
			if (crew.controlConsole != null)
				return crew;
		}

		return null;
	}

	void OnCollisionStay(Collision collision) {
		if (shields != null) {
			shields.OnCollisionStay(collision);
			return;
		}
	}

	void OnCollisionExit(Collision collision) {
		if (shields != null) {
			shields.OnCollisionExit(collision);
			return;
		}
	}

	public void StartTractorBeam(Vector2 pz) {
		foreach (var tractorBeam in GetBlockComponents<TractorBeam>()) {
			tractorBeam.Fire(pz);
		}
	}

	public void StopTractorBeam() {
		foreach (var tractorBeam in GetBlockComponents<TractorBeam>()) {
			tractorBeam.Stop();
		}
	}

	void UpdateMesh() {
		Profiler.BeginSample("UpdateMesh");

		/*if (shields != null) {
			var hypo = Mathf.Sqrt(mesh.bounds.size.x*mesh.bounds.size.x + mesh.bounds.size.y*mesh.bounds.size.y);
			var scale = new Vector3(mesh.bounds.size.x, mesh.bounds.size.y, 1);
			scale.x += hypo * mesh.bounds.size.x / (mesh.bounds.size.x+mesh.bounds.size.y);
			scale.y += hypo * mesh.bounds.size.y / (mesh.bounds.size.x+mesh.bounds.size.y);
			scale.z = Math.Max(scale.x, scale.y);

			shields.transform.localScale = scale;
		}*/

		Profiler.EndSample();
	}

}
