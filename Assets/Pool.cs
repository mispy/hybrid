using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pool {

	public static Dictionary<GameObject, Pool> pools = new Dictionary<GameObject, Pool>();

	public static PoolManager manager;

	public static void CreatePools() {
		Pool.For("WallCollider", 128);
		Pool.For("FloorCollider", 64);
	}

	public static void Recycle(GameObject obj) {
		obj.SetActive(false);
		foreach (var comp in obj.GetComponentsInChildren<PoolBehaviour>(includeInactive: true)) {
			comp.OnRecycle();
		}
	}

	public static Pool For(string name, int startingAmount = 16) {
		return Pool.For(Game.Prefab(name), startingAmount);
	}

	public static Pool For(GameObject prefab, int startingAmount = 16) {
		if (!Pool.pools.ContainsKey(prefab)) {
			var pool = new Pool(prefab, startingAmount);
			Pool.manager.allPools.Add(pool);
			Pool.pools[prefab] = pool;
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
