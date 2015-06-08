using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pool {
	public static GameObject holder;

	public static Dictionary<GameObject, Pool> pools = new Dictionary<GameObject, Pool>();

	public static void CreatePools() {
		Pool.holder = new GameObject();
		Pool.holder.name = "Pool";
		Pool.For("WallCollider", 128);
		Pool.For("FloorCollider", 64);
		Pool.For("Item", 128);
	}

	public static void Recycle(GameObject obj) {
		obj.transform.parent = Pool.holder.transform;
		obj.SetActive(false);
		foreach (var comp in obj.GetComponentsInChildren<PoolBehaviour>(includeInactive: true)) {
			comp.OnRecycle();
		}
	}

	public static Pool For(string name, int startingAmount = 16) {
		return Pool.For(Game.Prefab(name), startingAmount);
	}

	public static Pool For(GameObject prefab, int startingAmount = 16) {
		if (!pools.ContainsKey(prefab)) {
			pools[prefab] = new Pool(prefab, startingAmount);
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
			obj.transform.parent = Pool.holder.transform;
			foreach (var comp in obj.GetComponentsInChildren<PoolBehaviour>(includeInactive: true)) {
				comp.OnCreate();
			}
			pooledObjects.Add(obj);
		}
	}
	
	public GameObject TakeObject() {
		GameObject obj = null;

		for (int i = 0; i < pooledObjects.Count; i++) {
			if (!pooledObjects[i].activeSelf) {
				pooledObjects[i].transform.parent = Game.main.transform;
				return pooledObjects[i];
			}
		}

		// need more! double the available objects
		var currentTotal = pooledObjects.Count;
		for (var i = 0; i < currentTotal; i++) {
			GameObject obj2 = Object.Instantiate(prefab) as GameObject;
			obj2.transform.parent = Pool.holder.transform;
			pooledObjects.Add(obj2);
			if (obj == null) obj = obj2;
		}

		obj.transform.parent = Game.main.transform;
		return obj;
	}
}
