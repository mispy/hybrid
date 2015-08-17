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

	public static void OnRecycle(GameObject obj) {
		foreach (Transform child in obj.transform) {
			Pool.OnRecycle(child.gameObject);
		}
		
		foreach (var comp in obj.GetComponents<PoolBehaviour>()) {
			comp.OnRecycle();
		}
	}

	public static void Recycle(GameObject obj) {
		Pool.OnRecycle(obj);
		Object.Destroy(obj);	
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
			pooledObjects.Add(CreateNew());
		}
	}

	public GameObject CreateNew() {
		GameObject obj = Object.Instantiate(prefab) as GameObject;
		obj.name = prefab.name;
		obj.SetActive(false);
		obj.transform.parent = Pool.holder.transform;
		foreach (var comp in obj.GetComponentsInChildren<PoolBehaviour>(includeInactive: true)) {
			comp.OnCreate();
		}
		return obj;
	}
	
	public GameObject TakeObject() {
		GameObject obj = null;

		for (int i = 0; i < pooledObjects.Count; i++) {
			if (pooledObjects[i] == null) {
				pooledObjects[i] = CreateNew();
				pooledObjects[i].transform.SetParent(Game.main.currentSector.transform);
				return pooledObjects[i];
			} else if (!pooledObjects[i].activeSelf) {
				pooledObjects[i].transform.SetParent(Game.main.currentSector.transform);
				return pooledObjects[i];
			}
		}

		// need more! double the available objects
		var currentTotal = pooledObjects.Count;
		for (var i = 0; i < currentTotal; i++) {
			pooledObjects.Add(CreateNew());
		}

		return TakeObject();
	}

	public T Take<T>() {
		return TakeObject().GetComponent<T>();
	}
}
