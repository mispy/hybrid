using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonLeaveSector : MonoBehaviour {
	void Start () {
		Button b = GetComponent<Button>();
		b.onClick.AddListener(delegate() { JumpMap.Activate(); });
	}
}
