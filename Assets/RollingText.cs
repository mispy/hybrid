using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RollingText : MonoBehaviour {
    public string content;
    public int index = 0;
    public Text text;

    void Awake() {
        text = GetComponentInChildren<Text>();
        content = text.text;
    }

	void Start() {
        InvokeRepeating("UpdateText", 0f, 0.05f);
	}
	
	// Update is called once per frame
	void UpdateText() {        
        if (index >= content.Length)
            index = 0;
        
        index += 1;
        text.text = content.Substring(0, index);
	}
}
