using UnityEngine;
using System.Collections;

public class InertiaNegationAbility : BlockAbility {
    public override bool WorksWith(Block block) {
        return block.type.GetComponent<InertiaNegator>() != null;
    }
    
    void OnEnable() {
        InputEvent.For(MouseButton.Left).Bind(this, OnLeftClick);
    }

    public void OnLeftClick() {
        var field = Pool.For("InertiaNegationField").TakeObject();
        field.transform.position = Game.mousePos;
        field.gameObject.SetActive(true);
        //field.AddComponent<Transient>();
        gameObject.SetActive(false);
    }
}
