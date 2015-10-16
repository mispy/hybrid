using UnityEngine;
using System.Collections;

public class InertiaNegator : BlockComponent {    
    public static void UpdateInertia(Blockform form) {
        var numNegators = 0;
        
        foreach (var block in form.blocks.Find<InertiaNegator>()) {
            if (block.isPowered)
                numNegators += 2;
        }
        
        form.rigidBody.drag = numNegators;
        form.rigidBody.angularDrag = numNegators;
    }

    public void OnEnable() {
        UpdateInertia(form);
    }

    public void OnDisable() {
        UpdateInertia(form);
    }

    public void OnPowered() {
        UpdateInertia(form);
    }

    public void OnDepowered() {
        UpdateInertia(form);
    }
}
