using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class DialogueChoice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    Button button;
    Text text;
    LayoutElement layout;

    public void Awake() {
        button = GetComponent<Button>();
        text = GetComponentInChildren<Text>();
        layout = GetComponent<LayoutElement>();
    }

    public void OnPointerEnter(PointerEventData ev) {
        text.color = SpaceColor.dialogueChoiceHover;
    }

    public void OnPointerExit(PointerEventData ev) {
        text.color = SpaceColor.dialogueChoiceNormal;
    }
}
