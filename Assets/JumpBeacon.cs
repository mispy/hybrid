using UnityEngine;
using System.Collections;

public class JumpBeacon : MonoBehaviour {
    public static string[] names = new string[] {
        "Desmauliv",
        "Tufloirus",
        "Zaglora",
        "Ostarth",
        "Fuzuno",
        "Sioter",
        "Brecezuno",
        "Flaboruta",
        "Swinda",
        "Flao" 
    };
        
    public string name;
    public CombatMission mission;

    void Awake() {
        name = Util.GetRandom(names);
        mission = new CombatMission();
    }
}
