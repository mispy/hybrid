using UnityEngine;
using System.Collections;

public class NeedsCrew : BlockComponent {
    SpriteRenderer noCrewIcon;

	void Update () {
        if (block.crew == null && noCrewIcon == null) {
            noCrewIcon = Pool.For("NoCrew").Attach<SpriteRenderer>(transform);
            noCrewIcon.transform.position = transform.position;
        } else if (block.crew != null && noCrewIcon != null) {
            Pool.Recycle(noCrewIcon.gameObject);
        }
	}
}
