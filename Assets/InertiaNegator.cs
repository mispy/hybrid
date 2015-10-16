using UnityEngine;
using System.Collections;

public class InertiaNegator : BlockComponent {
    public void OnEnable() {
        UpdateInertia();

    }

    public void OnDisable() {
        UpdateInertia();
    }

    public void OnPowered() {
        UpdateInertia();
    }

    public void OnDepowered() {
        UpdateInertia();
    }

    void UpdateInertia() {
        bool hasNegator = false;
        foreach (var block in form.blocks.Find<InertiaNegator>()) {
            if (block.isPowered)
                hasNegator = true;
        }

        if (hasNegator) {
            form.rigidBody.drag = 2;
            form.rigidBody.angularDrag = 2;
        } else {
            form.rigidBody.drag = 0;
            form.rigidBody.angularDrag = 0;
        }
    }
}
