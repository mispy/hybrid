using UnityEngine;
using System.Collections;

public class CooldownCharger : BlockComponent {
    [HideInInspector]
    public bool isReady {
        get { return amountCharged >= 1f; }
    }
    public float chargeTime = 3f;
    public bool isPaused = false;

    [ReadOnly]
    public float amountCharged;
    LineRenderer line;

    void Start() {
        line = Pool.For("Line").Attach<LineRenderer>(transform);
        line.sortingLayerName = "UI";
    }

    public override void OnSerialize(MispyNetworkWriter writer, bool initial) {
        writer.Write(amountCharged);
    }

    public override void OnDeserialize(MispyNetworkReader reader, bool initial) {
        amountCharged = reader.ReadSingle();
    }

    public void Discharge() {
        amountCharged = 0f;
        if (hasAuthority)
            SpaceNetwork.Sync(this);
    }

	void Update() {
        if (block.isPowered && !isPaused && amountCharged < 1f)
            amountCharged = Mathf.Min(1f, amountCharged + Time.deltaTime/chargeTime);

        var startY = -Tile.worldSize/2f + 0.2f;
        var endY = Tile.worldSize/2f - 0.2f;

        var start = new Vector2(Tile.worldSize/2f, startY);
        var end = new Vector2(Tile.worldSize/2f, startY + (endY-startY)*amountCharged);

        line.SetWidth(0.2f, 0.2f);
        line.SetVertexCount(2);
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.SetColors(Color.yellow, Color.yellow);
	}
}
