using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Common : MonoBehaviour {
    public static void showBackPanel(Transform parent, Transform backPanel) {
        Transform cloneBackPanel = Instantiate(backPanel).transform;
        cloneBackPanel.name = "BackPanel(Clone)";
        cloneBackPanel.SetParent(parent, false);
        cloneBackPanel.localPosition = new Vector2(0, 0);
        cloneBackPanel.SetSiblingIndex(parent.childCount);
    } // 지정객체내에 암막을 복제합니다.

    public static void clearCloneUIObj(Transform target) {
        Transform blackPanel = target.Find("BackPanel(Clone)");
        if (blackPanel != null) {
            Destroy(blackPanel.gameObject);
        }
    } // 지정된 오브젝트에서 스크립트로 생성된 UI들을 모두 제거합니다.

    public static Transform createGroup(string grupName, Vector2 Size, Vector2 position, Transform parent) {
        Transform group = parent.Find(grupName);
        if (group != null) {
            Destroy(group.gameObject);
        }

        group = new GameObject(grupName, typeof(RectTransform)).transform;
        RectTransform groupRect = group.GetComponent<RectTransform>();

        group.SetParent(parent, false);
        group.localPosition = position;
        groupRect.sizeDelta = Size;

        return group;
    } // 빈 GameObject 그룹을 만들고 중복된 그룹은 제거합니다.
}
