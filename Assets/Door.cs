using UnityEngine;
using System.Collections;

public class Door : BlockComponent {
    Sprite closedSprite;
    public Sprite openSprite;

    SpriteRenderer spriteRenderer;

    public override void OnRealize() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        closedSprite = block.type.GetComponent<SpriteRenderer>().sprite;
	}
	
	// Update is called once per frame
	void Update () {
	    foreach (var body in form.maglockedCrew) {
            if (body.currentBlockPos == block.pos) {
                spriteRenderer.sprite = openSprite;
                return;
            }
        }

        spriteRenderer.sprite = closedSprite;
	}
}
