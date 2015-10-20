using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BlockSelector : MonoBehaviour {
    public RectTransform blockDescriber;
    public BlockType selectedType;
    List<Button> blockButtons = new List<Button>();

    void Awake() {
        MainUI.blockSelector = this;
    }

    public void CreateButtons() {
        // Clear any existing content
        blockButtons.Clear();
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        for (var i = 0; i < Block.allTypes.Count; i++) {
            var button = Pool.For("BlockButton").Take<Button>();
            button.transform.SetParent(transform);
            button.gameObject.SetActive(true);

            var j = i+1;
            button.onClick.AddListener(() => SelectBlock(j));
            blockButtons.Add(button);
            
            button.image.sprite = Block.allTypes[i].GetComponent<SpriteRenderer>().sprite;

            var text = button.GetComponentInChildren<Text>();
            text.text = (i+1).ToString();
        }
    }
    
    public void OnEnable() {
        CreateButtons();

        if (selectedType == null)
            SelectBlock(1);
    }

    /*public void Dismiss() {
        foreach (var button in blockButtons) {
            var x = button.transform.localPosition.x;
            StartCoroutine(AnimateButtonCoroutine(button, new Vector3(x, 0, 0), new Vector3(x, -Block.pixelSize*4, 0), 0.1f, true));
        }
    }*/

    void SelectBlock(int i) {
        selectedType = Block.allTypes[i-1];
        foreach (var button in blockButtons) button.image.color = Color.white;
        blockButtons[i-1].image.color = new Color(151/255f, 234/255f, 144/255f, 1);

        /*var header = blockDescriber.FindChild("HeaderText").GetComponent<Text>();
        var body = blockDescriber.FindChild("BodyText").GetComponent<Text>();
        var icon = blockDescriber.GetComponentInChildren<Button>();

        header.text = selectedType.descriptionHeader;
        body.text = selectedType.descriptionBody;
        icon.image.sprite = selectedType.GetComponent<SpriteRenderer>().sprite;*/
    }

    void Update() {
        int i = Util.GetNumericKeyDown();
        if (i > 0 && i <= Block.allTypes.Count) {
            SelectBlock(i);
        }
    }
}
