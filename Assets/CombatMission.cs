using UnityEngine;
using System;
using System.Collections;

public static class DifficultyExtensions {
    public static string ToFancyString(this Difficulty difficulty) {
        if (difficulty == Difficulty.VeryEasy) {
            return "<color=lime>Very Easy</color>";
        } else if (difficulty == Difficulty.Easy) {
            return "<color=green>Easy</color>";
        } else if (difficulty == Difficulty.Medium) {
            return "<color=orange>Medium</color>";
        } else if (difficulty == Difficulty.Hard) {
            return "<color=maroon>Hard</color>";
        } else if (difficulty == Difficulty.VeryHard) {
            return "<color=red>Very Hard</color>";
        }

        return "???";
    }
}

public enum Difficulty {
    VeryEasy=1,
    Easy=2,
    Medium=3,
    Hard=4,
    VeryHard=5
}

public class CombatMission {
    public Difficulty difficulty;

    public CombatMission() {
        difficulty = Util.GetRandom((Difficulty[])Enum.GetValues(typeof(Difficulty)));
    }

    public void Activate() {
    }
}
