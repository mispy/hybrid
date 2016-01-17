using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class BlockSelectorButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    public Button button;
    public BlockType blockType;

    void Awake() {
        button = GetComponent<Button>();
    }

    public void Initialize(BlockType blockType) {
        this.blockType = blockType;
        button.image.sprite = blockType.sprite;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        Game.blockSelector.SelectBlock(blockType);
        button.image.color = new Color(151/255f, 234/255f, 144/255f, 1);
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        Game.blockSelector.DescribeBlock(blockType);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        Game.blockSelector.DescribeBlock(Game.blockSelector.selectedType);
    }
}
