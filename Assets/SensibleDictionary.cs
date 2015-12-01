using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SensibleDictionary<T1, T2> {
    Dictionary<T1, T2> dict = new Dictionary<T1, T2>();

    public T2 this[T1 key] {
        get {
            if (!dict.ContainsKey(key)) {
                var keyStrings = new List<String>();
                foreach (var other in dict.Keys) {
                    keyStrings.Add(other.ToString());
                }
                Debug.LogFormat("No {0} found for {1}. Available keys are: {2}", typeof(T2).Name, key, String.Join(", ", keyStrings.ToArray()));
            }

            return dict[key];
        }

        set {
            dict[key] = value;
        }
    }

    public bool ContainsKey(T1 key) {
        return dict.ContainsKey(key);
    }

    public bool Remove(T1 key) {
        return dict.Remove(key);
    }

    public Dictionary<T1, T2>.ValueCollection Values {
       get {
            return dict.Values;
        }
    }

    public Dictionary<T1, T2>.KeyCollection Keys {
        get {
            return dict.Keys;
        }
    }
}
