using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pool {
	public static Pool ship;
	public static Pool shields;
	public static Pool ParticleThrust;
	public static Pool WallCollider;
	public static Pool FloorCollider;
	public static Pool ParticleBeam;
	public static Pool blueprint;

	public static void CreatePools() {
		Pool.blueprint = new Pool(Blueprint.prefab, 16);
		Pool.shields = new Pool(Shields.prefab, 16);
		Pool.ParticleThrust = new Pool(Game.main.thrustPrefab, 16);
		Pool.WallCollider = new Pool(Block.wallColliderPrefab, 128);
		Pool.FloorCollider = new Pool(Block.floorColliderPrefab, 64);
		Pool.ParticleBeam = new Pool(Game.main.particleBeamPrefab, 4);
		Pool.ship = new Pool(Ship.prefab, 16);
	}

	public GameObject prefab;
	public List<GameObject> pooledObjects;

	public Pool(GameObject prefab, int startingAmount) {
		this.prefab = prefab;
		pooledObjects = new List<GameObject>();
		for(int i = 0; i < startingAmount; i++)
		{
			GameObject obj = Object.Instantiate(prefab) as GameObject;
			obj.SetActive(false);
			pooledObjects.Add(obj);
		}
	}
	
	public GameObject TakeObject() {
		GameObject obj = null;

		for (int i = 0; i < pooledObjects.Count; i++) {
			if (pooledObjects[i] == null) {
				obj = Object.Instantiate(prefab) as GameObject;
				obj.SetActive(false);
				pooledObjects[i] = obj;
				return pooledObjects[i];
			}

			if (!pooledObjects[i].activeInHierarchy) {
				return pooledObjects[i];
			}
		}

		// need more! double the available objects
		var currentTotal = pooledObjects.Count;
		for (var i = 0; i < currentTotal; i++) {
			GameObject obj2 = Object.Instantiate(prefab) as GameObject;
			obj2.SetActive(false);
			pooledObjects.Add(obj2);
			if (obj == null) obj = obj2;
		}

		return obj;
	}
}
