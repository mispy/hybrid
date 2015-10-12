using UnityEngine;
using System.Collections;

public class Door : BlockComponent {
    Sprite closedSprite;
    public Sprite openSprite;

    BlockType complexBlock;

    void Awake() {
        complexBlock = GetComponent<BlockType>();
        closedSprite = complexBlock.renderer.sprite;
	}
	
	// Update is called once per frame
	void Update () {
	    foreach (var body in form.maglockedCrew) {
            if (body.currentBlockPos == block.pos) {
                complexBlock.renderer.sprite = openSprite;
                return;
            }
        }

        complexBlock.renderer.sprite = closedSprite;
	}
}
