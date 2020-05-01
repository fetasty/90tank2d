using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class MobileFire : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData) {
        Messager.Instance.Send<bool>(MessageID.MOBILE_FIRE_INPUT, true);
    }
    public void OnPointerUp(PointerEventData eventData) {
        Messager.Instance.Send<bool>(MessageID.MOBILE_FIRE_INPUT, false);
    }
}
