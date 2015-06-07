using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class PoolManager : MonoBehaviour, ISerializationCallbackReceiver {
	public List<Pool> allPools = new List<Pool>();

	public void OnBeforeSerialize() {
	}

	public void OnAfterDeserialize() {
		foreach (var pool in allPools) {
			Pool.pools[pool.prefab] = pool;
		}
	}


	// Use this for initialization
	void OnEnable() {
		Debug.Log("pool enable");
		Pool.manager = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
