using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pool {
	public static Pool ship;
	public static Pool blueprint;
	public static Pool shields;
	public static Pool wallCollider;
	public static Pool floorCollider;

	public static Dictionary<GameObject, Pool> pools = new Dictionary<GameObject, Pool>();

	public static void CreatePools() {
		Pool.blueprint = Pool.For("Blueprint");
		Pool.shields = Pool.For("Shields");
		Pool.wallCollider = new Pool(Block.wallColliderPrefab, 128);
		Pool.floorCollider = new Pool(Block.floorColliderPrefab, 64);
		Pool.ship = Pool.For("Ship");
	}

	public static void Recycle(GameObject obj) {
		obj.SetActive(false);
		foreach (var comp in obj.GetComponentsInChildren<PoolBehaviour>(includeInactive: true)) {
			comp.OnRecycle();
		}
	}

	public static Pool For(string name) {
		return Pool.For(Game.Prefab(name));
	}

	public static Pool For(GameObject prefab) {
		if (!pools.ContainsKey(prefab)) {
			pools[prefab] = new Pool(prefab, 16);
		}

		return pools[prefab];
	}

	public GameObject prefab;
	public List<GameObject> pooledObjects;

	public Pool(GameObject prefab, int startingAmount) {
		this.prefab = prefab;
		prefab.SetActive(false);
		pooledObjects = new List<GameObject>();
		for(int i = 0; i < startingAmount; i++)
		{
			GameObject obj = Object.Instantiate(prefab) as GameObject;
			foreach (var comp in obj.GetComponentsInChildren<PoolBehaviour>(includeInactive: true)) {
				comp.OnCreate();
			}
			pooledObjects.Add(obj);
		}
	}
	
	public GameObject TakeObject() {
		GameObject obj = null;

		for (int i = 0; i < pooledObjects.Count; i++) {
			if (pooledObjects[i] == null) {
				obj = Object.Instantiate(prefab) as GameObject;
				pooledObjects[i] = obj;
				return pooledObjects[i];
			}

			if (!pooledObjects[i].activeSelf) {
				return pooledObjects[i];
			}
		}

		// need more! double the available objects
		var currentTotal = pooledObjects.Count;
		for (var i = 0; i < currentTotal; i++) {
			GameObject obj2 = Object.Instantiate(prefab) as GameObject;
			pooledObjects.Add(obj2);
			if (obj == null) obj = obj2;
		}

		return obj;
	}
}
