using UnityEngine;
using System.Collections;

public class DialogueMenu : MonoBehaviour {
    public Ship talkingShip;
    public Crew talkingCrew;

    public void Awake() {

    }

    public void StartDialogue(Ship ship) {
        talkingShip = ship;
        talkingCrew = Util.GetRandom(ship.crew);



        gameObject.SetActive(true);
    }

    public void EndDialogue() {
        gameObject.SetActive(false);
    }
}
