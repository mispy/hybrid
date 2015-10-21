using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DialogueMenu : MonoBehaviour {
    public Ship talkingShip;
    public Crew talkingCrew;
    public List<Button> choices = new List<Button>();
    public Text shipName;
    public Text npcName;
    public Text npcLines;
    public Transform choiceHolder;

    public void AddChoice(DialogueChoice choice) {
        var button = Pool.For("DialogueButton").Attach<Button>(choiceHolder);
        button.onClick.AddListener(() => choice.result.Invoke());

        var text = button.GetComponentsInChildren<Text>(includeInactive: true).First();
        text.text = String.Format("{0}. {1}", choices.Count+1, choice.text);

        choices.Add(button);
    }

    public void OnEnable() {
        Game.Pause();
        InputEvent.Numeric.Bind(this, OnNumericInput);
    }

    public void OnNumericInput() {
        //if (i < choices.Count)
        //    choices[i-1].Invoke();
    }

    public void OnDisable() {
        Game.Unpause();
    }

    public void StartDialogue(Ship ship, DialogueNode node) {
        gameObject.SetActive(true);

        // Clean up any old dialogue choices
        choices.Clear();
        foreach (Transform child in choiceHolder)
            Destroy(child.gameObject);

        talkingShip = ship;
        talkingCrew = Util.GetRandom(ship.crew);

        shipName.text = talkingShip.nameWithColor;
        npcName.text = talkingCrew.fancyName;
        npcLines.text = node.text;

        foreach (var choice in node.choices) {
            AddChoice(choice);
        }

/*        AddChoice("<color='yellow'>[Trade]</color> What do you have in stock? Yadda yadda etc I'm going to ramble for a while to check that the choice layout flow behaves correctly.", () => EndDialogue());
        AddChoice("<color='red'>[Attack]</color> Hand over your goods!", () => {
            new NotableEvent_ShipAttacked(ship, Game.playerShip);
            EndDialogue();
        });
        AddChoice("<color='cyan'>[Leave]</color> Byebye.", () => EndDialogue());*/

    }

    public void EndDialogue() {
        gameObject.SetActive(false);
    }

    public void Update() {

    }
}
