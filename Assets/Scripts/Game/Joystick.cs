using System;
using UnityEngine;
using UnityEngine.EventSystems;
public class Joystick : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public RectTransform backRect;
    public RectTransform stickRect;
    public Action<Vector2> changeDirection;
    private Vector2 direction;
    private float radius;
    private void Start() {
        radius = backRect.sizeDelta.x * 0.5f;
    }
    public void OnBeginDrag(PointerEventData eventData) {
        stickRect.anchoredPosition = eventData.position - backRect.anchoredPosition;
        OnChangeDirection(stickRect.anchoredPosition);
    }
    public void OnDrag(PointerEventData eventData) {
        stickRect.anchoredPosition = Vector2.ClampMagnitude(eventData.position - backRect.anchoredPosition, radius);
        OnChangeDirection(stickRect.anchoredPosition);
    }
    public void OnEndDrag(PointerEventData eventData) {
        stickRect.anchoredPosition = Vector2.zero;
        OnChangeDirection(stickRect.anchoredPosition);
    }
    private void OnChangeDirection(Vector2 direction) {
        if (changeDirection != null) {
            changeDirection(stickRect.anchoredPosition);
        }
    }
}