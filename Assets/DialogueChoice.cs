using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class DialogueChoice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    Button button;

    public void Awake() {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData ev) {
        button.GetComponentInChildren<Text>().color = SpaceColor.dialogueChoiceHover;
    }

    public void OnPointerExit(PointerEventData ev) {
        button.GetComponentInChildren<Text>().color = SpaceColor.dialogueChoiceNormal;
    }
}
