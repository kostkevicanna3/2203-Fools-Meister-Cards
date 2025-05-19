using UnityEngine;
using UnityEngine.EventSystems;

public class ResetCardOnDrop : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        DraggablePlayerCard draggableCard = eventData.pointerDrag.GetComponent<DraggablePlayerCard>();
        if(draggableCard != null )
            draggableCard.ResetPosition();
    }
}