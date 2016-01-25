using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Console : BlockComponent {
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

    void Update() {
        if (crew != null && crew.currentBlock != block)
            crew = null;
    }
}
