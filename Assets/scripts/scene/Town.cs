using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class Town : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public GameObject ScrollObject;
    public GameObject bottomScrollObject;
    public float decelerationRate = 10f;

    private Vector2 startPosition;
    private bool dragFlag; // 드래그 하고있는지 확인
    private bool moveFlag; // 장면을 전환한다.

    private int fastSwipeThresholdDistance = 100;
    private int fastSwipeThresholdMaxLimit = 1080;
    private float fastSwipeThresholdTime = 0.3f;

    private float timeStamp;

    private int currentPage;

    private Vector2 targetPosition;

    private ScrollRect scrollRect;
    
	void Start () {
        scrollRect = this.gameObject.GetComponent<ScrollRect>();
        dragFlag = false;
        currentPage = 0;

        initScrollObjects();
        movePage(currentPage);
    }

    void initScrollObjects() {
        Transform originFirstScrollObj = ScrollObject.transform.GetChild(0);
        Transform originEndScrollObj = ScrollObject.transform.GetChild(ScrollObject.transform.childCount-1);
        Rect scrollObjRect = originFirstScrollObj.GetComponent<RectTransform>().rect;

        Transform firstScrollObj = Instantiate(originFirstScrollObj, ScrollObject.transform, true);
        firstScrollObj.localPosition = new Vector2(originEndScrollObj.localPosition.x + scrollObjRect.width, originEndScrollObj.localPosition.y); 
        Transform endScrollObj = Instantiate(originEndScrollObj, ScrollObject.transform, true);
        endScrollObj.localPosition = new Vector2(originFirstScrollObj.localPosition.x - scrollObjRect.width, originFirstScrollObj.localPosition.y);

        // 양쪽에 더미용 스크롤 페이지를 만든다(끝에서 끝으로 이동하는것처럼 연출하기위해)
    }

	void Update () {
        if (moveFlag) {
            float decelerate = Mathf.Min(decelerationRate * Time.deltaTime, 1f);
            Vector2 movePoint = Vector2.Lerp(ScrollObject.transform.localPosition, targetPosition, decelerate);
            ScrollObject.transform.localPosition = movePoint;
            bottomScrollObject.transform.localPosition = movePoint;

            if (Vector2.SqrMagnitude(new Vector2(ScrollObject.transform.localPosition.x, 0) - targetPosition) < 0.25f) {
                moveFlag = false;

                if (currentPage == ScrollObject.transform.childCount - 2) {
                    currentPage = 0;

                } else if (currentPage == ScrollObject.transform.childCount - 1) {
                    currentPage = ScrollObject.transform.childCount - 3;
                } // infinity scroll

                setTargerPosition(ScrollObject.transform.GetChild(currentPage).localPosition);
                ScrollObject.transform.localPosition = targetPosition;
                scrollRect.movementType = ScrollRect.MovementType.Elastic;
            }
        }
    }

    private void setTargerPosition(Vector2 setPosition) {
        // 객체 내부의 x는 반대로 향하므로 -1로 보정함
        targetPosition = new Vector2(setPosition.x * -1, setPosition.y);
    }

    private void movePage(int index) {
        Vector2 originPosition = ScrollObject.transform.GetChild(index).localPosition;
        setTargerPosition(originPosition); 

        moveFlag = true;
        currentPage = index;
        scrollRect.movementType = ScrollRect.MovementType.Clamped; // 이동 도중에는 드래그가 안되도록함
    }

    public void OnBeginDrag(PointerEventData data) {
        dragFlag = false;
    }
    
    public void OnEndDrag(PointerEventData data) {
        float difference;
        int moveIndex;
        difference = startPosition.x - ScrollObject.transform.position.x;
        
        if (Time.unscaledTime - timeStamp < fastSwipeThresholdTime &&
            Mathf.Abs(difference) > fastSwipeThresholdDistance &&
            Mathf.Abs(difference) < fastSwipeThresholdMaxLimit) {

            moveIndex = currentPage;
            if (moveIndex < ScrollObject.transform.childCount - 2) {
                moveIndex = difference > 0 ? currentPage + 1 : currentPage - 1;
            }
            
            if (moveIndex == -1) {
                moveIndex = ScrollObject.transform.childCount - 1;
                // 코드상에서 생성시킨 맨첫점
            }
            else if (moveIndex == ScrollObject.transform.childCount - 2) {
                moveIndex = ScrollObject.transform.childCount - 2;
                // 코드상에서 생성시킨 맨끝점
            }
            
            movePage(moveIndex);
        }

        dragFlag = false;
    }
    
    public void OnDrag(PointerEventData data) {
        if (!dragFlag) {
            dragFlag = true;
            timeStamp = Time.unscaledTime;
            startPosition = ScrollObject.transform.position;
        }

        if (scrollRect.movementType == ScrollRect.MovementType.Clamped) {
            ScrollObject.transform.localPosition = targetPosition;
            Debug.Log(moveFlag);
        }
    }
}
