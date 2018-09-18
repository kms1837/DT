using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour {
    private Color fadeColor;

    private Image fadeImage;

    public Sprite fadeTexture;
    public float fadeTime;
    public float waitTime;

    void Start() {
        fadeImage = this.gameObject.GetComponent<Image>();
    }

    private void fadeInit(Color setColor) {
        this.gameObject.SetActive(true);
        fadeColor = setColor;
        //fadeImage.color = fadeColor;
    }

    public void fadeInStart(UnityEngine.Events.UnityAction endCallback) {
        fadeInit(new Color(0f, 0f, 0f, 1f));

        StartCoroutine(FadeIn(endCallback));
    }

    IEnumerator FadeIn(UnityEngine.Events.UnityAction endCallback) {
        float elapsedTime = 0f;

        yield return new WaitForSeconds(waitTime);

        while (elapsedTime < fadeTime) {
            yield return new WaitForEndOfFrame();
            elapsedTime += Time.deltaTime;
            fadeColor.a = Mathf.Clamp01(elapsedTime / fadeTime);
            fadeImage.color = fadeColor;
        }

        yield return new WaitForSecondsRealtime(0.5f);

        if (endCallback != null) {
            endCallback();
        }
    }

    public void fadeOutStart(UnityEngine.Events.UnityAction endCallback) {
        fadeInit(new Color(0f, 0f, 0f, 0f));

        StartCoroutine(FadeOut(endCallback));
    }

    IEnumerator FadeOut(UnityEngine.Events.UnityAction endCallback) {
        float elapsedTime = 0f;

        yield return new WaitForSeconds(waitTime);

        while (elapsedTime < fadeTime) {
            yield return new WaitForEndOfFrame();
            elapsedTime += Time.deltaTime;
            fadeColor.a = 1.0f - Mathf.Clamp01(elapsedTime / fadeTime);
            fadeImage.color = fadeColor;
        }

        yield return new WaitForSecondsRealtime(0.5f);

        if (endCallback != null) {
            endCallback();
        }
    }

    public void fadeInOutStart(UnityEngine.Events.UnityAction endCallback) {
        fadeInit(new Color(255, 255, 255, 0f));

        StartCoroutine(FadeIn(() => {
            StartCoroutine(FadeOut(endCallback));
        }));
    }
}
