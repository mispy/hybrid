using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.IO;
using System;


public static class Save {
    public static T Load<T>(string path) {
        T obj = default(T);
        using (var save = new XMLSaveReader(path)) {
            save.BindDeep(typeof(T).Name, ref obj);
        }
        return obj;
    }
}

public interface ISaveBinder {
    void BindValue<T>(string name, ref T obj);
    void BindRef<T>(string name, ref T obj) where T : ISaveAsRef;
    void BindDeep<T>(string name, ref T obj);
    void BindList<T>(string name, ref List<T> list);
    void BindSet<T>(string name, ref HashSet<T> set);
}

public interface ISaveBindable {
    
    void Savebind(ISaveBinder save);
}

public interface ISaveAsString {
    string ToString();
}

public interface ISaveAsRef {
    string id { get; }
}

public class XMLSaveWriter : ISaveBinder, IDisposable {
    public readonly XmlTextWriter xml;
    
    public XMLSaveWriter(XmlTextWriter xml) {
        this.xml = xml;
    }

    public void Dispose() {
        xml.Close();
    }
    
    public void BindDeep<T>(string name, ref T obj) {
        xml.WriteStartElement(name);
        (obj as ISaveBindable).Savebind(this);
        xml.WriteEndElement();
    }
    
    public void BindValue<T>(string name, ref T obj) {
        xml.WriteElementString(name, obj.ToString());
    }
    
    public void BindRef<T>(string name, ref T obj) where T : ISaveAsRef {
        xml.WriteElementString(name, obj.id);
    }

    public void BindList<T>(string name, ref List<T> list) {
        xml.WriteStartElement(name);
        using (var enumerator = list.GetEnumerator()) {
            while (enumerator.MoveNext()) {
                T obj = enumerator.Current;
                
                if (obj is ISaveBindable) {
                    var bind = (ISaveBindable) obj;
                    BindDeep("li", ref bind);
                } else {
                    BindValue("li", ref obj);
                }
            }
        }
        xml.WriteEndElement();
    }


    public void BindSet<T>(string name, ref HashSet<T> set) {
        xml.WriteStartElement(name);
        using (var enumerator = set.GetEnumerator()) {
            while (enumerator.MoveNext()) {
                T obj = enumerator.Current;
                
                if (obj is ISaveBindable) {
                    var bind = (ISaveBindable) obj;
                    BindDeep("li", ref bind);
                } else {
                    BindValue("li", ref obj);
                }
            }
        }
        xml.WriteEndElement();
    }
}

public class XMLSaveReader : ISaveBinder, IDisposable {
    public readonly XmlReader xml;

    public XMLSaveReader(string path) {
        var file = new FileStream(path, FileMode.Open);
        XmlReaderSettings settings = new XmlReaderSettings();
        settings.IgnoreWhitespace = true;
        xml = XmlReader.Create(file, settings);
    }

    public void Dispose() {
        xml.Close();
    }
    
    public XMLSaveReader(XmlTextReader xml) {
        this.xml = xml;
    }
    
    public void BindDeep<T>(string name, ref T obj) {
        xml.ReadStartElement(name);
        // If the type has a parameterless constructor, use that to build it
        var constructor = typeof(T).GetConstructor(Type.EmptyTypes);
        if (constructor != null) {
            obj = Activator.CreateInstance<T>();
        } else {
            obj = (T)FormatterServices.GetUninitializedObject(typeof(T));
        }
        
        (obj as ISaveBindable).Savebind(this);
        ReadEndElement();
    }
    
    public void BindValue<T>(string name, ref T obj) {
        var s = xml.ReadElementString(name);
        
        if (typeof(ISaveAsString).IsAssignableFrom(typeof(T))) {
            obj = (T)typeof(T).GetMethod("FromString").Invoke(null, new object[] { s });
        } else if (typeof(T) == typeof(int)) {
            obj = (T)(object)Convert.ToInt32(s);
        } else {
            obj = (T)(object)s;
        }
    }
    
    public void BindRef<T>(string name, ref T obj) where T : ISaveAsRef {
        var id = xml.ReadElementString(name);
        obj = (T)typeof(T).GetMethod("FromId").Invoke(null, new object[] { id });
    }

    public void BindList<T>(string name, ref List<T> list) {
        xml.ReadStartElement(name);

        if (list == null)
            list = new List<T>();

        while (true) {
            T val = default(T);
            if (typeof(ISaveBindable).IsAssignableFrom(typeof(T))) {
                BindDeep("li", ref val);
            } else {
                BindValue("li", ref val);
            }
            list.Add(val);

            if (xml.Name != "li")
                break;
        }
        
        ReadEndElement();
    }

    
    public void BindSet<T>(string name, ref HashSet<T> set) {
        if (set == null) 
            set = new HashSet<T>();
        
        xml.ReadStartElement(name);     
        
        while (true) {
            T val = default(T);
            if (typeof(ISaveBindable).IsAssignableFrom(typeof(T))) {
                BindDeep("li", ref val);
            } else {
                BindValue("li", ref val);
            }
            set.Add(val);

            if (xml.Name != "li") break;            
        }

        ReadEndElement();
    }

    void ReadEndElement() {
        if (xml.NodeType != XmlNodeType.EndElement) {
            Debug.LogWarning("Expected end element");
            xml.Read();
        } else {            
            xml.ReadEndElement();
        }
    }
}
