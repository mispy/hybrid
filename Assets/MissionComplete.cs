using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class MissionComplete : MonoBehaviour {
    Image blockImage;
    Text rewardAmount;

    void Awake() {
        blockImage = GetComponentInChildren<Button>().image;   
        rewardAmount = blockImage.GetComponentInChildren<Text>();
    }

    public void Reward(Difficulty difficulty) {
        var targetValue = 10*((int)difficulty*(int)difficulty);
        foreach (var type in Util.Shuffle(BlockType.All)) {
            if (type.value <= targetValue) {
                Reward(type, Mathf.RoundToInt(targetValue/type.value));
                break;
            }
        }
    }

    public void Reward(BlockType type, int amount) {
        gameObject.SetActive(true);
        blockImage.sprite = type.sprite;
        rewardAmount.text = "+" + amount.ToString();
        Game.inventory[type] += amount;
        Invoke("Dismiss", 3f);
    }

    public void Dismiss() {
        gameObject.SetActive(false);  
    }
}
