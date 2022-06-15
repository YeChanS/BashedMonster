using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class VirtualStickControl : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler 
{

    Image _bg;
    Image _stick;

    Vector3 _inputVector;

    void Awake()
    {
        _bg = GetComponent<Image>();
        _stick = transform.GetChild(0).GetComponent<Image>();
    }


    public float _horizontalValue
    {
        get { return _inputVector.x; }
    }

    public float _verticalValue
    {
        get { return _inputVector.y;}
    }

    //패드 위치 받아와서 pos값 넣어주기.
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_bg.rectTransform, eventData.position, eventData.pressEventCamera, out pos))
        {
            pos.x = (pos.x / _bg.rectTransform.sizeDelta.x);
            pos.y = (pos.y / _bg.rectTransform.sizeDelta.y);

            _inputVector = new Vector3(pos.x, pos.y, 0);
            _inputVector = (_inputVector.magnitude > 1) ? _inputVector.normalized : _inputVector;

            _stick.rectTransform.anchoredPosition = new Vector3(_inputVector.x * (_bg.rectTransform.sizeDelta.x / 2)
                                                                , _inputVector.y * (_bg.rectTransform.sizeDelta.y / 2));
        }
    }


    //패드 원위치로 이동.
    public void OnPointerUp(PointerEventData eventData)
    {
        _inputVector = Vector3.zero;
        _stick.rectTransform.anchoredPosition = _inputVector;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }
}
