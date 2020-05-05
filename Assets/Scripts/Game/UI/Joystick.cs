using UnityEngine;
using UnityEngine.EventSystems;
public class Joystick : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public RectTransform backRect; // 背景大圈圈
    public RectTransform stickRect; // "可操作"的小圈圈
    // public Action<Vector2> MoveInput { get; set; }
    private float radius;
    private Vector2 originalPosition;
    private void Start() {
        radius = backRect.sizeDelta.x * 0.4f;
        originalPosition = backRect.position;
        transform.localScale = new Vector3(1f, 1f, 1f);
    }
    public void OnBeginDrag(PointerEventData eventData) {
        // stickRect.anchoredPosition = (eventData.position - (Vector2) backRect.position).normalized * radius;
        backRect.position = eventData.position;
        OnChangeDirection();
    }
    public void OnDrag(PointerEventData eventData) {
        stickRect.anchoredPosition = Vector2.ClampMagnitude(eventData.position - (Vector2) backRect.position, radius);
        // stickRect.anchoredPosition = (eventData.position - (Vector2) backRect.position).normalized * radius;
        OnChangeDirection();
    }
    public void OnEndDrag(PointerEventData eventData) {
        stickRect.anchoredPosition = Vector2.zero;
        backRect.position = originalPosition;
        OnChangeDirection();
    }
    private void OnChangeDirection() {
        Messager.Instance.Send<Vector2>(MessageID.MOBILE_MOVE_INPUT, stickRect.anchoredPosition);
    }
}