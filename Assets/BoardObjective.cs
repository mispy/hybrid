using UnityEngine;
using System;
using System.Collections;

public interface IObjective {
    string Describe();
    ObjectiveStatus status { get; }
}

public enum ObjectiveStatus {
    Active,
    Failed,
    Complete
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