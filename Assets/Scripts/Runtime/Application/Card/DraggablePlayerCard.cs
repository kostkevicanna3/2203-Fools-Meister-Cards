using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggablePlayerCard : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Image _cardImage;

    private Vector3 _defaultPosition;
    private Card _card;
    private Transform _originalParent;

    public Card Card => _card;

    public void Initialize(Sprite cardImage, Card card)
    {
        _cardImage.sprite = cardImage;
        _card = card;
        _originalParent = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = false;
        transform.SetParent(transform.parent.parent.parent.parent, true);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position += new Vector3(eventData.delta.x, eventData.delta.y, 0);
    }

    public void ResetPosition()
    {
        transform.SetParent(_originalParent, false);
    }
}
