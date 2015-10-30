using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SectorInfo : MonoBehaviour {
    Text text;

    void Awake() {
        text = GetComponentInChildren<Text>();
    }
}
