using UnityEngine;
using System.Collections;

public class RepairTool : MonoBehaviour {
    LineRenderer line;

    void Awake() {
        line = Pool.For("Line").Attach<LineRenderer>(transform);
        line.transform.position = transform.position;
    }

    public void Repair(Vector3 worldPos) {
        foreach (var block in Block.AtWorldPos(worldPos)) {
            if (!block.isDamaged) continue;
            Repair(block);
            break;
        }
    }

    public void Repair(Block block) {
        if (!block.isDamaged) return;
        block.health += 5*Time.deltaTime;

        var width = 0.5f;
        var color = Color.white;
        var start = transform.localPosition;
        var end = block.gameObject.transform.position - transform.position;

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
