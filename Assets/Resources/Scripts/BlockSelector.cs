using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BlockSelector : MonoBehaviour {
	public RectTransform panel;
	public float startX;
	public float startY;

	public List<Button> blockButtons = new List<Button>();
	public BlockType selectedType;

	void Awake() {
		selectedType = Block.allTypes[0];

		MainUI.blockSelector = this;
		panel = GetComponent<RectTransform>();
		startX = -panel.sizeDelta.x/2;
		startY = panel.sizeDelta.y/2;
	}

	IEnumerator AnimateButtonCoroutine(Button button, Vector3 startPos, Vector3 endPos, float duration) {
		var startTime = Time.time;
		while (true) {
			button.transform.localPosition = Vector3.Lerp(startPos, endPos, (Time.time - startTime)/duration);
			if ((endPos - button.transform.localPosition).magnitude < Vector3.kEpsilon) break;
			yield return new WaitForEndOfFrame();
		}
	}

	public void Enable() {
		if (blockButtons.Count == 0) {
			foreach (var type in Block.allTypes) {
				var button = GameObject.Instantiate(Game.Prefab("BlockButton")).GetComponent<Button>();
				button.gameObject.SetActive(true);
				button.transform.SetParent(transform);
				button.transform.localScale = new Vector3(1, 1, 1);
				blockButtons.Add(button);

				button.image.sprite = type.uiSprite;
			}
		}

		for (var i = 0; i < blockButtons.Count; i++) {
			var button = blockButtons[i];

			var text = button.GetComponentInChildren<Text>();
			text.text = (i+1).ToString();
				
			var x = startX + Block.pixelSize/2 + i * (Block.pixelSize + 5);
			StartCoroutine(AnimateButtonCoroutine(button, new Vector3(x, -Block.pixelSize*4, 0), new Vector3(x, 0, 0), 0.1f));
		}
	}

	public void Disable() {
		foreach (var button in blockButtons) {
			var x = button.transform.localPosition.x;
			StartCoroutine(AnimateButtonCoroutine(button, new Vector3(x, 0, 0), new Vector3(x, -Block.pixelSize*4, 0), 0.1f));
		}
	}

	void SelectBlock(int i) {
		selectedType = Block.allTypes[i-1];
		foreach (var button in blockButtons) button.image.color = Color.white;
		blockButtons[i-1].image.color = new Color(151/255f, 234/255f, 144/255f, 1);
	}

	void Update() {
		int i = Util.GetNumericKeyDown();
		if (i > 0 && i <= Block.allTypes.Count) {
			SelectBlock(i);
		}
	}
}
