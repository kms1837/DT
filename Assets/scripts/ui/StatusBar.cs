using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour
{
    private bool guiFlag = false; // onGUI flag
    public Vector2 position; // onGUI ver option

    private Image barImg;
    private Image moveBarImg;

    private Texture2D BarTexture;
    private Texture2D BackTexture;

    private float maximum;
    private float current;

    private Vector2 size;
    private Color barColor;

    private float origin;

    private float movePoint;

    const float moveSpeed = 0.05f;

    /*
     * 
     1. StatusBar temp = AddComponent<StatusBar>();
     2. temp.init(maximum, position, size, color);
     3. update temp.setCurrent() or temp.runProgress(second);
     */

    public void setCurrent(float setValue) {
        current = setValue;
    }

    public void setMaximum(float setValue) {
        maximum = setValue;
    }

    public void runProgress() {
        origin = 0;
        current = 0;
        StopAllCoroutines();
        StartCoroutine(progress(maximum));
    }

    private IEnumerator progress(float second) {
        while (current <= second) {
            origin += Time.deltaTime;
            current += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    public void init(float setValue, Color setColor) {
        maximum = setValue;
        current = setValue;
        barColor = setColor;

        barImg = this.transform.Find("Bar").GetComponent<Image>();
        moveBarImg = this.transform.Find("MoveBar").GetComponent<Image>();
        barImg.color = barColor;
    }

    public void init(bool mode, float setValue, Vector2 setPosition, Vector2 setSize, Color setColor) {
        this.init(setValue, setColor);
        position = setPosition;
        size = setSize;
    }// GUI ver

    public void setPosition(Vector2 positionVector) {
        position = positionVector;
    }

    private void Start() {
        if (guiFlag) {
            BarTexture = Resources.Load("imgs/dummy/statusbar") as Texture2D;
            BackTexture = Resources.Load("imgs/dummy/backbar") as Texture2D;
        }
    }

    private void Update() {
        movePoint = current - origin;
        origin = origin + moveSpeed * movePoint;

        if (maximum > 0 && current > 0 && maximum >= current) {
            float moveBarEndPoint = movePoint < 0 ? (origin / maximum) : (current / maximum);
            float barEndPoint = movePoint < 0 ? (current / maximum) : (origin / maximum);

            barImg.fillAmount = barEndPoint;
            moveBarImg.fillAmount = moveBarEndPoint;
        }
    }

    private void OnGUI() {
        if (guiFlag) {
            Vector2 texturePosition = new Vector2(position.x - (size.x / 2), position.y - (size.y / 2));
            GUI.DrawTexture(new Rect(texturePosition, size), BackTexture);

            if (maximum > 0 && current > 0 && maximum >= current) {
                Rect moveBarRect;
                Rect barRect;

                float moveBarEndPoint = movePoint < 0 ? (maximum / origin) : (maximum / current);
                float barEndPoint = movePoint < 0 ? (maximum / current) : (maximum / origin);

                moveBarRect = new Rect(texturePosition, new Vector2((size.x / moveBarEndPoint), size.y));
                barRect = new Rect(texturePosition, new Vector2((size.x / barEndPoint), size.y));

                GUI.color = new Color(0, 0, 0, 0.5f);
                GUI.DrawTexture(moveBarRect, BarTexture);
                GUI.color = barColor;
                GUI.DrawTexture(barRect, BarTexture);
                // Graphics.DrawTexture (권장사항 https://docs.unity3d.com/kr/530/ScriptReference/Graphics.DrawTexture.html)
            }
        }
    }
}
 
 