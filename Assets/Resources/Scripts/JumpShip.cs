using UnityEngine;
using System.Collections;

public class JumpShip : PoolBehaviour {
	public TileRenderer tiles;
	public JumpBeacon currentBeacon;
	public JumpBeacon destBeacon;
	public Ship ship;
	float speed = 3f;

	public static JumpShip For(Ship ship) {
		var jumpShip = Pool.For("JumpShip").Take<JumpShip>();
		jumpShip.ship = ship;
		jumpShip.tiles.SetBlocks(ship.blocks);
		jumpShip.Rescale();
		jumpShip.gameObject.SetActive(true);
		return jumpShip;
	}

	public override void OnCreate() {
		tiles = GetComponent<TileRenderer>();
	}

	public void FoldJump(JumpBeacon beacon) {
		if (currentBeacon != null)
			currentBeacon.ships.Remove(this);
		currentBeacon = null;
		destBeacon = beacon;
	}

	public void Rescale() {
		var desiredSize = 0.5f;
		var sizeX = Tile.worldSize * (ship.blocks.maxX - ship.blocks.minX);
		var sizeY = Tile.worldSize * (ship.blocks.maxY - ship.blocks.minY);
		transform.localScale = new Vector3(desiredSize/sizeX * 0.5f, desiredSize/sizeY * 0.5f, 1.0f);
	}

	public void JumpUpdate() {
		if (destBeacon == null) return;

		var targetDir = (destBeacon.transform.position - transform.position).normalized;
		transform.rotation = Quaternion.LookRotation(Vector3.forward, targetDir);
		var dist = targetDir * speed * Time.deltaTime;

		if (Vector3.Distance(destBeacon.transform.position, transform.position) <= dist.magnitude) {
			destBeacon.PlaceShip(this);
		}

		transform.position += dist;
	}
}
