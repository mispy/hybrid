using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BlockSelector : MonoBehaviour {
    public RectTransform blockDescriber;
    public BlockType selectedType;
    List<BlockSelectorButton> blockButtons = new List<BlockSelectorButton>();
    public GameObject buttonPrefab;

    void Awake() {
        buttonPrefab = Pool.RuntimePrefab(GetComponentInChildren<BlockSelectorButton>().gameObject);
    }

    public void CreateButtons() {
        // Clear any existing content
        blockButtons.Clear();
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        foreach (var type in BlockType.All) {
            var button = Pool.For(buttonPrefab).Attach<BlockSelectorButton>(transform);
            button.Initialize(type);
            blockButtons.Add(button);            

            var text = button.GetComponentInChildren<Text>();
            text.text = Game.inventory[type].ToString();
        }
    }

    public void OnEnable() {
        CreateButtons();

        if (selectedType == null)
            SelectBlock(blockButtons[0].blockType);
    }

    /*public void Dismiss() {
        foreach (var button in blockButtons) {
            var x = button.transform.localPosition.x;
            StartCoroutine(AnimateButtonCoroutine(button, new Vector3(x, 0, 0), new Vector3(x, -Block.pixelSize*4, 0), 0.1f, true));
        }
    }*/

    public void DescribeBlock(BlockType type) {
        var header = blockDescriber.FindChild("HeaderText").GetComponent<Text>();
        var body = blockDescriber.FindChild("BodyText").GetComponent<Text>();

        header.text = type.descriptionHeader;
        body.text = type.descriptionBody;
    }

    public void SelectBlock(BlockType type) {
        selectedType = type;
        foreach (var button in blockButtons) button.button.image.color = Color.white;
        DescribeBlock(selectedType);
    }

    void Update() {
        int i = Util.GetNumericKeyDown();
        if (i > 0 && i <= BlockType.All.Count) {
            SelectBlock(blockButtons[i].blockType);
        }
    }
}
