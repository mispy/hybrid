using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockHealthBar : MonoBehaviour {
    public Block block;
    LineRenderer line;

    void Awake() {
        line = Pool.For("Line").Attach<LineRenderer>(transform);
        line.sortingLayerName = "UI";
        line.transform.position = transform.position;
        line.transform.rotation = transform.rotation;
    }

    void OnDestroy() {
        Destroy(line);
    }

    public void OnHealthUpdate() {
        var startX = -Tile.worldSize/2f + 0.2f;
        var endX = Tile.worldSize/2f - 0.2f;
        var fracHealth = block.health / block.type.maxHealth;
        var color = Color.Lerp(Color.red, Color.green, fracHealth);

        var start = new Vector2(startX, Tile.worldSize/2f);
        var end = new Vector2(startX + (endX-startX)*fracHealth, Tile.worldSize/2f);
        
        line.SetWidth(0.3f, 0.3f);
        line.SetVertexCount(2);
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.SetColors(color, color);
    }
}
