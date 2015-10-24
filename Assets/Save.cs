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
        using (var save = new XmlSaveReader(path)) {
            save.BindDeep(typeof(T).Name, ref obj);
        }
        return obj;
    }

    public static void Dump(ISaveAsRef obj) {
        var path = obj.savePath;
        using (var save = new XmlSaveWriter(path)) {
            save.BindDeep(obj.GetType().Name, ref obj);
        }
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
    string savePath { get; }
}

public class XmlSaveWriter : ISaveBinder, IDisposable {
    public readonly XmlWriter xml;    
    public readonly string path;
    public readonly FileStream file;
    
    public XmlSaveWriter(string path) {
        this.path = path;
        file = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;
        settings.OmitXmlDeclaration = true;
        xml = XmlWriter.Create(file, settings);
    }

    public void Dispose() {
        xml.Close();
        file.Dispose();
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

public class XmlSaveReader : ISaveBinder, IDisposable {
    public readonly XmlReader xml;
    public readonly string path;
    public readonly FileStream file;

    public XmlSaveReader(string path) {
        this.path = path;
        file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        XmlReaderSettings settings = new XmlReaderSettings();
        settings.IgnoreWhitespace = true;
        settings.IgnoreComments = true;
        xml = XmlReader.Create(file, settings);
        xml.Read();
    }

    public void Dispose() {
        xml.Close();
        file.Dispose();
    }

    public void WarnFormat(string s, params object[] format) {
        IXmlLineInfo info = xml as IXmlLineInfo;
        var prefix = String.Format("{0} line {1}: ", Util.GetIdFromPath(path), info.LineNumber);
        Debug.LogErrorFormat(prefix + s, format);
    }
    
    public void BindDeep<T>(string name, ref T obj) {
        if (xml.Name != name) {
            WarnFormat("Expected <{0}>, found {1} {2}", name, xml.NodeType, xml.Name);
            return;
        }

        xml.ReadStartElement(name);

        // If the type has a parameterless constructor, use that to build it
        var constructor = typeof(T).GetConstructor(Type.EmptyTypes);
        if (constructor != null) {
            obj = Activator.CreateInstance<T>();
        } else {
            obj = (T)FormatterServices.GetUninitializedObject(typeof(T));
        }
                        
        (obj as ISaveBindable).Savebind(this);
        ReadEndElement(name);
    }
    
    public void BindValue<T>(string name, ref T obj) {
        if (xml.Name != name) {
            WarnFormat("Expected <{0}>, found {1} {2}", name, xml.NodeType, xml.Name);
            return;
        }
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
        if (xml.Name != name) {
            WarnFormat("Expected <{0}>, found {1} {2}", name, xml.NodeType, xml.Name);
            return;
        }
        var id = xml.ReadElementString(name);
        obj = (T)typeof(T).GetMethod("FromId").Invoke(null, new object[] { id });
    }

    public void BindList<T>(string name, ref List<T> list) {
        if (xml.Name != name) {
            WarnFormat("Expected <{0}>, found {1} {2}", name, xml.NodeType, xml.Name);
            return;
        }

        if (list == null)
            list = new List<T>();

        if (xml.IsEmptyElement) {
            xml.Skip();
            return;
        }

        xml.ReadStartElement(name);

        var depth = xml.Depth;
        do {
            T val = default(T);
            if (typeof(ISaveBindable).IsAssignableFrom(typeof(T))) {
                BindDeep("li", ref val);
            } else {
                BindValue("li", ref val);
            }
            list.Add(val);
        } while (xml.Name == "li" && xml.Depth == depth);

        ReadEndElement(name);
    }

    
    public void BindSet<T>(string name, ref HashSet<T> set) {
        if (xml.Name != name) {
            WarnFormat("Expected <{0}>, found {1} {2}", name, xml.NodeType, xml.Name);
            return;
        }

        if (set == null) 
            set = new HashSet<T>();
                
        if (xml.IsEmptyElement) {
            xml.Skip();
            return;
        }

        xml.ReadStartElement(name);

        var depth = xml.Depth;
        do {
            T val = default(T);
            if (typeof(ISaveBindable).IsAssignableFrom(typeof(T))) {
                BindDeep("li", ref val);
            } else {
                BindValue("li", ref val);
            }
            set.Add(val);
        } while (xml.Name == "li" && xml.Depth == depth);

        ReadEndElement(name);
    }

    void ReadEndElement(string name) {
        if (xml.NodeType != XmlNodeType.EndElement) {
            WarnFormat("Expected </{0}>, found {1} {2}", name, xml.NodeType, xml.Name);
            xml.Read();
        } else {            
            xml.ReadEndElement();
        }
    }
}
