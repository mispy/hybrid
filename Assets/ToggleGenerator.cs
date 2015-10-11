using UnityEngine;
using System.Collections;

public class ToggleGenerator : BlockAbility {
    public override bool WorksWith(Block block) {
        return block.type.GetComponent<PowerProducer>() != null;
    }
    
    void OnEnable() {
        foreach (var block in blocks) {
            var producer = block.gameObject.GetComponent<PowerProducer>();
            producer.isProducing = !producer.isProducing;
        }    
        
        gameObject.SetActive(false);
    }
}
