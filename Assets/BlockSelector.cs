using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BlockSelector : MonoBehaviour {
    public RectTransform blockDescriber;
    public BlockType selectedType;
    List<BlockButton> blockButtons = new List<BlockButton>();

    public void CreateButtons() {
        // Clear any existing content
        blockButtons.Clear();
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        for (var i = 0; i < BlockType.All.Count; i++) {
            var button = Pool.For("BlockButton").Attach<BlockButton>(transform);
            button.Initialize(BlockType.All[i]);
            blockButtons.Add(button);            

            var text = button.GetComponentInChildren<Text>();
            text.text = (i+1).ToString();
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
