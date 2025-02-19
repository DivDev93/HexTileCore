using PrimeTween;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityUtils;



public class PointerClickDraggable : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, ISelectHandler
{
    [Inject]
    TileGameDataScriptableObject gameData;

    //script to enable draggable functionality on attached gameobject
    private Vector3 mOffset;
    private float mZCoord;
    private float mXCoord;
    private float mYCoord;
    public bool isDragging = false;
    public bool isDragged = false;
    private Vector3 mPos;

    private Vector3 mLastPos;
    private Vector3 mLastPos2;
    //float startPosY = 0f;
    Tween tweenUp;

    void Awake()
    {
        //startPosY = transform.position.y;
        
    }

    public void TweenUp()
    {
        if (!tweenUp.IsPlaying())
        {
            tweenUp = Tween.PositionY(transform, gameData.cardSelectionPlaneHeight, gameData.selectAnimationDuration);//.OnComplete(() => isTweeningUp = false);
            tweenUp.SetEase(Ease.InBounce);
        }
    }
   
    public void OnPointerClick(PointerEventData eventData)
    {
        //if (!isDragged)
        //{
        //    Debug.Log("Clicked: " + gameObject.name);
        //}
        isDragged = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(tweenUp.IsPlaying())
        {
            tweenUp.Stop();
        }

        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        mXCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).x;
        mYCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).y;
        mPos = eventData.pointerCurrentRaycast.worldPosition.With(y: gameData.cardSelectionPlaneHeight);
        mLastPos = gameObject.transform.position;
        mLastPos2 = gameObject.transform.position;
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        mPos = eventData.pointerCurrentRaycast.worldPosition.With(y: gameData.cardSelectionPlaneHeight);
        if (isDragging)
        {
            transform.position = mPos + gameData.YOffset * Vector3.up;
            isDragged = true;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        //if (Vector3.Distance(mLastPos, mPos) < 0.1f)
        //{
        //    Debug.Log("Clicked: " + gameObject.name);
        //}
    }

    private Vector3 GetMouseAsWorldPoint(Vector3 mousePoint)
    {
        // Pixel coordinates of mouse (x,y)
        // z coordinate of game object on screen
        mousePoint.z = mZCoord;
        // Convert it to world points
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    public void OnSelect(BaseEventData eventData)
    {
        
    }
}
