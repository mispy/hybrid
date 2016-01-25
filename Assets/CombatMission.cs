using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
        var desiredSize = 30*((int)difficulty*(int)difficulty);
        var currentSize = 0;

        var toUse = new List<ShipTemplate2>();

        while (currentSize < desiredSize) {
            var found = false;
            var templates = Util.Shuffle(ShipTemplate2.All.ToList());

            foreach (var template in templates) {
                if (currentSize+template.blocks.baseSize <= desiredSize) {
                    toUse.Add(template);
                    currentSize += template.blocks.baseSize;
                    found = true;
                }
            }

            if (found == false) {
                // We've filled as much as we can
                break;
            }
        }

        var enemyShips = new List<Blockform>();
        foreach (var template in toUse) {
            var ship = Blockform.FromTemplate(template);
            ship.transform.position = Game.activeSector.FindSpotFor(ship);
            var floor = ship.blocks.Find("Floor").First();
            var crew = Pool.For("Crew").Attach<CrewBody>(ship.transform);
            crew.transform.position = ship.BlockToWorldPos(floor);
            enemyShips.Add(ship);
        }
     
        Game.activeSector.objectives.Add(new DefeatObjective(difficulty, enemyShips));
    }
}
