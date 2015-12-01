using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NetworkStats : MonoBehaviour {
    Text text;
    float lastUpdateTime = 0;
    int bytesPerSecond = 0;
    int lastBytes = 0;

	void Awake() {
        text = GetComponent<Text>();
	}

    void Start() {
        InvokeRepeating("UpdateText", 0f, 0.5f);
    }
	
	void UpdateText() {
        var deltaTime = (Time.time - lastUpdateTime);

        var stats = NetworkClient.GetTotalConnectionStats();

        //if (SpaceNetwork.isServer)
        //    stats = NetworkServer.GetConnectionStats();

        var bytes = 0;
        foreach (var key in stats.Keys) {
            bytes += stats[key].bytes;
        }

        var bps = (bytes - lastBytes) / deltaTime;
        //bytesPerSecond = Mathf.RoundToInt((bps + bytesPerSecond) / 2.0f);
        bytesPerSecond = Mathf.RoundToInt(bps);

        lastBytes = bytes;
        text.text = (bytesPerSecond*8).ToString() + " b/s";
    }

}
