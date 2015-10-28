using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System;

[CustomEditor(typeof(Marker))]
public class MarkerEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        
        Marker marker = (Marker)target;

        foreach (var template in marker.matchingTemplates) {
            GUI.enabled = false;
            EditorGUILayout.ObjectField(template, template.GetType(), false);
            GUI.enabled = true;
        }
    }
}

[ExecuteInEditMode]
public class Marker : MonoBehaviour, ISerializationCallbackReceiver {
    bool needsUpdate = true;
    public string tagged = "";

    [NonSerialized]
    public List<ShipTemplate2> matchingTemplates = new List<ShipTemplate2>();

    public string[] tags {
        get {
            return tagged.Split(' ');
        }
    }

    public bool Matches(ShipTemplate2 template) {
        foreach (var tag in tags) {
            if (template.tags.Contains(tag))
                continue;

            return false;
        }

        return true;

    }

    void UpdateMatches() {
        matchingTemplates.Clear();
        foreach (var template in ShipTemplate2.All) {
            if (this.Matches(template)) {
                matchingTemplates.Add(template);
            }
        }
    }

    public void Realize() {
        var match = Util.GetRandom(matchingTemplates.ToArray());
        match.Realize(transform.position);
        Pool.Recycle(this.gameObject);
    }

    public void OnBeforeSerialize() {

    }

    public void OnAfterDeserialize() {
        needsUpdate = true;
    }

    public void Update() {
        if (EditorApplication.isPlaying) {
            Realize();
            return;
        }

        if (!needsUpdate) return;
        needsUpdate = false;

        UpdateMatches();
        var sprite = Game.Sprite("NoPower");

        var width = 0;
        var height = 0;

        foreach (var template in matchingTemplates) {
            sprite = Game.Sprite("ShipIcon");

            if (template.blocks.width > width)
                width = template.blocks.width;
            if (template.blocks.height > height)
                height = template.blocks.height;
        }

        if (matchingTemplates.Any())
            transform.localScale = new Vector3(width, height, 1);
        GetComponent<SpriteRenderer>().sprite = sprite;
    }
}