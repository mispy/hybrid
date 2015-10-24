using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConsoleData : ISaveBindable {
    public List<IntVector2> connections;

    public void Savebind(ISaveBinder save) {
        save.BindList("connections", ref connections);
    }

    public ConsoleData() {
        connections = new List<IntVector2>();
    }
}

public class Console : BlockComponent {
    public override void OnNewBlock(Block block) {
        block.extraData.Add(new ConsoleData());
    }
}
