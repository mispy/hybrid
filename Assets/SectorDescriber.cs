using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class SectorDescriber : MonoBehaviour {
	Text text;

    void Update () {
        var beacon = Game.jumpMap.selectedBeacon;
        text = GetComponentInChildren<Text>();
        text.text = String.Format("{0}\n\n<color=red>Combat Mission</color>\nObjectives: Defeat all enemies\nDifficulty: {1}", beacon.name, beacon.mission.difficulty.ToFancyString());
	}
}
