using UnityEngine;
using System.Collections;

public class CrewHealthBar : MonoBehaviour {
    public CrewBody crew;
    LineRenderer line;

    void Awake() {
        crew = GetComponentInParent<CrewBody>();
        line = Pool.For("Line").Attach<LineRenderer>(transform);
        line.sortingLayerName = "UI";
        line.transform.position = transform.position;
        line.transform.rotation = transform.rotation;
    }

    public void Update() {
        /*if (crew.health == crew.maxHealth) {
            line.enabled = false;
            return;
        } else {
            line.enabled = true;
        }*/

        var startX = -Tile.worldSize/2f + 0.2f;
        var endX = Tile.worldSize/2f - 0.2f;
        var fracHealth = (float)crew.health / crew.maxHealth;
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
