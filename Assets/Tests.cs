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
    XMLSaveWriter save;
    XmlTextReader xmlReader;
    XMLSaveReader load;

    string written {
        get {
            var s = Encoding.UTF8.GetString(ms.ToArray());
            ms.Seek(0, SeekOrigin.Begin);
            return s;
        }
    }


    public void RunTests() {
        ms = new MemoryStream();
        xmlWriter = new XmlTextWriter(ms, Encoding.UTF8);
        save = new XMLSaveWriter(xmlWriter);
        xmlReader = new XmlTextReader(ms);
        load = new XMLSaveReader(xmlReader);

        /*var i = 1;
        save.BindValue("i", ref i);
        xmlReader.Close();
        Debug.Assert(written == "<i>1</i>", written);

        var j = 0;
        load.BindValue("i", ref j);
        Debug.Assert(j == 1);*/

        xmlWriter.Close();
        xmlReader.Close();
        ms.Close();
    }
}

public static class Tests {
    public static void Run() {
        (new SaveTester()).RunTests();
    }
}
