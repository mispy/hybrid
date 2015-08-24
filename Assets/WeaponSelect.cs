using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class WeaponSelect : MonoBehaviour {
	public BlockType selectedType { get; private set; }
	float startX;
	float startY;
	RectTransform panel;
	Ship ship;

	List<BlockType> fireableTypes = new List<BlockType>();
	List<Button> blockButtons = new List<Button>();

	void Awake() {
		panel = GetComponent<RectTransform>();
		startX = -panel.sizeDelta.x/2;
		startY = panel.sizeDelta.y/2;
	}
	
	public void OnEnable() {
		ship = Crew.player.maglockShip;

		foreach (var type in Block.allTypes) {
			if (type.canBeFired && ship.blocks.Has(type)) {
				fireableTypes.Add(type);

				var button = Pool.For("BlockButton").Take<Button>();
				button.gameObject.SetActive(true);
				button.transform.SetParent(transform);
				button.transform.localScale = new Vector3(1, 1, 1);
				blockButtons.Add(button);
				
				button.image.sprite = type.GetComponent<SpriteRenderer>().sprite;
			}
		}

		for (var i = 0; i < blockButtons.Count; i++) {
			var button = blockButtons[i];
			
			var text = button.GetComponentInChildren<Text>();
			text.text = (i+1).ToString();
			
			var x = startX + Tile.pixelSize/2 + i * (Tile.pixelSize + 5);
			button.transform.localPosition = new Vector3(x, 0, 0);
		}
	}

	public void OnDisable() {
		foreach (var button in blockButtons)
			Pool.Recycle(button.gameObject);
		blockButtons.Clear();
		fireableTypes.Clear();
	}
	
	void SelectBlock(int i) {
		selectedType = fireableTypes[i-1];
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
