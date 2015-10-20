using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class FPSCounter : MonoBehaviour {
    Text text;
    float deltaTime = 0.0f;

	// Use this for initialization
	void Start () {
	    text = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Game.isPaused) {
            text.text = "PAUSED";
        } else {
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
            text.text = String.Format("{0} FPS", Mathf.FloorToInt(1.0f / deltaTime));
        }
	}
}
