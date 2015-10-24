using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;

public class SaveTester {
    MemoryStream ms;
    XmlTextWriter xmlWriter;
    XmlSaveWriter save;
    XmlTextReader xmlReader;
    XmlSaveReader load;

    string written {
        get {
            var s = Encoding.UTF8.GetString(ms.ToArray());
            ms.Seek(0, SeekOrigin.Begin);
            return s;
        }
    }


    public void RunTests() {
    }
}

public static class Tests {
    public static void Run() {
        (new SaveTester()).RunTests();
    }
}
