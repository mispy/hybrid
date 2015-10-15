using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DialogueChoice {
    public string text;
    public Action result;

    public DialogueChoice(string text, Action result) {
        this.text = text;
        this.result = result;
    }
}

public class DialogueNode {
    public readonly string text;
    public List<DialogueChoice> choices = new List<DialogueChoice>();

    public DialogueNode(string text) {
        this.text = text;
    }

    public void AddChoice(string text, Action result) {
        choices.Add(new DialogueChoice(text, result));
    }
}
