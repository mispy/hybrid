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
    public int lastIndex = 0;

    public Pool(GameObject prefab, int startingAmount) {
        this.prefab = prefab;
        pooledObjects = new List<GameObject>();
        for(int i = 0; i < startingAmount; i++)
        {
            pooledObjects.Add(CreateNew());
        }
    }

    public GameObject CreateNew() {
        prefab.SetActive(false);

        GameObject obj = Object.Instantiate(prefab) as GameObject;
        obj.name = prefab.name;
        obj.SetActive(false);
        obj.transform.SetParent(Pool.holder.transform);
        foreach (var comp in obj.GetComponentsInChildren<PoolBehaviour>(includeInactive: true)) {
            comp.OnCreate();
        }

        prefab.SetActive(true);
        return obj;
    }
    
    public GameObject TakeObject() {
        if (lastIndex < pooledObjects.Count) {
            var obj = pooledObjects[lastIndex];
            lastIndex += 1;
            return obj;
        }

        // need more! double the available objects
        for (var i = 0; i < lastIndex; i++) {
            pooledObjects.Add(CreateNew());
        }

        return TakeObject();
    }

    public T Take<T>() {
        return TakeObject().GetComponent<T>();
    }
}
