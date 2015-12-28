using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Linq;

public class NetworkDebugUI : MonoBehaviour {
    GameObject lineTemplate;

    public void Start() {
        lineTemplate = GameObject.Instantiate(GetComponentInChildren<Text>().gameObject);
        InvokeRepeating("UpdateStats", 0f, 1f);
    }

    public void UpdateStats() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        foreach (var stat in SpaceNetwork.stats.OrderBy((s) => -s.Value)) {
            var text = Pool.For(lineTemplate).Attach<Text>(transform);
            text.text = String.Format("{0} {1}", stat.Key, stat.Value);
        }
    }
}
