using System;
using UnityEngine;
using UnityEngine.EventSystems;
public class Joystick : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public RectTransform backRect; // 背景大圈圈
    public RectTransform stickRect; // "可操作"的小圈圈
    // public Action<Vector2> MoveInput { get; set; }
    private float radius;
    private void Start() {
        radius = backRect.sizeDelta.x * 0.5f;
    }
    public void OnBeginDrag(PointerEventData eventData) {
        stickRect.anchoredPosition = eventData.position - backRect.anchoredPosition;
        backRect.position = eventData.position;
        OnChangeDirection();
    }
    public void OnDrag(PointerEventData eventData) {
        stickRect.anchoredPosition = Vector2.ClampMagnitude(eventData.position - backRect.anchoredPosition, radius);
        OnChangeDirection();
    }
    public void OnEndDrag(PointerEventData eventData) {
        stickRect.anchoredPosition = Vector2.zero;
        OnChangeDirection();
    }
    private void OnChangeDirection() {
        GameController.Instance.PostMsg(new Msg(MsgID.MOBILE_MOVE_INPUT, stickRect.anchoredPosition));
    }
}