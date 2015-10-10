using UnityEngine;
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
    List<Button> buttons = new List<Button>();
    int selectedIndex;


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

    void Clear() {
        selectedIndex = -1;
        buttons = new List<Button>();

        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
    }

    void SelectAbility(int i) {
        activeAbilities[i].blocks = Game.shipControl.selectedBlocks;
        activeAbilities[i].gameObject.SetActive(true);
        buttons[i].image.color = new Color(151/255f, 234/255f, 144/255f, 1);
        selectedIndex = i;
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

        foreach (var ability in activeAbilities) {
            var button = Pool.For("BlockButton").Take<Button>();
            button.gameObject.SetActive(true);
            button.transform.SetParent(transform);
            button.transform.localScale = new Vector3(1, 1, 1);
            button.image.sprite = ability.GetComponent<SpriteRenderer>().sprite;
            buttons.Add(button);
            
            var i = buttons.Count-1;           
            
            var text = button.GetComponentInChildren<Text>();
            text.text = (i+1).ToString();
            
            var x = startX + Tile.pixelSize/2 + i * (Tile.pixelSize + 5);
            button.transform.localPosition = new Vector3(x, 0, 0);

            button.onClick.AddListener(() => {
                DeselectAbility();
                SelectAbility(i);
            });

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
        if (selectedIndex < 0 || selectedIndex > activeAbilities.Count) return;

        activeAbilities[selectedIndex].gameObject.SetActive(false);
        buttons[selectedIndex].image.color = Color.white;
        selectedIndex = -1;
    }

    void Update() {
        if (selectedIndex >= 0 && !activeAbilities[selectedIndex].isActiveAndEnabled) {
            DeselectAbility();
        }

        int i = Util.GetNumericKeyDown();
        if (i > 0 && i <= buttons.Count) {
            buttons[i-1].onClick.Invoke();
        }
    }
}
