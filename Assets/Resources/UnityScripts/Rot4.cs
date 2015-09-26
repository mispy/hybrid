using UnityEngine;
using System.Collections;

public struct Rot4 {
    private byte rotInt;

    public static Rot4 Up {
        get { return new Rot4(0); }
    }

    public static Rot4 Right {
        get { return new Rot4(1); }
    }

    public static Rot4 Down {
        get { return new Rot4(2); }
    }

    public static Rot4 Left {
        get { return new Rot4(3); }
    }

    public Rot4(byte rotation) {
        this.rotInt = rotation;
    }
}
