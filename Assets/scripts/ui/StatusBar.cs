using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusBar : MonoBehaviour
{
    private Texture2D BarTexture;
    private Texture2D BackTexture;

    private float maximum;
    private float current;

    public Vector2 position;
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


    public StatusBar (float setValue, Vector2 setPosition, Vector2 setSize, Color setColor) {
        init(setValue, setPosition, setSize, setColor);
    } // unity내에서 new키워드로 객체내 오브젝트 포함못시킴 addComponent를 사용하길 권장함.

    public void setCurrent(float setValue) {
        current = setValue;
    }

    public void setMaximum(float setValue) {
        maximum = setValue;
    }

    public void runProgress(float second) {
        maximum = second;
        origin = 0;
        current = 0;
        StopAllCoroutines();
        StartCoroutine(progress(second));
    }

    private IEnumerator progress(float second) {
        while (current <= second) {
            origin += Time.deltaTime;
            current += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    public void init(float setValue, Vector2 setPosition, Vector2 setSize, Color setColor) {
        maximum = setValue;
        current = setValue;
        position = setPosition;
        size = setSize;
        barColor = setColor;
    }

    public void setPosition(Vector2 positionVector) {
        position = positionVector;
    }

    private void OnGUI() {
        Vector2 texturePosition = new Vector2(position.x - (size.x/2), position.y - (size.y / 2));
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
        }
    }

    private void Start() {
        BarTexture = Resources.Load("imgs/dummy/statusbar") as Texture2D;
        BackTexture = Resources.Load("imgs/dummy/backbar") as Texture2D;
    }

    private void Update() {
        movePoint = current - origin;
        origin = origin + moveSpeed * movePoint;
    }
}
 
 