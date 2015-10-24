using UnityEngine;
using System.Collections;

public class ConsoleData : ISaveBindable {
    public int foo = 1;

    public void Savebind(ISaveBinder save) {
        save.BindValue("foo", ref foo);
    }

    public ConsoleData() {

    }
}

public class Console : BlockComponent {
    public override void OnNewBlock(Block block) {
        block.extraData.Add(new ConsoleData());
    }
}
