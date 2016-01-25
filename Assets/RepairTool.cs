using UnityEngine;
using System.Collections;

public class RepairTool : MonoBehaviour {
    public static float range = 2f;
    LineRenderer line;
    float repairRate = 10f;

    void Awake() {
        line = Pool.For("Line").Attach<LineRenderer>(transform);
        line.transform.position = transform.position;
        line.sortingLayerName = "UI";
    }

    public void Repair(Vector3 worldPos) {
        foreach (var block in Block.AtWorldPos(worldPos)) {
            if (!block.isDamaged) continue;
            Repair(block);
            break;
        }
    }

    void Repair(Block block) {
        if (!block.isDamaged) return;
        var targetPos = block.ship.BlockToWorldPos(block);

        if (Vector2.Distance(targetPos, transform.position) > range)
            return;

        block.health += repairRate*Time.deltaTime;

        var width = 0.5f;
        var color = Color.yellow;
        var start = Vector2.zero;
        var end = targetPos - (Vector2)transform.position;

        line.enabled = true;
        line.SetWidth(width, width);
        line.SetVertexCount(2);
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.SetColors(color, color);

        CancelInvoke("HideLine");
        Invoke("HideLine", 0.1f);
    }

    public void HideLine() {
        line.enabled = false;
    }
}
