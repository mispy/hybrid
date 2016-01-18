using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public interface IObjective {
    string Describe();
    ObjectiveStatus status { get; }
}

public enum ObjectiveStatus {
    Active,
    Failed,
    Complete
}

public class DefeatObjective : IObjective {
    List<Blockform> enemies;
    bool seenEnemies = false;
    Difficulty difficulty;

    public ObjectiveStatus status { get; private set; }

    string IObjective.Describe() {
        var remainingEnemies = enemies.Count;
        foreach (var enemy in enemies) {
            if (enemy.poweredWeapons.Count == 0)
                remainingEnemies -= 1;
        }

        if (seenEnemies && remainingEnemies == 0 && status != ObjectiveStatus.Complete) {
            status = ObjectiveStatus.Complete;
            Game.missionComplete.Reward(difficulty);
        } else if (remainingEnemies > 0)
            seenEnemies = true;
        
        return String.Format("Defeat enemy ships {0}/{1}", enemies.Count-remainingEnemies, enemies.Count);
    }

    public DefeatObjective(Difficulty difficulty, List<Blockform> enemies) {
        this.difficulty = difficulty;
        this.enemies = enemies;   
    }
}

public class BoardObjective : MonoBehaviour, IObjective {
    public Blockform target;
    public ObjectiveStatus status { get; private set; }

    string IObjective.Describe() {
        return "";//String.Format("Defeat the {0}", target.name);            
    }

    void Start() {
        InvokeRepeating("UpdateStatus", 0f, 1f);
    }

    void UpdateStatus() {
        status = ObjectiveStatus.Active;
        foreach (var player in Game.players) {
            if (player.maglockShip == target)
                status = ObjectiveStatus.Complete;
        }
    }
}