using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ObjectivesUI : MonoBehaviour {
    GameObject lineTemplate;

    public void Start() {
        foreach (Transform child in transform) {
            lineTemplate = GameObject.Instantiate(child.gameObject);
            break;
        }
        InvokeRepeating("UpdateObjectives", 0f, 0.5f);
    }

    public void UpdateObjectives() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        foreach (var objective in Game.activeSector.objectives) {
            var text = Pool.For(lineTemplate).Attach<Text>(transform);
            text.text = objective.Describe();
            if (objective.status == ObjectiveStatus.Complete)
                text.text = "<color=green>" + text.text + "</color>";
        }
    }
}
