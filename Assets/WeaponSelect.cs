using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class WeaponSelect : MonoBehaviour {
	public BlockType selectedType { get; private set; }
    float startX;
    float startY;
    RectTransform panel;

    List<BlockType> fireableTypes = new List<BlockType>();
    List<Button> blockButtons = new List<Button>();

    void Awake() {
        panel = GetComponent<RectTransform>();
        startX = -panel.sizeDelta.x/2;
        startY = panel.sizeDelta.y/2;

        // clean up placeholder UI
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
    }

	void OnBlockAdded(Block block) {
		if (block.type.canBeFired && !fireableTypes.Contains(block.type)) {
			fireableTypes.Add(block.type);
			
			var button = Pool.For("BlockButton").Take<Button>();
			button.gameObject.SetActive(true);
			button.transform.SetParent(transform);
			button.transform.localScale = new Vector3(1, 1, 1);
			blockButtons.Add(button);
			
			button.image.sprite = block.type.GetComponent<SpriteRenderer>().sprite;

			var i = blockButtons.Count-1;			

			var text = button.GetComponentInChildren<Text>();
			text.text = (i+1).ToString();
			
			var x = startX + Tile.pixelSize/2 + i * (Tile.pixelSize + 5);
			button.transform.localPosition = new Vector3(x, 0, 0);
		}
	}
    
    void OnEnable() {
		foreach (var block in Game.playerShip.blocks.allBlocks)
			OnBlockAdded(block);
		Game.playerShip.blocks.OnBlockAdded += OnBlockAdded;
        InputEvent.Numeric.Bind(this, OnNumericValue);
    }

    public void OnDisable() {
        foreach (var button in blockButtons)
            Pool.Recycle(button.gameObject);
        blockButtons.Clear();
        fireableTypes.Clear();
    }
    
    public void SelectBlocks(int i) {
        foreach (var button in blockButtons) button.image.color = Color.white;
		if (i == -1)
			selectedType = null;
		else {
			selectedType = fireableTypes[i-1];
			blockButtons[i-1].image.color = new Color(151/255f, 234/255f, 144/255f, 1);
		}

        Game.shipControl.DeselectBlocks();
        foreach (var block in Game.playerShip.blocks.Find(selectedType)) {
            Game.shipControl.SelectBlock(block);
        }

        Game.abilityMenu.SelectDefault();
    }

    public void OnNumericValue(int i) {
        if (i > 0 && i <= blockButtons.Count) {
            SelectBlocks(i);
        }
    }
}
