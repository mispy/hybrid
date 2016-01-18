using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MissionComplete : MonoBehaviour {
    Image blockImage;
    Text rewardAmount;

    void Awake() {
        blockImage = GetComponentInChildren<Button>().image;   
        rewardAmount = blockImage.GetComponentInChildren<Text>();
    }

    public void Reward(Difficulty difficulty) {
        Reward(BlockType.FromId("PlasmaTurret"), 2);
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
