using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConsoleLinker : MonoBehaviour {
    Block originBlock;
    List<Block> possibleTargets = new List<Block>();
    Block currentTarget;
    bool isLinking = false;
    LineRenderer newLine;

    void Awake() {
        newLine = Pool.For("Line").Attach<LineRenderer>(transform);
        newLine.SetWidth(0.5f, 0.5f);
        newLine.sortingLayerName = "UI";
        newLine.useWorldSpace = true;
    }

    void OnEnable() {
        InputEvent.For(KeyCode.Escape).Bind(this, () => gameObject.SetActive(false));
        InputEvent.For(MouseButton.Left).Bind(this, HandleLeftClick);
        Game.shipDesigner.cursor.gameObject.SetActive(false);
    }

    void OnDisable() {        
        Game.shipDesigner.cursor.gameObject.SetActive(true);
    }

    void StartConnection(Block block) {
        originBlock = block;
        possibleTargets.Clear();
        newLine.enabled = true;
        isLinking = true;
    }

    void HandleLeftClick() {      
        foreach (var block in Game.playerShip.blueprint.BlocksAtWorldPos(Game.mousePos)) {
            if (block.Is<Console>()) {

            }
            StartConnection(block);
            break;
        }
    }

    void Update() {
        if (!isLinking) return;

        newLine.SetVertexCount(2);
        newLine.SetPosition(0, originBlock.ship.BlockToWorldPos(originBlock));
        newLine.SetPosition(1, Game.mousePos);
       
        /*        line.SetVertexCount(2);
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.SetColors(color, color);*/

    }
}
