using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class MobileFire : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData) {
        GameController.Instance.PostMsg(new Msg(MsgID.MOBILE_FIRE_INPUT, true));
    }
    public void OnPointerUp(PointerEventData eventData) {
        GameController.Instance.PostMsg(new Msg(MsgID.MOBILE_FIRE_INPUT, false));
    }
}
