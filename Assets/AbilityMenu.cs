﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AbilityMenu : MonoBehaviour {
    public BlockType selectedType { get; private set; }
    float startX;
    float startY;
    RectTransform panel;    
    
    List<BlockAbility> allAbilities = new List<BlockAbility>();
    List<BlockAbility> activeAbilities = new List<BlockAbility>();
    Dictionary<BlockAbility, Button> buttons = new Dictionary<BlockAbility, Button>();
    Button backButton;
    BlockAbility selected;

    void Awake() {
        panel = GetComponent<RectTransform>();
        startX = -panel.sizeDelta.x/2;
        startY = panel.sizeDelta.y/2;
        Clear();
            
        // Abilities are singleton objects that are activated and deactivated
        // as selected
        foreach (var prefab in Game.LoadPrefabs("BlockAbilities")) {
            var ability = Pool.For(prefab).Take<BlockAbility>();
            ability.GetComponent<SpriteRenderer>().enabled = false;
            allAbilities.Add(ability);
        }
    }

    public void OnEnable() {
    }

    void Clear() {
        buttons.Clear();

        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
    }

    public void SelectDefault() {
        SelectAbility(activeAbilities[0]);
    }

    public void SelectAbility(BlockAbility ability) {
        if (!activeAbilities.Contains(ability)) return;
        DeselectAbility();

        ability.blocks = Game.shipControl.selectedBlocks;
        ability.gameObject.SetActive(true);
        buttons[ability].image.color = new Color(151/255f, 234/255f, 144/255f, 1);
        selected = ability;
    }

    public void OnBlockSelectionUpdate() {
        Clear();
        var blocks = Game.shipControl.selectedBlocks;
        activeAbilities = new List<BlockAbility>();

        foreach (var block in blocks) {
            foreach (var ability in allAbilities) {
                if (ability.WorksWith(block))
                    activeAbilities.Add(ability);
            }
            break;
        }

        var i = 0;

        backButton = Pool.For("BackButton").Take<Button>();
        backButton.gameObject.SetActive(true);
        backButton.transform.SetParent(transform);

        var text = backButton.GetComponentsInChildren<Text>(includeInactive: true).First();
        text.text = "`";

        backButton.transform.localPosition = new Vector3(startX + Tile.pixelSize/2, 0, 0);

        backButton.onClick.AddListener(() => {
            DeselectAbility();
            Game.shipControl.DeselectBlocks();
            gameObject.SetActive(false);
        });

        i += 1;

        foreach (var ability in activeAbilities) {
            var button = Pool.For("BlockButton").Take<Button>();
            button.gameObject.SetActive(true);
            button.transform.SetParent(transform);
            button.image.sprite = ability.GetComponent<SpriteRenderer>().sprite;
            buttons[ability] = button;

            text = button.GetComponentInChildren<Text>();
            text.text = ability.key;
            
            var x = startX + Tile.pixelSize/2 + i * (Tile.pixelSize + 5);
            button.transform.localPosition = new Vector3(x, 0, 0);

            var toSelect = ability;
            button.onClick.AddListener(() => {
                SelectAbility(toSelect);
            });

            InputEvent.For(ability.keyCode).Bind(this, button.onClick.Invoke);

            i += 1;
        }
    }

    /*public void SelectBlock(int i) {
        foreach (var button in blockButtons) button.image.color = Color.white;
        if (i == -1)
            selectedType = null;
        else {
            selectedType = fireableTypes[i-1];
            blockButtons[i-1].image.color = new Color(151/255f, 234/255f, 144/255f, 1);
        }
    }*/

    void DeselectAbility() {
        if (selected == null) return;

        buttons[selected].image.color = Color.white;
        selected.gameObject.SetActive(false);
        selected = null;
    }

    void Update() {
        if (selected != null && selected.gameObject.activeInHierarchy == false)
            DeselectAbility();

        if (Game.shipControl.selectedBlocks.Count == 0)
            gameObject.SetActive(false);

        if (Input.GetKeyDown(KeyCode.BackQuote))
            backButton.onClick.Invoke();
    }
}
