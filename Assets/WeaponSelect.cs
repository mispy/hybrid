using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class WeaponSelect : MonoBehaviour {
	public BlockType selectedType { get; private set; }

    List<BlockType> fireableTypes = new List<BlockType>();
    List<Button> blockButtons = new List<Button>();

    GameObject buttonPrefab;
    bool needsRefresh = true;

    void Awake() {
        foreach (Transform child in transform) {
            buttonPrefab = Pool.RuntimePrefab(child.gameObject);
            break;
        }

        Clear();
    }

    void Clear() {
        fireableTypes.Clear();
        blockButtons.Clear();
        foreach (Transform child in transform) {
            Pool.Recycle(child.gameObject);
        }
    }

    void Refresh() {
        Clear();

        foreach (var type in BlockType.All) {
            if (type.showInMenu && Game.playerShip.blocks.Has(type)) {
                fireableTypes.Add(type);
            }
        }

        foreach (var type in fireableTypes) {
            var button = Pool.For(buttonPrefab).Attach<Button>(transform);
            blockButtons.Add(button);

            button.image.sprite = type.GetComponent<SpriteRenderer>().sprite;

            var i = blockButtons.Count-1;           

            var text = button.GetComponentInChildren<Text>();
            text.text = (i+1).ToString();
        }

        needsRefresh = false;
    }

	void OnBlockAdded(Block block) {
        needsRefresh = true;
	}

    void OnBlockRemoved(Block block) {
        needsRefresh = true;
    }

    void OnEnable() {
        Refresh();
		Game.playerShip.blocks.OnBlockAdded += OnBlockAdded;
        Game.playerShip.blocks.OnBlockRemoved += OnBlockRemoved;
        InputEvent.Numeric.Bind(this, OnNumericValue);
    }

    void OnDisable() {
        Game.playerShip.blocks.OnBlockAdded -= OnBlockAdded;
        Game.playerShip.blocks.OnBlockRemoved -= OnBlockRemoved;
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
        foreach (var block in Game.shipControl.console.connectedBlocks) {
            if (block.type == selectedType)
                Game.shipControl.SelectBlock(block);
        }

        Game.abilityMenu.SelectDefault();
    }

    public void OnNumericValue(int i) {
        if (i > 0 && i <= blockButtons.Count) {
            SelectBlocks(i);
        }
    }

    void Update() {
        if (needsRefresh) Refresh();   
    }
}
