using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeOverlay : MonoBehaviour {
    Image image;
    float elapsed;
    float duration;
    bool fadingOut = true;

	void Awake() {
        image = GetComponent<Image>();
	}

    public void FadeOut(float duration) {
        this.duration = duration;
        elapsed = 0f;
        fadingOut = true;
        gameObject.SetActive(true);

        image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
    }

    public void FadeIn(float duration) {
        this.duration = duration;
        elapsed = 0f;
        fadingOut = false;
        gameObject.SetActive(true);

        image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
    }
	
	// Update is called once per frame
	void Update() {
        if (elapsed >= duration && !fadingOut) {
            gameObject.SetActive(false);
            return;
        }

        elapsed += Time.deltaTime;

        if (fadingOut) {
            image.color = new Color(image.color.r, image.color.g, image.color.b, elapsed/duration);
        } else {
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1 - elapsed/duration);
        }
	}
}
