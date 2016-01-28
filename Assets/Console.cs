using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Console : BlockComponent {
    LineRenderer radiusLine = null;

    private CrewBody _crew;
    public CrewBody crew {
        get { return _crew; }
        set {
            if (value == _crew) return;
            _crew = value;

            if (crew == null || crew.mind == null) {
                foreach (var block in connectedBlocks) {
                    block.mind = null;
                }
            } else {
                foreach (var block in connectedBlocks) {
                    block.mind = crew.mind;
                }
            }
        }
    }
    float controlRadius = 5f;

    public IEnumerable<Block> connectedBlocks {
        get {
            return GetControllable();
        }
    }

    public IEnumerable<Block> GetControllable() {
        foreach (var otherBlock in form.blocks.allBlocks) {
            if (IntVector2.Distance(otherBlock.pos, block.pos) <= controlRadius)
                yield return otherBlock;
        }
    }

    void Start() {
        radiusLine = Pool.For("Line").Attach<LineRenderer>(transform);
        radiusLine.sortingLayerName = "UI";
        DrawRadius();
    }

    void DrawRadius() {
        var lineWidth = 0.05f;
        var ellipse = new Ellipse(0, 0, controlRadius, controlRadius, 0);

        radiusLine.SetWidth(lineWidth, lineWidth);
        radiusLine.SetVertexCount(ellipse.positions.Length);
        for (int i = 0; i < ellipse.positions.Length; i++) {
            radiusLine.SetPosition(i, ellipse.positions[i]);
        }

        var color = new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, Color.cyan.a/2);
        radiusLine.SetColors(color, color);
    }

    void Update() {
        if (crew != null && crew.currentBlock != block)
            crew = null;
    }
}
